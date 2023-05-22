using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;



namespace LonghornBankWebApp.Controllers
{
    public class HomeController : Controller
    {


        // TODO: Add to the Home/Index view to display ads if not registered & button to make new account
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity.Name != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("AnonIndex", "Home");
            }

        }


        public IActionResult AnonIndex()
        {
            if (User.Identity.Name != null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
            
        }

        [Authorize(Roles = "Customer")]
        public IActionResult NewUser()
        {
            return View();
        }

        public IActionResult HowTo()
        {
            return View();
        }

    }
}
