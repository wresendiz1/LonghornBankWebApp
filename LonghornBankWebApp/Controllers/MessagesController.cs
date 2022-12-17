using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;



namespace LonghornBankWebApp.Controllers
{

    [Authorize(Roles = "Admin, Customer")]
    public class MessagesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        
        public MessagesController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Messages
        public async Task<IActionResult> Index()
        {
            
            AppUser user = _userManager.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            List<Message> Pending = await _context.Messages.Where(m => m.Admins.Contains(user)).ToListAsync();

            List<Message> Viewed = await _context.Messages.Where(m => m.Admins.Contains(user) == false).ToListAsync();


            return View(Pending.OrderByDescending(x => x.MessageID));
        }

        // GET: Messages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Messages == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageID == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        private SelectList GetAllAdmins()
        {
            List<AppUser> AllAdmins = _userManager.GetUsersInRoleAsync("Admin").Result.ToList();
            // select admins that are isActive
            var query = from a in AllAdmins where a.IsActive == true select a;

            List<AppUser> ActiveAdmins = query.ToList<AppUser>();

            //AppUser All = new AppUser() { Email = "All", FirstName = "All", LastName = "Admins" };
            //ActiveAdmins.Add(All);

            SelectList AllAdminsList = new SelectList(ActiveAdmins.OrderBy(u => u.FirstName), "Email", "FullName");

            return AllAdminsList;
        }

        // GET: Messages/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Admins = GetAllAdmins();
            return View();
        }

        // POST: Messages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MessageID,Subject,Info")] Message message, string Admin)
        {
            if (ModelState.IsValid)
            {
                // all admins
                if (Admin == null)
                {
                        List<AppUser> AllAdmins = _userManager.GetUsersInRoleAsync("Admin").Result.ToList();
                    
                        var query = from a in AllAdmins where a.IsActive == true select a;

                        List<AppUser> ActiveAdmins = query.ToList<AppUser>();

                        message.Admins = ActiveAdmins;

                    message.Receiver = "All";
                }
                else
                {
                    AppUser user = await _userManager.FindByEmailAsync(Admin);

                    if(user.Email == User.Identity.Name)
                    {
                        ModelState.AddModelError("Receiver", "You cannot send a message to yourself");
                        ViewBag.Admins = GetAllAdmins();
                        return View(message);
                    }
                    message.Admins = new List<AppUser>();
                    message.Admins.Add(user);

                    message.Receiver = user.FullName;
                }
                message.Date = DateTime.Now;
                message.Sender = User.Identity.Name;

                _context.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Admins = GetAllAdmins();
            return View(message);
        }
        // function that returns the number of messages for a user
        public int GetNumberOfMessages()
        {
            AppUser user = _userManager.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            List<Message> Pending = _context.Messages.Where(m => m.Admins.Contains(user)).ToList();
            
            return Pending.Count();
        }

        public async Task<IActionResult> Read(int? id)
        {

            if(id == null)
            {
                return NotFound();
            }

            Message message = await _context.Messages.Include(m => m.Admins).FirstOrDefaultAsync(m => m.MessageID == id);

            AppUser user = _userManager.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            
            message.Admins.Remove(user);

            _context.Update(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Messages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Messages == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageID == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Messages == null)
            {
                return Problem("Entity set 'AppDbContext.Messages'  is null.");
            }
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(int id)
        {
          return _context.Messages.Any(e => e.MessageID == id);
        }
    }
}
