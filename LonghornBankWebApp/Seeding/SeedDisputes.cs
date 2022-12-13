using Microsoft.AspNetCore.Identity;
using IronXL;

using LonghornBankWebApp.Models;
using LonghornBankWebApp.Utilities;
using LonghornBankWebApp.DAL;
using System.Text;

namespace LonghornBankWebApp.Seeding
{
    public class SeedDisputes
    {
        public static void SeedAllDisputes(UserManager<AppUser> userManager, AppDbContext _context)
        {
            String path = @"C:\College\Classes\Fall 2022\MIS 333K\MIS333Kproject\LonghornBankWebApp\LonghornBankWebApp\wwwroot\Files\BankData.xlsx";

            var workBook = new WorkBook(path);
            var workSheet = workBook.WorkSheets[7];


            List<Dispute> allDisputes = new List<Dispute>();

            for (var y = 2; y <= 3; y++)
            {
                var cells = workSheet[$"A{y}:E{y}"].ToList();

                Dispute dis = new Dispute();
                
                dis.Transaction = _context.Transactions.FirstOrDefault(t => t.TransactionNumber == Convert.ToInt32(cells[1].Value.ToString()));

                dis.Transaction.BankAccount = _context.BankAccounts.FirstOrDefault(ba => ba.Transactions.Contains(dis.Transaction));
                
                dis.Transaction.BankAccount.User = userManager.FindByEmailAsync(cells[0].Value.ToString()).Result;

                dis.CorrectAmount = Convert.ToDecimal(cells[2].Value);

                dis.DisputeNotes = cells[3].Value.ToString();

                if (cells[4].Value.ToString() == "Submitted")
                {
                    dis.DisputeStatus = DisputeStatus.Submitted;
                }

                // add other for enum values for future seeding if necessary


                allDisputes.Add(dis);

            }

            String notes = "Start";

            try
            {
                foreach(Dispute d in allDisputes)
                {
                    notes = d.DisputeNotes;
                    
                    Dispute dbDispute = _context.Disputes.FirstOrDefault(dis => dis.DisputeID == d.DisputeID);

                    if (dbDispute == null)
                    {
                        _context.Disputes.Add(d);
                        _context.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Error seeding dispute ");
                msg.Append(notes);
                throw new Exception(msg.ToString(), ex);
            }
        }


    }
}

