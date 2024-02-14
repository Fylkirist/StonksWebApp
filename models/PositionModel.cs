namespace StonksWebApp.models;

public class PositionModel
{
    public string Ticker { get; set; }
    public bool IsLong { get; set; }
    public int Size { get; set; }

    public PositionModel(string ticker, bool isLong, int size)
    {
        Ticker = ticker;
        IsLong = isLong;
        Size = size;
    }
}

