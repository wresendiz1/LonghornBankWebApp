namespace LonghornBankWebApp.Models
{
    public class StockPrice
    {
        public Int32 StockPriceID { get; set; }


        public Decimal CurrentPrice { get; set; }

        public DateTime Date { get; set; }


        // Navigational property
        public Stock Stock { get; set; }
    }
}
