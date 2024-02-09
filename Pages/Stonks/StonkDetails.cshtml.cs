using System.Globalization;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.models;
using StonksWebApp.Services;

namespace StonksWebApp.Pages.Stonks;

public class StonkDetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Ticker { get; set; }
    public void OnGet()
    {
    }

    public IActionResult OnPostStockGraph(string interval, string range)
    {
        if (!Request.IsHtmx())
        {
            return Redirect("/Stonks");
        }
        var now = DateTime.Now;
        var then = range switch
        {
            "week" => now.AddDays(-7),
            "year" => now.AddMonths(-12),
            "month" => now.AddDays(-30),
            _ => throw new NotImplementedException(),
        };
        var prices = FetchingService.GetHistoricalPrices(Ticker, then, now);

        string[] intervals = range switch
        {
            "week" => new []{"hour", "day"},
            "month" => new []{"hour", "day", "week"},
            "year" => new []{"hour", "day", "week", "month"},
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null)
        };

        var filtered = CreateConcatenatedCandles(interval, prices);

        _priceGraphPartialModel model = new _priceGraphPartialModel(filtered, intervals,
            new[] { "week", "month", "year" }, range, interval, Ticker);
        return Partial("_priceGraphPartial", model);
    }

    private PriceCandleModel[] CreateConcatenatedCandles(string interval, PriceCandleModel[] prices)
    {
        List<PriceCandleModel> newPrices = new();

        if (interval == "day")
        {
            var groupedCandles = prices.GroupBy(candle => candle.Date.Date);

            newPrices.AddRange(from @group in groupedCandles
            let totalVolume = @group.Sum(candle => candle.Volume)
            select new PriceCandleModel
            {
                Date = @group.Key,
                Open = @group.First().Open,
                Close = @group.Last().Close,
                High = @group.Max(candle => candle.High),
                Low = @group.Min(candle => candle.Low),
                Volume = totalVolume
            });
        }
        else if (interval == "week")
        {
            var groupedCandles = prices.GroupBy(candle => GetWeekOfYear(candle.Date));

            newPrices.AddRange(from @group in groupedCandles
                let totalVolume = @group.Sum(candle => candle.Volume)
                select new PriceCandleModel
                {
                    Date = @group.Min(candle => candle.Date),
                    Open = @group.First().Open,
                    Close = @group.Last().Close,
                    High = @group.Max(candle => candle.High),
                    Low = @group.Min(candle => candle.Low),
                    Volume = totalVolume
                });
        }
        else if (interval == "month")
        {
            var groupedCandles = prices.GroupBy(candle => candle.Date.Month);

            newPrices.AddRange(from @group in groupedCandles
                let totalVolume = @group.Sum(candle => candle.Volume)
                select new PriceCandleModel
                {
                    Date = @group.Min(candle => candle.Date),
                    Open = @group.First().Open,
                    Close = @group.Last().Close,
                    High = @group.Max(candle => candle.High),
                    Low = @group.Min(candle => candle.Low),
                    Volume = totalVolume
                });
        }
        else
        {
            return prices;
        }

        return newPrices.ToArray();
    }

    private int GetWeekOfYear(DateTime date)
    {
        return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}