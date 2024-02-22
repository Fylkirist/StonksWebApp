namespace StonksWebApp.models;

public class NetPositionModel
{
    public string Ticker { get; set; }
    public bool IsLong { get; set; }
    public int Size { get; set; }
    public double AverageValue { get; set; }

    public NetPositionModel(string ticker, bool isLong, int size, double averageValue)
    {
        Ticker = ticker;
        IsLong = isLong;
        Size = size;
        AverageValue = averageValue;
    }
}

