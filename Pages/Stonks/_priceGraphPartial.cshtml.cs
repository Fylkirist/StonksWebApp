using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.models;

namespace StonksWebApp.Pages.Stonks;

public class _priceGraphPartialModel : PageModel
{
    public PriceCandleModel[] Prices;
    public string[] Intervals;
    public string[] Ranges;
    public string CurrentRange;
    public string CurrentInterval;
    public string Ticker;

    public _priceGraphPartialModel(PriceCandleModel[] prices, string[] intervals, string[] ranges, string currentRange, string currentInterval, string ticker)
    {
        Prices = prices;
        Intervals = intervals;
        Ranges = ranges;
        CurrentRange = currentRange;
        CurrentInterval = currentInterval;
        Ticker = ticker;
    }

    public void OnGet()
    {
    }
}