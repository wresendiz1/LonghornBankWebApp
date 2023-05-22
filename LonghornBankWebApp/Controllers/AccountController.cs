using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;
using LonghornBankWebApp.Utilities;

namespace LonghornBankWebApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly PasswordValidator<AppUser> _passwordValidator;
        private readonly AppDbContext _context;

        public AccountController(AppDbContext appDbContext, UserManager<AppUser> userManager, SignInManager<AppUser> signIn)
        {
            _context = appDbContext;
            _userManager = userManager;
            _signInManager = signIn;
            //user manager only has one password validator
            _passwordValidator = (PasswordValidator<AppUser>)userManager.PasswordValidators.FirstOrDefault();
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }


        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel rvm)
        {
            //if registration data is valid, create a new user on the database
            if (ModelState.IsValid == false)
            {
                //this is the sad path - something went wrong, 
                //return the user to the register page to try again
                return View(rvm);
            }

            AppUser u = await _userManager.FindByEmailAsync(rvm.Email);

            if(u != null)
            {
                ModelState.AddModelError(nameof(rvm.Email), "Email already taken");
                return View(rvm);
            }

            //this code maps the RegisterViewModel to the AppUser domain model
            AppUser newUser = new()
            {
                UserName = rvm.Email,
                Email = rvm.Email,
                PhoneNumber = rvm.PhoneNumber,

                //Custom user fields here
                FirstName = rvm.FirstName,
                MidIntName = rvm.MidIntName,
                LastName = rvm.LastName,
                Street = rvm.Street,
                State = rvm.State,
                City = rvm.City,
                ZipCode = rvm.ZipCode,
                DOB = rvm.DOB,
                // NOTE: All customers accounts start out as active
                IsActive = true,
                // Customers start with no accounts
                UserHasAccount = false
                
            };

            //create AddUserModel
            AddUserModel aum = new()
            {
                User = newUser,
                Password = rvm.Password,
                // Only Customer role can be registered 
                RoleName = "Customer"
            };

            //This code uses the AddUser utility to create a new user with the specified password
            IdentityResult result = await AddUser.AddUserWithRoleAsync(aum, _userManager, _context);

            if (result.Succeeded) //everything is okay
            { 
                //NOTE: This code logs the user into the account that they just created
                //You may or may not want to log a user in directly after they register - check
                //the business rules!
                Microsoft.AspNetCore.Identity.SignInResult result2 = await _signInManager.PasswordSignInAsync(rvm.Email, rvm.Password, false, lockoutOnFailure: false);

                if (result2.Succeeded)
                {
                    // Send email
                    EmailMessaging.SendEmail(rvm.Email, "Welcome to Longhorn Bank", "Thank you for registering with Longhorn Bank. We look forward to serving you!");

                    // Create notifaction
                    Message m = new()
                    {
                        Info = "Thank you for registering with Longhorn Bank. We look forward to serving you!",
                        Subject = "Welcome to Longhorn Bank",
                        Date = DateTime.Today,
                        Sender = "Longhorn Bank",
                        Receiver = rvm.Email,
                        Admins = new List<AppUser>()
                    };
                    m.Admins.Add(aum.User);
                    _context.Add(m);
                    _context.SaveChanges();

                    //Send the user to apply for a banking Account
                    return RedirectToAction("NewUser", "Home");
                }
                else //the login didn't work, send them back to the login page
                {
                    return RedirectToAction("Login", "Account");
                }


            }
            else  //the add user operation didn't work, and we need to show an error message
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                //send user back to page with errors
                return View(rvm);
            }
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Employee") == false) //user has been redirected here from a page they're not authorized to see
            {
                return View("Error", new string[] { "Access Denied" });
            }
            else if (User.Identity.IsAuthenticated && User.IsInRole("Employee"))
            {
                //sign the user out of the application
                _signInManager.SignOutAsync();

                //send the user back to the home page
                return RedirectToAction("Index", "Home");
            }

            _signInManager.SignOutAsync(); //this removes any old cookies hanging around
            ViewBag.ReturnUrl = returnUrl; //pass along the page the user should go back to
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel lvm, string returnUrl)
        {
            //if user forgot to include user name or password,
            //send them back to the login page to try again
            if (ModelState.IsValid == false)
            {
                return View(lvm);
            }

            //attempt to sign the user in using the SignInManager
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(lvm.Email, lvm.Password, lvm.RememberMe, lockoutOnFailure: false);

            //if the login worked, take the user to either the url
            //they requested OR the homepage if there isn't a specific url
            // if they do redirect to the home page with their accounts
            if (result.Succeeded)
            {
                //return ?? "/" means if returnUrl is null, substitute "/" (home)

                // NOTE: old way of checking if user has an account, use "UserHasAccount" property instead
                //BankAccount ba = _context.BankAccounts.Where(b => b.User.UserName == lvm.Email).FirstOrDefault();
                //StockPortfolio sp = _context.StockPortfolios.Where(s => s.User.UserName == lvm.Email).FirstOrDefault();
                
                AppUser user = _userManager.FindByNameAsync(lvm.Email).Result;
                bool UserCustomer = _userManager.IsInRoleAsync(user, "Customer").Result;
                bool UserAdmin = _userManager.IsInRoleAsync(user, "Admin").Result;
                bool UserEmployee = _userManager.IsInRoleAsync(user, "Employee").Result;

                List<Transaction> allTrans = _context.Transactions.Include(t => t.BankAccount).Include(t => t.StockPortfolio).ToList();

                foreach (Transaction t in allTrans)
                {
                    if (t.TransactionStatus == TransactionStatuses.Scheduled)
                    {
                        //if (t.TransactionDate.AddDays(-100) <= DateTime.Today)
                        if (t.TransactionDate <= DateTime.Today)
                        {

                            if (t.BankAccount != null)
                            {
                                if (t.BankAccount.BankAccountBalance + t.TransactionAmount < 0)
                                {

                                    // TRANSACTION WILL SEND THE ACCOUNT INTO OVERDRAFT
                                    t.TransactionStatus = TransactionStatuses.Deleted;
                                    EmailMessaging.SendEmail(user.Email, "Longhorn Bank: Transaction Canceled for Bank Account "
                                        + t.BankAccount.BankAccountNumber, "Your transaction for $" + t.TransactionAmount
                                        + " has been canceled because it would have sent your account into overdraft. Please contact us if you have any questions.");
                                    
                                    // Create notifaction
                                    Message m = new()
                                    {
                                        Info = "Your transaction for $" + t.TransactionAmount
                                        + " has been canceled because it would have sent your account into overdraft. Please contact us if you have any questions.",
                                        Subject = "Longhorn Bank: Transaction Canceled for Bank Account " + t.BankAccount.BankAccountNumber,
                                        Date = DateTime.Today,
                                        Sender = "Longhorn Bank",
                                        Receiver = user.Email,
                                        Admins = new List<AppUser>()
                                    };
                                    m.Admins.Add(user);
                                    _context.Add(m);
                                }
                                else
                                {
                                    t.BankAccount.BankAccountBalance += t.TransactionAmount;
                                    t.TransactionStatus = TransactionStatuses.Approved;
                                }

                                _context.Update(t.BankAccount);
                            }
                            else if (t.StockPortfolio != null)
                            {
                                if (t.StockPortfolio.CashValuePortion + t.TransactionAmount < 0)
                                {
                                    // TRANSACTION WILL SEND THE ACCOUNT INTO OVERDRAFT
                                    t.TransactionStatus = TransactionStatuses.Deleted;
                                    EmailMessaging.SendEmail(user.Email, "Longhorn Bank: Transaction Canceled for Stock Portfolio " + t.StockPortfolio.PortfolioNumber,
                                        "Your transaction for $" + t.TransactionAmount +
                                        "has been canceled because it would have sent your account into overdraft. Please contact us if you have any questions.");

                                    // Create notifaction
                                    Message m = new()
                                    {
                                        Info = "Your transaction for $" + t.TransactionAmount
                                        + " has been canceled because it would have sent your account into overdraft. Please contact us if you have any questions.",
                                        Subject = "Longhorn Bank: Transaction Canceled for Stock Portfolio " + t.StockPortfolio.PortfolioNumber,
                                        Date = DateTime.Today,
                                        Sender = "Longhorn Bank",
                                        Receiver = user.Email,
                                        Admins = new List<AppUser>()
                                    };
                                    m.Admins.Add(user);
                                    _context.Add(m);
                                }
                                else
                                {
                                    t.StockPortfolio.CashValuePortion += t.TransactionAmount;
                                    t.TransactionStatus = TransactionStatuses.Approved;
                                }

                                _context.Update(t.StockPortfolio);
                            }

                            _context.Update(t);
                        }
                    }
                }

                // list of bank accounts from user
                List<BankAccount> bankAccountsList = _context.BankAccounts.Where(b => b.User.UserName == lvm.Email).ToList();


                foreach (BankAccount bankAccount in bankAccountsList)
                {

                    if (bankAccount.BankAccountType == BankAccountTypes.IRA)
                    {
                        DateTime today = DateTime.Today;
                        
                        int age = today.Year - bankAccount.User.DOB.Year;

                        if (bankAccount.User.DOB > today.AddYears(-age))
                        {
                            age--;
                        }
                        // update whether or not IRA accounts are qualified
                        if ( age > 65 && bankAccount.IRAQualified == false)
                        {
                            bankAccount.IRAQualified = true;
                            _context.Update(bankAccount);
                        }
                        else
                        {
                            bankAccount.IRAQualified = false;
                        }
                    }
                }

                _context.SaveChanges();

                

                // if CUSTOMER has no account, redirect to new user view
                if (user.UserHasAccount == false && UserCustomer)
                {
                    // Send email
                    return RedirectToAction("NewUser", "Home");
                }
                else if (user.UserHasAccount && UserCustomer && user.IsActive)
                {
                    
                    return RedirectToAction("Index", "BankAccounts");

                }

                else if (UserCustomer && user.IsActive == false)
                {
                     
                    return RedirectToAction("Index", "BankAccounts", new { message = "Web Account has been disabled, please contact your local branch for more information" });
                }
                // Admin needs to be redirected to home screen with tasks awaiting attention
                // Large deposits or disputes

                else if (UserAdmin)
                {
                    return RedirectToAction("ManageTask", "RoleAdmin");
                }

                else if (UserEmployee && user.IsActive)
                {
                    return RedirectToAction("Index", "RoleAdmin");
                }
                else
                {
                    //Show message that account has been deactivated
                    ModelState.AddModelError("", "Account has been terminated.");
                    return View(lvm);

                }

                
                
            }
            else //log in was not successful
            {
                //add an error to the model to show invalid attempt
                ModelState.AddModelError("", "Invalid login attempt.");
                //send user back to login page to try again
                return View(lvm);
            }
        }

        public IActionResult AccessDenied()
        {
            return View("Error", new string[] { "You are not authorized for this resource" });
        }

        //GET: Account/Index
        
        public IActionResult Index()
        {
            IndexViewModel ivm = new();

            //get user info
            String id = User.Identity.Name;
            AppUser user = _context.Users.FirstOrDefault(u => u.UserName == id);

            //populate the view model
            //(i.e. map the domain model to the view model)
            ivm.Email = user.Email;
            ivm.HasPassword = true;
            ivm.UserID = user.Id;
            ivm.UserName = user.UserName;
            // Add added fields to view model
            ivm.Street = user.Street;
            ivm.State = user.State;
            ivm.City = user.City;
            ivm.ZipCode = user.ZipCode;
            ivm.PhoneNumber = user.PhoneNumber;

            ivm.FirstName = user.FirstName;
            ivm.MidIntName = user.MidIntName;
            ivm.LastName = user.LastName;

            //send data to the view
            return View(ivm);
        }

        public IActionResult Modify()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modify(ModifyViewModel mvm)
        {
            if (ModelState.IsValid)
            {
                // Count changes
                int countToChange = 0;

                //get user info
                AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);

                if (String.IsNullOrEmpty(mvm.Street) == false)
                {
                    user.Street = mvm.Street;
                    await _userManager.UpdateAsync(user);
                    countToChange++;

                }

                if (String.IsNullOrEmpty(mvm.City) == false)
                {
                    user.City = mvm.City;
                    await _userManager.UpdateAsync(user);
                    countToChange++;
                }

                if (String.IsNullOrEmpty(mvm.State) == false)
                {
                    user.State = mvm.State;
                    await _userManager.UpdateAsync(user);
                    countToChange++;
                }

                if (String.IsNullOrEmpty(mvm.ZipCode) == false)
                {
                    user.ZipCode = mvm.ZipCode;
                    await _userManager.UpdateAsync(user);
                    countToChange++;
                }

                if (String.IsNullOrEmpty(mvm.PhoneNumber) == false)
                {
                    user.PhoneNumber = mvm.PhoneNumber;
                    await _userManager.UpdateAsync(user);
                    countToChange++;
                }

                if (String.IsNullOrEmpty(mvm.FirstName) == false)
                {
                    user.FirstName = mvm.FirstName;
                    await _userManager.UpdateAsync(user);
                    countToChange++;
                }

                if (String.IsNullOrEmpty(mvm.MidIntName) == false)
                {
                    user.MidIntName = mvm.MidIntName;
                    await _userManager.UpdateAsync(user);
                    countToChange++;
                }

                if (String.IsNullOrEmpty(mvm.LastName) == false)
                {
                    user.LastName = mvm.LastName;
                    await _userManager.UpdateAsync(user);
                    countToChange++;
                }

                if (countToChange == 0)
                {
                    ModelState.AddModelError("", "No changes were made");
                    return View(mvm);
                }
                else
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Account");
                }
            }
            
            return View(mvm);
        }



        //Logic for change password
        // GET: /Account/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel cpvm)
        {
            //if user forgot a field, send them back to 
            //change password page to try again
            if (ModelState.IsValid == false)
            {
                return View(cpvm);
            }

            //Find the logged in user using the UserManager
            AppUser userLoggedIn = await _userManager.FindByNameAsync(User.Identity.Name);

            //Attempt to change the password using the UserManager
            var result = await _userManager.ChangePasswordAsync(userLoggedIn, cpvm.OldPassword, cpvm.NewPassword);

            //if the attempt to change the password worked
            if (result.Succeeded)
            {
                //sign in the user with the new password
                await _signInManager.SignInAsync(userLoggedIn, isPersistent: false);

                //send the user back to the home page
                return RedirectToAction("Index", "Home");
            }
            else //attempt to change the password didn't work
            {
                //Add all the errors from the result to the model state
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                //send the user back to the change password page to try again
                return View(cpvm);
            }
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogOff()
        {
            //sign the user out of the application
            _signInManager.SignOutAsync();

            //send the user back to the home page
            return RedirectToAction("Index", "Home");
        }           
    }
}