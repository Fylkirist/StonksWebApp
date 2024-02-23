using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.models;

namespace StonksWebApp.Pages.Portfolio
{
    public class _stonkSearchResultPartialModel : PageModel
    {
        public CompanyFinancialModel[] Companies { get; set; }
        public int PortfolioId { get; set; }
        public void OnGet()
        {
        }
    }
}
