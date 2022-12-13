using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace LonghornBankWebApp.Models
{

    //ASK: Should gains be displayed separately for each stock or entire portfolio?
    public class StockPortfolio
    {
        public Int32 StockPortfolioID { get; set; }


        [Display(Name = "Portfolio Name")]
        public String PortfolioName { get; set; }


        [Display(Name = "Portfolio Number")]
        public UInt32 PortfolioNumber { get; set; }

        // Cash Value Portion + Stock Portion
        [Display(Name = "Total Value")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal PortfolioValue { get; set; }

        [Display(Name = "Total Fees Incurred")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal TotalFees { get; set; }

        [Display(Name = "Total Net Gains")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal TotalGains { get; set; }

        [Display(Name = "Total Bonuses")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal TotalBonuses { get; set; }



        // NOTE: Portfolio must be activates before it can be used
        public Boolean isActive { get; set; }

        // NOTE: To be balanced:
        // 1. At least two ordinary stocks
        // 2. One index fund
        // 3. One mutual fund
        [Display(Name = "Portfolio Balance")]
        public Boolean IsBalanced { get; set; }

        // CONSIDER: avaliable cash
        [Display(Name = "Avaliable Cash")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal CashValuePortion { get; set; }

        public String TransferDropDown { get; set; }

        //Navigational Properties

        public AppUser User { get; set; }

        // Buying or selling stock
        public List<StockTransaction> StockTransactions { get; set; }

        // Depositing or Withdrawing money from portfolio
        public List<Transaction> Transactions { get; set; }

        public StockPortfolio()
        {
            if (StockTransactions == null)
            {
                StockTransactions = new List<StockTransaction>();
            }
            if (Transactions == null)
            {
                Transactions = new List<Transaction>();
            }
         
        }

    }
}
