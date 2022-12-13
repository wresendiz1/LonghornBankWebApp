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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;

namespace LonghornBankWebApp.Controllers
{
    [Authorize]

    public class StockTransactionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;


        public StockTransactionsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;


        }

        // GET: StockTransactions
        public async Task<IActionResult> Index()
        {
            return View(await _context.StockTransactions.ToListAsync());
        }

        // GET: StockTransactions/Details/5
        public async Task<IActionResult> Details(int? id, int? stockLeft)
        {
            if (id == null || _context.StockTransactions == null)
            {
                return NotFound();
            }

            StockTransaction stockTransaction = await _context.StockTransactions.Include(s => s.StockPortfolio).Include(s => s.Stock).ThenInclude(s => s.StockType)
                .FirstOrDefaultAsync(r => r.StockTransactionNumber == id);

            if (stockTransaction == null)
            {
                return NotFound();
            }

            ViewBag.Left = stockLeft;

            return View(stockTransaction);
        }

        public async Task<IActionResult> StockTransDetails(int? id)
        {
            if (id == null || _context.StockTransactions == null)
            {
                return NotFound();
            }

            StockTransaction stockTransaction = await _context.StockTransactions.Include(s => s.StockPortfolio).Include(s => s.Stock).ThenInclude(s => s.StockType)
                .FirstOrDefaultAsync(r => r.StockTransactionID == id);

            if (stockTransaction == null)
            {
                return NotFound();
            }

            return View(stockTransaction);
        }

        // GET: StockTransactions/Create
        public IActionResult Create()
        {
            ViewBag.DropDown = GetStockDropDown();
            return View();
        }




        // POST: StockTransactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockTransaction stockTransaction, int? SelectedStock, bool? modify)
        {
            AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;
            StockPortfolio sp = _context.StockPortfolios.FirstOrDefault(s => s.User.UserName == User.Identity.Name);
            
            if (u.IsActive == false || sp.isActive == false)
            {
                return RedirectToAction("Index", "StockPortfolio", new { message = "Your account is not active. Please contact an administrator to activate your account." });

            }
            if (ModelState.IsValid && SelectedStock != null)
            {

                int result = DateTime.Compare(DateTime.Now.Date, stockTransaction.TransactionDate.Date);
                //ensure stock transaction date is today or later
                if (result > 0)
                {
                    ModelState.AddModelError("TransactionDate", "Transaction date must be today or later");
                    ViewBag.DropDown = GetStockDropDown();
                    return View(stockTransaction);
                }
                
                // Find stock from database

                Stock st = _context.Stocks.Include(s => s.StockType).FirstOrDefault(s => s.StockID == SelectedStock);

                if (st != null)
                {
                    // take into account the $10 fee
                    Decimal total = (stockTransaction.NumberOfShares * st.CurrentPrice) + 10;
                    StockPortfolio stp = _context.StockPortfolios.FirstOrDefault(s => s.User.UserName == User.Identity.Name);

                    // Ensure enough funds are available
                    if (total > stp.CashValuePortion)
                    {
                        // add model error and return to create view
                        ModelState.AddModelError("Not Enough Money!", "You do not have enough money in your portfolio to buy this many shares");
                        ViewBag.DropDown = GetStockDropDown();
                        return View(stockTransaction);

                    }
                    else if (modify == false)
                    {
                        //ModelState.SetModelValue("NumberOfShares", new ValueProviderResult(stockTransaction.NumberOfShares.ToString(), System.Globalization.CultureInfo.CurrentCulture));
                        //ModelState.SetModelValue("TransactionDate", new ValueProviderResult(stockTransaction.TransactionDate.ToString(), System.Globalization.CultureInfo.CurrentCulture));
                        ModelState.AddModelError("Succesful", "Confirm to buy " + stockTransaction.NumberOfShares + " shares of " + st.StockName + " for $" + total + " on " + stockTransaction.TransactionDate.ToString("d"));
                        ModelState.AddModelError("Balance", "Your balance will be $" + (stp.CashValuePortion - total));
                        ViewBag.DropDown = GetStockDropDown();
                        return View(stockTransaction);
                    }

                    else if (modify == true)
                    {
                        // Update cash value portion
                        stp.CashValuePortion -= total;
                        stp.TotalFees += 10;

                        // create stock transaction for purchase
                        stockTransaction.TotalPrice = -1 * (total - 10);
                        stockTransaction.StockPortfolio = stp;
                        stockTransaction.Stock = st;
                        stockTransaction.PricePerShare = st.CurrentPrice;
                        stockTransaction.StockTransactionType = StockTransactionTypes.Withdrawal;
                        stockTransaction.StockTransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);
                        stockTransaction.StockTransactionNotes = "Stock Purchase - Account " + stp.PortfolioNumber.ToString();

                        // create a purchase fee stocktrans
                        StockTransaction fee = new StockTransaction();
                        fee.TotalPrice = -10;
                        fee.StockPortfolio = stp;
                        fee.Stock = st;
                        fee.StockTransactionType = StockTransactionTypes.Fee;
                        fee.StockTransactionNumber = GenerateNextNum.GetNextTransactionNum(_context) + 1;
                        fee.StockTransactionNotes = "Fee for purchase of " + st.StockName;
                        fee.TransactionDate = stockTransaction.TransactionDate;



                        _context.StockPortfolios.Update(stp);
                        _context.StockTransactions.Add(stockTransaction);
                        _context.StockTransactions.Add(fee);
                        await _context.SaveChangesAsync();
                        
                        ViewBag.Balance = stp.CashValuePortion;
                        return View("PurchaseSuccess", stockTransaction);

                    }
                    else
                    {
                        return NotFound();
                    }

                    
                }

                else
                {
                    return NotFound();
                }


            }
            else
            {
                ModelState.AddModelError("", "Please select a stock");
                ViewBag.DropDown = GetStockDropDown();
                return View(stockTransaction);

            }
        }

        public IActionResult PurchaseSuccess() {
            return View();
        }



        private IEnumerable<SelectListItem> GetStockDropDown()
        {
            // return stock properties in selectlist
            List<Stock> AllStocks = _context.Stocks.Include(s => s.StockType).ToList();


            List<SelectListItem> StockList = new List<SelectListItem>();

            foreach (Stock st in AllStocks)
            {

                StockList.Add(new SelectListItem
                {
                    Text = st.StockName + "    " + st.TickerSymbol + "    " + st.
                    StockType.StockTypeName + "     $" + st.CurrentPrice + " + $10 purchase fee"
                    ,
                    Value = st.StockID.ToString()
                });

            }

            return StockList;

        }
        
        // Not used anymore
        //private SelectList GetAllStocks()
        //{
        //    List<Stock> StocksList = _context.Stocks.Include(s => s.StockType).ToList();
        //    SelectList AllStocksList = new SelectList(StocksList, "StockID", "StockDropDown");

        //    return AllStocksList;

        //}

        // GET: StockTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.StockTransactions == null)
            {
                return NotFound();
            }

            var stockTransaction = await _context.StockTransactions.FindAsync(id);
            if (stockTransaction == null)
            {
                return NotFound();
            }
            return View(stockTransaction);
        }

        // POST: StockTransactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StockTransactionID,StockTransactionNumber,PricePerShare,NumberOfShares,TotalPrice,TransactionDate,StockTransactionType,StockTransactionNotes")] StockTransaction stockTransaction)
        {
            if (id != stockTransaction.StockTransactionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockTransactionExists(stockTransaction.StockTransactionID))
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
            return View(stockTransaction);
        }

        private static IDictionary<Int32, Int32> GetIndivHoldings(StockPortfolio stonks)
        {
            IDictionary<Int32, Int32> holdings = new Dictionary<Int32, Int32>();

            foreach (StockTransaction st in stonks.StockTransactions)
            {
                if (st.StockTransactionType == StockTransactionTypes.Withdrawal)
                {

                    // Increment the number of shares of the stock that was purchased
                    if (holdings.Keys.Contains(st.StockTransactionNumber))
                    {
                        holdings[st.StockTransactionNumber] += st.NumberOfShares;
                    }

                    // Creates new key if stock is not in dict
                    else
                    {
                        holdings.Add(st.StockTransactionNumber, st.NumberOfShares);
                    }
                }

                // Removes shares from dict if sold
                if (st.StockTransactionType == StockTransactionTypes.Deposit)
                {
                    holdings[st.SellingTransactionNumber] -= st.NumberOfShares;
                }
            }

            // remove purchase transaction that have been completely sold 
            foreach (Int32 key in holdings.Keys.ToList())
            {
                if (holdings[key] == 0)
                {
                    holdings.Remove(key);
                }
            }

            return holdings;
        }
        public IActionResult SellStock(int? id)
        {

            if (id == null || _context.StockPortfolios == null)
            {
                return NotFound();
            }

            // Find User stock portfolio
            var stockPortfolio = _context.StockPortfolios.Include(sp => sp.StockTransactions).ThenInclude(s => s.Stock).ThenInclude(s => s.StockType).FirstOrDefault(r => r.StockPortfolioID == id);


            // Get a dictionary for the holding by each purchase transaction
            IDictionary<Int32, Int32> holdings_dict = GetIndivHoldings(stockPortfolio);


            // Store in viewbag
            ViewBag.StocksToSell = holdings_dict;



            // Convert to a list of dictionaries
            var st = holdings_dict.ToList();

            // Query user stock transactions
            List<StockTransaction> stockTransactions = new List<StockTransaction>();

            foreach (var item in st)
            {
                StockTransaction str = _context.StockTransactions.Where(s => s.StockTransactionType == StockTransactionTypes.Withdrawal)
                    .Include(s => s.Stock).ThenInclude(s => s.StockType).FirstOrDefault(r => r.StockTransactionNumber == item.Key);
                stockTransactions.Add(str);
            }


            return View(stockTransactions);

            
        }
        
        public IActionResult SellConfirm(int? id, int? numShares, int? stockLeft)
        {
            
                if (id == null || _context.StockTransactions == null)
                {
                    return NotFound();
                }

                AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;
                StockPortfolio sp = _context.StockPortfolios.FirstOrDefault(s => s.User.UserName == User.Identity.Name);

                if (u.IsActive == false || sp.isActive == false)
                {
                    return RedirectToAction("Index", "StockPortfolio", new { message = "Your account is not active. Please contact an administrator to activate your account." });

                }

            StockTransaction stockTransaction = _context.StockTransactions.Include(s => s.StockPortfolio).Include(s => s.Stock).ThenInclude(s => s.StockType).FirstOrDefault(r => r.StockTransactionNumber == id);

                if (stockTransaction == null)
                {
                    return NotFound();
                }

                if (Convert.ToInt32(numShares) <= 0)
                {
                   
                    return RedirectToAction("SellStock", "StockTransactions", new { id = stockTransaction.StockPortfolio.StockPortfolioID });
                 }


                // compare
                var difference = stockTransaction.Stock.CurrentPrice - stockTransaction.PricePerShare;

                // Create sale/deposit transaction
                StockTransaction st = new StockTransaction();
                st.StockPortfolio = stockTransaction.StockPortfolio;
                st.Stock = stockTransaction.Stock;
                st.NumberOfShares = Convert.ToInt32(numShares);
                st.PricePerShare = stockTransaction.PricePerShare;
                st.TotalPrice = st.PricePerShare * st.NumberOfShares;
                st.StockTransactionType = StockTransactionTypes.Deposit;
                st.StockTransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);
                st.TransactionDate = DateTime.Now;
                // Associate the sale transaction with the purchase transaction
                st.SellingTransactionNumber = stockTransaction.StockTransactionNumber;

                ViewBag.Remaining = stockLeft - st.NumberOfShares;


                // get total gain/loss for sale of stock
                var totaldif = difference * st.NumberOfShares;

                st.GainLoss = totaldif;

                String gainOrLoss = "";

                if (difference < 0)
                {
                    gainOrLoss += "Loss of ";
                    gainOrLoss += totaldif.ToString();
                }
                else if (difference > 0)
                {
                    gainOrLoss += "Gain of ";
                    gainOrLoss += totaldif.ToString();

                }
                else
                {
                    gainOrLoss += "No Gain ";

                }

                
                ViewBag.GainOrLoss = gainOrLoss;


                return View(st);
            
        }

        public IActionResult Sell(int? id, int? numShares, string profit)
        {
            

            
                if (id == null || _context.StockTransactions == null)
                {
                    return NotFound();
                }

                
                // Original purchase transaction
                StockTransaction stockTransaction = _context.StockTransactions.Include(s => s.StockPortfolio).Include(s => s.Stock)
                    .ThenInclude(s => s.StockType).FirstOrDefault(r => r.StockTransactionNumber == id);

                if (stockTransaction == null)
                {
                    return NotFound();
                }


                // compare
                var difference = stockTransaction.Stock.CurrentPrice - stockTransaction.PricePerShare;

                // Create sale/deposit transaction
                StockTransaction st = new StockTransaction();
                st.StockPortfolio = stockTransaction.StockPortfolio;
                

                st.Stock = stockTransaction.Stock;
                st.NumberOfShares = Convert.ToInt32(numShares);
                st.PricePerShare = stockTransaction.Stock.CurrentPrice;
                st.TotalPrice = st.PricePerShare * st.NumberOfShares;
                st.StockTransactionType = StockTransactionTypes.Deposit;
                st.StockTransactionNumber = GenerateNextNum.GetNextTransactionNum(_context);
                st.TransactionDate = DateTime.Now;
                // Associate the sale transaction with the purchase transaction
                st.SellingTransactionNumber = stockTransaction.StockTransactionNumber;

                // Change balance and fees
                st.StockPortfolio.CashValuePortion += st.TotalPrice;
                st.StockPortfolio.TotalFees += 15;
                
                // get total gain/loss for sale of stock
                var totaldif = difference * st.NumberOfShares;

                String gainOrLoss = "";

                if (difference < 0)
                {
                    gainOrLoss += "Loss of ";
                    gainOrLoss += totaldif.ToString();
                }
                else if (difference > 0)
                {
                    gainOrLoss += "Gain of ";
                    gainOrLoss += totaldif.ToString();

                }
                else
                {
                    gainOrLoss += "No Gain ";

                }

                st.StockTransactionNotes = "Sale of " + st.Stock.StockName + " - " + st.NumberOfShares.ToString() + " shares" + " - "
                + st.PricePerShare.ToString() + " current stock price" + " - " + stockTransaction.PricePerShare.ToString()
                + " initial stock price" + " - " + gainOrLoss;


                // Create a fee transaction
                StockTransaction fee = new StockTransaction();
                fee.StockPortfolio = stockTransaction.StockPortfolio;
                fee.Stock = stockTransaction.Stock;
                fee.TotalPrice = -15;
                fee.StockTransactionType = StockTransactionTypes.Fee;
                fee.StockTransactionNumber = GenerateNextNum.GetNextTransactionNum(_context) + 1;
                fee.TransactionDate = DateTime.Now;
                fee.StockTransactionNotes = "Fee for sale of " + st.Stock.StockName;

                // Change bank account
                st.StockPortfolio.CashValuePortion += fee.TotalPrice;
                st.StockPortfolio.TotalGains += st.GainLoss;
                st.StockPortfolio.TotalFees += fee.TotalPrice;

                // Add to database
                _context.StockPortfolios.Update(st.StockPortfolio);
                _context.StockTransactions.Add(st);
                _context.StockTransactions.Add(fee);
                _context.SaveChanges();

                //return RedirectToAction("Details", "StockPortfolio", new { id = st.StockPortfolio.StockPortfolioID});
                ViewBag.GainOrLoss = profit;
                return View("SellConfirmed", st);
            
        }


        // GET: StockTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.StockTransactions == null)
            {
                return NotFound();
            }

            var stockTransaction = await _context.StockTransactions
                .FirstOrDefaultAsync(m => m.StockTransactionID == id);
            if (stockTransaction == null)
            {
                return NotFound();
            }

            return View(stockTransaction);
        }

        // POST: StockTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.StockTransactions == null)
            {
                return Problem("Entity set 'AppDbContext.StockTransactions'  is null.");
            }
            var stockTransaction = await _context.StockTransactions.FindAsync(id);
            if (stockTransaction != null)
            {
                _context.StockTransactions.Remove(stockTransaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StockTransactionExists(int id)
        {
            return _context.StockTransactions.Any(e => e.StockTransactionID == id);
        }
    }
}
