using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;
using System.Collections;
using LonghornBankWebApp.Models.ViewModels;
using IronXL.Styles;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;
using LonghornBankWebApp.Utilities;
using Microsoft.AspNetCore.Identity;

namespace LonghornBankWebApp.Controllers
{
    [Authorize]
    public class StockPortfolioController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public StockPortfolioController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        private ArrayList GetOrderedQuery(ArrayList queried, String selected, String order)
        {
            List<Int32> ordered = new List<Int32>();

            if (selected == "Number")
            {
                List<Int32> number_ordered = new List<Int32>();

                foreach (var item in queried)
                {
                    if (item is Transaction)
                    {
                        number_ordered.Add(((Transaction)item).TransactionNumber);
                    }

                    if (item is StockTransaction)
                    {
                        number_ordered.Add(((StockTransaction)item).StockTransactionNumber);
                    }
                }

                if (order == "Descending")
                {
                    number_ordered.Sort();
                    number_ordered.Reverse();
                }
                else
                {
                    number_ordered.Sort();
                }

                ordered = number_ordered;

            }

            else if (selected == "Amount")
            {
                // for storing
                IDictionary<Int32, decimal> amount_ordered = new Dictionary<Int32, decimal>();

                foreach (var item in queried)
                {
                    if (item is Transaction)
                    {
                        amount_ordered.Add(((Transaction)item).TransactionNumber, ((Transaction)item).TransactionAmount);
                    }

                    if (item is StockTransaction)
                    {
                        amount_ordered.Add(((StockTransaction)item).StockTransactionNumber, ((StockTransaction)item).TotalPrice);
                    }
                }

                if (order == "Descending")
                {
                    amount_ordered.OrderByDescending(x => x.Value);
                }

                else
                {
                    amount_ordered.OrderBy(x => x.Value);
                }

                ordered = amount_ordered.Keys.ToList();
            }

            else if (selected == "Type")
            {
                IDictionary<Int32, string> type_ordered = new Dictionary<Int32, string>();

                foreach (var item in queried)
                {
                    if (item is Transaction)
                    {
                        type_ordered.Add(((Transaction)item).TransactionNumber, ((Transaction)item).TransactionType.ToString());
                    }

                    if (item is StockTransaction)
                    {
                        type_ordered.Add(((StockTransaction)item).StockTransactionNumber, ((StockTransaction)item).StockTransactionType.ToString());
                    }
                }

                if (order == "Descending")
                {
                    type_ordered.OrderByDescending(x => x.Value);
                }

                else
                {
                    type_ordered.OrderBy(x => x.Value);
                }


                ordered = type_ordered.Keys.ToList();

            }

            else if (selected == "Notes")
            {
                IDictionary<Int32, string> notes_ordered = new Dictionary<Int32, string>();
                foreach (var item in queried)
                {
                    if (item is Transaction)
                    {
                        notes_ordered.Add(((Transaction)item).TransactionNumber, ((Transaction)item).TransactionNotes);
                    }

                    if (item is StockTransaction)
                    {
                        notes_ordered.Add(((StockTransaction)item).StockTransactionNumber, ((StockTransaction)item).StockTransactionNotes);
                    }
                }

                if (order == "Descending")
                {
                    notes_ordered.OrderByDescending(x => x.Value);
                }
                else
                {
                    notes_ordered.OrderBy(x => x.Value);
                }

                ordered = notes_ordered.Keys.ToList();
            }

            else if (selected == "Date")
            {
                IDictionary<Int32, DateTime> date_ordered = new Dictionary<Int32, DateTime>();
                foreach (var item in queried)
                {
                    if (item is Transaction)
                    {
                        date_ordered.Add(((Transaction)item).TransactionNumber, ((Transaction)item).TransactionDate);
                    }

                    if (item is StockTransaction)
                    {
                        date_ordered.Add(((StockTransaction)item).StockTransactionNumber, ((StockTransaction)item).TransactionDate);
                    }
                }

                if (order == "Descending")
                {
                    date_ordered.OrderByDescending(x => x.Value);
                }
                else
                {
                    date_ordered.OrderBy(x => x.Value);
                }

                ordered = date_ordered.Keys.ToList();
            }
            



            else
            {
                foreach (var item in queried)
                {
                    if (item is Transaction)
                    {
                        ordered.Add(((Transaction)item).TransactionNumber);
                    }

                    if (item is StockTransaction)
                    {
                        ordered.Add(((StockTransaction)item).StockTransactionNumber);
                    }
                }
            }


            Transaction tadd = new Transaction();
            StockTransaction stadd = new StockTransaction();

            var queried_ordered = new ArrayList();

            foreach (Int32 tnum in ordered)
            {
                tadd = null;
                stadd = null;

                tadd = _context.Transactions.FirstOrDefault(x => x.TransactionNumber == tnum);
                if (tadd == null)
                {
                    stadd = _context.StockTransactions.FirstOrDefault(x => x.StockTransactionNumber == tnum);
                    queried_ordered.Add(stadd);
                }
                else
                {
                    queried_ordered.Add(tadd);
                }
            }
            
            return queried_ordered;
        }

        // GET: StockPortfolio
        public static (StockPortfolio, IDictionary<Stock, Int32>) UpdatePortfolio(StockPortfolio stonks, AppDbContext _context)
        {
            IDictionary<Int32, Int32> holdings = GetHoldings(stonks);
            decimal sum = 0;
            int ordinary = 0;
            bool index = false;
            bool mutual = false;
            Stock temp = new Stock();
            

            IDictionary<Stock, Int32> stock_holdings = new Dictionary<Stock, Int32>();

            foreach (KeyValuePair<Int32, Int32> holding in holdings)
            {
                sum += _context.Stocks.Find(holding.Key).CurrentPrice * holding.Value;
                temp = _context.Stocks.Include(s=>s.StockType).FirstOrDefault(s=> s.StockID == holding.Key);

                if (temp.StockType.StockTypeName == "Ordinary")
                {
                    ordinary++;
                }
                if (temp.StockType.StockTypeName == "Index Fund")
                {
                    index = true;
                }
                if (temp.StockType.StockTypeName == "Mutual Fund")
                {
                    mutual = true;
                }

                stock_holdings.Add(temp, holding.Value);
            }

            stonks.PortfolioValue = stonks.CashValuePortion + sum;
            

            if (index && mutual && ordinary >= 2)
            {
                stonks.IsBalanced = true;
            }
            else
            {
                stonks.IsBalanced = false;
            }

            stonks.TotalFees = 0;
            stonks.TotalGains = 0;
            foreach (StockTransaction stn in stonks.StockTransactions)
            {
                if (stn.StockTransactionType == StockTransactionTypes.Fee)
                {
                    stonks.TotalFees += stn.TotalPrice;
                }
                if (stn.StockTransactionType == StockTransactionTypes.Deposit)
                {
                    stonks.TotalGains += stn.GainLoss;
                }
            }

            foreach (Transaction t in stonks.Transactions)
            {
                if(t.TransactionType == TransactionTypes.Fee)
                {
                    stonks.TotalFees += t.TransactionAmount;
                }
            }


            _context.Update(stonks);
            _context.SaveChanges();

            return (stonks, stock_holdings);
        }
        
        public IActionResult Index(string SearchString, string selected, string order, int id, string message, string bonus)
        {
            
            // get the stock portfolio from the database
            StockPortfolio stonks = new StockPortfolio();

            ViewBag.Message = message;
            ViewBag.bonus = bonus;

            if(User.IsInRole("Customer"))
            {
                stonks = _context.StockPortfolios.Include(s => s.Transactions).Include(sp => sp.StockTransactions).ThenInclude(s => s.Stock).FirstOrDefault(r => r.User.UserName == User.Identity.Name);
            }
            
            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                stonks = _context.StockPortfolios.Include(s => s.Transactions).Include(sp => sp.StockTransactions).ThenInclude(s => s.Stock).FirstOrDefault(r => r.StockPortfolioID == id);
            }

            if (stonks == null && User.IsInRole("Customer"))
            {
                return RedirectToAction("Create");
            }

            IDictionary<Stock, Int32> holdings = new Dictionary<Stock, Int32>();

            (stonks, holdings) = UpdatePortfolio(stonks, _context);

            ViewBag.holdings = holdings;

            var queried = new ArrayList();

            List<Transaction> t = stonks.Transactions;
            List<StockTransaction> st = stonks.StockTransactions;

            // if the user actually entered something to search by
            if (SearchString != "" && SearchString != null)
            {
                foreach (Transaction transaction in t)
                {
                    if (transaction.TransactionNotes != "" && transaction.TransactionNotes != null)
                    {
                        if (transaction.TransactionNotes.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                        {
                            queried.Add(transaction);
                        }
                    }
                }

                foreach (StockTransaction transaction in st)
                {
                    if (transaction.StockTransactionNotes != "" && transaction.StockTransactionNotes != null)
                    {
                        if (transaction.StockTransactionNotes.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                        {
                            queried.Add(transaction);
                        }
                    }
                }
            }

            else
            {
                queried.AddRange(t);
                queried.AddRange(st);
            }

            var queried_ordered = new ArrayList();


            if (selected == null && order == null)
            {
                queried_ordered = GetOrderedQuery(queried, "Number", "Descending");
            }
            else
            {
                queried_ordered = GetOrderedQuery(queried, selected, order);
            }

            ViewBag.query = queried_ordered;
            ViewBag.sortby = getSortBy();
            ViewBag.order = GetOrder();
            ViewBag.total = t.Count() + st.Count();


            decimal withScheduled = 0;
            decimal withPending = 0;
            foreach(Transaction tr in stonks.Transactions)
            {
                if (tr.TransactionStatus == TransactionStatuses.Pending)
                {
                    withPending += tr.TransactionAmount;
                }
                if (tr.TransactionStatus == TransactionStatuses.Scheduled)
                {
                    withScheduled += tr.TransactionAmount;
                }
            }

            withScheduled += stonks.CashValuePortion;
            withPending += stonks.CashValuePortion;

            ViewBag.withPending = withPending;
            ViewBag.withScheduled = withScheduled;

            ViewBag.holdingsvalue = stonks.PortfolioValue - stonks.CashValuePortion;

            return View(stonks);


        }


        public IActionResult CalcBonus(Int32 id)
        {
            StockPortfolio stonk = _context.StockPortfolios.Include(s => s.Transactions).Include(sp => sp.StockTransactions).ThenInclude(s => s.Stock).FirstOrDefault(r => r.StockPortfolioID == id);
            decimal bonus = (stonk.PortfolioValue-stonk.CashValuePortion) * 0.1m;

            //ViewBag.bonus = bonus;
            //return View(stonk);
            
            String bonus1 = "You have earned a bonus of $" + bonus + " !";
            return RedirectToAction("Index", new { bonus = bonus1 });

        }

        public IActionResult DetailedSearch(Int32 id)
        {
            SearchViewModel svm = new SearchViewModel();

            List<SelectListItem> sortby = new List<SelectListItem>();

            ViewBag.sortby = getSortBy();

            ViewBag.order = GetOrder();

            ViewBag.id = id;
            return View(svm);
        }

        public IActionResult DisplaySearchResults(SearchViewModel svm, String? selected, String? order, Int32 id)
        {

            List<StockPortfolio> stonks = new List<StockPortfolio>();
            if (User.IsInRole("Customer"))
            {
                stonks=_context.StockPortfolios.Include(s => s.Transactions).Include(sp => sp.StockTransactions).ThenInclude(s => s.Stock).Where(r => r.User.UserName == User.Identity.Name).ToList();
            }
            if(User.IsInRole("Admin"))
            {
                stonks = _context.StockPortfolios.Include(s => s.Transactions).Include(sp => sp.StockTransactions).ThenInclude(s => s.Stock).Where(r => r.StockPortfolioID == id).ToList();
            }
            

            // just make sure that the user has a stock portfolio before proceeding
            if (stonks.Count == 0)
            {
                return View("Error", new string[] { "You have not bought any stock yet!" });
            }


            var queried = new ArrayList();

            List<Transaction> t = stonks[0].Transactions;
            List<StockTransaction> st = stonks[0].StockTransactions;

            // if the user actually entered something to search by
            if (svm.TransactionDescription != "" && svm.TransactionDescription != null)
            {
                foreach (Transaction transaction in t)
                {
                    if (transaction.TransactionNotes != "" && transaction.TransactionNotes != null)
                    {
                        if (transaction.TransactionNotes.Contains(svm.TransactionDescription, StringComparison.OrdinalIgnoreCase))
                        {
                            queried.Add(transaction);
                        }
                    }
                }

                foreach (StockTransaction transaction in st)
                {
                    if (transaction.StockTransactionNotes != "" && transaction.StockTransactionNotes != null)
                    {
                        if (transaction.StockTransactionNotes.Contains(svm.TransactionDescription, StringComparison.OrdinalIgnoreCase))
                        {
                            queried.Add(transaction);
                        }
                    }
                }
            }

            if (svm.SelectedType != null)
            {
                foreach (Transaction transaction in t)
                {
                    
                    if (transaction.TransactionType == svm.SelectedType)
                    {
                        queried.Add(transaction);
                    }
                    
                }

                foreach (StockTransaction transaction in st)
                {           
                    if (transaction.StockTransactionType.ToString() == svm.SelectedType.ToString())
                    {
                        queried.Add(transaction);
                    }  
                }
            }

            if (svm.TransactionNumber != null && svm.TransactionNumber != "")
            {
                foreach (Transaction transaction in t)
                {

                    if (transaction.TransactionNumber.ToString() == svm.TransactionNumber)
                    {
                        queried.Add(transaction);
                    }


                }

                foreach (StockTransaction transaction in st)
                {
                    if (transaction.StockTransactionNumber.ToString() == svm.TransactionNumber.ToString())
                    {
                        queried.Add(transaction);
                    }
                }

            }

            if (svm.LowAmount != null) //user entered something
            {
                foreach (Transaction transaction in t)
                {

                    if (transaction.TransactionAmount >= svm.LowAmount)
                    {
                        queried.Add(transaction);
                    }

                }

                foreach (StockTransaction transaction in st)
                {
                    if (transaction.TotalPrice >= svm.LowAmount)
                    {
                        queried.Add(transaction);
                    }
                }
            }

            if (svm.HighAmount != null)
            {
                foreach (Transaction transaction in t)
                {

                    if (transaction.TransactionAmount <= svm.HighAmount)
                    {
                        queried.Add(transaction);
                    }

                }

                foreach (StockTransaction transaction in st)
                {
                    if (transaction.TotalPrice <= svm.HighAmount)
                    {
                        queried.Add(transaction);
                    }
                }

            }

            if (svm.DateLow != null)
            {
                foreach (Transaction transaction in t)
                {

                    if (transaction.TransactionDate >= svm.DateLow)
                    {
                        queried.Add(transaction);
                    }

                }

                foreach (StockTransaction transaction in st)
                {
                    if (transaction.TransactionDate >= svm.DateLow)
                    {
                        queried.Add(transaction);
                    }
                }
            }


            if(svm.DateHigh != null)
            {
                foreach (Transaction transaction in t)
                {

                    if (transaction.TransactionDate <= svm.DateHigh)
                    {
                        queried.Add(transaction);
                    }

                }

                foreach (StockTransaction transaction in st)
                {
                    
                    if (transaction.TransactionDate <= svm.DateHigh)
                    {
                        queried.Add(transaction);
                    }
                }
            }

            var queried_ordered = GetOrderedQuery(queried, selected, order);

            ViewBag.query = queried_ordered;
            ViewBag.sortby = getSortBy();
            ViewBag.order = GetOrder();
            ViewBag.total = stonks[0].Transactions.Count + stonks[0].StockTransactions.Count;
            ViewBag.id = id;
            
            IDictionary<Stock, Int32> holdings = new Dictionary<Stock, Int32>();
            (stonks[0], holdings) = UpdatePortfolio(stonks[0], _context);
            ViewBag.holdings = holdings;

            return View("Index", stonks[0]);

        }

        private List<SelectListItem> getSortBy()
        {
            
            List<SelectListItem> sortby = new List<SelectListItem>();

            sortby.Add(new SelectListItem(text: "Transaction Number", value: "Number"));
            sortby.Add(new SelectListItem(text: "Transaction Type", value: "Type"));
            sortby.Add(new SelectListItem(text: "Transaction Notes", value: "Notes"));
            sortby.Add(new SelectListItem(text: "Transaction Amount", value: "Amount"));


            sortby.Add(new SelectListItem(text: "Transaction Date", value: "Date"));

            return sortby;

        }

        private List<SelectListItem> GetOrder()
        {
            List<SelectListItem> orderList = new List<SelectListItem>();

            orderList.Add(new SelectListItem(text: "Descending", value: "Descending"));
            orderList.Add(new SelectListItem(text: "Ascending", value: "Ascending"));

            return orderList;
        }
        

        // GET: StockPortfolio/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.StockPortfolios == null)
            {
                return NotFound();
            }

            var stockPortfolio = await _context.StockPortfolios.Include(sp => sp.StockTransactions).ThenInclude(s => s.Stock).ThenInclude(s => s.StockType).FirstOrDefaultAsync(r => r.StockPortfolioID == id);

            if (stockPortfolio == null)
            {
                return NotFound();
            }

            ViewBag.holdings = updateBalanced(stockPortfolio, _context);

            return View(stockPortfolio);
        }


        public static IDictionary<Stock, Int32> updateBalanced(StockPortfolio stockPortfolio, AppDbContext _context)
        {
            IDictionary<Int32, Int32> holdings_dict = GetHoldings(stockPortfolio);



            IDictionary<Stock, Int32> holdings = new Dictionary<Stock, Int32>();

            Stock temp = new Stock();

            bool index = false;
            bool mutual = false;
            int ordinary = 0;
            bool changed = false;

            foreach (Int32 stockID in holdings_dict.Keys)
            {

                temp = _context.Stocks.Find(stockID);
                holdings.Add(temp, holdings_dict[stockID]);

                if (temp.StockType.StockTypeName == "Ordinary")
                {
                    ordinary++;
                }
                if (temp.StockType.StockTypeName == "Index Fund")
                {
                    index = true;
                }
                if (temp.StockType.StockTypeName == "Mutual Fund")
                {
                    mutual = true;
                }
            }

            if (index && mutual && ordinary >= 2)
            {
                stockPortfolio.IsBalanced = true;
                changed = true;
                _context.Update(stockPortfolio);
            }


            if(changed)
            {
                _context.SaveChanges();
            }
            return holdings;
        }

        // POST: StockPortfolio/Create
        public IActionResult Create(string msg)
        {
            ViewBag.msg = msg;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Decimal init_depo, String name)
        {

            int count = _context.StockPortfolios.Where(sp => sp.User.Email == User.Identity.Name).Count();

            if (count >= 1)
            {
                return View("Error", new String[] { "You can only have one stock portfolio!" });
            }
            if (init_depo <= 0)
            {
                ViewBag.msg = "Initial Deposit must be greater than $0";
                return View("Create");
            }

            return RedirectToAction("Confirm", new { init_depo = init_depo, name = name });


        }
        public IActionResult Confirm(decimal init_depo, String name)
        {
            ViewBag.init_depo = init_depo;
            ViewBag.name = name;
            return View();
        }

        public IActionResult CreatePortfolio(Decimal init_depo, String name, bool confirm)
        {
            int count = _context.StockPortfolios.Where(sp => sp.User.Email == User.Identity.Name).Count();

            if (count >= 1)
            {
                return View("Error", new String[] { "You can only have one stock portfolio!" });
            }

            
            StockPortfolio sp = new StockPortfolio();


            if (name == "" || name == null)
            {
                name = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name).FirstName + " Portfolio";
            }

            sp.PortfolioName = name;
            sp.User = _context.Users.Where(u => u.Email == User.Identity.Name).FirstOrDefault();

            sp.PortfolioNumber = Utilities.GenerateNextNum.GetNextAccountNum(_context);

            Transaction id = new Transaction();
            id.StockPortfolio = sp;
            id.TransactionAmount = init_depo;
            id.TransactionDate = DateTime.Today;


            if (id.TransactionAmount > 5000)
            {
                id.TransactionStatus = TransactionStatuses.Pending;
                EmailMessaging.SendEmail(sp.User.Email, "Stock Portfolio: " + sp.PortfolioNumber + " has been created",
                "Note: Your initial deposit is currenlty being procesesd, check back within 2-3 business days");

                // Create notifaction
                Message m = new Message()
                {
                    Info = "Congratulations on creating your account! Note: Your initial deposit was over $5,000 and will be processed by an admin within 2-3 business days.",
                    Subject = "Stock Portfolio has been created",
                    Date = DateTime.Today,
                    Sender = "Longhorn Bank",
                    Receiver = sp.User.Email
                };
                m.Admins = new List<AppUser>();
                m.Admins.Add(sp.User);
                _context.Add(m);

                // Create notification for admins
                Message m2 = new Message()
                {
                    Info = "An initial deposit of over $5,000 by " + sp.User.FullName + ". Please resolve the dispute.",
                    Subject = "Deposit that needs approval created by " + sp.User.FullName,
                    Date = DateTime.Today,
                    Sender = "Deposit System",
                    Receiver = "All"
                };
                List<AppUser> AllAdmins = _userManager.GetUsersInRoleAsync("Admin").Result.ToList();

                var query = from a in AllAdmins where a.IsActive == true select a;

                List<AppUser> ActiveAdmins = query.ToList<AppUser>();

                m2.Admins = ActiveAdmins;
                _context.Add(m2);
            }
            else
            {
                id.TransactionStatus = TransactionStatuses.Approved;
                EmailMessaging.SendEmail(sp.User.Email, "Stock Portfolio: " + sp.PortfolioNumber + " has been created",
                "Log in and start buying stocks!");

                // Create notifaction
                Message m = new Message()
                {
                    Info = "Congratulations on creating your account! Your initial deposit was under $5,000 and has been processed.",
                    Subject = "Stock Portfolio has been created",
                    Date = DateTime.Today,
                    Sender = "Longhorn Bank",
                    Receiver = sp.User.Email
                };
                m.Admins = new List<AppUser>();
                m.Admins.Add(sp.User);
                _context.Add(m);
                
                sp.CashValuePortion += id.TransactionAmount;
            }

            id.TransactionNumber = Utilities.GenerateNextNum.GetNextTransactionNum(_context);
            id.TransactionNotes = "Congratulations on opening your stock portfolio!";

            

            _context.Add(sp);
            _context.Add(id);

            sp.User.UserHasAccount = true;
            sp.isActive = true;


            _context.Update(sp.User);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        //// POST: StockPortfolio/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("StockPortfolioID,PortfolioName,PortfolioNumber,PortfolioValue,isActive,IsBalanced,CashValuePortion")] StockPortfolio stockPortfolio)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(stockPortfolio);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(stockPortfolio);
        //}

        // GET: StockPortfolio/Edit/5
        public async Task<IActionResult> Edit(int? id, string message)
        {
            if (id == null || _context.StockPortfolios == null)
            {
                return NotFound();
            }

            var stockPortfolio = await _context.StockPortfolios.FindAsync(id);
            if (stockPortfolio == null)
            {
                return NotFound();
            }
            ViewBag.Message = message;
            return View(stockPortfolio);
        }

        // POST: StockPortfolio/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, String portfolioName)
        {

            StockPortfolio sp = _context.StockPortfolios.Where(sp => sp.StockPortfolioID == id).Include(sp => sp.User).FirstOrDefault();

            if (sp.isActive == false || sp.User.IsActive == false)
            {
                return RedirectToAction("Edit", new { message = "This bank account has been disabled no changes can be made" });
            }
            sp.PortfolioName = portfolioName;
            _context.Update(sp);
            _context.SaveChanges();


            return RedirectToAction(nameof(Index));
        }

        // GET: StockPortfolio/Delete/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.StockPortfolios == null)
            {
                return NotFound();
            }

            var stockPortfolio = await _context.StockPortfolios
                .FirstOrDefaultAsync(m => m.StockPortfolioID == id);
            if (stockPortfolio == null)
            {
                return NotFound();
            }

            return View(stockPortfolio);
        }

        // POST: StockPortfolio/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.StockPortfolios == null)
        //    {
        //        return Problem("Entity set 'AppDbContext.StockPortfolios'  is null.");
        //    }
        //    var stockPortfolio = await _context.StockPortfolios.FindAsync(id);
        //    if (stockPortfolio != null)
        //    {
        //        _context.StockPortfolios.Remove(stockPortfolio);
        //    }
            
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool StockPortfolioExists(int id)
        {
          return _context.StockPortfolios.Any(e => e.StockPortfolioID == id);
        }


        private static IDictionary<Int32, Int32> GetHoldings(StockPortfolio stonks)
        {
            IDictionary<Int32, Int32> holdings = new Dictionary<Int32, Int32>();

            foreach (StockTransaction st in stonks.StockTransactions)
            {
                if (st.StockTransactionType == StockTransactionTypes.Withdrawal)
                {

                    // Increment the number of shares of the stock that was purchased
                    if (holdings.Keys.Contains(st.Stock.StockID))
                    {
                        holdings[st.Stock.StockID] += st.NumberOfShares;
                    }

                    // Creates new key if stock is not in dict
                    else
                    {
                        holdings.Add(st.Stock.StockID, st.NumberOfShares);
                    }
                }
                
                // Removes shares from dict if sold
                if (st.StockTransactionType == StockTransactionTypes.Deposit)
                {
                    holdings[st.Stock.StockID] -= st.NumberOfShares;
                }
            }

            foreach (KeyValuePair<Int32,Int32> kp in holdings)
            {
                if (kp.Value == 0)
                {
                    holdings.Remove(kp.Key);
                }
            }

            return holdings;
        }



    }
}
