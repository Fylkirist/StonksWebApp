using StonksWebApp.connections;

namespace StonksWebApp.models;

public class CompanyFinancialModel
{
    public int CompanyId { get; set; }
    public string Name { get; set; }
    public string Ticker { get; set; }
    public int CIK { get; set; }
    public DateTime FiscalYearStart { get; set; }

    public long Shares { get; set; }
    public string Sector { get; set; }

    public CompanyFinancialModel(string name, string ticker, int cik, DateTime fiscalYearStart, long shares, string sector, int companyId)
    {
        Name = name;
        Ticker = ticker;
        CIK = cik;
        FiscalYearStart = fiscalYearStart;
        Shares = shares;
        Sector = sector;
        CompanyId = companyId;
    }

    public CompanyFinancialModel()
    {

    }

    public static CompanyFinancialModel[] ConvertQueryResults(QueryResult result)
    {
        int num = result.Rows.Length;
        var models = new CompanyFinancialModel[num];
        Dictionary<string,int> columnMapping = new Dictionary<string,int>();
        for (int i = 0; i <result.Columns.Length; i++)
        {
            columnMapping[result.Columns[i]] = i;
        }

        int idx = 0;
        foreach (var row in result.Rows)
        {
            int id = columnMapping.ContainsKey("id") ? row.Data[columnMapping["id"]] : -1;
            string name = columnMapping.ContainsKey("cname") ? row.Data[columnMapping["cname"]] : "";
            string ticker = columnMapping.ContainsKey("ticker") ? row.Data[columnMapping["ticker"]] : "";
            int cik = columnMapping.ContainsKey("cik") ? row.Data[columnMapping["cik"]] : 0;
            string date = columnMapping.ContainsKey("fiscalyearend") ? row.Data[columnMapping["fiscalyearend"]].ToString() : DateTime.MinValue.ToString();
            long shares = columnMapping.ContainsKey("shares") ? row.Data[columnMapping["shares"]] : 0;
            string sector = columnMapping.ContainsKey("sector") ? row.Data[columnMapping["sector"]] : string.Empty;

            models[idx] = new CompanyFinancialModel(name, ticker, cik, DateTime.Parse(date), shares, sector, id);
            idx++;
        }
        return models;
    }
}