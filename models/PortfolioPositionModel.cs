using StonksWebApp.connections;

namespace StonksWebApp.models;

public class PortfolioPositionModel
{
    public int Id { get; set; }
    public int PortfolioId { get;}
    public string Ticker { get; set; }
    public string CompanyName { get; set; }
    public double EntryPrice { get; set; }
    public int PositionSize { get; set; }
    public bool IsLong { get; set; }

    public PortfolioPositionModel(int id, int portfolioId, string ticker, string companyName, double entryPrice, int positionSize, bool isLong)
    {
        Id = id;
        PortfolioId = portfolioId;
        Ticker = ticker;
        CompanyName = companyName;
        EntryPrice = entryPrice;
        PositionSize = positionSize;
        IsLong = isLong;
    }

    public PortfolioPositionModel()
    {

    }

    public static PortfolioPositionModel[] ConvertQueryResult(QueryResult result)
    {
        Dictionary<string, int> columnMapping = new Dictionary<string, int>();
        for (int i = 0; i < result.Columns.Length; i++)
        {
            columnMapping[result.Columns[i]] = i;
        }
        var models = new PortfolioPositionModel[result.Rows.Length];
        int idx = 0;
        foreach (var row in result.Rows)
        {
            var rowData = row.Data;
            int id = rowData[columnMapping["id"]];
            int portfolioId = rowData[columnMapping["portfolioid"]];
            string ticker = rowData[columnMapping["ticker"]];
            string cname = rowData[columnMapping["cname"]];
            double entryPrice = rowData[columnMapping["averageprice"]];
            int positionSize = rowData[columnMapping["positionsize"]];
            int isLong = rowData[columnMapping["islong"]];
            models[idx] = new PortfolioPositionModel(id,portfolioId,ticker,cname,entryPrice,positionSize,Convert.ToBoolean(isLong));
            idx++;
        }

        return models;
    }
}