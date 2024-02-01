using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StonksWebApp.Pages.Stonks
{
    public class StonkDetailsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Ticker { get; set; }
        public void OnGet()
        {
        }
    }
}
