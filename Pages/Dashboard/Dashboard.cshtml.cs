using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.Services;

namespace StonksWebApp.Pages.Dashboard
{
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnGetPortfolioList()
        {
            var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(Request.Cookies["sessionToken"],
                Request.HttpContext.Connection.RemoteIpAddress);
            if (!Request.IsHtmx() || !valid)
            {
                return Redirect("/Index");
            }

            return Partial("_dashboardPortfolioList", session);
        }

        public IActionResult OnGetWatchList()
        {
            var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(Request.Cookies["sessionToken"],
                Request.HttpContext.Connection.RemoteIpAddress);
            if (!Request.IsHtmx() || !valid)
            {
                return Redirect("/Index");
            }

            return Partial("_dashboardWatchlist", session);
        }
    }
}
