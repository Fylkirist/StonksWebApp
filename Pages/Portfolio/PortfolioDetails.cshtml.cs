using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StonksWebApp.Pages.Portfolio
{
    public class PortfolioDetailsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int PortfolioId { get; set; }
        public void OnGet()
        {
        }
    }
}
