using Microsoft.AspNetCore.Identity;
using IronXL;

using LonghornBankWebApp.Models;
using LonghornBankWebApp.Utilities;
using LonghornBankWebApp.DAL;
using System.Text;
using System.Globalization;

namespace LonghornBankWebApp.Seeding
{
    public class SeedStocks
    {

        public static void SeedAllStockTypes(AppDbContext _context)
        {
            String path = @"C:\Fall 2022\LonghornBankWebApp\LonghornBankWebApp\wwwroot\Files\BankData.xlsx";

            var workBook = new WorkBook(path);
            var workSheet = workBook.WorkSheets[4];

            List<StockType> allStockTypes = new List<StockType>();

            for (var y = 2; y <= 21; y++)
            {
                var cells = workSheet[$"A{y}:D{y}"].ToList();

                StockType stockType = new StockType();

                stockType.StockTypeName = cells[1].Value.ToString();

               // our foreach does this too
                bool containsType = allStockTypes.Any(item => item.StockTypeName == stockType.StockTypeName);

                if (containsType == false)
                {
                    allStockTypes.Add(stockType);
                }

            }
            string StockTypeName = "Start";

            try
            {


                foreach (StockType s in allStockTypes)
                {

                    StockTypeName = s.StockTypeName;
                    StockType dbStockType = _context.StockTypes.FirstOrDefault(st => st.StockTypeName == s.StockTypeName);

                    if (dbStockType == null)
                    {
                        _context.StockTypes.Add(s);
                        _context.SaveChanges();
                    }
                }

            }

            catch (Exception ex)
            {
                StringBuilder message = new StringBuilder();
                message.Append("There was an error adding the stock types to the database: ");
                message.Append(StockTypeName);
                throw new Exception(message.ToString(), ex);
            }


        }



        public static void SeedAllStocks(AppDbContext _context)
        {
            String path = @"C:\Fall 2022\LonghornBankWebApp\LonghornBankWebApp\wwwroot\Files\BankData.xlsx";
            
            var workBook = new WorkBook(path);
            var workSheet = workBook.WorkSheets[4];

            List<Stock> allStocks = new List<Stock>();

            List<StockPrice> allStockPrices = new List<StockPrice>();

            for (var y = 2; y <= 21; y++)
            {
                var cells = workSheet[$"A{y}:D{y}"].ToList();

                Stock stock = new Stock();



                stock.TickerSymbol = cells[0].Value.ToString();

                stock.StockType = _context.StockTypes.FirstOrDefault(st => st.StockTypeName == (cells[1].Value).ToString());

                stock.StockName = cells[2].Value.ToString();

                stock.CurrentPrice = Convert.ToDecimal(cells[3].Value);

                allStocks.Add(stock);


                // Track history of stock prices
                StockPrice stockPrice = new StockPrice();
                
                stockPrice.CurrentPrice = Convert.ToDecimal(cells[3].Value);
                stockPrice.Date = DateTime.Now;
                stockPrice.Stock = stock;

                allStockPrices.Add(stockPrice);


            }
            String StockName = "Start";
            
            try
            {
                foreach (Stock s in allStocks)
                {
                    StockName = s.StockName;

                    // ticker symbol could be better?
                    Stock dbStock = _context.Stocks.FirstOrDefault(st => st.StockName == s.StockName);

                    if (dbStock == null)
                    {
                        _context.Stocks.Add(s);
                        _context.SaveChanges();
                    }
                }

            }
            catch (Exception ex)
            {
                StringBuilder message = new StringBuilder();
                message.Append("There was an error adding the stocks to the database: ");
                message.Append(StockName);
                throw new Exception(message.ToString(), ex);
            }

            //ADDED: Add stock prices to database
            try
            {
                foreach (StockPrice sp in allStockPrices)
                {
                    StockName = sp.Stock.StockName;
                    StockPrice dbStockPrice = _context.StockPrices.FirstOrDefault(st => st.Stock.TickerSymbol == sp.Stock.TickerSymbol);

                    if (dbStockPrice == null)
                    {
                        _context.StockPrices.Add(sp);
                        _context.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {
                StringBuilder message = new StringBuilder();
                message.Append("There was an error adding the stock prices to the database: ");
                message.Append(StockName);
                throw new Exception(message.ToString(), ex);
            }
        }

    }
}
