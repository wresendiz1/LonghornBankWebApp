using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System;
using LonghornBankWebApp.Utilities;
using MailBee.ImapMail;
using NuGet.Packaging.Signing;
using System.Collections.Generic;
using MailBee.Mime;

namespace LonghornBankWebApp.Controllers
{
    // Authorize both employee and admin
    [Authorize(Roles = "Employee, Admin")]
    public class RoleAdminController : Controller
    {
        //create private variables for the services needed in this controller
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        //RoleAdminController constructor
        public RoleAdminController(AppDbContext appDbContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signIn)
        {
            //populate the values of the variables passed into the controller
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signIn;
            _context = appDbContext;
        }
        
        // GET: /RoleAdmin/
        public async Task<ActionResult> Index()
        {
            //Create a list of roles that will need to be edited
            List<RoleEditModel> roles = new List<RoleEditModel>();
            
            //loop through each of the existing roles
            foreach (IdentityRole role in _roleManager.Roles)
            {

                var UsersInRole = _userManager.GetUsersInRoleAsync(role.Name);


                //create a new instance of the role edit model
                RoleEditModel rem = new RoleEditModel();

                //populate the properties of the role edit model
                rem.Role = role; //role from database
                rem.ActiveMembers = await UsersInRole;

                //add this role to the list of role edit models
                roles.Add(rem);  
            }

            //pass the list of roles to the view
            return View(roles);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ManageTask()
        {
            //this is a list of all the transactions that are pending
            List<Transaction> PendingTrans = _context.Transactions.Where(t => t.TransactionStatus == TransactionStatuses.Pending).
                Include(t => t.BankAccount).ThenInclude(ba => ba.User).Include(t=>t.StockPortfolio).ThenInclude(sp=>sp.User).ToList();


            // list of disputes
            List<Dispute> Disputes = _context.Disputes.Where(d => d.DisputeStatus == DisputeStatus.Submitted).ToList();

            // Creating a list of objects in case we need to add more to the view


            // Show # of tasks that need to be dealt
            ViewBag.PendingTrans = PendingTrans.Count();
            ViewBag.Disputes = Disputes.Count();


            List<PortfolioProcess> processed = new List<PortfolioProcess>();

            processed = _context.PortfolioProcesses.ToList();


            PortfolioProcess mostRecent = processed.MaxBy(pp => pp.DateProcessed);

            ViewBag.mostRecent = mostRecent;

            ViewBag.Messages = _context.Messages.Where(m => m.Admins.Contains(_userManager.Users.FirstOrDefault(u => u.UserName == User.Identity.Name))).Count();

            return View();

        }

        [Authorize(Roles = "Admin")]
        public IActionResult DisplayProcessing()
        {
            
            return View(_context.PortfolioProcesses.ToList());
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult ProcessPortfolios()
        {
            PortfolioProcess pp = new PortfolioProcess();

            pp.DateProcessed = DateTime.Today;
            pp.ProcessedBy = User.Identity.Name;
            _context.Add(pp);



            List<StockPortfolio> portfolios = _context.StockPortfolios.Include(sp => sp.Transactions).Include(sp=>sp.StockTransactions).ThenInclude(st=>st.Stock).ThenInclude(s=>s.StockType).ToList();

            Transaction bonus;
            bool changed = false;
            foreach (StockPortfolio sp in portfolios)
            {

                Controllers.StockPortfolioController.UpdatePortfolio(sp, _context);
                

                if (sp.IsBalanced == true)
                {
                    bonus = new Transaction();
                    bonus.TransactionAmount = (sp.PortfolioValue - sp.CashValuePortion) * 0.1m;
                    bonus.TransactionType = TransactionTypes.Bonus;
                    bonus.TransactionDate = DateTime.Today;
                    bonus.TransactionNumber = Utilities.GenerateNextNum.GetNextTransactionNum(_context);
                    bonus.TransactionStatus = TransactionStatuses.Approved;
                    bonus.StockPortfolio = sp;
                    bonus.TransactionNotes = "Balanced Portfolio Bonus";
                    sp.CashValuePortion += bonus.TransactionAmount;
                    sp.TotalBonuses += bonus.TransactionAmount;

                    _context.Update(bonus);
                    _context.Update(sp);
                    changed = true;

                }
            }

            if(changed == true)
            {

                _context.SaveChanges();
            }

            return RedirectToAction(actionName:"ManageTask");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult PendingTrans()
        {
            //this is a list of all the transactions that are pending
            List<Transaction> PendingTrans = _context.Transactions.Where(t => t.TransactionStatus == TransactionStatuses.Pending).
                Include(t => t.BankAccount).ThenInclude(ba => ba.User).Include(t=>t.StockPortfolio).ThenInclude(sp=>sp.User).ToList();

            return View(PendingTrans);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult PendingDisputes(bool showAll = false)
        {
            
            List<Dispute> Disputes = new List<Dispute>();

            if (showAll == false)
            {
                Disputes = _context.Disputes.Where(d => d.DisputeStatus == DisputeStatus.Submitted).Include(d => d.Transaction)
                .ThenInclude(t => t.BankAccount).ThenInclude(ba => ba.User).Include(d=> d.Transaction).ThenInclude(t=> t.StockPortfolio).ThenInclude(t=>t.User).ToList();
                ViewBag.showAll = false;
            }

            else
            {
                Disputes = _context.Disputes.Include(d => d.Transaction).ThenInclude(t => t.BankAccount).ThenInclude(ba => ba.User).ToList();
                ViewBag.showAll = true;
            }
            

            return View(Disputes);

        }

        [Authorize(Roles = "Admin")]
        public IActionResult DisputeResolution(Int32 id)
        {

            Dispute dispute = _context.Disputes.Include(d=>d.Transaction).FirstOrDefault(d=>d.DisputeID==id);

            return RedirectToAction(controllerName: "Transactions", actionName: "Details", routeValues: new { id = dispute.Transaction.TransactionID });

        }



        // GET: /RoleAdmin/Create
        // ID -> transaction number
        // action -> approve or reject
        // request is sent in form of dictionary
        [Authorize(Roles = "Admin")]
        public IActionResult ProcessDeposit(string id, string status)
        {

            if (id == null)
            {

                return NotFound();

            }

            Transaction tr = _context.Transactions.Include(t => t.BankAccount).ThenInclude(ba => ba.User).Include(t => t.StockPortfolio).ThenInclude(ba => ba.User).FirstOrDefault(t => t.TransactionNumber == Convert.ToInt32(id));

            if (tr == null)
            {
                return NotFound();
            }

            //update transaction status to approved
            if(status == "Approve")
            {
                if(tr.TransactionDate <= DateTime.Today)
                {
                    tr.TransactionStatus = TransactionStatuses.Approved;

                    if (tr.BankAccount != null)

                    {
                        if (tr.BankAccount.BankAccountType == BankAccountTypes.IRA)
                        {
                            if (tr.BankAccount.IRAContribution + tr.TransactionAmount > 5000)
                            {
                                return View("Error", new string[] { "Deposit " + tr.TransactionNumber + " will cause IRA account " + tr.BankAccount.BankAccountNumber + " to exceed the annual contribution limit." });
                            }
                        }

                        tr.BankAccount.BankAccountBalance += tr.TransactionAmount;
                    }

                    else if (tr.StockPortfolio != null)
                    {
                        tr.StockPortfolio.CashValuePortion += tr.TransactionAmount;
                    }
                    
                }
                else
                {
                    tr.TransactionStatus = TransactionStatuses.Scheduled;
                }
                
                
            }
            else
            {
                tr.TransactionStatus = TransactionStatuses.Rejected;
            }

            if (tr.BankAccount != null)
            {
                EmailMessaging.SendEmail(tr.BankAccount.User.Email, "Transaction: " + tr.TransactionNumber
                + " has been " + tr.TransactionStatus.ToString(), "Transaction " + tr.TransactionNumber + " has been " + tr.TransactionStatus.ToString() + " by an administrator." +
                " Please login to your account to view the details of this transaction.");
                
                _context.Update(tr.BankAccount);

                // Create notifaction
                Message m = new Message()
                {
                    Info = "Transaction " + tr.TransactionNumber + " has been " + tr.TransactionStatus.ToString() + " by an administrator." +
                "Head over to your account to view the details of this transaction.",
                    Subject = "Transaction: " + tr.TransactionNumber + " has been " + tr.TransactionStatus.ToString(),
                    Date = DateTime.Today,
                    Sender = "Longhorn Bank",
                    Receiver = tr.BankAccount.User.Email
                };
                m.Admins = new List<AppUser>();
                m.Admins.Add(tr.BankAccount.User);
                _context.Add(m);

            }

            else if (tr.StockPortfolio!= null)
            {
                EmailMessaging.SendEmail(tr.StockPortfolio.User.Email, "Transaction: " + tr.TransactionNumber
                + " has been " + tr.TransactionStatus.ToString(), "Transaction " + tr.TransactionNumber + " has been " + tr.TransactionStatus.ToString() + " by an administrator." +
                " Please login to your account to view the details of this transaction.");

                _context.Update(tr.StockPortfolio);

                // Create notifaction
                Message m = new Message()
                {
                    Info = "Transaction " + tr.TransactionNumber + " has been " + tr.TransactionStatus.ToString() + " by an administrator." +
                "Head over to your account to view the details of this transaction.",
                    Subject = "Transaction: " + tr.TransactionNumber + " has been " + tr.TransactionStatus.ToString(),
                    Date = DateTime.Today,
                    Sender = "Longhorn Bank",
                    Receiver = tr.StockPortfolio.User.Email
                };
                m.Admins = new List<AppUser>();
                m.Admins.Add(tr.StockPortfolio.User);
                _context.Add(m);
            }

            _context.Update(tr);
            _context.SaveChanges();

            return RedirectToAction("PendingTrans");
        }


        public IActionResult Inbox()
        {
            // TODO: Retrieve emails from IMAP

            Imap imp = new Imap();
            imp.Connect("imap.gmail.com", 993);
            imp.Login("longhornbanktrust@gmail.com", "uddtvaxvzyhsmwee");
            
        

            imp.ExamineFolder("INBOX");
            List<Message> messages = new List<Message>();

            if (imp.MessageCount > 1)
            
            {

                var max = imp.MessageCount;

                
                for (var i = 1; i <= max; i++)
                {
                    MailMessage m = imp.DownloadEntireMessage(i, false);
                    Message msg = new Message();
                    
                    msg.Receiver = m.To;
                    msg.Sender = m.From;
                    msg.Subject = m.Subject;
                    msg.Date = m.Date;
                    msg.Info = m.BodyPlainText;
                    msg.IsRead = false;

                    messages.Add(msg);



                }

                imp.Disconnect();

                messages = messages.Where(m => m.Subject.Contains("Team 33")).ToList();

                return View(messages);
                
            }   

            else
            {
                imp.Disconnect();
                return View();
            }


        }


        
        public ActionResult Edit(string id)
        {
            //look up the role requested by the user
            IdentityRole role = _roleManager.FindByIdAsync(id).Result;

            //create a list for the members of the role
            List<AppUser> Active = new List<AppUser>();

            List<AppUser> NonActive = new List<AppUser>();

            var UsersInRole = _userManager.GetUsersInRoleAsync(role.Name).Result;  


            foreach(AppUser user in UsersInRole)
            {
                if (user.IsActive == true)
                {
                    Active.Add(user);
                }
                else
                {
                    NonActive.Add(user);

                }
            }

            //create a new instance of the role edit model
            RoleEditModel rem = new RoleEditModel();


            //populate the properties of the role edit model
            rem.Role = role; //role from database
            rem.ActiveMembers = Active; //list of users in this role
            rem.InActiveMembers = NonActive;

            //send user to view with populated role edit model
            return View(rem);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(RoleModificationModel rmm)
        {

            //if RoleModificationModel is valid, add new users
            if (ModelState.IsValid)
            {
                //if there are users to re-activate, then add them
                if (rmm.IdsToActivate != null)
                {
                    foreach (string userId in rmm.IdsToActivate)
                    {
                        //find the user in the database using their id
                        AppUser user = await _userManager.FindByIdAsync(userId);

                        //attempt to add the user to the role using the UserManager
                        user.IsActive = true;
                        await _userManager.UpdateAsync(user);
                        await _context.SaveChangesAsync();


                    }
                }

                //if there are users to deactivate
                if (rmm.IdsToDeactivate != null)
                {
                    //loop through all the ids to remove from role
                    foreach (string userId in rmm.IdsToDeactivate)
                    {
                        //find the user in the database using their id
                        AppUser user = _userManager.FindByIdAsync(userId).Result;
                        user.IsActive = false;
                        await _userManager.UpdateAsync(user);
                        await _context.SaveChangesAsync();
                    }
                }

                //this is the happy path - all edits worked
                //take the user back to the RoleAdmin Index page
                return RedirectToAction("Index");
            }

            //this is a sad path - the role was not found
            //show the user the error page
            return View("Error", new string[] { "Role Not Found" });
        }

        [Authorize(Roles = "Admin")]
        // GET: /Account/Register
        public IActionResult RegisterEmployee()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterEmployee(EmployeeRegisterViewModel rvm)
        {
            //if registration data is valid, create a new user on the database
            if (ModelState.IsValid == false)
            {
                //this is the sad path - something went wrong, 
                //return the user to the register page to try again
                return View(rvm);
            }

            AppUser u = await _userManager.FindByEmailAsync(rvm.Email);

            if (u != null)
            {
                ModelState.AddModelError(nameof(rvm.Email), "Email already taken");
                return View(rvm);
            }
            //this code maps the RegisterViewModel to the AppUser domain model
            AppUser newUser = new AppUser
            {
                UserName = rvm.Email,
                Email = rvm.Email,
                PhoneNumber = rvm.PhoneNumber,

                FirstName = rvm.FirstName,
                MidIntName = rvm.MidIntName,
                LastName = rvm.LastName,
                Street = rvm.Street,
                State = rvm.State,
                City = rvm.City,
                ZipCode = rvm.ZipCode,
                IsActive = true

            };

            //create AddUserModel
            AddUserModel aum = new AddUserModel()
            {
                User = newUser,
                Password = rvm.Password,
                // Only Customer role can be registered 
                RoleName = "Employee"
            };

            //This code uses the AddUser utility to create a new user with the specified password
            IdentityResult result = await Utilities.AddUser.AddUserWithRoleAsync(aum, _userManager, _context);

            if (result.Succeeded) //everything is okay
            {
                // Send email to new employee
                EmailMessaging.SendEmail(aum.User.Email, "Welcome to Longhorn Bank", 
                    "You have been added as an employee to Longhorn Bank. Please login to your account to view your account information." +
                    "Passoword: " + aum.Password + ". Please contact your manager if you have any questions.");

                //Send the admin to his home page
                return RedirectToAction("Index");
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

        public async Task<IActionResult> AccountIndex(string username, string id)
        {

            var UsersInRole = _userManager.GetUsersInRoleAsync(id);


            if (UsersInRole.Result.Count() == 0)
            {
                return View("Error", new string[] { "No Users Found" });
            }

            var result = from cus in await UsersInRole select cus;

            ViewBag.AllUsers = result.Count();

            if (String.IsNullOrEmpty(username) == false)
            {
                result = from cus in await UsersInRole where cus.UserName.Contains(username) select cus;

            }

            
            ViewBag.SelectedUsers = result.Count();

            return View(result.OrderBy(c => c.UserName));
            


        }

        
        public async Task<IActionResult> AccountDetails(string user)
        {
            if (user == null)
            {
                return NotFound();
            }

            AppUser customer = await _userManager.FindByNameAsync(user);

            if (customer == null)
            {
                return View("Error", new string[] { "User Not Found" });
            }

            IndexViewModel ivm = new IndexViewModel();

            //populate the view model
            //(i.e. map the domain model to the view model)
            ivm.Email = customer.Email;
            ivm.HasPassword = true;
            ivm.UserID = customer.Id;
            ivm.UserName = customer.UserName;
            // Add added fields to view model
            ivm.Street = customer.Street;
            ivm.State = customer.State;
            ivm.City = customer.City;
            ivm.ZipCode = customer.ZipCode;
            ivm.PhoneNumber = customer.PhoneNumber;

            ivm.FirstName = customer.FirstName;
            ivm.MidIntName = customer.MidIntName;
            ivm.LastName = customer.LastName;


            ViewBag.Role = await _userManager.GetRolesAsync(customer);
            ViewBag.Role = ViewBag.Role[0];
            
            //send data to the view
            return View(ivm);

        }

        
        public async Task<IActionResult> AccountModify(string email)
        {
            
            ViewBag.Email = email;
            AppUser User = await _userManager.FindByEmailAsync(email);

            ViewBag.Role = await _userManager.GetRolesAsync(User);
            ViewBag.Role = ViewBag.Role[0];


            return View();
        }
        

        public async Task<IActionResult> ChangePassword(string user)
        {
            ViewBag.UserName = user;

            // reload viewbag for post changepassword redirect
            AppUser User = await _userManager.FindByNameAsync(user);
            ViewBag.Role = await _userManager.GetRolesAsync(User);
            ViewBag.Role = ViewBag.Role[0];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(EmpChangePasswordViewModel cpvm, string id)
        {
            if (ModelState.IsValid == false)
            {
                return View(cpvm);
            }

            AppUser userLoggedIn = await _userManager.FindByNameAsync(id);

            // Use passwordHash to add new password
            userLoggedIn.PasswordHash = _userManager.PasswordHasher.HashPassword(userLoggedIn, cpvm.NewPassword);
            var result = await _userManager.UpdateAsync(userLoggedIn);

            if (result.Succeeded)
            {

                EmailMessaging.SendEmail(userLoggedIn.Email, "Password Changed", "Your password has been changed. Your new password is: " + cpvm.NewPassword);
                return RedirectToAction("AccountDetails", new { user = userLoggedIn.UserName });
                

            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                
                ViewBag.UserName = userLoggedIn.UserName;
                return View(cpvm);

            }



        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountModify(ModifyViewModel mvm, string id)
        {
            if (ModelState.IsValid)
            {
                // Count changes
                int countToChange = 0;
                
                //get user info
                AppUser user = await _userManager.FindByEmailAsync(id);

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

                if (countToChange == 0)
                {
                    ModelState.AddModelError("", "No changes were made");
                    ViewBag.Email = id;
                    return View(mvm);
                }
                else
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("AccountDetails", new {user = id});
                }
            }

            return View(mvm);
        }



        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BaIndex(string username)
        {
            
            var UsersInRole = _userManager.GetUsersInRoleAsync("Customer");


            if (UsersInRole.Result.Count() == 0)
            {
                return View("Error", new string[] { "No Customers Found" });
            }

            var result = from cus in await UsersInRole select cus;
            
            ViewBag.AllUsers = result.Count();

            if (username != null)
            {
                result = from cus in await UsersInRole where cus.UserName.Contains(username) select cus;

            }


            ViewBag.SelectedUsers = result.Count();

            return View(result.OrderBy(c => c.UserName));



        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BaDetails(String user)
        {
            if (user == null)
            {
                return NotFound();
            }

            List<BankAccount> dbBankAccounts = await _context.BankAccounts.Where(ba => ba.User.UserName == user).ToListAsync();
            ViewBag.SP = await _context.StockPortfolios.FirstOrDefaultAsync(sp => sp.User.UserName == user);


            if (dbBankAccounts == null && ViewBag.SP == null)
            {
                return View("Error", new string[] { "Bank Account Not Found" });
            }
            
            return View(dbBankAccounts);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditBa(string id, string status )
        {
            if (id == null | status == null)
            {
                return NotFound();
            }

            BankAccount dbBankAccount = await _context.BankAccounts.Include(ba => ba.User).FirstOrDefaultAsync(ba => ba.BankAccountNumber == Convert.ToUInt32(id));
            StockPortfolio dBStockPort = await _context.StockPortfolios.Include(sp => sp.User).FirstOrDefaultAsync(sp => sp.PortfolioNumber == Convert.ToUInt32(id));


            try
            {

                if (dbBankAccount != null)
                {
                    if (status == "activate")
                    {
                        dbBankAccount.isActive = true;
                    }

                    if (status == "deactivate")
                    {
                        dbBankAccount.isActive = false;

                    }
                    //only update the account status 
                    _context.Update(dbBankAccount);
                    await _context.SaveChangesAsync();

                }

                else
                {
                    if (dBStockPort != null)
                    {
                        if (status == "activate")
                        {
                            dBStockPort.isActive = true;
                        }

                        if (status == "deactivate")
                        {
                            dBStockPort.isActive = false;

                        }
                        //only update the account status 
                        _context.Update(dBStockPort);
                        await _context.SaveChangesAsync();

                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BankAccountExists(dbBankAccount.BankAccountID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }


            
            if(dbBankAccount != null)
            {
       
                    return RedirectToAction("BaDetails", new { user = dbBankAccount.User.UserName });
            }

            else
            {
                    return RedirectToAction("BaDetails", new { user = dBStockPort.User.UserName });
            }
            

        }




        private bool BankAccountExists(int id)
        {
            return _context.BankAccounts.Any(e => e.BankAccountID == id);
        }
    }


}