using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;
using LonghornBankWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using LonghornBankWebApp.Utilities;

namespace LonghornBankWebApp.Controllers
{
    [Authorize]
    public class DisputesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;


        public DisputesController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Disputes
        public async Task<IActionResult> Index()
        {
              return View(await _context.Disputes.ToListAsync());
        }

        // GET: Disputes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Disputes == null)
            {
                return NotFound();
            }

            var dispute = await _context.Disputes.Include(d => d.Transaction).ThenInclude(t=>t.BankAccount).Include(t =>t.Transaction).ThenInclude(t => t.StockPortfolio)
                .FirstOrDefaultAsync(m => m.DisputeID == id);


            if (dispute == null)
            {
                return NotFound();
            }

            return View(dispute);
        }


        public IActionResult Resolve(Int32 id, String? message, Decimal adjusted = -1, bool reject = false, bool accept = false )
        {
            Dispute dispute = _context.Disputes.Include(t=>t.Transaction).ThenInclude(t=>t.BankAccount).Include(t => t.Transaction).ThenInclude(t => t.StockPortfolio).FirstOrDefault(d => d.DisputeID == id);

            dispute.oldAmount = dispute.Transaction.TransactionAmount;

            dispute.dateResolved = DateTime.Today;

            dispute.adminMessage = message;

            dispute.adminEmail = User.Identity.Name.ToString();

            if(accept)
            {
                if (dispute.DeleteTransaction)
                {
                    dispute.Transaction.TransactionStatus = TransactionStatuses.Deleted;
                    if (dispute.Transaction.StockPortfolio != null)
                    {
                        dispute.Transaction.StockPortfolio.CashValuePortion -= dispute.Transaction.TransactionAmount;
                    }
                    else if (dispute.Transaction.BankAccount != null)
                    {
                        dispute.Transaction.BankAccount.BankAccountBalance -= dispute.Transaction.TransactionAmount;
                    }
                }
                else
                {
                    if (dispute.Transaction.StockPortfolio != null)
                    {
                        dispute.Transaction.StockPortfolio.CashValuePortion = dispute.Transaction.StockPortfolio.CashValuePortion - dispute.Transaction.TransactionAmount + dispute.CorrectAmount;
                    }
                    else if (dispute.Transaction.BankAccount != null)
                    {
                        dispute.Transaction.BankAccount.BankAccountBalance = dispute.Transaction.BankAccount.BankAccountBalance - dispute.Transaction.TransactionAmount + dispute.CorrectAmount;
                    }
                    
                    dispute.Transaction.TransactionAmount = dispute.CorrectAmount;
                }

                
                dispute.DisputeStatus = DisputeStatus.Accepted;
                dispute.Transaction.TransactionNotes = "(Dispute accepted) " + dispute.Transaction.TransactionNotes;

                //ASK: what do we do for transaction notes of transactions that have been disputed multiple times


            }
            if(reject)
            {
                dispute.DisputeStatus = DisputeStatus.Rejected;
                dispute.Transaction.TransactionNotes = "(Dispute rejected) " + dispute.Transaction.TransactionNotes;
            }
            if(adjusted != -1)
            {
                dispute.CorrectAmount = adjusted;
                dispute.DisputeStatus = DisputeStatus.Adjusted;
                dispute.Transaction.TransactionNotes = "(Dispute adjusted) " + dispute.Transaction.TransactionNotes;

                if (dispute.Transaction.StockPortfolio != null)
                {
                    dispute.Transaction.StockPortfolio.CashValuePortion = dispute.Transaction.StockPortfolio.CashValuePortion - dispute.Transaction.TransactionAmount + dispute.CorrectAmount;
                }
                else if (dispute.Transaction.BankAccount != null)
                {
                    dispute.Transaction.BankAccount.BankAccountBalance = dispute.Transaction.BankAccount.BankAccountBalance - dispute.Transaction.TransactionAmount + dispute.Transaction.TransactionAmount;
                }

                dispute.Transaction.TransactionAmount = dispute.CorrectAmount;
            }

            _context.Update(dispute);
            _context.Update(dispute.Transaction);
            if (dispute.Transaction.StockPortfolio != null)
            {
                _context.Update(dispute.Transaction.StockPortfolio);
                
                // find user email
                StockPortfolio sp = _context.StockPortfolios.Include(sp => sp.User).FirstOrDefault(sp => sp.StockPortfolioID == dispute.Transaction.StockPortfolio.StockPortfolioID);
                EmailMessaging.SendEmail(sp.User.Email, "Dispute for transaction: " + dispute.Transaction.TransactionNumber + " has been resolved"
                    , "Transaction " + dispute.Transaction.TransactionNumber + " has been " + dispute.DisputeStatus.ToString() + " by admin " + dispute.adminEmail + ". Please check your account for more details. Message from admin: " + dispute.adminMessage);
            }
            else if (dispute.Transaction.BankAccount != null)
            {
                _context.Update(dispute.Transaction.BankAccount);

                // find user email
                BankAccount ba = _context.BankAccounts.Include(ba => ba.User).FirstOrDefault(ba => ba.BankAccountID == dispute.Transaction.BankAccount.BankAccountID);
                EmailMessaging.SendEmail(ba.User.Email, "Dispute for transaction: " + dispute.Transaction.TransactionNumber + " has been resolved"
                    , "Transaction " + dispute.Transaction.TransactionNumber + " has been " + dispute.DisputeStatus.ToString() + " by admin" + dispute.adminEmail + ". Please check your account for more details. Message from admin: " + dispute.adminMessage);

            }
            _context.SaveChanges();

            return RedirectToAction(controllerName: "Transactions", actionName: "Details", routeValues: new { id = dispute.Transaction.TransactionID });
        }

        // GET: Disputes/Create
        public IActionResult Create(TransactionDispute td)
        {
            AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;
            if (u.IsActive == false)
            {
                return RedirectToAction("Index", "BankAccounts", new { message = "Your account is not active. Please contact an administrator to activate your account." });

            }
            // TODO: Account for stock portfolios
            
            Transaction transaction = _context.Transactions.Include(t => t.BankAccount).ThenInclude(b => b.User).FirstAsync(t => t.TransactionID == td.transaction.TransactionID).Result;

            Dispute dispute = td.dispute;

            dispute.Transaction = transaction;

            dispute.dateCreated = DateTime.Today;

            // create message for admins
            Message message = new Message();
            message.Date = DateTime.Today;

            if(transaction.BankAccount != null)
            {
                message.Subject = "Dispute created by " + transaction.BankAccount.User.FullName;
                message.Info = "A dispute has been created for transaction " + transaction.TransactionID + " by " + transaction.BankAccount.User.FullName + ". Please resolve the dispute.";
            }

            else if (transaction.StockPortfolio != null)
            {
                message.Subject = "Dispute created by " + transaction.StockPortfolio.User.FullName;
                message.Info = "A dispute has been created for transaction " + transaction.TransactionID + " by " + transaction.StockPortfolio.User.FullName + ". Please resolve the dispute.";
            }

            
            
            message.Sender = "Dispute System";
            message.Receiver = "All";

            List<AppUser> AllAdmins = _userManager.GetUsersInRoleAsync("Admin").Result.ToList();

            var query = from a in AllAdmins where a.IsActive == true select a;

            List<AppUser> ActiveAdmins = query.ToList<AppUser>();

            message.Admins = ActiveAdmins;

            _context.Add(message);
            _context.Add(dispute);
            _context.SaveChanges();

            return RedirectToAction(controllerName:"Transactions", actionName:"Details", routeValues: new {id=transaction.TransactionID});
        }

        //// POST: Disputes/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("DisputeID,DisputeNotes,CorrectAmount,DeleteTransaction,DisputeStatus")] Dispute dispute)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(dispute);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(dispute);
        //}

        // GET: Disputes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Disputes == null)
            {
                return NotFound();
            }

            var dispute = await _context.Disputes.FindAsync(id);
            if (dispute == null)
            {
                return NotFound();
            }
            return View(dispute);
        }

        // POST: Disputes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DisputeID,DisputeNotes,CorrectAmount,DeleteTransaction,DisputeStatus")] Dispute dispute)
        {
            if (id != dispute.DisputeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                AppUser u = _userManager.FindByNameAsync(User.Identity.Name).Result;
                if (u.IsActive == false)
                {
                    return RedirectToAction("Index", "BankAccounts", new { message = "Your account is not active. Please contact an administrator to activate your account." });

                }
                try
                {
                    _context.Update(dispute);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DisputeExists(dispute.DisputeID))
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
            return View(dispute);
        }

        // GET: Disputes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Disputes == null)
            {
                return NotFound();
            }

            var dispute = await _context.Disputes
                .FirstOrDefaultAsync(m => m.DisputeID == id);
            if (dispute == null)
            {
                return NotFound();
            }

            return View(dispute);
        }

        // POST: Disputes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Disputes == null)
            {
                return Problem("Entity set 'AppDbContext.Disputes'  is null.");
            }
            var dispute = await _context.Disputes.FindAsync(id);
            if (dispute != null)
            {
                _context.Disputes.Remove(dispute);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DisputeExists(int id)
        {
          return _context.Disputes.Any(e => e.DisputeID == id);
        }
    }
}
