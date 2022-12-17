
using Microsoft.AspNetCore.Identity;
using IronXL;

using LonghornBankWebApp.Models;
using LonghornBankWebApp.Utilities;
using LonghornBankWebApp.DAL;
using System.Text;
using System.Globalization;
using NuGet.Protocol;

namespace LonghornBankWebApp.Seeding
{
    public static class SeedTransactions
    {

        public static void SeedAllTransactions(UserManager<AppUser> userManager, AppDbContext _context)
        {
            String path = @"C:\Fall 2022\LonghornBankWebApp\LonghornBankWebApp\wwwroot\Files\BankData.xlsx";

            var workBook = new WorkBook(path);
            var workSheet = workBook.WorkSheets[5];

            List<Transaction> allTransactions = new List<Transaction>();


            //iterate through every transaction case
            for (var y = 2; y <= 12; y++)
            {
                var cells = workSheet[$"A{y}:I{y}"].ToList();


                if (cells[2].Value.ToString() == "Transfer")
                {
                    // Create two transaction objects
                    Transaction transactionTo = new Transaction();
                    Transaction transactionFrom = new Transaction();


                    // To (Positive amount)
                    transactionTo.TransactionNumber = Convert.ToInt32(cells[0].Value);
                    transactionTo.TransactionType = TransactionTypes.Transfer;

                    // See if its IRA/Checkings/Savings or StockPortfolio
                    if (_context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == Convert.ToUInt32(cells[3].Value)) != null)
                    {
                        transactionTo.BankAccount = _context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == Convert.ToUInt32(cells[3].Value));
                        transactionTo.BankAccount.User = userManager.FindByEmailAsync(cells[1].Value.ToString()).Result;

                    }
                    else
                    {
                        transactionTo.StockPortfolio = _context.StockPortfolios.FirstOrDefault(sp => sp.PortfolioNumber == Convert.ToUInt32(cells[3].Value));
                        transactionTo.StockPortfolio.User = userManager.FindByEmailAsync(cells[1].Value.ToString()).Result;
                    }


                    transactionTo.BankAccount = _context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == Convert.ToUInt32(cells[3].Value));
                    transactionTo.TransactionAmount = Convert.ToDecimal(cells[5].Value);
                    transactionTo.TransactionDate = Convert.ToDateTime(cells[6].Value);
                    // Check transaction stat
                    if (cells[7].ToString() == "Yes")
                    {
                        transactionTo.TransactionStatus = TransactionStatuses.Approved;
                    }
                    else
                    {
                        transactionTo.TransactionStatus = TransactionStatuses.Pending;
                     
                    }
                    transactionTo.TransactionNotes = cells[8].Value.ToString();



                    //From (Negative amount)
                    transactionFrom.TransactionNumber = Convert.ToInt32(cells[0].Value);
                    transactionFrom.TransactionType = TransactionTypes.Transfer;
                    // See if its IRA/Checkings/Savings or StockPortfolio
                    if (_context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == Convert.ToUInt32(cells[4].Value)) != null)
                    {
                        transactionFrom.BankAccount = _context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == Convert.ToUInt32(cells[4].Value));
                        transactionFrom.BankAccount.User = userManager.FindByEmailAsync(cells[1].Value.ToString()).Result;

                    }
                    else
                    {
                        transactionFrom.StockPortfolio = _context.StockPortfolios.FirstOrDefault(sp => sp.PortfolioNumber == Convert.ToUInt32(cells[4].Value));
                        transactionFrom.StockPortfolio.User = userManager.FindByEmailAsync(cells[1].Value.ToString()).Result;
                    }
                    transactionFrom.TransactionAmount = Convert.ToDecimal(cells[5].Value) * -1;
                    transactionFrom.TransactionDate = Convert.ToDateTime(cells[6].Value);
                    transactionFrom.TransactionStatus = transactionTo.TransactionStatus;
                    transactionFrom.TransactionNotes = cells[8].Value.ToString();

                    allTransactions.Add(transactionFrom);
                    allTransactions.Add(transactionTo);

                }


                else
                {
                    if (cells[2].Value.ToString() == "Withdraw")
                    {
                        Transaction withdraw = new Transaction();
                        withdraw.TransactionNumber = Convert.ToInt32(cells[0].Value);
                        withdraw.TransactionType = TransactionTypes.Withdrawal;

                        // See if its IRA/Checkings/Savings or StockPortfolio
                        if (_context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == Convert.ToUInt32(cells[4].Value)) != null)
                        {
                            withdraw.BankAccount = _context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == Convert.ToUInt32(cells[4].Value));
                            withdraw.BankAccount.User = userManager.FindByEmailAsync(cells[1].Value.ToString()).Result;

                        }
                        else
                        {
                            withdraw.StockPortfolio = _context.StockPortfolios.FirstOrDefault(sp => sp.PortfolioNumber == Convert.ToUInt32(cells[4].Value));
                            withdraw.StockPortfolio.User = userManager.FindByEmailAsync(cells[1].Value.ToString()).Result;
                        }

                        // Negative
                        withdraw.TransactionAmount = Convert.ToDecimal(cells[5].Value) * -1;
                        withdraw.TransactionDate = Convert.ToDateTime(cells[6].Value);
                        
                        if (cells[7].ToString() == "Yes")
                        {
                            withdraw.TransactionStatus = TransactionStatuses.Approved;
                        }
                        else
                        {
                            withdraw.TransactionStatus = TransactionStatuses.Pending;

                        }
                        
                        withdraw.TransactionNotes = cells[8].Value.ToString();

                        allTransactions.Add(withdraw);

                    }
                    if (cells[2].Value.ToString() == "Deposit")
                    {
                        Transaction deposit = new Transaction();
                        deposit.TransactionNumber = Convert.ToInt32(cells[0].Value);
                        deposit.TransactionType = TransactionTypes.Deposit;
                        // See if its IRA/Checkings/Savings or StockPortfolio
                        if (_context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == Convert.ToUInt32(cells[3].Value)) != null)
                        {
                            deposit.BankAccount = _context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == Convert.ToUInt32(cells[3].Value));
                            deposit.BankAccount.User = userManager.FindByEmailAsync(cells[1].Value.ToString()).Result;

                        }
                        else
                        {
                            deposit.StockPortfolio = _context.StockPortfolios.FirstOrDefault(sp => sp.PortfolioNumber == Convert.ToUInt32(cells[3].Value));
                            deposit.StockPortfolio.User = userManager.FindByEmailAsync(cells[1].Value.ToString()).Result;
                        }
                        // Positive Amount
                        deposit.TransactionAmount = Convert.ToDecimal(cells[5].Value);
                        deposit.TransactionDate = Convert.ToDateTime(cells[6].Value);
                        
                        if (cells[7].ToString() == "Yes")
                        {
                            deposit.TransactionStatus = TransactionStatuses.Approved;
                        }
                        else
                        {
                            deposit.TransactionStatus = TransactionStatuses.Pending;

                        }
                        
                        deposit.TransactionNotes = cells[8].Value.ToString();

                        allTransactions.Add(deposit);
                    }




                }


            }

            // at this point you have a list of all the bank account objects from the excel sheet
            Int32 transID = 0;
            String TransName = "Start";

            try
            {
                foreach (Transaction tr in allTransactions)
                {
                    transID = tr.TransactionNumber;

                    TransName = tr.TransactionNotes;
                    if (tr.BankAccount == null)
                    {
                        TransName = tr.StockPortfolio.PortfolioName;

                    }
                    else
                    {
                        TransName = tr.BankAccount.BankAccountName;

                    }

                    // these transactions can only have one transaction with same number
                    if (tr.TransactionType != TransactionTypes.Transfer)
                    {
                        //check to see if transaction is already in db
                        Transaction dbTransaction = _context.Transactions.FirstOrDefault(b => b.TransactionNumber == tr.TransactionNumber);

                        // if its null then we can proceed to add to DB
                        if (dbTransaction == null)
                        {
                            _context.Transactions.Add(tr);
                            _context.SaveChanges();
                        }

                    }
                    else
                    {
                        _context.Transactions.Add(tr);
                        _context.SaveChanges();
                    }

                }
            }

            catch (Exception ex)// throw error if problem in 
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("there was a problem adding the transcation with the bank account name: ");
                msg.Append(TransName);
                msg.Append(" (transaction number: ");
                msg.Append(transID);
                msg.Append(")");
                throw new Exception(msg.ToString(), ex);
            }

        }



        public static void SeedAllStockTrans(UserManager<AppUser> userManager, AppDbContext _context)
        {
            String path = @"C:\Fall 2022\LonghornBankWebApp\LonghornBankWebApp\wwwroot\Files\BankData.xlsx";

            var workBook = new WorkBook(path);
            var workSheet = workBook.WorkSheets[6];

            List<StockTransaction> allStockTrans = new List<StockTransaction>();
            int counter = 12;
            
            for (var y = 2; y <= 4; y++)
            {
                var cells = workSheet[$"A{y}:G{y}"].ToList();

                StockTransaction st = new StockTransaction();

           

                if (cells[1].Value.ToString() == "Purchase")
                {
                    // Funds from = purchase
                    st.StockPortfolio = _context.StockPortfolios.FirstOrDefault(sp => sp.User.Email == cells[0].Value.ToString());
                    st.PricePerShare = Convert.ToDecimal(cells[3].Value);
                    st.NumberOfShares = Convert.ToInt32(cells[4].Value);
                    st.TotalPrice = st.PricePerShare * st.NumberOfShares * -1;
                    st.StockTransactionType = StockTransactionTypes.Withdrawal;
                    st.StockPortfolio.User = userManager.FindByEmailAsync(cells[0].Value.ToString()).Result;
                    st.Stock = _context.Stocks.FirstOrDefault(s => s.TickerSymbol == cells[5].Value.ToString());
                    st.TransactionDate = Convert.ToDateTime(cells[6].Value);
                    st.StockTransactionNumber = counter;
                    counter += 1;
                    st.StockTransactionNotes = "Stock Purchase - Account " + st.StockPortfolio.PortfolioNumber.ToString();
                    st.Stock.StockType = _context.StockTypes.FirstOrDefault(stt => stt.Stocks.Contains(st.Stock));

                    // Purchase fee
                    StockTransaction fee = new StockTransaction();
                    fee.StockPortfolio = _context.StockPortfolios.FirstOrDefault(sp => sp.User.Email == cells[0].Value.ToString());
                    fee.TotalPrice = -10;
                    fee.StockTransactionType = StockTransactionTypes.Fee;
                    fee.StockPortfolio.User = st.StockPortfolio.User;
                    // NOTE: need stock to know which stock is associated with the fee
                    fee.Stock = st.Stock;
                    fee.TransactionDate = st.TransactionDate;
                    // Associate fee to purchase
                    fee.StockTransactionNumber = counter;
                    counter += 1;
                    fee.StockTransactionNotes = "Fee for purchase of " + st.Stock.StockName.ToString();
                    fee.Stock.StockType = st.Stock.StockType;

                    allStockTrans.Add(st);
                    allStockTrans.Add(fee);
                }
                if (cells[1].Value.ToString() == "Sell")
                {
                    // Funds to = sell
                    st.StockPortfolio = _context.StockPortfolios.FirstOrDefault(sp => sp.User.Email == cells[0].Value.ToString());
                    st.PricePerShare = Convert.ToDecimal(cells[3].Value);
                    st.NumberOfShares = Convert.ToInt32(cells[4].Value);
                    st.TotalPrice = st.PricePerShare * st.NumberOfShares;
                    st.StockTransactionType = StockTransactionTypes.Deposit;
                    st.StockTransactionNumber = counter;
                    counter += 1;
                    st.StockPortfolio.User = userManager.FindByEmailAsync(cells[0].Value.ToString()).Result;
                    st.Stock = _context.Stocks.FirstOrDefault(s => s.TickerSymbol == cells[5].Value.ToString());
                    st.TransactionDate = Convert.ToDateTime(cells[6].Value);
                    st.StockTransactionNotes = "Stock Sale - Account " + st.StockPortfolio.PortfolioNumber.ToString() + " - " + st.Stock.StockName.ToString() +
                       " - " + st.NumberOfShares.ToString() + " shares at $" + st.PricePerShare.ToString() + " per share";
                    
                    st.Stock.StockType = _context.StockTypes.FirstOrDefault(stt => stt.Stocks.Contains(st.Stock));

                    // Sell fee
                    StockTransaction fee = new StockTransaction();
                    fee.StockPortfolio = _context.StockPortfolios.FirstOrDefault(sp => sp.User.Email == cells[0].Value.ToString());
                    fee.TotalPrice = -15;
                    fee.StockTransactionType = StockTransactionTypes.Fee;
                    fee.StockTransactionNumber = counter;
                    counter += 1;
                    fee.StockPortfolio.User = st.StockPortfolio.User;
                    fee.Stock = st.Stock;
                    fee.TransactionDate = st.TransactionDate;
                    fee.StockTransactionNotes = "Fee for sale of " + st.Stock.StockName.ToString();
                    fee.Stock.StockType = st.Stock.StockType;

                    allStockTrans.Add(st);
                    allStockTrans.Add(fee);
                }
                // other if statements for other type of transaction (Bonus)
            }
                String TransName = "Start";

                try
                {
                    foreach (StockTransaction str in allStockTrans)
                    {
                        TransName = str.StockPortfolio.PortfolioName;

                        StockTransaction dbTransaction = _context.StockTransactions.FirstOrDefault(b => b.StockTransactionNumber == str.StockTransactionNumber);

                        if (dbTransaction == null)
                        {
                            _context.StockTransactions.Add(str);
                            _context.SaveChanges();
                        }
                    }


                }
                catch(Exception ex)
                {
                    StringBuilder msg = new StringBuilder();
                    msg.Append("there was a problem adding the transcation with the bank account name: ");
                    msg.Append(TransName);
                    throw new Exception(msg.ToString(), ex);
                }
        }
    }
}   
    