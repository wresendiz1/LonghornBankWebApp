using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using LonghornBankWebApp.Models;
using LonghornBankWebApp.DAL;


namespace LonghornBankWebApp.Controllers
{
    public class HomeController : Controller
    {

        // TODO: Add to the Home/Index view to display ads if not registered & button to make new account
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
            // display list of a users accounts
            
        }

        public IActionResult NewUser()
        {
            return View();
        }

        public IActionResult Pending()
        {
            // TODO: send admins here upon login

            return View();
        }
    }
}
