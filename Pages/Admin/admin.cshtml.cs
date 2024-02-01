using Htmx;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.models;
using StonksWebApp.Services;

namespace StonksWebApp.Pages;

public class AdminModel : PageModel
{
    private readonly IAntiforgery _antiforgery;

    public AdminModel(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public IActionResult OnGet()
    {
        var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(Request.Cookies["sessionToken"] ?? "");
        if (!valid || session?.Role != "admin")
        {
            return Redirect("/Login");
        }

        return Page();
    }
    public IActionResult OnGetCompanies(string searchInput)
    {
        var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(Request.Cookies["sessionToken"] ?? "");
        if (!valid || session?.Role != "admin")
        {
            return Redirect("/Login");
        }
        if (!Request.IsHtmx())
        {
            return Page();
        }

        var db = DatabaseConnectionService.Instance;
        CompanyFinancialModel[] results = db.GetCompanies(searchInput);
        return Partial("StocksAdminView", results);
    }

    public void OnPostUpdateSingleValue(string ticker)
    {
        FetchingService.GetAndUpdateSingleValue(ticker.ToUpper());
    }

    public void OnPostUpdateFromCsv(IFormFile csvFile)
    {
        if (csvFile != null && csvFile.Length > 0)
        {
            using var stream = new StreamReader(csvFile.OpenReadStream());
            var csvContent = stream.ReadToEnd();
            _ = FetchingService.UpdateDatabaseFromCsv(csvContent);
        }
    }

    public IActionResult OnPostUpdateTickerPrice(string tickerSymbol)
    {
        var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(Request.Cookies["sessionToken"] ?? "");
        if (!valid || session?.Role != "admin")
        {
            return Redirect("/Login");
        }
        if (Request.IsHtmx())
        {
            var price = FetchingService.GetCurrentPrice(tickerSymbol);
            return Partial("_selfUpdatingTickerPartial", new TickerPriceModel(price.CurrentPrice, tickerSymbol, price.PercentChange));
        }
        return Page();
    }

    public IActionResult OnGetGetFilings(string searchInput, string formType, DateTime? fromDate, DateTime? toDate)
    {
        var (valid, session) = LoginManagerService.Instance.CheckUserSessionToken(Request.Cookies["sessionToken"] ?? "");
        if (!valid || session?.Role != "admin")
        {
            return Redirect("/Login");
        }

        if (!Request.IsHtmx())
        {
            return Page();
        }

        FilingModel[] result;
        if (fromDate.HasValue && toDate.HasValue)
        { 
            result = DatabaseConnectionService.Instance.GetFilings(searchInput.ToUpper(), fromDate.Value, toDate.Value, formType);
        }
        else
        {
            result = DatabaseConnectionService.Instance.GetFilings(searchInput.ToUpper(), formType);
        }
        return Partial("_filingTablePartial", result);

    }
}