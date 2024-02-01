namespace StonksWebApp.models;

public class StonkSearchModel
{
    public string Ticker { get; set; }
    public string Name { get; set; }
    public int MarketCap { get; set; }
    public double CurrentPrice { get; set; }

    public StonkSearchModel(string ticker, string name, int marketCap, double currentPrice)
    {
        Ticker = ticker;
        Name = name;
        MarketCap = marketCap;
        CurrentPrice = currentPrice;
    }
}