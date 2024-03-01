using System.Net;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.Services;

namespace StonksWebApp.Pages.Portfolio;

public class PortfolioDetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

}