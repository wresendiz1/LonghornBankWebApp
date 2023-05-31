using System.ComponentModel.DataAnnotations;

namespace LonghornBankWebApp.Models.ViewModels
{

    public class SearchViewModel
    {

        public Int32 baIdentifier { get; set; }

        [Display(Name = "Transaction Number")]
        public String TransactionNumber { get; set; }

        [Display(Name = "Transaction Description")]
        public String TransactionDescription { get; set; }

        [Display(Name = "Transaction Type")]
        public TransactionTypes? SelectedType { get; set; }

        [Display(Name = "Transaction Range: Low")]
        public Int32? LowAmount { get; set; }

        [Display(Name = "Transaction Range: High")]
        public Int32? HighAmount { get; set; }

        [Display(Name = "Search Date Range (Low):")]
        [DataType(DataType.Date)]
        public DateTime? DateLow { get; set; }

        [Display(Name = "Search Date Range (High):")]
        [DataType(DataType.Date)]
        public DateTime? DateHigh { get; set; }

    }

}
