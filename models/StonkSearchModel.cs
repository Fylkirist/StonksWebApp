namespace StonksWebApp.models;

public class StonkSearchModel
{
    public string Ticker { get; set; }
    public string Name { get; set; }
    public long MarketCap { get; set; }
    public double CurrentPrice { get; set; }
    public double PriceChange { get; set; }

    public StonkSearchModel(string ticker, string name, long marketCap, double currentPrice, double priceChange)
    {
        Ticker = ticker;
        Name = name;
        MarketCap = marketCap;
        CurrentPrice = currentPrice;
        PriceChange = priceChange;
    }
}