using Microsoft.AspNetCore.Routing.Tree;
using StonksWebApp.connections;

namespace StonksWebApp.models;

public class FilingModel
{
    public string Url { get; set; }
    public string Type { get; set; }
    public DateTime Date { get; set; }
    public string Institution { get; set; }
    public CompanyFinancialModel? Organization { get; set; }

    public FilingModel(string url, string type, DateTime date, string institution, CompanyFinancialModel organization)
    {
        Url = url;
        Type = type;
        Date = date;
        Institution = institution;
        Organization = organization;
    }

    public static FilingModel[] TranslateQueryResults(QueryResult result)
    {
        var models = new List<FilingModel>();
        Dictionary<string, int> columnMapping = new Dictionary<string, int>();
        for (int i = 0; i < result.Columns.Length; i++)
        {
            columnMapping[result.Columns[i]] = i;
        }

        foreach (var row in result.Rows)
        {
            string cname = columnMapping.ContainsKey("cname") ? row.Data[columnMapping["cname"]] : "";
            string ticker = columnMapping.ContainsKey("ticker") ? row.Data[columnMapping["ticker"]] : "";
            DateTime fiscalYear = columnMapping.ContainsKey("fiscalyearend")
                ? row.Data[columnMapping["fiscalyearend"]]
                : DateTime.MinValue;
            int shares = columnMapping.ContainsKey("shares") ? row.Data[columnMapping["shares"]] : int.MinValue;
            int cik = columnMapping.ContainsKey("cik") ? row.Data[columnMapping["cik"]] : 0;
            string sector = columnMapping.ContainsKey("sector") ? row.Data[columnMapping["sector"]] : String.Empty;

            CompanyFinancialModel cModel = new CompanyFinancialModel(cname,ticker,cik,fiscalYear,shares, sector);
            string url = columnMapping.ContainsKey("link") ? row.Data[columnMapping["link"]] : "";
            string type = columnMapping.ContainsKey("filingtype") ? row.Data[columnMapping["filingtype"]] : "";
            DateTime date = columnMapping.ContainsKey("filingdate") ? row.Data[columnMapping["filingdate"]] : DateTime.MinValue;
            string institution = columnMapping.ContainsKey("org") ? row.Data[columnMapping["org"]] : string.Empty;
            FilingModel model = new FilingModel(url, type, date, institution, cModel);
            models.Add(model);
        }

        return models.ToArray();
    }
}