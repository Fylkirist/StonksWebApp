using StonksWebApp.connections;

namespace StonksWebApp.models;

public class PortfolioModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; }
    public double Capital { get; set; }
    public double StartCapital { get; set; }
    public TradeModel[] Trades { get; set; }

    public PortfolioModel()
    {

    }
    public PortfolioModel(int id, string name, DateTime created, double capital, double startCapital)
    {
        Id = id;
        Name = name;
        Created = created;
        Capital = capital;
        StartCapital = startCapital;
        Trades = Array.Empty<TradeModel>();
    }

    public static PortfolioModel[] GetPortfolioModel(QueryResult result)
    {
        Dictionary<string, int> columnMapping = new Dictionary<string, int>();
        for (int i = 0; i < result.Columns.Length; i++)
        {
            columnMapping[result.Columns[i]] = i;
        }
        var models = new PortfolioModel[result.Rows.Length];
        int idx = 0;
        foreach (var row in result.Rows)
        {
            var rowData = row.Data;
            int id = rowData[columnMapping["id"]];
            string name = rowData[columnMapping["portfolioname"]];
            DateTime created = rowData[columnMapping["dateadded"]];
            double capital = rowData[columnMapping["capital"]];
            double startCapital = rowData[columnMapping["startingcapital"]];
            models[idx] = new PortfolioModel(id, name, created, capital, startCapital);
            idx++;
        }

        return models;
    }
}

public class TradeModel
{
    public string Ticker { get; set; }
    public int OrderType { get; set; }
    public DateTime OrderDate { get; set; }
    public double OrderPrice { get; set; }
    public int OrderSize { get; set; }

    public TradeModel()
    {

    }
    public TradeModel(string ticker, int orderType, DateTime orderDate, double orderPrice, int orderSize)
    {
        Ticker = ticker;
        OrderType = orderType;
        OrderDate = orderDate;
        OrderPrice = orderPrice;
        OrderSize = orderSize;
    }
    public static TradeModel[] GetTradeModels(QueryResult result)
    { 
        Dictionary<string, int> columnMapping = new Dictionary<string, int>();
        for (int i = 0; i < result.Columns.Length; i++)
        {
            columnMapping[result.Columns[i]] = i;
        }
        var models = new TradeModel[result.Rows.Length];
        int idx = 0;
        foreach (var row in result.Rows)
        {
            var rowData = row.Data;
            int type = rowData[columnMapping["ordertype"]];
            DateTime date = rowData[columnMapping["orderdate"]];
            double price = rowData[columnMapping["orderprice"]];
            int size = rowData[columnMapping["ordersize"]];
            string ticker = rowData[columnMapping["ticker"]];
            models[idx] = new TradeModel(ticker, type, date, price, size);
            idx++;
        }

        return models;
    }

}