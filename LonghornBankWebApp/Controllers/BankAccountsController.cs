using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;
using LonghornBankWebApp.Utilities;
using Microsoft.AspNetCore.Identity;
using static NuGet.Packaging.PackagingConstants;
using LonghornBankWebApp.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;

namespace LonghornBankWebApp.Controllers
{
    [Authorize]

    public class BankAccountsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BankAccountsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: BankAccounts
        public IActionResult Index(String message, string SearchString)
        {
            AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;
            if (u.UserHasAccount == false && User.IsInRole("Customer"))
            {
                return RedirectToAction("Create", "BankAccounts");
            }


            List<BankAccount> bankAccounts;

            ViewBag.Message = message;
            

            
            // query and display only the user's bank accounts
            if (User.IsInRole("Customer"))
            {
                
                bankAccounts = _context.BankAccounts.Include(r => r.Transactions).Where(r => r.User.UserName == User.Identity.Name).ToList();
                StockPortfolio holder = new StockPortfolio();

                holder = _context.StockPortfolios.FirstOrDefault(r => r.User.UserName == User.Identity.Name);

                ViewBag.SP = holder;
            }
            else
            {
                var sp = from port in _context.StockPortfolios
                         select port;
                var ba = from acc in _context.BankAccounts select acc;

                ViewBag.All = ba.Count() + sp.Count();
                if (String.IsNullOrEmpty(SearchString) == false)
  
                {
                    sp = from port in _context.StockPortfolios
                         where port.PortfolioName.Contains(SearchString)
                         select port;
                    ba = from acc in _context.BankAccounts
                         where acc.BankAccountName.Contains(SearchString)
                         select acc;


                }
                ViewBag.Selected = ba.Count() + sp.Count();

                ViewBag.portfolios = sp;

                return View(ba.OrderBy(r => r.User.UserName));

            }

            StockPortfolio sp1 = _context.StockPortfolios.FirstOrDefault(r => r.User.UserName == User.Identity.Name);
            
            if(sp1 == null && u.IsActive)
            {
                ViewBag.Create = "Create a Stock Portfolio";

            }
            else if(u.IsActive == false && ViewBag.Message == null)
            {
                ViewBag.Message = "Web Account has been disabled, please contact your local branch for more information";
            }

            return View(bankAccounts);
        }


        public IActionResult DetailedSearch(Int32 id)
        {
            ViewBag.id = id;
            SearchViewModel svm = new SearchViewModel();
            svm.baIdentifier = id;

            List<SelectListItem> sortby = new List<SelectListItem>();

            sortby.Add(new SelectListItem(text: "Transaction Number", value: "Number"));
            sortby.Add(new SelectListItem(text: "Transaction Type", value: "Type"));
            sortby.Add(new SelectListItem(text: "Transaction Notes", value: "Notes"));
            sortby.Add(new SelectListItem(text: "Transaction Amount", value: "Amount"));
            sortby.Add(new SelectListItem(text: "Transaction Date", value: "Date"));

            ViewBag.sortby = sortby;


            List<SelectListItem> orderList = new List<SelectListItem>();

            orderList.Add(new SelectListItem(text: "Descending", value: "Descending"));
            orderList.Add(new SelectListItem(text: "Ascending", value: "Ascending"));

            ViewBag.order = orderList;

            return View(svm);
        }

        public IActionResult DisplaySearchResults(SearchViewModel svm, String? selected, String? order)
        {
            //var query = from r in _context.Transactions.Include(t => t.BankAccount).Where(b=> b.BankAccount.BankAccountID == id) select r;
            var query = from r in _context.Transactions select r;

            query = query.Where(r => r.BankAccount.BankAccountID == svm.baIdentifier);

            if (svm.TransactionDescription != null && svm.TransactionDescription != "") //user entered something
            {
                query = query.Where(r => r.TransactionNotes.Contains(svm.TransactionDescription));
            }

            if (svm.SelectedType != null)
            {
                query = query.Where(r => r.TransactionType == svm.SelectedType);
            }

            if (svm.TransactionNumber != null && svm.TransactionNumber != "") //user entered something
            {
                query = query.Where(r => r.TransactionNumber.ToString() == (svm.TransactionNumber));//not working currently - see what is wrong
            }

            if (svm.LowAmount != null && svm.HighAmount != null) //user entered something
            {
                query = query.Where(r => r.TransactionAmount < svm.HighAmount && r.TransactionAmount > svm.LowAmount);//ASK: does this include endpoints? 
            }

            else if (svm.LowAmount != null)
            {
                query = query.Where(r => r.TransactionAmount > svm.LowAmount);
            }
            else if (svm.HighAmount != null)
            {
                query = query.Where(r => r.TransactionAmount < svm.HighAmount);
            }

            if (svm.DateLow != null && svm.DateHigh != null)
            {
                query = query.Where(r => r.TransactionDate >= svm.DateLow && r.TransactionDate <= svm.DateHigh);//ASK: does this include endpoints
            }

            else if(svm.DateHigh != null)
            {
                query = query.Where(r => r.TransactionDate <= svm.DateHigh);
            }

            else if(svm.DateLow != null)
            {
                query = query.Where(r => r.TransactionDate >= svm.DateLow);
            }

            if (selected == "Number")
            {
                if (order == "Descending")
                {
                    query = query.OrderByDescending(x => x.TransactionNumber); ;
                }
                else
                {
                    query = query.OrderBy(x => x.TransactionNumber);
                }
            }

            if (selected == "Type")
            {
                if (order == "Descending")
                {
                    query = query.OrderByDescending(x => x.TransactionType);
                }
                else
                {
                    query = query.OrderBy(x => x.TransactionType);
                }
            }
            if (selected == "Notes")
            {
                if (order == "Descending")
                {
                    query = query.OrderByDescending(x => x.TransactionNotes);
                }
                else
                {
                    query = query.OrderBy(x => x.TransactionNotes);
                }
            }
            if (selected == "Amount")
            {
                if (order == "Descending")
                {
                    query = query.OrderByDescending(x => x.TransactionAmount);
                }
                else
                {
                    query = query.OrderBy(x => x.TransactionAmount);
                }
            }
            if (selected == "Date")
            {
                if (order == "Descending")
                {
                    query = query.OrderByDescending(x => x.TransactionDate);
                }
                else
                {
                    query = query.OrderBy(x => x.TransactionDate);
                }
            }


            ViewBag.queried = query.ToList();

            var bankAccount = _context.BankAccounts.Include(t => t.Transactions).FirstOrDefault(m => m.BankAccountID == svm.baIdentifier);

            List<SelectListItem> sortby = new List<SelectListItem>();

            sortby.Add(new SelectListItem(text: "Transaction Number", value: "Number"));
            sortby.Add(new SelectListItem(text: "Transaction Type", value: "Type"));
            sortby.Add(new SelectListItem(text: "Transaction Notes", value: "Notes"));
            sortby.Add(new SelectListItem(text: "Transaction Amount", value: "Amount"));
            sortby.Add(new SelectListItem(text: "Transaction Date", value: "Date"));

            ViewBag.sortby = sortby;


            List<SelectListItem> orderList = new List<SelectListItem>();

            orderList.Add(new SelectListItem(text: "Descending", value: "Descending"));
            orderList.Add(new SelectListItem(text: "Ascending", value: "Ascending"));

            ViewBag.order = orderList;
            ViewBag.id = svm.baIdentifier;

            return View("Details", bankAccount);
        }

        public void UpdateIRA()
        {   
            foreach (BankAccount ba in _context.BankAccounts.Include(ba => ba.User).ToList())
            {
                if(ba.BankAccountType == BankAccountTypes.IRA)
                {
                    if ((DateTime.Now.Subtract(ba.User.DOB).Days / 365) > 65 && ba.IRAQualified == false)
                    {
                        ba.IRAQualified = true;
                        _context.Update(ba);
                    }
                    else
                    {
                        ba.IRAQualified = false;
                    }
                }
            }
            _context.SaveChanges();
        }

        // GET: BankAccounts/Details/5
        // NOTE: Bank account balance should not include pending transactions
        public async Task<IActionResult> Details(int? id, String SearchString, String selected, String order)
        {
            UpdateIRA();

            if (id == null || _context.BankAccounts == null)
            {
                return View("Error", new String[] { "Please specify which bank account you'd like to view the details for." });
            }

            BankAccount bankAccount;

            if (User.IsInRole("Customer"))
            {
                bankAccount = await _context.BankAccounts.Include(t => t.Transactions)
                .FirstOrDefaultAsync(m => m.BankAccountID == id && m.User.Email == User.Identity.Name);
            }
            else
            {
                bankAccount = await _context.BankAccounts.Include(t => t.Transactions)
                .FirstOrDefaultAsync(m => m.BankAccountID == id);
            }


            if (bankAccount == null)
            {
                return View("Error", new String[] { "This bank account could not be found in the database" });
            }


            List<Transaction> queried = new List<Transaction>();

            

            if (String.IsNullOrEmpty(SearchString) == false)
            {
                foreach (Transaction t in bankAccount.Transactions)
                {
                    
                    if(t.TransactionNotes != "" && t.TransactionNotes != null)
                    {
                        if (t.TransactionNotes.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                        {
                            queried.Add(t);
                        }
                    }  
                }
            }
            else
            {
                queried = bankAccount.Transactions.ToList();
                queried = queried.OrderByDescending(x => x.TransactionNumber).ToList();
            }

            if (selected == "Number")
            {
                if (order == "Ascending")
                {
                    queried = queried.OrderBy(x => x.TransactionNumber).ToList();
                    
                }
                else
                {
                    queried = queried.OrderByDescending(x => x.TransactionNumber).ToList();
                }
            }

            if (selected == "Type")
            {
                if (order == "Descending")
                {
                    queried = queried.OrderByDescending(x => x.TransactionType).ToList();
                }
                else
                {
                    queried = queried.OrderBy(x => x.TransactionType).ToList();
                }
            }
            if (selected == "Notes")
            {
                if (order == "Descending")
                {
                    queried = queried.OrderByDescending(x => x.TransactionNotes).ToList();
                }
                else
                {
                    queried = queried.OrderBy(x => x.TransactionNotes).ToList();
                }
            }
            if (selected == "Amount")
            {
                if (order == "Descending")
                {
                    queried = queried.OrderByDescending(x => x.TransactionAmount).ToList();
                }
                else
                {
                    queried = queried.OrderBy(x => x.TransactionAmount).ToList();
                }
            }
            if (selected == "Date")
            {
                if (order == "Descending")
                {
                    queried = queried.OrderByDescending(x => x.TransactionDate).ToList();
                }
                else
                {
                    queried = queried.OrderBy(x => x.TransactionDate).ToList();
                }
            }

            foreach (Transaction t in queried)
            {
                if (t.TransactionStatus == TransactionStatuses.Scheduled)
                {
                    if (t.TransactionDate <= DateTime.Today)
                    {
                        t.TransactionStatus = TransactionStatuses.Approved;
                        _context.Update(t);
                        
                    }
                }
            }

            ViewBag.id = id;

            List<SelectListItem> sortby = new List<SelectListItem>();

            sortby.Add(new SelectListItem(text: "Transaction Number", value: "Number"));
            sortby.Add(new SelectListItem(text: "Transaction Type", value: "Type"));
            sortby.Add(new SelectListItem(text: "Transaction Notes", value: "Notes"));
            sortby.Add(new SelectListItem(text: "Transaction Amount", value: "Amount"));
            sortby.Add(new SelectListItem(text: "Transaction Date", value: "Date"));

            ViewBag.sortby = sortby;


            List<SelectListItem> orderList = new List<SelectListItem>();

            orderList.Add(new SelectListItem(text: "Descending", value: "Descending"));
            orderList.Add(new SelectListItem(text: "Ascending", value: "Ascending"));
            

            ViewBag.order = orderList;

            ViewBag.queried = queried;

            _context.SaveChanges();



            decimal withScheduled = 0;
            decimal withPending = 0;
            foreach (Transaction t in bankAccount.Transactions)
            {
                if (t.TransactionStatus == TransactionStatuses.Scheduled)
                {
                    withScheduled += t.TransactionAmount;

                }
                if(t.TransactionStatus == TransactionStatuses.Pending)
                {
                    withPending += t.TransactionAmount;
                }
            }
            withScheduled += bankAccount.BankAccountBalance;
            withPending += bankAccount.BankAccountBalance;
            ViewBag.withScheduled = withScheduled;
            ViewBag.withPending = withPending;

            return View(bankAccount);
        }

        // GET: BankAccounts/Create

        // CONSIDER: phantom text in textbox for account default names (might be difficult because it depends on account type
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            AppUser u = await _userManager.FindByNameAsync(User.Identity.Name);
            if (User.IsInRole("Customer") && u.IsActive == true)
            {
                BankAccount bankAccount = new BankAccount();
                bankAccount.User = await _userManager.FindByNameAsync(User.Identity.Name);
                return View(bankAccount);
            }
            else if (User.IsInRole("Customer") && u.IsActive == false)
            {
                // Validate web account is active
      
                 return RedirectToAction("Index", new { message = "Your account is not active. Please contact an administrator to activate your account." });
   
            }
            
            return View("Error");
        }


        //TODO: Customer should see an appropriate confirmation message when succesfully applying for account
        // POST: BankAccounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BankAccountID,BankAccountNumber,BankAccountName,BankAccountType,BankAccountBalance,InitialDeposit, User")] BankAccount bankAccount)
        {        
            if (ModelState.IsValid)
            {
                bankAccount.User = await _userManager.FindByNameAsync(bankAccount.User.UserName);


                    // calculate age
                int age = DateTime.Now.Subtract(bankAccount.User.DOB).Days / 365;
                
                // ensure user is < 70 yo if they want to create IRA

                // ensure user doesn't already have an IRA
                if (bankAccount.BankAccountType == BankAccountTypes.IRA)
                {
                    // count number of IRA accounts
                    int count = _context.BankAccounts.Where(ba => ba.BankAccountType == BankAccountTypes.IRA).Where(ba => ba.User.UserName ==
                    bankAccount.User.UserName).Count();
                    
                    if(count >= 1)
                    {
                        ModelState.AddModelError("BankAccountType", "You already have an IRA account.");
                        return View(bankAccount);
                    }
                    if (age>= 70)
                    {
                        return View("Error", new String[] { "You must be under 70 years old to apply for an IRA!" });
                    }
                    
                    if(bankAccount.InitialDeposit > 5000)
                    {
                        ModelState.AddModelError(nameof(bankAccount.InitialDeposit), "Max Contribution cannot exceed $5000");
                        return View(bankAccount);

                    }
                }


                // set account number
                bankAccount.BankAccountNumber = GenerateNextNum.GetNextAccountNum(_context);

                // set default names if user didn't enter
                if (bankAccount.BankAccountName == null)
                {
                    if (bankAccount.BankAccountType == BankAccountTypes.Checking)
                    {
                        bankAccount.BankAccountName = "Longhorn Checkings";
                    }
                    else if (bankAccount.BankAccountType == BankAccountTypes.Savings)
                    {
                        bankAccount.BankAccountName = "Longhorn Savings";
                    }
                    else if (bankAccount.BankAccountType == BankAccountTypes.IRA)
                    {
                        bankAccount.BankAccountName = bankAccount.User.FirstName + "IRA";
                    }
                }

                if (bankAccount.InitialDeposit <= 0)
                {
                    ModelState.AddModelError(nameof(bankAccount.InitialDeposit), "Initial Deposit must be greater than $0");
                    return View(bankAccount);
                }

                //set initial deposit
                Transaction init_deposit = new Transaction();
                init_deposit.BankAccount = bankAccount;
                init_deposit.TransactionAmount = bankAccount.InitialDeposit;
                init_deposit.TransactionType = TransactionTypes.Deposit;
                init_deposit.TransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);
                init_deposit.TransactionDate = DateTime.Today;
                init_deposit.TransactionNotes = "Congratulations on creating your account!";

                // check if approval is needed for inital deposit
                if (bankAccount.InitialDeposit>5000)
                {
                    init_deposit.TransactionStatus = TransactionStatuses.Pending;
                    
                }
                else
                {
                    init_deposit.TransactionStatus = TransactionStatuses.Approved;
                    bankAccount.BankAccountBalance += init_deposit.TransactionAmount;
                }

                
                
                return RedirectToAction(nameof(CreateConfirmation), bankAccount);
            }
            return View(bankAccount);
        }


        public IActionResult CreateConfirmation(BankAccount bankAccount)
        {
            // direct user to confirm view where they can see if their initial deposit needs approval
            // they can also see what they entered, confirm, or go back and edit
            if (bankAccount.InitialDeposit > 5000)
            {
                ViewBag.InitDepoStatus = "Initial deposits over $5,000 will need to be approved by an admin before showing up in balance (2-3 business days)";
            }

            return View(bankAccount);
        }

        public async Task<IActionResult> Confirm(UInt32 num, String name, BankAccountTypes type, Decimal deposit)
        {
            BankAccount bankAccount = new BankAccount();
            bankAccount.InitialDeposit = deposit;

            

            Transaction init_deposit = new Transaction();
            init_deposit.BankAccount = bankAccount;
            init_deposit.TransactionAmount = bankAccount.InitialDeposit;
            init_deposit.TransactionType = TransactionTypes.Deposit;
            init_deposit.TransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);
            init_deposit.TransactionDate = DateTime.Today;
            init_deposit.TransactionNotes = "Congratulations on creating your account!";

            

            bankAccount.BankAccountNumber = num;
            bankAccount.BankAccountName = name;
            bankAccount.BankAccountType = type;
            bankAccount.User = await _userManager.FindByNameAsync(User.Identity.Name);
            bankAccount.User.UserHasAccount = true;
            bankAccount.isActive = true;

            // check if approval is needed for inital deposit
            if (bankAccount.InitialDeposit > 5000)
            {
                init_deposit.TransactionStatus = TransactionStatuses.Pending;
            }
            else
            {
                init_deposit.TransactionStatus = TransactionStatuses.Approved;
                bankAccount.BankAccountBalance += init_deposit.TransactionAmount;
                if (bankAccount.BankAccountType==BankAccountTypes.IRA)
                {
                    bankAccount.IRAContribution += init_deposit.TransactionAmount;
                }
            }

            if (init_deposit.TransactionStatus == TransactionStatuses.Pending)
            {
                EmailMessaging.SendEmail(bankAccount.User.Email, "Account Created", 
                    "Congratulations on creating your account! Note: Your initial deposit was over $5,000 and will be processed by an admin within 2-3 business days.");
                
                // Create notifaction
                Message m = new Message()
                {
                    Info = "Congratulations on creating your account! Note: Your initial deposit was over $5,000 and will be processed by an admin within 2-3 business days.",
                    Subject = "Account Created",
                    Date = DateTime.Today,
                    Sender = "Longhorn Bank",
                    Receiver = bankAccount.User.Email
                };
                m.Admins = new List<AppUser>();
                m.Admins.Add(bankAccount.User);
                _context.Add(m);
            }
            else
            {
                EmailMessaging.SendEmail(bankAccount.User.Email, "Account Created",
                    "Congratulations on creating your account! Your initial deposit was under $5,000 and has been processed.");


                // Create notifaction
                Message m = new Message()
                {
                    Info = "Congratulations on creating your account! Your initial deposit was under $5,000 and has been processed.",
                    Subject = "Account Created",
                    Date = DateTime.Today,
                    Sender = "Longhorn Bank",
                    Receiver = bankAccount.User.Email
                };
                m.Admins = new List<AppUser>();
                m.Admins.Add(bankAccount.User);
                _context.Add(m);
            }
            
            // create message for admins
            // extra func only needed for disputes
            Message message = new Message();


            if (init_deposit.TransactionStatus == TransactionStatuses.Pending)
            {
                message.Date = DateTime.Today;
                message.Subject = "Deposit that needs approval created by " + init_deposit.BankAccount.User.FullName;
                message.Info = "An initial deposit of over $5,000 by " + bankAccount.User.FullName + ". Please resolve the dispute.";
                message.Sender = "Deposit System";
                message.Receiver = "All";

                List<AppUser> AllAdmins = _userManager.GetUsersInRoleAsync("Admin").Result.ToList();

                var query = from a in AllAdmins where a.IsActive == true select a;

                List<AppUser> ActiveAdmins = query.ToList<AppUser>();

                message.Admins = ActiveAdmins;



            }

            _context.Add(message);
            await _userManager.UpdateAsync(bankAccount.User);
            _context.Add(bankAccount);
            _context.Add(init_deposit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            
        }






        // GET: BankAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id, string message)
        {
            if (id == null || _context.BankAccounts == null)
            {
                return NotFound();
            }

            var bankAccount = await _context.BankAccounts.FindAsync(id);
            if (bankAccount == null)
            {
                return NotFound();
            }
            
            ViewBag.Message = message;
            return View(bankAccount);
        }

        // POST: BankAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  BankAccount bankAccount)
        {
            if (id != bankAccount.BankAccountID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    BankAccount dbBA = await _context.BankAccounts.Include(ba => ba.User).FirstOrDefaultAsync(ba => ba.BankAccountID == bankAccount.BankAccountID);

                    if (dbBA.isActive == false || dbBA.User.IsActive == false)
                    {
                        return RedirectToAction("Edit", new { message = "This bank account has been disabled no changes can be made" });
                    }
                    // only change name
                    dbBA.BankAccountName = bankAccount.BankAccountName;
                    _context.Update(dbBA);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankAccountExists(bankAccount.BankAccountID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bankAccount);
        }

        // GET: BankAccounts/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BankAccounts == null)
            {
                return NotFound();
            }

            var bankAccount = await _context.BankAccounts
                .FirstOrDefaultAsync(m => m.BankAccountID == id);
            if (bankAccount == null)
            {
                return NotFound();
            }

            return View(bankAccount);
        }

        // POST: BankAccounts/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.BankAccounts == null)
        //    {
        //        return Problem("Entity set 'AppDbContext.BankAccounts'  is null.");
        //    }
        //    var bankAccount = await _context.BankAccounts.FindAsync(id);
        //    if (bankAccount != null)
        //    {
        //        _context.BankAccounts.Remove(bankAccount);
        //    }
            
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool BankAccountExists(int id)
        {
          return _context.BankAccounts.Any(e => e.BankAccountID == id);
        }
    }
}
