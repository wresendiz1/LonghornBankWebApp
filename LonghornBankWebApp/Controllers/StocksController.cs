using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;
using Highsoft.Web.Mvc.Charts;

namespace LonghornBankWebApp.Controllers
{
    public class StocksController : Controller
    {
        private readonly AppDbContext _context;

        public StocksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Stocks
        public async Task<IActionResult> Index()
        {
              return View(await _context.Stocks.Include(s => s.StockType).Include(s => s.StockTransactions).ToListAsync());
        }

        private IEnumerable<SelectListItem> GetStockTypes()
       
        {
            List<StockType> StockTypes = _context.StockTypes.ToList();

            List<SelectListItem> StockTypeList = new List<SelectListItem>();

            foreach (StockType s in StockTypes)
            {
                SelectListItem newStockType = new SelectListItem();
                newStockType.Text = s.StockTypeName;
                newStockType.Value = s.StockTypeID.ToString();
                StockTypeList.Add(newStockType);
            }
            
            return StockTypeList;
        }


        // GET: Stocks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Stocks == null)
            {
                return NotFound();
            }

            var stock = await _context.Stocks
                .FirstOrDefaultAsync(m => m.StockID == id);
            if (stock == null)
            {
                return NotFound();
            }

            var stockPrices = await _context.StockPrices.Where(sp => sp.Stock.TickerSymbol == stock.TickerSymbol).ToListAsync();

            var stockPriceData = new List<StockPrice>();

            foreach (var stockPrice in stockPrices)
            {
                stockPriceData.Add(new StockPrice
                {
                    Date = stockPrice.Date,
                    CurrentPrice = stockPrice.CurrentPrice
                });
            }
            //ADDED: Create a line graph of the historical stock price

            List<LineSeriesData> prices = new List<LineSeriesData>();
            List<string> dates = new List<string>();

            stockPriceData.ForEach(s => prices.Add(new LineSeriesData { Y = Decimal.ToDouble(s.CurrentPrice) }));
            stockPriceData.ForEach(s => dates.Add(s.Date.ToString()));
            
            ViewData["PriceX"] = dates;
            ViewData["PriceY"] = prices;



            //List<double> tokyoValues = new List<double> { 7.0, 6.9, 9.5, 14.5, 18.2, 21.5, 25.2, 26.5, 23.3, 18.3, 13.9, 9.6 };
            //List<double> nyValues = new List<double> { -0.2, 0.8, 5.7, 11.3, 17.0, 22.0, 24.8, 24.1, 20.1, 14.1, 8.6, 2.5 };
            //List<double> berlinValues = new List<double> { -0.9, 0.6, 3.5, 8.4, 13.5, 17.0, 18.6, 17.9, 14.3, 9.0, 3.9, 1.0 };
            //List<double> londonValues = new List<double> { 3.9, 4.2, 5.7, 8.5, 11.9, 15.2, 17.0, 16.6, 14.2, 10.3, 6.6, 4.8 };
            //List<LineSeriesData> tokyoData = new List<LineSeriesData>();
            //List<LineSeriesData> nyData = new List<LineSeriesData>();
            //List<LineSeriesData> berlinData = new List<LineSeriesData>();
            //List<LineSeriesData> londonData = new List<LineSeriesData>();

            //tokyoValues.ForEach(p => tokyoData.Add(new LineSeriesData { Y = p }));
            //nyValues.ForEach(p => nyData.Add(new LineSeriesData { Y = p }));
            //berlinValues.ForEach(p => berlinData.Add(new LineSeriesData { Y = p }));
            //londonValues.ForEach(p => londonData.Add(new LineSeriesData { Y = p }));


            //ViewData["tokyoData"] = tokyoData;
            //ViewData["nyData"] = nyData;
            //ViewData["berlinData"] = berlinData;
            //ViewData["londonData"] = londonData;


            return View(stock);
        }

       


        // GET: Stocks/Create
        public IActionResult Create()
        {
            ViewBag.Types = GetStockTypes();
            return View();
        }

        // POST: Stocks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Stock stock, int? SelectedID, bool? modify)
        {
            
            if(SelectedID == null)
            {
                ModelState.AddModelError("", "Valid stock type is required");
                ViewBag.Types = GetStockTypes();
                return View(stock);
            }

            // ensure that no ticker symbol is identical
            Stock dbStock = _context.Stocks.FirstOrDefault(s => s.TickerSymbol == stock.TickerSymbol);

            if(dbStock != null)
            {
                ModelState.AddModelError("TickerSymbol", "Ticker symbol must be unique");
                ViewBag.Types = GetStockTypes();
                return View(stock);
            }
            if (decimal.Compare(stock.CurrentPrice, (decimal)0.01) < 0)
            {
                ModelState.AddModelError("CurrentPrice", "Current price must be greater than $0.01");
                ViewBag.Types = GetStockTypes();
                return View(stock);

            }

            StockType dbStockType = _context.StockTypes.FirstOrDefault(s => s.StockTypeID == SelectedID);
            stock.StockType = dbStockType;

            if (ModelState.IsValid)       
            {

                if(modify == false)
                {
                    ModelState.AddModelError("Valid", "Confirm to create " + stock.StockName + " stock with current price $" 
                        + stock.CurrentPrice + " and type " + stock.StockType.StockTypeName);
                    ViewBag.Types = GetStockTypes();
                    return View(stock);
                }
                else if(modify == true)
                {
                    _context.Add(stock);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
 
               
            }
            return View(stock);
        }

        // GET: Stocks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Stocks == null)
            {
                return NotFound();
            }

            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }
            return View(stock);
        }

        // POST: Stocks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StockID,StockName,CurrentPrice,TickerSymbol")] Stock stock)
        {
            if (id != stock.StockID)
            {
                return NotFound();
            }
            
            if (stock.CurrentPrice < (decimal)0.01)
            {
                ModelState.AddModelError("CurrentPrice", "Current price must be greater than $0.01");
                return View(stock);
            }

            try
            {
                Stock dbStock = _context.Stocks.FirstOrDefault(s => s.StockID == id);
                dbStock.CurrentPrice = stock.CurrentPrice;
                _context.Update(dbStock);

                //ADDED: add stock price history
                StockPrice sp = new StockPrice();
                sp.Stock = dbStock;
                sp.CurrentPrice = stock.CurrentPrice;
                sp.Date = DateTime.Now;
                _context.Add(sp);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockExists(stock.StockID))
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

        // GET: Stocks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Stocks == null)
            {
                return NotFound();
            }

            var stock = await _context.Stocks
                .FirstOrDefaultAsync(m => m.StockID == id);
            if (stock == null)
            {
                return NotFound();
            }

            return View(stock);
        }

        // POST: Stocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Stocks == null)
            {
                return Problem("Entity set 'AppDbContext.Stocks'  is null.");
            }
            var stock = await _context.Stocks.FindAsync(id);
            if (stock != null)
            {
                _context.Stocks.Remove(stock);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StockExists(int id)
        {
          return _context.Stocks.Any(e => e.StockID == id);
        }
    }
}
