using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.Services;
using System.Net;

namespace StonksWebApp.Pages.Portfolio
{
    public class PortfolioModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnGetNewPortfolio()
        {
            return Partial("NewPortfolio");
        }

        public IActionResult OnPostNewPortfolio(string PortfolioName, int StartingCapital)
        {
            var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(
                Request.Cookies["sessionToken"] ?? "",
                Request.HttpContext.Connection.RemoteIpAddress ?? IPAddress.None);
            if (!valid)
            {
                return RedirectToPage("/Login");
            }

            var db = DatabaseConnectionService.Instance;
            var id = db.CreatePortfolio(PortfolioName, StartingCapital, session.Id);

            return RedirectToPage($"/Portfolio/{id}");
        }
        public IActionResult OnPostStonksList(string searchParam, int id)
        {
            var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(
                Request.Cookies["sessionToken"] ?? "",
                Request.HttpContext.Connection.RemoteIpAddress ?? IPAddress.None);
            if (!Request.IsHtmx())
            {
                return BadRequest();
            }

            if (!valid || !DatabaseConnectionService.Instance.PortfolioBelongsTo(id, session?.Name ?? ""))
            {
                return Partial("_redirectToLogin", "/Login");
            }

            var companies = DatabaseConnectionService.Instance.GetCompanies(searchParam);
            var model = new _stonkSearchResultPartialModel
            {
                Companies = companies,
                PortfolioId = id
            };
            return Partial("_stonkSearchResultPartial", model);
        }
    }
}
