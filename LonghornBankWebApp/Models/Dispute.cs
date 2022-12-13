using System.ComponentModel.DataAnnotations;

namespace LonghornBankWebApp.Models
{

    public enum DisputeStatus { Submitted, Accepted, Rejected, Adjusted}
    public class Dispute
    {

        public Int32 DisputeID { get; set; }

        [Required(ErrorMessage = "Dispute description is required")]
        [Display(Name = "Dispute Notes")]
        public String DisputeNotes { get; set; }

        [Required(ErrorMessage = "Correct Amount is required")]
        [Display(Name = "Correct Amount")]
        public Decimal CorrectAmount { get; set; }

        [Required]
        [Display(Name = "Delete Transaction")]
        public Boolean DeleteTransaction { get; set; }

        public Decimal oldAmount { get; set; }

        [Display(Name = "Dispute Status")]
        public DisputeStatus DisputeStatus { get; set; }

        public String AdminNotes { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime dateResolved { get; set; }

        public String adminMessage { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime dateCreated { get; set; }

        public String adminEmail { get; set; }
        
        // navigational properties
        public Transaction Transaction { get; set; }



    }
}
