using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;
using Highsoft.Web.Mvc.Charts;
using Microsoft.AspNetCore.Authorization;

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
              return View(await _context.Stocks.Include(s => s.StockType).ToListAsync());
        }

        private IEnumerable<SelectListItem> GetStockTypes()
       
        {
            List<StockType> StockTypes = _context.StockTypes.ToList();

            List<SelectListItem> StockTypeList = new();

            foreach (StockType s in StockTypes)
            {
                SelectListItem newStockType = new()
                {
                    Text = s.StockTypeName,
                    Value = s.StockTypeID.ToString()
                };
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


            var stock = await _context.Stocks.Where(s => s.StockID == id).Select(
                s => new 
                {
                    Stock = s,
                    Prices = s.StockPrices.Select(sp => new LineSeriesData { Y = Decimal.ToDouble(sp.CurrentPrice)}),
                    Dates = s.StockPrices.Select(sp => sp.Date.ToString())
                }).FirstOrDefaultAsync();

            if (stock == null)
            {
                return NotFound();
            }

            //ADDED: Create a line graph of the historical stock price
            ViewData["PriceX"] = stock.Dates.ToList();
            ViewData["PriceY"] = stock.Prices.ToList();



            return View(stock.Stock);
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
                StockPrice sp = new()
                {
                    Stock = dbStock,
                    CurrentPrice = stock.CurrentPrice,
                    Date = DateTime.Now
                };
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
        [Authorize(Roles ="Admin")]
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
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Stocks == null)
        //    {
        //        return Problem("Entity set 'AppDbContext.Stocks'  is null.");
        //    }
        //    var stock = await _context.Stocks.FindAsync(id);
        //    if (stock != null)
        //    {
        //        _context.Stocks.Remove(stock);
        //    }
            
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool StockExists(int id)
        {
          return _context.Stocks.Any(e => e.StockID == id);
        }
    }
}
