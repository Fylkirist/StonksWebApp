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
            var id = db.CreatePortfolio(PortfolioName, (double)StartingCapital, session.Id);

            return Redirect($"/Portfolio/{id}");
        }
    }
}
