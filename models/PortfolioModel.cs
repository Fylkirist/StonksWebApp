namespace StonksWebApp.models;

public class PortfolioModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; }
    public double Capital { get; set; }
    public double StartCapital { get; set; }
    public TradeModel[] Trades { get; set; }
}

public class TradeModel
{
    public string Ticker { get; set; }
    public string OrderType { get; set; }
    public DateTime OrderDate { get; set; }
    public double OrderPrice { get; set; }
    public int OrderSize { get; set; }
}