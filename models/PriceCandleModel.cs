using Microsoft.VisualBasic;
using StonksWebApp.connections;

namespace StonksWebApp.models;

public class PriceCandleModel
{
    public DateTime Date;
    public double High;
    public double Low;
    public double Open;
    public double Close;
    public long Volume;

    public PriceCandleModel(DateTime date, double high, double low, double open, double close, long volume)
    {
        Date = date;
        High = high;
        Low = low;
        Open = open;
        Close = close;
        Volume = volume;
    }

    public PriceCandleModel()
    {

    }
    public static PriceCandleModel[] ConvertQueryResult(QueryResult result)
    {
        int num = result.Rows.Length;
        var models = new PriceCandleModel[num];
        Dictionary<string, int> columnMapping = new Dictionary<string, int>();
        for (int i = 0; i < result.Columns.Length; i++)
        {
            columnMapping[result.Columns[i]] = i;
        }

        int idx = 0;
        foreach (var row in result.Rows)
        {
            DateTime date = columnMapping.ContainsKey("pdate") ? row.Data[columnMapping["pdate"]] : "";
            double open = columnMapping.ContainsKey("open") ? row.Data[columnMapping["open"]] : 0;
            double close = columnMapping.ContainsKey("close") ? row.Data[columnMapping["close"]] : 0;
            long volume = columnMapping.ContainsKey("volume") ? row.Data[columnMapping["volume"]] : 0;
            double high = columnMapping.ContainsKey("high") ? row.Data[columnMapping["high"]] : 0;
            double low = columnMapping.ContainsKey("low") ? row.Data[columnMapping["low"]] : 0;
            models[idx] = new PriceCandleModel(date, high, low, open, close, volume);
            idx++;
        }
        return models;
    }
}