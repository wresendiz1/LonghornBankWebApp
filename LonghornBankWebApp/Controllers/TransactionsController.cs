using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;
using LonghornBankWebApp.Utilities;
using LonghornBankWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LonghornBankWebApp.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public TransactionsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Transactions
        public IActionResult Index(string SearchString, string selected, string order)
        {
            List<Transaction> all = new();
            if (User.IsInRole("Customer"))
            {
                all = _context.Transactions.Include(t => t.BankAccount).Include(t => t.StockPortfolio).Where(t => t.BankAccount.User.UserName == User.Identity.Name || t.StockPortfolio.User.UserName == User.Identity.Name).ToList();
            }

            // should employees see this too
            else if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                all = _context.Transactions.Include(t => t.BankAccount).Include(t => t.StockPortfolio).ToList();
            }



            ViewBag.total = all.Count;
            List<Transaction> queried = new();

            if (string.IsNullOrEmpty(SearchString) == false)
            {
                foreach (Transaction t in all)
                {
                    if (t.TransactionNotes != "" && t.TransactionNotes != null)
                    {
                        if (t.TransactionNotes.Contains(SearchString))
                        {
                            queried.Add(t);
                        }
                    }
                }
            }
            else
            {
                queried = all;
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
                if (order == "Ascending")
                {
                    queried = queried.OrderBy(x => x.TransactionAmount).ToList();

                }
                else
                {
                    queried = queried.OrderByDescending(x => x.TransactionAmount).ToList();
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

            List<SelectListItem> sortby = new()
            {
                new SelectListItem(text: "Transaction Number", value: "Number"),
                new SelectListItem(text: "Transaction Type", value: "Type"),
                new SelectListItem(text: "Transaction Notes", value: "Notes"),
                new SelectListItem(text: "Transaction Amount", value: "Amount")
            };

            ViewBag.sortby = sortby;


            List<SelectListItem> orderList = new()
            {
                new SelectListItem(text: "Descending", value: "Descending"),
                new SelectListItem(text: "Ascending", value: "Ascending")
            };

            ViewBag.order = orderList;

            ViewBag.queriedCount = queried.Count;

            _context.SaveChanges();
            return View(queried);

        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id, bool displayDispute = false)
        {
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.Include(t => t.BankAccount).Include(t => t.StockPortfolio).Include(t => t.Disputes).FirstOrDefaultAsync(m => m.TransactionID == id);
            if (transaction == null)
            {
                return NotFound();
            }

            ViewBag.displayDispute = displayDispute;

            TransactionDispute td = new()
            {
                transaction = transaction
            };

            Dispute d = new();
            td.dispute = d;

            ViewBag.disputable = true;
            foreach (Dispute disp in transaction.Disputes)
            {
                if (disp.DisputeStatus == DisputeStatus.Submitted)
                {
                    ViewBag.action = true;
                    ViewBag.current = disp.DisputeID;
                    ViewBag.disputable = false;
                }

                if (disp.DisputeStatus == DisputeStatus.Accepted)
                {
                    ViewBag.disputable = false;
                }
            }

            return View(td);

            //return View(transaction);
        }


        [HttpGet]
        public IActionResult CreateDeposit()
        {
            AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;
            if (u.IsActive == false)
            {
                return RedirectToAction("Index", "BankAccounts", new { message = "Your account is not active. Please contact an administrator to activate your account." });

            }
            Transaction deposit = new();
            ViewBag.SelectedBankAccount = 0;
            ViewBag.Accts = GetAllBankAccounts();
            return View(deposit);
        }

        public ActionResult DetailedSearch()
        {
            return View();
        }


        public ActionResult DisplaySearchResults(SearchViewModel svm)
        {
            var query = from r in _context.Transactions
                        select r;

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

            if (svm.DateLow != null && svm.DateHigh != null)
            {
                query = query.Where(r => r.TransactionDate >= svm.DateLow && r.TransactionDate <= svm.DateHigh);//ASK: does this include endpoints
            }

            List<Transaction> SelectedTransactions = query.ToList();

            ViewBag.AllTransactions = _context.Transactions.Count();
            ViewBag.SelectedTransactions = SelectedTransactions.Count;

            return View("Index", SelectedTransactions);

        }








        [HttpPost]
        public IActionResult CreateDeposit(Transaction transaction, uint SelectedAccountNum, bool? modify)
        {
            bool isPortfolio = false;
            StockPortfolio dbStockPortfolio = new();

            if (transaction.TransactionDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(transaction.TransactionDate), "Transaction must be today or a future date");
            }

            AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;

            if (u.IsActive == false)

            {
                return RedirectToAction("Index", "BankAccounts", new { message = "Your web account is not active. Please contact an administrator to activate your account." });

            }


            BankAccount dbBankAccount = _context.BankAccounts.Include(b => b.User).FirstOrDefault(b => b.BankAccountNumber == SelectedAccountNum && b.User.Email == User.Identity.Name);


            // for stock portfolios
            if (dbBankAccount == null)
            {
                isPortfolio = true;

                dbStockPortfolio = _context.StockPortfolios.Include(b => b.User).FirstOrDefault(b => b.PortfolioNumber == SelectedAccountNum && b.User.Email == User.Identity.Name);

                if (dbStockPortfolio.isActive == false)
                {
                    ModelState.AddModelError("NotActive", "This portfolio is not active. Please contact an administrator to activate your portfolio.");
                    ViewBag.SelectedBankAccount = 0;
                    ViewBag.Accts = GetAllBankAccounts();
                    return View(transaction);
                }
                transaction.StockPortfolio = dbStockPortfolio;

            }

            // for bank accounts
            else
            {

                if (dbBankAccount.isActive == false)
                {
                    ModelState.AddModelError("NotActive", "This account is not active. Please contact an administrator to activate your account.");
                    ViewBag.SelectedBankAccount = 0;
                    ViewBag.Accts = GetAllBankAccounts();
                    return View(transaction);
                }
                // for ira
                if (dbBankAccount.BankAccountType == BankAccountTypes.IRA)
                {

                    // no deposit if age > 70
                    if (DateTime.Now.Subtract(dbBankAccount.User.DOB).Days / 365 > 70) { ModelState.AddModelError(nameof(dbBankAccount.User.DOB), "Must be under 70 years old to contribute to IRA"); }

                    // can't contribute more than 5000 in a year
                    if (dbBankAccount.IRAContribution >= 5000) { ModelState.AddModelError("Maxed IRA Contribution", "You have already contributed $5000, the max contribution limit, to your IRA this year."); }

                    // they pressed modify
                    else if (modify == true)
                    {
                        // modify the amount and refresh the form
                        transaction.TransactionAmount = 5000 - dbBankAccount.IRAContribution;
                        ViewBag.Accts = GetAllBankAccounts();
                        return View(transaction);
                    }

                    // if they are submitting the form for the first time with an invalid amount
                    else if (dbBankAccount.IRAContribution + transaction.TransactionAmount > 5000)
                    {
                        ModelState.AddModelError("ContributionLimit", "Transaction amount exceeds max IRA contribution limit");
                        ViewBag.ExceedLimit = (dbBankAccount.IRAContribution + transaction.TransactionAmount) - 5000;
                    }

                    // update the contribution limit
                    dbBankAccount.IRAContribution += transaction.TransactionAmount;

                }


                transaction.BankAccount = dbBankAccount;

            }


            if (ModelState.IsValid == false)
            {
                ViewBag.Accts = GetAllBankAccounts();
                return View(transaction);
            }

            transaction.TransactionType = TransactionTypes.Deposit;
            transaction.TransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);

            if (transaction.TransactionAmount > 5000)
            {
                transaction.TransactionStatus = TransactionStatuses.Pending;

                // create message for admins
                // extra func only needed for disputes
                
                var fullname = transaction.BankAccount == null ? transaction.StockPortfolio.User.FullName : transaction.BankAccount.User.FullName;
                Message message = new()
                {
                    Date = DateTime.Today,
                    Subject = "Deposit that needs approval created by " + fullname,
                    Info = "A deposit of over $5,000 by " + fullname + ". Please resolve the dispute.",
                    Sender = "Deposit System",
                    Receiver = "All"
                };

                List<AppUser> AllAdmins = _userManager.GetUsersInRoleAsync("Admin").Result.ToList();

                var query = from a in AllAdmins where a.IsActive == true select a;

                List<AppUser> ActiveAdmins = query.ToList<AppUser>();

                message.Admins = ActiveAdmins;

                _context.Add(message);


                // notify user that their deposit is pending
                Message userMessage = new()
                {
                    Date = DateTime.Today,
                    Subject = "Deposit Pending",
                    Info = "Your deposit of $" + transaction.TransactionAmount + " is pending. An administrator will resolve the transaction.",
                    Sender = "Deposit System",
                    Receiver = u.Email,
                    Admins = new List<AppUser>()
                };
                userMessage.Admins.Add(u);
                _context.Add(userMessage);
            }
            else
            {
                if (transaction.TransactionDate > DateTime.Today)
                {
                    transaction.TransactionStatus = TransactionStatuses.Scheduled;
                }
                else
                {
                    transaction.TransactionStatus = TransactionStatuses.Approved;
                    if (isPortfolio)
                    {
                        dbStockPortfolio.CashValuePortion += transaction.TransactionAmount;
                        _context.Update(dbStockPortfolio);
                    }
                    else
                    {
                        dbBankAccount.BankAccountBalance += transaction.TransactionAmount;
                        _context.Update(dbBankAccount);
                    }
                }

            }

            _context.Add(transaction);

            _context.SaveChanges();

            if (isPortfolio == true)
            {
                return RedirectToAction(actionName: "Index", controllerName: "StockPortfolio");
            }
            else
            {
                return RedirectToAction(actionName: "Details", controllerName: "BankAccounts", routeValues: new { id = dbBankAccount.BankAccountID });
            }
        }











        [HttpGet]
        public IActionResult CreateWithdrawal()
        {
            AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;
            if (u.IsActive == false)
            {
                return RedirectToAction("Index", "BankAccounts", new { message = "Your account is not active. Please contact an administrator to activate your account." });

            }
            Transaction withdrawal = new()
            {
                TransactionType = TransactionTypes.Withdrawal
            };
            ViewBag.Accts = GetAllBankAccounts();
            ViewBag.DropDown = GetTransferDropDown();
            return View(withdrawal);
        }

        [HttpPost]
        public IActionResult CreateWithdrawal(Transaction transaction, uint? SelectedAccountNum, bool? include)
        {

            StockPortfolio dbStockPortfolio = _context.StockPortfolios.Include(b => b.User).Include(b => b.Transactions).FirstOrDefault(b => b.PortfolioNumber == SelectedAccountNum && b.User.Email == User.Identity.Name);
            bool isPortfolio = false;
            BankAccount dbBankAccount = _context.BankAccounts.Include(b => b.User).Include(b => b.Transactions).FirstOrDefault(b => b.BankAccountNumber == SelectedAccountNum && b.User.Email == User.Identity.Name);

            transaction.TransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);
            transaction.TransactionType = TransactionTypes.Withdrawal;

            AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;

            if (u.IsActive == false)

            {
                return RedirectToAction("Index", "BankAccounts", new { message = "Your web account is not active. Please contact an administrator to activate your account." });

            }


            // for stock portfolio
            if (dbBankAccount == null)
            {
                if (dbStockPortfolio.isActive == false)
                {
                    ModelState.AddModelError("NotActive", "This portfolio is not active. Please contact an administrator to activate your portfolio.");
                    ViewBag.Accts = GetAllBankAccounts();
                    ViewBag.DropDown = GetTransferDropDown();
                    return View(transaction);
                }



                isPortfolio = true;
                (dbStockPortfolio, transaction) = PortfolioWithdrawal(dbStockPortfolio, transaction);
            }

            // for bank account
            else
            {

                if (dbBankAccount.isActive == false)
                {
                    ModelState.AddModelError("NotActive", "This account is not active. Please contact an administrator to activate your account.");
                    ViewBag.Accts = GetAllBankAccounts();
                    ViewBag.DropDown = GetTransferDropDown();
                    return View(transaction);
                }

                transaction.BankAccount = dbBankAccount;

                if (dbBankAccount.BankAccountType == BankAccountTypes.IRA && dbBankAccount.IRAQualified == false)
                {
                    (dbBankAccount, transaction) = IRAWithdrawal(dbBankAccount, transaction, include, fromTransfer: false);
                }

                // for non unqualified IRA
                else if (transaction.TransactionAmount > dbBankAccount.BankAccountBalance)
                {
                    ModelState.AddModelError(nameof(transaction.TransactionAmount), "Withdrawal amount exceeds current bank account balance");

                }
            }


            // general error check 
            if (transaction.TransactionDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(transaction.TransactionDate), "Transaction must be today or a future date");
            }

            if (ModelState.IsValid == false)
            {
                ViewBag.Accts = GetAllBankAccounts();
                ViewBag.DropDown = GetTransferDropDown();
                return View(transaction);
            }

            //happy path, book transaction
            if (transaction.TransactionDate > DateTime.Today)
            {
                transaction.TransactionStatus = TransactionStatuses.Scheduled;
                transaction.TransactionAmount = -1 * transaction.TransactionAmount;
            }
            else
            {
                transaction.TransactionStatus = TransactionStatuses.Approved;
                transaction.TransactionAmount = -1 * transaction.TransactionAmount;
                if (isPortfolio) { dbStockPortfolio.CashValuePortion += transaction.TransactionAmount; }
                else { dbBankAccount.BankAccountBalance += transaction.TransactionAmount; }
            }


            _context.Add(transaction);

            if (isPortfolio)
            {
                _context.Update(dbStockPortfolio);
                _context.SaveChanges();
                return RedirectToAction(controllerName: "StockPortfolio", actionName: "Index");
            }

            else
            {
                _context.Update(dbBankAccount);
                _context.SaveChanges();
                return RedirectToAction(controllerName: "BankAccounts", actionName: "Details", routeValues: new { id = dbBankAccount.BankAccountID });
            }

        }


        public (StockPortfolio, Transaction) PortfolioWithdrawal(StockPortfolio dbStockPortfolio, Transaction transaction)
        {
            transaction.StockPortfolio = dbStockPortfolio;

            // check to make sure withdrawal does not exceed balance
            if (transaction.TransactionAmount > dbStockPortfolio.CashValuePortion)
            {
                ModelState.AddModelError(nameof(transaction.TransactionAmount), "Withdrawal amount exceeds current stock portfolio cash balance");
            }

            return (dbStockPortfolio, transaction);

        }

        public (BankAccount, Transaction) IRAWithdrawal(BankAccount dbBankAccount, Transaction transaction, bool? include, bool? fromTransfer)
        {
            if (include == null)
            {
                ModelState.AddModelError("IRA Unqualified", "Withdrawals from unqualified IRA distributions have a max limit of $3,000 and a $30 fee.");
            }


            // else they made selection of include or add
            else
            {
                if (include == true)
                {
                    // stupid include
                    if (transaction.TransactionAmount <= 30)
                    {
                        ModelState.AddModelError(nameof(transaction.TransactionAmount), "Cannot include $30 fee in transaction amounts of $30 or less");
                    }
                    else
                    {
                        transaction.TransactionAmount -= 30;
                    }
                }

                // try to withdraw more than limit
                if (transaction.TransactionAmount > 3000)
                {
                    ModelState.AddModelError(nameof(transaction.TransactionAmount), "Withdrawals from unqualified IRA distributions have a max limit of $3,000.");
                }

                // try to withdraw more than balance
                if (transaction.TransactionAmount + 30 > dbBankAccount.BankAccountBalance)
                {
                    ModelState.AddModelError(nameof(transaction.TransactionAmount), "Withdrawal amount exceeds current bank account balance");
                }


                Transaction fee = new()
                {
                    TransactionType = TransactionTypes.Fee,
                    BankAccount = transaction.BankAccount,
                    TransactionAmount = -30,
                    TransactionDate = transaction.TransactionDate,
                    TransactionNumber = GenerateNextNum.GetNextTransactionNum(_context) + 1,
                    TransactionNotes = "Fee for transaction " + transaction.TransactionNumber.ToString(),
                    TransactionStatus = TransactionStatuses.Approved
                };

                dbBankAccount.BankAccountBalance += fee.TransactionAmount;

                if (fromTransfer == true) { fee.TransactionNumber += 1; }

                _context.Add(fee);

            }


            return (dbBankAccount, transaction);

        }



        [HttpGet]
        public IActionResult CreateTransfer()
        {
            AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;
            if (u.IsActive == false)
            {
                return RedirectToAction("Index", "BankAccounts", new { message = "Your account is not active. Please contact an administrator to activate your account." });

            }
            Transaction transfer = new()
            {
                TransactionType = TransactionTypes.Transfer
            };

            ViewBag.DropDown = GetTransferDropDown();
            return View(transfer);
        }

        [HttpPost]
        public IActionResult CreateTransfer(Transaction deposit, uint SelectedFromAccount, uint SelectedToAccount, bool? include, bool? modify)
        {
            if (SelectedFromAccount == SelectedToAccount)
            {
                ModelState.AddModelError(nameof(deposit.BankAccount), "Cannot transfer funds to and from the same account");
                ViewBag.DropDown = GetTransferDropDown();
                return View(deposit);
            }

            AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;

            if (u.IsActive == false)

            {
                return RedirectToAction("Index", "BankAccounts", new { message = "Your web account is not active. Please contact an administrator to activate your account." });

            }


            StockPortfolio dbStockPortfolioTo = _context.StockPortfolios.Include(sp => sp.User).FirstOrDefault(sp => sp.PortfolioNumber == SelectedToAccount && sp.User.Email == User.Identity.Name);
            BankAccount dbBankAccountTo = _context.BankAccounts.Include(ba => ba.User).FirstOrDefault(ba => ba.BankAccountNumber == SelectedToAccount && ba.User.Email == User.Identity.Name);

            bool stockFrom = false;
            bool stockTo = false;

            if (dbBankAccountTo == null)
            {
                if (dbStockPortfolioTo.isActive == false)
                {
                    ModelState.AddModelError("NotActive", "Cannot transfer funds to an inactive portfolio account");
                    ViewBag.DropDown = GetTransferDropDown();
                    return View(deposit);
                }


                (dbStockPortfolioTo, deposit) = CreateDepositTransferPortfolio(deposit, dbStockPortfolioTo);
                stockTo = true;
            }
            else
            {
                if (dbBankAccountTo.isActive == false)

                {
                    ModelState.AddModelError("NotActive", "Cannot transfer funds to an inactive bank account");
                    ViewBag.DropDown = GetTransferDropDown();
                    return View(deposit);
                }


                // refresh view with modified amount
                if (modify == true)
                {
                    deposit.TransactionAmount = 5000 - dbBankAccountTo.IRAContribution;
                    ViewBag.DropDown = GetTransferDropDown();
                    return View(deposit);
                }

                (dbBankAccountTo, deposit) = CreateDepositTransferAccount(deposit, dbBankAccountTo, modify);
            }


            Transaction withdrawal = new()
            {
                TransactionAmount = deposit.TransactionAmount,
                TransactionDate = deposit.TransactionDate
            };

            StockPortfolio dbStockPortfolioFrom = _context.StockPortfolios.FirstOrDefault(sp => sp.PortfolioNumber == SelectedFromAccount && sp.User.Email == User.Identity.Name);
            BankAccount dbBankAccountFrom = _context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == SelectedFromAccount && ba.User.Email == User.Identity.Name);

            if (dbBankAccountFrom == null)
            {
                if (dbStockPortfolioFrom.isActive == false)
                {
                    ModelState.AddModelError("NotActive", "Cannot transfer funds from an inactive portfolio account");
                    ViewBag.DropDown = GetTransferDropDown();
                    return View(deposit);
                }


                (dbStockPortfolioFrom, withdrawal) = CreateWithdrawalTransferPortfolio(withdrawal, dbStockPortfolioFrom);
                stockFrom = true;
            }
            else
            {
                if (dbBankAccountFrom.isActive == false)
                {
                    ModelState.AddModelError("NotActive", "Cannot transfer funds from an inactive bank account");
                    ViewBag.DropDown = GetTransferDropDown();
                    return View(deposit);
                }

                (dbBankAccountFrom, withdrawal) = CreateWithdrawalTransferAccount(withdrawal, dbBankAccountFrom, include);
            }

            if (ModelState.IsValid == false)
            {

                //HARDCODED FIX
                if (ViewData.ModelState.Keys.Contains("IRA Unqualified"))
                {
                    dbBankAccountTo.BankAccountBalance -= deposit.TransactionAmount;
                }
                ViewBag.DropDown = GetTransferDropDown();
                return View(deposit);
            }

            deposit.TransactionType = TransactionTypes.Transfer;
            withdrawal.TransactionType = TransactionTypes.Transfer;


            //return RedirectToAction("TransferConfirm", routeValues: new { stockTo=stockTo, stockFrom= stockFrom, dbStockPortfolioTo=dbStockPortfolioTo, dbStockPortfolioFrom=dbStockPortfolioFrom, dbBankAccountTo= dbBankAccountTo, dbBankAccountFrom= dbBankAccountFrom, withdrawal= withdrawal, deposit= deposit });
            if (stockTo == true && stockFrom == false)
            {


                // deposit to portfolio and withdraw from bank account
                withdrawal.TransactionNotes = "Transfer to " + dbStockPortfolioTo.PortfolioNumber.ToString() + " " + dbStockPortfolioTo.PortfolioName;
                deposit.TransactionNotes = "Transfer from " + dbBankAccountFrom.BankAccountNumber.ToString() + " " + dbBankAccountFrom.BankAccountName;

                _context.Update(dbStockPortfolioTo);
                _context.Update(dbBankAccountFrom);
            }
            if (stockFrom == true && stockTo == false)
            {


                // deposit to bank account and withdraw from portfolio
                withdrawal.TransactionNotes = "Transfer to " + dbBankAccountTo.BankAccountNumber.ToString() + " " + dbBankAccountTo.BankAccountName;
                deposit.TransactionNotes = "Transfer from " + dbStockPortfolioFrom.PortfolioNumber.ToString() + " " + dbStockPortfolioFrom.PortfolioName;



                _context.Update(dbStockPortfolioFrom);
                _context.Update(dbBankAccountTo);
            }
            if (stockFrom == false && stockTo == false)
            {


                deposit.TransactionNotes = "Transfer from " + dbBankAccountFrom.BankAccountNumber.ToString() + " " + dbBankAccountFrom.BankAccountName;
                withdrawal.TransactionNotes = "Transfer to " + dbBankAccountTo.BankAccountNumber.ToString() + " " + dbBankAccountTo.BankAccountName;

                _context.Update(dbBankAccountTo);
                _context.Update(dbBankAccountFrom);
            }

            if (deposit.TransactionNumber == withdrawal.TransactionNumber) { deposit.TransactionNumber += 1; }

            _context.Add(deposit);
            _context.Add(withdrawal);
            _context.SaveChanges();

            return RedirectToAction(actionName: "Index", controllerName: "BankAccounts");

        }


        //public IActionResult TransferConfirm1(Transaction deposit, Transaction withdrawal, StockPortfolio dbStockPortfolioTo, BankAccount dbBankAccountFrom)
        //{
        //    _context.Update(dbStockPortfolioTo);
        //    _context.Update(dbBankAccountFrom);
        //    _context.Add(deposit);
        //    _context.Add(withdrawal);
        //    if (deposit.TransactionNumber == withdrawal.TransactionNumber) { deposit.TransactionNumber += 1; }

        //    ViewBag.destination = dbStockPortfolioTo;
        //    ViewBag.deposit = deposit;
        //    ViewBag.withdrawal = withdrawal;
        //    ViewBag.dbStockPortfolioTo = dbStockPortfolioTo;
        //    ViewBag.dbBankAccountFrom = dbBankAccountFrom;
        //    return View("TransferConfirm", withdrawal);
        //}

        //public IActionResult TransferConfirm2(Transaction deposit, Transaction withdrawal, StockPortfolio dbStockPortfolioFrom, BankAccount dbBankAccountTo)
        //{
        //    _context.Update(dbStockPortfolioFrom);
        //    _context.Update(dbBankAccountTo);
        //    _context.Add(deposit);
        //    _context.Add(withdrawal);
        //    if (deposit.TransactionNumber == withdrawal.TransactionNumber) { deposit.TransactionNumber += 1; }

        //    ViewBag.destination = dbBankAccountTo;
        //    ViewBag.deposit = deposit;
        //    ViewBag.withdrawal = withdrawal;
        //    ViewBag.dbStockPortfolioFrom = dbStockPortfolioFrom;
        //    ViewBag.dbBankAccountTo = dbBankAccountTo;
        //    return View("TransferConfirm", withdrawal);
        //}

        //public IActionResult TransferConfirm3(Transaction deposit, Transaction withdrawal, BankAccount dbBankAccountTo, BankAccount dbBankAccountFrom)
        //{
        //    _context.Update(dbBankAccountTo);
        //    _context.Update(dbBankAccountFrom);
        //    _context.Add(deposit);
        //    _context.Add(withdrawal);
        //    if (deposit.TransactionNumber == withdrawal.TransactionNumber) { deposit.TransactionNumber += 1; }

        //    ViewBag.destination = dbBankAccountTo;
        //    ViewBag.deposit = deposit;
        //    ViewBag.withdrawal = withdrawal;
        //    ViewBag.dbBankAccountFrom = dbBankAccountFrom;
        //    ViewBag.dbBankAccountTo = dbBankAccountTo;
        //    return View("TransferConfirm", withdrawal);
        //}


        public (BankAccount, Transaction) CreateWithdrawalTransferAccount(Transaction transaction, BankAccount dbBankAccount, bool? include)
        {
            // set known amounts
            transaction.TransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);
            transaction.TransactionType = TransactionTypes.Transfer;
            transaction.BankAccount = dbBankAccount;

            // for unqualified IRA
            if (dbBankAccount.BankAccountType == BankAccountTypes.IRA && dbBankAccount.IRAQualified == false)
            {

                (dbBankAccount, transaction) = IRAWithdrawal(dbBankAccount, transaction, include, fromTransfer: true);
            }

            // for non unqualified IRA
            else if (transaction.TransactionAmount > dbBankAccount.BankAccountBalance)
            {
                ModelState.AddModelError(nameof(transaction.TransactionAmount), "Withdrawal amount exceeds current bank account balance");
            }

            // general error check 
            if (transaction.TransactionDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(transaction.TransactionDate), "Transaction must be today or a future date");
            }

            if (transaction.TransactionDate > DateTime.Today)
            {
                transaction.TransactionStatus = TransactionStatuses.Scheduled;
                transaction.TransactionAmount = -1 * transaction.TransactionAmount;
            }
            else
            {
                transaction.TransactionStatus = TransactionStatuses.Approved;
                transaction.TransactionAmount = -1 * transaction.TransactionAmount;
                if (ModelState.IsValid) { dbBankAccount.BankAccountBalance += transaction.TransactionAmount; }

            }

            return (dbBankAccount, transaction);

        }

        public (StockPortfolio, Transaction) CreateWithdrawalTransferPortfolio(Transaction transaction, StockPortfolio dbStockPortfolio)
        {

            // set known amounts
            transaction.TransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);
            transaction.TransactionType = TransactionTypes.Withdrawal;
            transaction.StockPortfolio = dbStockPortfolio;

            // general error check 
            if (transaction.TransactionDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(transaction.TransactionDate), "Transaction must be today or a future date");
            }
            if (transaction.TransactionAmount > dbStockPortfolio.CashValuePortion)
            {
                ModelState.AddModelError(nameof(transaction.TransactionAmount), "Withdrawal amount exceeds current stock portfolio cash balance");
            }

            if (transaction.TransactionDate > DateTime.Today)
            {
                transaction.TransactionStatus = TransactionStatuses.Scheduled;
                transaction.TransactionAmount = -1 * transaction.TransactionAmount;
            }
            else
            {
                transaction.TransactionStatus = TransactionStatuses.Approved;
                transaction.TransactionAmount = -1 * transaction.TransactionAmount;
                dbStockPortfolio.CashValuePortion += transaction.TransactionAmount;
            }

            return (dbStockPortfolio, transaction);

        }

        [HttpPost]
        public (BankAccount, Transaction) CreateDepositTransferAccount(Transaction transaction, BankAccount dbBankAccount, bool? modify)
        {

            if (transaction.TransactionDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(transaction.TransactionDate), "Transaction must be today or a future date");
            }

            transaction.BankAccount = dbBankAccount;
            transaction.TransactionType = TransactionTypes.Deposit;
            transaction.TransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);

            // for ira
            if (dbBankAccount.BankAccountType == BankAccountTypes.IRA)
            {

                // no deposit if age > 70
                if (DateTime.Now.Subtract(dbBankAccount.User.DOB).Days / 365 > 70) { ModelState.AddModelError(nameof(dbBankAccount.User.DOB), "Must be under 70 years old to contribute to IRA"); }

                // can't contribute more than 5000 in a year
                if (dbBankAccount.IRAContribution >= 5000) { ModelState.AddModelError("Maxed IRA Contribution", "You have already contributed $5000, the max contribution limit, to your IRA this year."); }

                // if they are submitting the form for the first time with an invalid amount
                else if (dbBankAccount.IRAContribution + transaction.TransactionAmount > 5000)
                {
                    ModelState.AddModelError("ContributionLimit", "Transaction amount exceeds max IRA contribution limit");
                    ViewBag.ExceedLimit = (dbBankAccount.IRAContribution + transaction.TransactionAmount) - 5000;
                }

                if (ModelState.IsValid) { dbBankAccount.IRAContribution += transaction.TransactionAmount; }

            }

            if (transaction.TransactionAmount > 5000)
            {
                transaction.TransactionStatus = TransactionStatuses.Pending;
            }

            else
            {
                if (transaction.TransactionDate > DateTime.Today)
                {
                    transaction.TransactionStatus = TransactionStatuses.Scheduled;
                }
                else
                {
                    transaction.TransactionStatus = TransactionStatuses.Approved;
                    if (ModelState.IsValid) { dbBankAccount.BankAccountBalance += transaction.TransactionAmount; }
                }
            }

            return (dbBankAccount, transaction);
        }

        // OVERLOAD FOR TRANSFER METHOD
        public (StockPortfolio, Transaction) CreateDepositTransferPortfolio(Transaction transaction, StockPortfolio dbStockPortfolio)
        {

            if (transaction.TransactionDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(transaction.TransactionDate), "Transaction must be today or a future date");
            }

            transaction.StockPortfolio = dbStockPortfolio;
            transaction.TransactionType = TransactionTypes.Deposit;
            transaction.TransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);

            if (transaction.TransactionAmount > 5000)
            {
                transaction.TransactionStatus = TransactionStatuses.Pending;
            }
            else
            {
                if (transaction.TransactionDate > DateTime.Today)
                {
                    transaction.TransactionStatus = TransactionStatuses.Scheduled;
                }
                else
                {
                    transaction.TransactionStatus = TransactionStatuses.Approved;
                    dbStockPortfolio.CashValuePortion += transaction.TransactionAmount;
                }
            }

            return (dbStockPortfolio, transaction);
        }

        private List<SelectListItem> GetTransferDropDown()
        {
            List<BankAccount> bankAccountsList = _context.BankAccounts.Where(r => r.User.UserName == User.Identity.Name).Include(b => b.User).ToList();
            List<SelectListItem> items = new();


            foreach (BankAccount bankAccount in bankAccountsList)
            {
               
                bankAccount.TransferDropDown = bankAccount.BankAccountName + "     " + "XXXXXX" + bankAccount.BankAccountNumber.ToString()[^4..] + "     $" + bankAccount.BankAccountBalance;
                items.Add(new SelectListItem(text: bankAccount.TransferDropDown, value: bankAccount.BankAccountNumber.ToString()));


            }

            List<StockPortfolio> holder = _context.StockPortfolios.Where(r => r.User.UserName == User.Identity.Name).ToList();
            if (holder.Count > 0)
            {
                holder[0].TransferDropDown = holder[0].PortfolioName + "     " + "XXXXXX" + holder[0].PortfolioNumber.ToString()[^4..] + "     $" + holder[0].CashValuePortion;
                items.Add(new SelectListItem(text: holder[0].TransferDropDown, value: holder[0].PortfolioNumber.ToString()));
            }

            return items;

        }

        // helper method to get select list of all bank accounts
        private List<SelectListItem> GetAllBankAccounts()
        {
            List<BankAccount> bankAccountsList = _context.BankAccounts.Where(r => r.User.UserName == User.Identity.Name).Include(b => b.User).ToList();


            List<SelectListItem> items = new();

            // update whether or not IRA accounts are qualified
            foreach (BankAccount bankAccount in bankAccountsList.OrderBy(m => m.BankAccountNumber))
            {
                if (bankAccount.BankAccountType == BankAccountTypes.IRA)
                {
                    // if the customer is older than 65 then their distribution is unqualified
                    if ((DateTime.Now.Subtract(bankAccount.User.DOB).Days / 365) > 65)
                    {
                        bankAccount.IRAQualified = true;
                    }
                    else
                    {
                        bankAccount.IRAQualified = false;
                    }
                }

                items.Add(new SelectListItem(text: bankAccount.BankAccountName, value: bankAccount.BankAccountNumber.ToString()));
            }

            List<StockPortfolio> holder = _context.StockPortfolios.Where(r => r.User.UserName == User.Identity.Name).ToList();
            if (holder.Count > 0)
            {
                items.Add(new SelectListItem(text: holder[0].PortfolioName, value: holder[0].PortfolioNumber.ToString()));
            }

            //SelectList bankAccountsSelList = new SelectList(bankAccountsList.OrderBy(m => m.BankAccountID), "BankAccountID", "BankAccountName");

            SelectList bankAccountsSelList = new(items);

            return items;
        }

        public IActionResult ModifyAmountTransfer(decimal amount, int bankAccountID)
        {
            // CONSIDER: add error checks             
            Transaction transaction = new();
            BankAccount dbBankAccount = _context.BankAccounts.Include(b => b.User).FirstOrDefault(b => b.BankAccountID == bankAccountID);
            transaction.BankAccount = dbBankAccount;

            // extra check to make sure modify amounts only for those that exceed
            if (dbBankAccount.IRAContribution + amount > 5000)
            {
                transaction.TransactionAmount = 5000 - dbBankAccount.IRAContribution;
            }

            ViewBag.Accts = GetAllBankAccounts();

            // ASK: how do I return the view without refreshing the selected values?

            return View("CreateDeposit", transaction);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionID,TransactionNumber,TransactionAmount,TransactionType,TransactionDate,TransactionNotes,TransactionStatus")] Transaction transaction)
        {
            if (id != transaction.TransactionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionID))
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
            return View(transaction);
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionID == id);
        }

        // GET: Transactions/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(m => m.TransactionID == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Transactions == null)
        //    {
        //        return Problem("Entity set 'AppDbContext.Transactions'  is null.");
        //    }
        //    var transaction = await _context.Transactions.FindAsync(id);
        //    if (transaction != null)
        //    {
        //        _context.Transactions.Remove(transaction);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}




    }
}
