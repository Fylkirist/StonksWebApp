using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenQA.Selenium.Interactions;
using StonksWebApp.models;
using StonksWebApp.Services;

namespace StonksWebApp.Pages.Portfolio
{
    public class _stonkSearchResultPartialModel : PageModel
    {
        public CompanyFinancialModel[] Companies { get; set; }
        public int PortfolioId { get; set; }
        public void OnGet()
        {
        }

        public IActionResult OnPost(int companyId, int id, int numShares, string orderType, string ticker)
        {
            var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(
                Request.Cookies["sessionToken"] ?? "",
                Request.HttpContext.Connection.RemoteIpAddress ?? IPAddress.None);
            if (!valid)
            {

            }

            var db = DatabaseConnectionService.Instance;
            bool userIsOwner = db.PortfolioBelongsTo(id, session.Name);
            if (!userIsOwner)
            {

            }

            db.PushTradeOrder(companyId, numShares, orderType == "buy", FetchingService.GetCurrentPrice(ticker).CurrentPrice, id);
            return Redirect($"/Portfolio/{id}");
        }
    }
}
