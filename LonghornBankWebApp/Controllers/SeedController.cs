using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LonghornBankWebApp.Utilities;
using LonghornBankWebApp.Models;
using LonghornBankWebApp.DAL;

namespace LonghornBankWebApp.Controllers
{
    public class SeedController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedController(AppDbContext db, UserManager<AppUser> um, RoleManager<IdentityRole> rm)
        {
            _context = db;
            _userManager = um;
            _roleManager = rm;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SeedAllAsync()
        {
            _ = await SeedRoles();
            _ = await SeedPeople();
            _ = SeedStockTypes();
            _ = SeedStocks();
            _ = SeedBankAccounts();
            _ = SeedTransactions();
            _ = SeedStockTransactions();
            _ = SeedDisputes();

            return View("Confirm");

        }

       
        public async Task<IActionResult> SeedRoles()
        {
            try
            {
                //call the method to seed the roles
                await Seeding.SeedRoles.AddAllRoles(_roleManager);
            }
            catch (Exception ex)
            {
                //add the error messages to a list of strings
                List<string> errorList = new()
                {
                    //Add the outer message
                    ex.Message,

                    //Add the message from the inner exception
                    ex.InnerException.Message
                };

                //Add additional inner exception messages, if there are any
                if (ex.InnerException.InnerException != null)
                {
                    errorList.Add(ex.InnerException.InnerException.Message);
                }

                return View("Error", errorList);
            }

            //this is the happy path - seeding worked!
            return View("Confirm");
        }
        public async Task<IActionResult> SeedPeople()
        {
            try
            {
                //call the method to seed the users
                await Seeding.SeedUsers.SeedAllUsers(_userManager, _context);
            }
            catch (Exception ex)
            {
                //add the error messages to a list of strings
                List<string> errorList = new()
                {
                    //Add the outer message
                    ex.Message,

                    //Add the message from the inner exception
                    ex.InnerException.Message
                };

                //Add additional inner exception messages, if there are any
                if (ex.InnerException.InnerException != null)
                {
                    errorList.Add(ex.InnerException.InnerException.Message);
                }

                return View("Error", errorList);
            }

            //this is the happy path - seeding worked!
            return View("Confirm");
        }

     
        public IActionResult SeedTransactions()
        {

            try
            {
                // ASK: does this need to be await
                Seeding.SeedTransactions.SeedAllTransactions(_userManager, _context);
            }
            catch (Exception ex)
            {
                //create a new list for the error messages
                List<string> errors = new()
                {
                    //add a generic error message
                    "There was a problem adding transaction to DB",

                    //add message from the exception
                    ex.Message
                };

                //add messages from inner exceptions, if there are any
                if (ex.InnerException != null)
                {
                    errors.Add(ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        errors.Add(ex.InnerException.InnerException.Message);
                        if (ex.InnerException.InnerException.InnerException != null)
                        {
                            errors.Add(ex.InnerException.InnerException.InnerException.Message);
                        }
                    }
                }


                return View("Error", errors);
            }
            return View("Confirm");
        }


        public IActionResult SeedStockTransactions()
        {

            try
            {
                // ASK: does this need to be await
                Seeding.SeedTransactions.SeedAllStockTrans(_userManager, _context);
            }
            catch (Exception ex)
            {
                //create a new list for the error messages
                List<string> errors = new()
                {
                    //add a generic error message
                    "There was a problem adding transaction to DB",

                    //add message from the exception
                    ex.Message
                };

                //add messages from inner exceptions, if there are any
                if (ex.InnerException != null)
                {
                    errors.Add(ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        errors.Add(ex.InnerException.InnerException.Message);
                        if (ex.InnerException.InnerException.InnerException != null)
                        {
                            errors.Add(ex.InnerException.InnerException.InnerException.Message);
                        }
                    }
                }


                return View("Error", errors);
            }
            return View("Confirm");
        }
        //ASK: does the return type need to be Task<IActionResult>
        public IActionResult SeedStockTypes()
        {

            try
            {
                Seeding.SeedStocks.SeedAllStockTypes(_context);
            }
            catch (Exception ex)
            {
                //create a new list for the error messages
                List<string> errors = new()
                {
                    //add a generic error message
                    "There was a problem adding transaction to DB",

                    //add message from the exception
                    ex.Message
                };

                //add messages from inner exceptions, if there are any
                if (ex.InnerException != null)
                {
                    errors.Add(ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        errors.Add(ex.InnerException.InnerException.Message);
                        if (ex.InnerException.InnerException.InnerException != null)
                        {
                            errors.Add(ex.InnerException.InnerException.InnerException.Message);
                        }
                    }
                }


                return View("Error", errors);
            }
            return View("Confirm");
        }
        public IActionResult SeedStocks()
        {

            try
            {
                Seeding.SeedStocks.SeedAllStocks(_context);
            }
            catch (Exception ex)
            {
                //create a new list for the error messages
                List<string> errors = new()
                {
                    //add a generic error message
                    "There was a problem adding transaction to DB",

                    //add message from the exception
                    ex.Message
                };

                //add messages from inner exceptions, if there are any
                if (ex.InnerException != null)
                {
                    errors.Add(ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        errors.Add(ex.InnerException.InnerException.Message);
                        if (ex.InnerException.InnerException.InnerException != null)
                        {
                            errors.Add(ex.InnerException.InnerException.InnerException.Message);
                        }
                    }
                }


                return View("Error", errors);
            }
            return View("Confirm");
        }

        public IActionResult SeedDisputes()
        {

            try
            {
                Seeding.SeedDisputes.SeedAllDisputes(_userManager, _context);
            }
            catch (Exception ex)
            {
                //create a new list for the error messages
                List<string> errors = new()
                {
                    //add a generic error message
                    "There was a problem adding transaction to DB",

                    //add message from the exception
                    ex.Message
                };

                //add messages from inner exceptions, if there are any
                if (ex.InnerException != null)
                {
                    errors.Add(ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        errors.Add(ex.InnerException.InnerException.Message);
                        if (ex.InnerException.InnerException.InnerException != null)
                        {
                            errors.Add(ex.InnerException.InnerException.InnerException.Message);
                        }
                    }
                }


                return View("Error", errors);
            }
            return View("Confirm");
        }

        public IActionResult SeedBankAccounts()
        {
            try
            { 
                // ASK: does this need to be await
                Seeding.SeedBankAccounts.SeedAllBankAccounts(_userManager, _context);
            }
            catch (Exception ex)
            {
                //create a new list for the error messages
                List<string> errors = new()
                {
                    //add a generic error message
                    "There was a problem adding bank account to DB",

                    //add message from the exception
                    ex.Message
                };

                //add messages from inner exceptions, if there are any
                if (ex.InnerException != null)
                {
                    errors.Add(ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        errors.Add(ex.InnerException.InnerException.Message);
                        if (ex.InnerException.InnerException.InnerException != null)
                        {
                            errors.Add(ex.InnerException.InnerException.InnerException.Message);
                        }
                    }
                }


                return View("Error", errors);
            }
            return View("Confirm");
        }
    }
}
