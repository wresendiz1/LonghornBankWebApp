using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;

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
