using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json;
using StonksWebApp.models;
using Yahoo.Finance;


namespace StonksWebApp.Services;

public static class FetchingService
{
    public static Dictionary<string, TickerInfoMap> TickerInfoMap = new();
    private static Dictionary<string, CachedCurrentStockPrice> _priceCache = new();
    public static string FinnhubKey = "";
    private static OutgoingRateLimiter _limiter = new OutgoingRateLimiter();
    private static string GetFinnhubApiKey()
    {
        return FinnhubKey;
    }

    public static PriceCandleModel[] GetHistoricalPrices(string symbol, DateTime from, DateTime to)
    {
        var provider = new HistoricalDataProvider();
        
        provider.DownloadHistoricalDataAsync(symbol, from, to).Wait();
        if (provider.DownloadResult != HistoricalDataDownloadResult.Successful)
        {
            return Array.Empty<PriceCandleModel>();
        }

        var results = new List<PriceCandleModel>();
        foreach (var record in provider.HistoricalData)
        {
            results.Add(new PriceCandleModel(record.Date, record.High, record.Low, record.Open, record.Close, record.Volume));
        }
        return results.ToArray();
    }
    
    public static FinnhubPriceResponse GetCurrentPrice(string ticker)
    {
        if (!TickerInfoMap.ContainsKey(ticker.ToUpper()))
        {
            return new FinnhubPriceResponse();
        }

        if (_priceCache.TryGetValue(ticker,out CachedCurrentStockPrice? cached))
        {
            if (cached.Expires > DateTime.UtcNow)
            {
                return cached.PriceResponse;
            }
        }

        _limiter.WaitForFinnhubLimiter();

        HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://finnhub.io/api/v1/quote?symbol={ticker}");
        request.Headers.Add("X-Finnhub-Token" , GetFinnhubApiKey());
        var response = client.Send(request);
        var deserialized = JsonSerializer.Deserialize<FinnhubPriceResponse>(response.Content.ReadAsStringAsync().Result);
        return deserialized ?? new FinnhubPriceResponse();
    }

    public static string ScrapeSubmissions(string url)
    {
        var driver = new ChromeDriver();
        driver.Url = url;
        Thread.Sleep(200);
        var elem = driver.FindElement(By.TagName("pre"));
        var text = elem.Text;
        driver.Close();
        return text ?? "";
    }

    public static async void GetAndUpdateSingleValue(string ticker)
    {
        string cik;
        try
        {
            cik = TickerInfoMap[ticker.ToUpper()].CIK.ToString();
        }
        catch
        {
            Console.WriteLine($"Ticker: {ticker} does not have a CIK mapping, and thus can not be upserted");
            return;
        }
        var cikPadded = cik.PadLeft(10, '0');
        var companyInfoUrl = $"https://data.sec.gov/submissions/CIK{cikPadded}.json";

        _limiter.WaitForSECLimiter();

        HttpClient client = new HttpClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, companyInfoUrl);
        request.Headers.Host = "www.sec.gov";
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Chromium", "120"));
        request.Headers.Referrer = new Uri("https://data.sec.gov");
        bool fallback = false;
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"SEC request for {ticker} failed, falling back to scraping mechanism");
            fallback = true;
        }

        string jsonContent = !fallback ? await response.Content.ReadAsStringAsync() : ScrapeSubmissions(companyInfoUrl);

        var deserialized = JsonSerializer.Deserialize<CompanyFactsResponse>(jsonContent);

        if (deserialized == null)
        {
            Console.WriteLine($"Deserialization of {ticker} json response failed");
            return;
        }

        DatabaseConnectionService service = DatabaseConnectionService.Instance;

        string baseUrl = $"https://www.sec.gov/Archives/edgar/data/{deserialized.Cik}/";

        Range monthRange = 0..2;
        Range dayRange = 2..4;

        DateTime fiscalYearEnd =
            DateTime.Parse($"2000-{deserialized.FiscalYearEnd[monthRange]}-{deserialized.FiscalYearEnd[dayRange]}");

        HttpRequestMessage finnhubRequest =
            new HttpRequestMessage(HttpMethod.Get, $"https://finnhub.io/api/v1/stock/profile2?symbol={deserialized.Tickers[0].ToUpper()}");
        finnhubRequest.Headers.Add("X-Finnhub-Token",GetFinnhubApiKey());

        var finnhubResponse = await client.SendAsync(finnhubRequest);
        var finnhubMessage = finnhubResponse.EnsureSuccessStatusCode();
        var serializedFinnhubObj = finnhubMessage.Content.ReadAsStringAsync().Result;
        var finnhubObj = JsonSerializer.Deserialize<FinnhubCompanyInfo>(serializedFinnhubObj);
        if (finnhubObj == null)
        {
            Console.WriteLine("Finnhub response deserialization failed");
            return;
        }

        string query = @$"
        INSERT INTO Companies (Id, CName, CIK, FiscalYearEnd, Ticker, Shares, Sector)
        VALUES (DEFAULT, '{deserialized.Name}', {deserialized.Cik}, '{fiscalYearEnd}', '{deserialized.Tickers[0]}', {(int)finnhubObj.ShareOutstanding*10000}, '{finnhubObj.FinnhubIndustry}')
        ON CONFLICT (CIK) DO UPDATE
        SET
        CName = '{deserialized.Name}',
        FiscalYearEnd = '{fiscalYearEnd}',
        Ticker = '{deserialized.Tickers[0]}',
        Shares = {(int)finnhubObj.ShareOutstanding * 10000},
        Sector = '{finnhubObj.FinnhubIndustry}'
        RETURNING Id;
        ";

        int companyId = service.UpsertAndReturnId(query);
        int numRows = 1;
        for (int i = 0; i < deserialized.Filings.Recent.AccessionNumber.Length; i++)
        {
            string upsert = @$"
                INSERT INTO Filings (Id, CompanyId, Link, FilingDate, FilingType, Org)
                VALUES (DEFAULT, {companyId}, '{baseUrl + deserialized.Filings.Recent.AccessionNumber[i].Replace("-", "") + "/" + deserialized.Filings.Recent.PrimaryDocument[i]}', '{deserialized.Filings.Recent.AcceptanceDateTime[i]}', '{deserialized.Filings.Recent.Form[i]}', 'SEC');
            ";
            var _ = service.UpsertRows(upsert);
            numRows++;
        }

        Console.WriteLine($"Upsert: {numRows} rows affected");
    }

    public static async Task UpdateDatabaseFromCsv(string csv)
    {
        string tickerPattern = @"^[A-Z0-9\.\-\^]{1,8}$";
        Regex match = new Regex(tickerPattern);

        List<string> parsed = csv.Split(',').ToList();

        for (int i = parsed.Count - 1; i >= 0; i--)
        {
            if (!match.IsMatch(parsed[i]))
            {
                parsed.RemoveAt(i);
            }
        }

        Stack<string> parsedStack = new Stack<string>(parsed);

        int reqCount = 0;
        while (parsedStack.Count > 0)
        {
            string current = parsedStack.Pop();
            GetAndUpdateSingleValue(current);
            reqCount++;
            if (reqCount == 10)
            {
                await Task.Delay(1500);
                reqCount = 0;
            }
        }
    }

    public static Dictionary<string, string> GetTickerCikMapping()
    {
        Dictionary<string, string> tickerCikMapping = new Dictionary<string, string>();
        ChromeDriver driver = new ChromeDriver();
        driver.Url = "https://www.sec.gov/include/ticker.txt";
        Thread.Sleep(2000);
        var elem = driver.FindElement(By.TagName("pre"));
        var text = elem.Text;
        foreach (var line in text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            string[] tokens = line.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            tickerCikMapping.Add(tokens[0], tokens[^1]);
        }
        driver.Close();
        return tickerCikMapping;
    }

    public static Dictionary<string, TickerInfoMap> GetTickerInfoMap()
    {
        Dictionary<string,TickerInfoMap> map = new Dictionary<string, TickerInfoMap>();
        CookieContainer cookies = new CookieContainer();
        using HttpClientHandler handler = new HttpClientHandler{CookieContainer = cookies, AutomaticDecompression = DecompressionMethods.GZip};
        HttpClient client = new HttpClient(handler);
        HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Get, "https://www.sec.gov/files/company_tickers_exchange.json");

        request.Headers.Referrer = new Uri("https://www.sec.gov");
        request.Headers.Host = "www.sec.gov";
        request.Headers.AcceptEncoding.Clear();
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Chromium", "120"));
        

        var response = client.SendAsync(request).Result;
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("CIK Fetch cringed, falling back to local CIK file");
            return GetTickerInfoMapFromFile();
        }

        var result = response.Content.ReadAsStringAsync().Result;
        return DeserializeAndMapTickerInfo(result);
    }

    public static Dictionary<string, TickerInfoMap> GetTickerInfoMapFromFile()
    {
        Dictionary<string, TickerInfoMap> map = new Dictionary<string, TickerInfoMap>();

        string relativePathToFile = "Resources/CIKS.json";
        var ciksFile = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),relativePathToFile));

        return DeserializeAndMapTickerInfo(ciksFile);
    }

    private static Dictionary<string, TickerInfoMap> DeserializeAndMapTickerInfo(string json)
    {
        var map = new Dictionary<string, TickerInfoMap>();
        dynamic? deserialized = JsonConvert.DeserializeObject(json);

        if (deserialized == null)
        {
            throw new Exception("CIK map deserialization cringed");
        }

        foreach (var arr in deserialized.data)
        {
            int cik = (int)arr[0];
            string name = (string)arr[1];
            string ticker = (string)arr[2];
            string exchange = (string)arr[3];
            TickerInfoMap tickerInfo = new TickerInfoMap(
                cik,
                name,
                ticker.ToUpper(),
                exchange
            );
            map.Add(ticker, tickerInfo);
        }

        return map;
    }
}

public class CompanyFactsResponse
{
    [JsonPropertyName("cik")]
    public string Cik { get; set; }

    [JsonPropertyName("entityType")]
    public string EntityType { get; set; }

    [JsonPropertyName("sic")]
    public string Sic { get; set; }

    [JsonPropertyName("sicDescription")]
    public string SicDecription { get; set; }

    [JsonPropertyName("insiderTransactionForOwnerExists")]
    public int InsiderTransactionForOwnerExists { get; set; }

    [JsonPropertyName("insiderTransactionForIssuerExists")]
    public int InsiderTransactionForIssuerExists { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("tickers")]
    public string[] Tickers { get; set; }

    [JsonPropertyName("exchanges")]
    public string[] Exchanges { get; set; }

    [JsonPropertyName("ein")]
    public string Ein { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("website")]
    public string Website { get; set; }

    [JsonPropertyName("investorWebsite")]
    public string InvestorWebsite { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("fiscalYearEnd")]
    public string FiscalYearEnd { get; set; }

    [JsonPropertyName("stateOfIncorporation")]
    public string StateOfIncorporation { get; set; }

    [JsonPropertyName("stateOfIncorporationDescription")]
    public string StateOfIncorporationDescription { get; set; }

    [JsonPropertyName("addresses")]
    public EdgarCompanyAddressContainer Addresses { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("flags")]
    public string Flags { get; set; }

    [JsonPropertyName("formerNames")]
    public FormerName[] FormerNames { get; set; }

    [JsonPropertyName("filings")]
    public EdgarFilings Filings { get; set; }

    public CompanyFactsResponse()
    {

    }
}

public class EdgarFilings
{
    [JsonPropertyName("recent")]
    public Filings Recent { get; set; }
    [JsonPropertyName("files")]
    public EdgarFiles[] Files { get; set; }

    public EdgarFilings()
    {
            
    }
}

public class EdgarFiles
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("filingCount")]
    public int FilingCount { get; set; }
    [JsonPropertyName("filingFrom")]
    public string FilingFrom { get; set; }
    [JsonPropertyName("filingTo")]
    public string FilingTo { get; set; }

    public EdgarFiles()
    {
            
    }
}

public class EdgarCompanyAddressContainer
{
    [JsonPropertyName("mailing")]
    public EdgarCompanyAddress Mailing { get; set; }
    [JsonPropertyName("business")]
    public EdgarCompanyAddress Business { get; set; }

    public EdgarCompanyAddressContainer()
    {
            
    }
}
public class EdgarCompanyAddress
{
    [JsonPropertyName("street1")]
    public string Street1 { get; set; }
    [JsonPropertyName("street2")]
    public string? Street2 { get; set; }
    [JsonPropertyName("city")]
    public string City { get; set; }
    [JsonPropertyName("stateOrCounty")]
    public string StateOrCounty { get; set; }
    [JsonPropertyName("zipCode")]
    public string ZipCode { get; set; }
    [JsonPropertyName("stateOrCountyDescription")]
    public string StateOrCountyDescription { get; set; }

    public EdgarCompanyAddress()
    {
            
    }
}

public class FormerName
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("from")]
    public string From { get; set; }
    [JsonPropertyName("to")]
    public string To { get; set; }

    public FormerName()
    {
        
    }
}

public class Filings
{
    [JsonPropertyName("accessionNumber")]
    public string[] AccessionNumber { get; set; }

    [JsonPropertyName("filingDate")]
    public string[] FilingDate { get; set; }

    [JsonPropertyName("reportDate")]
    public string[] ReportDate { get; set; }

    [JsonPropertyName("acceptanceDateTime")]
    public string[] AcceptanceDateTime { get; set; }

    [JsonPropertyName("act")]
    public string[] Act { get; set; }

    [JsonPropertyName("form")]
    public string[] Form { get; set; }

    [JsonPropertyName("fileNumber")]
    public string[] FileNumber { get; set; }

    [JsonPropertyName("filmNumber")]
    public string[] FilmNumber { get; set; }

    [JsonPropertyName("items")]
    public string[] Items { get; set; }

    [JsonPropertyName("size")]
    public int[] Size { get; set; }

    [JsonPropertyName("isXBRL")]
    public int[] IsXBRL { get; set; }

    [JsonPropertyName("isInlineXBRL")]
    public int[] IsInlineXBRL { get; set; }

    [JsonPropertyName("primaryDocument")]
    public string[] PrimaryDocument { get; set; }

    [JsonPropertyName("primaryDocDescription")]
    public string[] PrimaryDocDescription { get; set; }

    public Filings()
    {
            
    }
}

public class TickerInfoMap
{
    public int CIK;
    public string Name;
    public string Ticker;
    public string Exchange;

    public TickerInfoMap(int cik, string name, string ticker, string exchange)
    {
        CIK = cik;
        Name = name;
        Ticker = ticker;
        Exchange = exchange;
    }

    public TickerInfoMap()
    {

    }
}

public class CTEFormat
{
    [JsonPropertyName("fields")]
    public string[] Fields { get; set; }
    [JsonPropertyName("data")]
    public string[][] Data { get; set; }

    public CTEFormat()
    {
        
    }
}

public class FinnhubPriceResponse
{
    [JsonPropertyName("c")]
    public double CurrentPrice { get; set; }

    [JsonPropertyName("d")]
    public double Change { get; set; }

    [JsonPropertyName("dp")]
    public double PercentChange { get; set; }

    [JsonPropertyName("h")]
    public double HighPrice { get; set; }

    [JsonPropertyName("l")]
    public double LowPrice { get; set; }

    [JsonPropertyName("o")]
    public double OpenPrice { get; set; }

    [JsonPropertyName("pc")]
    public double PreviousClosePrice { get; set; }
}

public class CachedCurrentStockPrice
{
    public FinnhubPriceResponse PriceResponse { get; set; }
    public DateTime Expires { get; set; }

    public CachedCurrentStockPrice(FinnhubPriceResponse priceResponse)
    {
        Expires = DateTime.UtcNow + new TimeSpan(0, 0, 5, 0);
        PriceResponse = priceResponse;
    }
}

public class FinnhubCompanyInfo
{
    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("exchange")]
    public string Exchange { get; set; }

    [JsonPropertyName("ipo")]
    public DateTime Ipo { get; set; }

    [JsonPropertyName("marketCapitalization")]
    public double MarketCapitalization { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("shareOutstanding")]
    public double ShareOutstanding { get; set; }

    [JsonPropertyName("ticker")]
    public string Ticker { get; set; }

    [JsonPropertyName("weburl")]
    public string WebUrl { get; set; }

    [JsonPropertyName("logo")]
    public string Logo { get; set; }

    [JsonPropertyName("finnhubIndustry")]
    public string FinnhubIndustry { get; set; }

    public FinnhubCompanyInfo()
    {
        
    }
}
