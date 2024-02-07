using System.Net;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.Services;

namespace StonksWebApp.Pages.Login;

public class LoginModel : PageModel
{
    [TempData]
    public string ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost(string usernameInput, string passwordInput)
    {
        var ip = Request.HttpContext.Connection.RemoteIpAddress;
        var (token, valid) = LoginManagerService.Instance.HandleLogin(usernameInput, passwordInput, ip);
        if (valid)
        {
            Response.Cookies.Append("sessionToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                }
            );
            return Redirect("/Index");
        }
        ErrorMessage = "Invalid username or password.";
        return Page();
    }
}