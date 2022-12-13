using System.ComponentModel.DataAnnotations;

namespace LonghornBankWebApp.Models
{

    public class StockType
    {

        public Int32 StockTypeID { get; set; }

        [Display(Name = "Stock Type")]
        public String StockTypeName { get; set; }



        // Navigational propereties

        public List<Stock> Stocks { get; set; }


        public StockType()
        {
            if (Stocks == null)
            {
                Stocks = new List<Stock>();
            }
         
        }


    }
}
