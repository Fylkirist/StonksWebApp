using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.models;
using StonksWebApp.Services;

namespace StonksWebApp.Pages.Stonks
{
    public class StonksModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPostQueryStonks(string ticker)
        {
            var ip = Request.HttpContext.Connection.RemoteIpAddress;
            var (valid, session) =
                LoginManagerService.Instance.CheckUserSessionToken(Request.Cookies["sessionToken"] ?? "", ip ?? IPAddress.None);
            if (!valid)
            {
                return Redirect("/Index");
            }

            var companyModels = DatabaseConnectionService.Instance.GetCompanies(ticker);
            var searchModels = new List<StonkSearchModel>();
            foreach (var company in companyModels)
            {
                var price = FetchingService.GetCurrentPrice(company.Ticker);
                long cap = (long)(price.CurrentPrice * company.Shares);
                searchModels.Add(new StonkSearchModel(company.Ticker, company.Name, cap, price.CurrentPrice, price.PercentChange));
            }

            return Partial("_stonkListPartial", searchModels.ToArray());
        }
    }
}
