using System.Net;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.Services;

namespace StonksWebApp.Pages.Portfolio
{
    public class PortfolioDetailsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }
        public void OnGet()
        {
        }

        public IActionResult OnGetStonksList(string searchParam)
        {
            var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(
                Request.Cookies["sessionToken"] ?? "",
                Request.HttpContext.Connection.RemoteIpAddress ?? IPAddress.None);
            if (!Request.IsHtmx())
            {
                return RedirectToPage("/");
            }

            if (valid || !DatabaseConnectionService.Instance.PortfolioBelongsTo(Id, session?.Name??""))
            {
                return Partial("_redirectToLogin", "/Login");
            }

            var companies = DatabaseConnectionService.Instance.GetCompanies(searchParam);
            var model = new _stonkSearchResultPartialModel
            {
                Companies = companies,
                PortfolioId = Id
            };
            return Partial("_stonkSearchResultPartial", model);
        }
    }
}
