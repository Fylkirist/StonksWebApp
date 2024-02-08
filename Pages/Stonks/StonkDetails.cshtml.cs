using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.Services;

namespace StonksWebApp.Pages.Stonks
{
    public class StonkDetailsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Ticker { get; set; }
        public void OnGet()
        {
        }

        public IActionResult OnPostStockGraph(string interval, string range)
        {
            var now = DateTime.Now;
            var then = range switch
            {
                "week" => now.AddDays(-7),
                "year" => now.AddMonths(-12),
                "month" => now.AddDays(-30),
                _ => throw new NotImplementedException(),
            };
            var prices = FetchingService.GetHistoricalPrices(Ticker, then, now);
            return Partial("_priceGraphPartial");
        }
    }
}
