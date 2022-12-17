using Microsoft.AspNetCore.Identity;
using IronXL;

using LonghornBankWebApp.Models;
using LonghornBankWebApp.Utilities;
using LonghornBankWebApp.DAL;
using System.Text;

namespace LonghornBankWebApp.Seeding
{
    public static class SeedBankAccounts
    {

        // ASK: return Task<IdentityResult>
        public static void SeedAllBankAccounts(UserManager<AppUser> userManager, AppDbContext _context)
        {
            String path = @"C:\Fall 2022\LonghornBankWebApp\LonghornBankWebApp\wwwroot\Files\BankData.xlsx";

            var workBook = new WorkBook(path);
            var workSheet = workBook.WorkSheets[2];

            // Create a list of Bank Account and Stock Portfolio Accounts
            List<BankAccount> allBankAccounts = new List<BankAccount>();
            List<StockPortfolio> allStockPortfolios = new List<StockPortfolio>();

            //iterate through every account case
            for (var y = 2; y <= 23; y++)
            {
                var cells = workSheet[$"A{y}:E{y}"].ToList();
                
                    // just to store the record and it's attributes before adding it to the list
                    BankAccount bankAccount = new BankAccount();
                    
                    //set the bank account type
                    if (cells[3].Value.ToString() == "Checking")
                    {
                        bankAccount.BankAccountType = BankAccountTypes.Checking;
                    }
                    else if(cells[3].Value.ToString() == "Savings")
                    {
                        bankAccount.BankAccountType = BankAccountTypes.Savings;
                    }
                    else if(cells[3].Value.ToString() == "IRA")
                    {
                        bankAccount.BankAccountType = BankAccountTypes.IRA;
                    }
                    
                    // set the bank account number
                    bankAccount.BankAccountNumber = Convert.ToUInt32(cells[0].Value);
                    // set the bank account name
                    bankAccount.BankAccountName = cells[2].Value.ToString();
                    //sets the balance
                    bankAccount.BankAccountBalance = Convert.ToDecimal(cells[4].Value);
                    // sets the user
                    bankAccount.User = userManager.FindByEmailAsync(cells[1].Value.ToString()).Result;

                    bankAccount.isActive = true;
                    
              
                    // adds account object to list of all
                    allBankAccounts.Add(bankAccount);
               
        
            }

            workSheet = workBook.WorkSheets[3];
            for (var y = 2; y <= 6; y++)
            {
                 var cells = workSheet[$"A{y}:D{y}"].ToList();

                StockPortfolio stockPortfolio = new StockPortfolio();
                // set portfolio number
                stockPortfolio.PortfolioNumber = Convert.ToUInt32(cells[0].Value);
                // set portfolio name
                stockPortfolio.PortfolioName = cells[2].Value.ToString();
                // set portfolio user
                stockPortfolio.User = userManager.FindByEmailAsync(cells[1].Value.ToString()).Result;
                
                // set portfolio balance
                stockPortfolio.CashValuePortion = Convert.ToDecimal(cells[3].Value);

                stockPortfolio.isActive = true;

                allStockPortfolios.Add(stockPortfolio);

                
            }

            // at this point you have a list of all the bank account objects from the excel sheet
            Int32 intBankID = 0;
            String strBankAccount = "Start";

            try
            {
                foreach(BankAccount ba in allBankAccounts)
                {
                    intBankID = ba.BankAccountID;
                    strBankAccount = ba.BankAccountName;
                    //check to see if bankaccount is already in db
                    BankAccount dbBankAccount = _context.BankAccounts.FirstOrDefault(b => b.BankAccountNumber == ba.BankAccountNumber);
                    AppUser dbCustomer = userManager.FindByEmailAsync(ba.User.Email).Result;


                    // if its null then we can proceed to add to DB
                    if (dbBankAccount == null)
                    {
                        dbCustomer.UserHasAccount = true;
                        userManager.UpdateAsync(dbCustomer).Wait();
                        _context.BankAccounts.Add(ba);
                        _context.SaveChanges();
                    }
                    else
                    //Update fields
                    {
                        dbBankAccount.BankAccountNumber = ba.BankAccountNumber;
                        dbBankAccount.BankAccountName = ba.BankAccountName;
                        dbBankAccount.BankAccountType = ba.BankAccountType;
                        dbBankAccount.BankAccountBalance = ba.BankAccountBalance;
                        _context.SaveChanges();

                    }

                }
            } catch(Exception ex)// throw error if problem in 
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("There was a problem adding the bank account with the title: ");
                msg.Append(strBankAccount);
                msg.Append(" (BankAccountID: ");
                msg.Append(intBankID);
                msg.Append(")");
                throw new Exception(msg.ToString(), ex);
            }

            // at this point you have a list of all the bank account objects from the excel sheet
            
            Int32 intPortfolioID = 0;
            String PortfolioName = "Start";

            try
            {
                foreach (StockPortfolio sp in allStockPortfolios)
                {
                    intPortfolioID = sp.StockPortfolioID;
                    PortfolioName = sp.PortfolioName;
                    //check to see if bankaccount is already in db
                    StockPortfolio dbStockPortfolio = _context.StockPortfolios.FirstOrDefault(p => p.PortfolioNumber == sp.PortfolioNumber);
                    AppUser dbCustomer = userManager.FindByEmailAsync(sp.User.Email).Result;


                    // if its null then we can proceed to add to DB
                    if (dbStockPortfolio == null)
                    {
                        dbCustomer.UserHasAccount = true;
                        userManager.UpdateAsync(dbCustomer).Wait();
                        _context.StockPortfolios.Add(sp);
                        _context.SaveChanges();
                    }
                    else
                    //Update fields
                    {
                        dbStockPortfolio.PortfolioNumber = sp.PortfolioNumber;
                        dbStockPortfolio.PortfolioName= sp.PortfolioName;
                        dbStockPortfolio.CashValuePortion = sp.CashValuePortion;
                        _context.SaveChanges();

                    }

                }
            }
            catch (Exception ex)// throw error if problem in 
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("There was a problem adding the stock portfolio with the title: ");
                msg.Append(PortfolioName);
                msg.Append(" (StockPortfolioID: ");
                msg.Append(intPortfolioID);
                msg.Append(")");
                throw new Exception(msg.ToString(), ex);
            }
        }
    }
}
