namespace StonksWebApp.models;

public class TickerPriceModel
{
    public double Price { get; set; }
    public double Change { get; set; }
    public string TickerSymbol { get; set; }
    public TickerPriceModel(double price, string tickerSymbol, double change)
    {
        Price = price;
        TickerSymbol = tickerSymbol;
        Change = change;
    }
}