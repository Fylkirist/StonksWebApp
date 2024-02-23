using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StonksWebApp.Pages.Portfolio
{
    public class _openPositionsListPartialModel : PageModel
    {
        public int PortfolioId { get; set; }
        public void OnGet()
        {
        }
    }
}
