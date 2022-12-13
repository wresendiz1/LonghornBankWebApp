using System.ComponentModel.DataAnnotations;

namespace LonghornBankWebApp.Models
{
    public class PortfolioProcess
    {

        public Int32 PortfolioProcessID { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime DateProcessed { get; set; }

        public String ProcessedBy { get; set; }
    }
}
