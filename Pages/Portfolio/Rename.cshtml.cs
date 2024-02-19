using System.Net;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.Services;

namespace StonksWebApp.Pages.Portfolio;

public class RenameModel : PageModel
{
    public models.PortfolioModel Portfolio { get; set; }
    public void OnGet()
    {
    }

    public IActionResult OnPost(int portfolioId, string newPortfolioName)
    {
        var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(Request.Cookies["sessionToken"] ?? "",
            Request.HttpContext.Connection.RemoteIpAddress ?? IPAddress.None);
        var db = DatabaseConnectionService.Instance;
        var isOwner = db.PortfolioBelongsTo(portfolioId, session?.Name ?? "");
        if (!Request.IsHtmx() || !valid || !isOwner)
        {
            return Partial("_redirectToLogin", "/Login");
        }
        db.UpdatePortfolioName(portfolioId, newPortfolioName);

        return Partial("_portfolioListPartial", session);
    }
}