using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StonksWebApp.models;
using StonksWebApp.Services;
using System.ComponentModel.DataAnnotations;

namespace StonksWebApp.Pages.Register
{
    public class RegisterModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPostRegister(string email, string password, string confirmPassword)
        {
            bool passValid = true;
            bool emailValid = true;
            if (password.Length < 8 || password.Length > 20 || password != confirmPassword)
            {
                passValid = false;
            }

            UserModel? checkedUser = DatabaseConnectionService.Instance.GetUser(email);
            var checker = new EmailAddressAttribute();
            if (checkedUser != null || !checker.IsValid(email))
            {
                emailValid = false;
            }

            if (emailValid && passValid)
            {
                DatabaseConnectionService.Instance.CreateNewUser(email, password, false);
            }
            return Partial("_registrationForm", (passValid, emailValid));
        }
    }
}
