using System.ComponentModel.DataAnnotations;

namespace LonghornBankWebApp.Models
{
    public class Stock
    {

        public Int32 StockID { get; set; }

        [Display(Name = "Stock Name")]
        [Required(ErrorMessage = "Stock Name is required")]
        public String StockName { get; set; }

        [Display(Name = "Stock Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Required(ErrorMessage = "Stock Price is required")]
        public Decimal CurrentPrice { get; set; }

        [Display(Name = "Stock Symbol")]
        [RegularExpression(@"^[a-zA-Z]{1,5}$", ErrorMessage = "Stock symbol must be 1-5 letters")]
        [Required(ErrorMessage = "Ticker symbol is required")]
        public String TickerSymbol { get; set; }


        // navigational properties

        public StockType StockType { get; set; }

        public List<StockTransaction> StockTransactions { get; set; }

        public List<StockPrice> StockPrices { get; set; }

        public Stock()
        {
            if (StockTransactions == null)
            {
                StockTransactions = new List<StockTransaction>();

            }
            if (StockPrices == null)
            {
                StockPrices = new List<StockPrice>();
            }
        }
    }
}
