using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;

namespace LonghornBankWebApp.Utilities
{
    public class GenerateNextNum
    {

        //NOTE: First account number should be 2290000001
        public static UInt32 GetNextAccountNum(AppDbContext _context)
        {
            const UInt32 START_NUMBER = 2290000001;

            UInt32 intMaxOrderNumber;
            UInt32 intNextOrderNumber;
            if (!_context.BankAccounts.Any() && !_context.StockPortfolios.Any())
            {
                intMaxOrderNumber = START_NUMBER;
            }
            else
            {

                intMaxOrderNumber = Math.Max(_context.BankAccounts.Max(c => c.BankAccountNumber), _context.StockPortfolios.Max(c => c.PortfolioNumber));
            }
            if (intMaxOrderNumber < START_NUMBER)
            {
                intMaxOrderNumber = START_NUMBER;
            }

            intNextOrderNumber = intMaxOrderNumber + 1;


            BankAccount ba = _context.BankAccounts.FirstOrDefault(ba => ba.BankAccountNumber == intNextOrderNumber);
            StockPortfolio portfolio = _context.StockPortfolios.FirstOrDefault(ba => ba.PortfolioNumber == intNextOrderNumber);

            if (ba != null & portfolio != null)
            {
                return 1;
            }

            return intNextOrderNumber;
        }

        public static Int32 GetNextTransactionNum(AppDbContext _context)
        {
            const Int32 START_NUMBER = 1;
            Int32 intMaxTNumber;
            Int32 intNextTNumber;



            

            if (!_context.Transactions.Any() && !_context.StockTransactions.Any())
            {
                intMaxTNumber = START_NUMBER;
            }
            else
            {

                intMaxTNumber = Math.Max(_context.Transactions.Max(c => c.TransactionNumber), _context.StockTransactions.Max(c => c.StockTransactionNumber));

            }
            if (intMaxTNumber < START_NUMBER )
            {
                intMaxTNumber = START_NUMBER;
            }
            

            intNextTNumber = intMaxTNumber + 1;
 

            return intNextTNumber;
        }

        

    }
}
