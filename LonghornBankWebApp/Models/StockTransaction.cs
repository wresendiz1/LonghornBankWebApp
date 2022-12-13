using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace LonghornBankWebApp.Models
{

    // Withdrawal = Purchase
    // Deposit = Sell
 
    public enum StockTransactionTypes { Withdrawal, Deposit, Fee, Bonus }
    public class StockTransaction
    {
        public Int32 StockTransactionID { get; set; }

        public Int32 StockTransactionNumber { get; set; }

        // NOTE: Used to stock deposits and associate the # of sold shares with withdrawal
        public Int32 SellingTransactionNumber{get; set; }

        [Display(Name = "Price per Share")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal PricePerShare { get; set; }

        [Display(Name = "Number of Shares")]
        [Range(1, Int32.MaxValue, ErrorMessage = "Number of shares must be greater than 0")]
        [Required(ErrorMessage = "Number of shares is required")]
        public Int32 NumberOfShares { get; set; }

        [Display(Name = "Total Transaction Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal TotalPrice { get; set; }

        [Display(Name = "Stock Transaction Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Transaction date is required")]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "Stock Transaction Type")]
        public StockTransactionTypes StockTransactionType { get; set; }


        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal GainLoss { get; set; }

        // ADDED: must have notes
        [Display(Name = "Stock Transaction Notes")]
        public String StockTransactionNotes { get; set; }

        // Navigational properties
        public StockPortfolio StockPortfolio { get; set; }

        public Stock Stock { get; set; }



    }
}
