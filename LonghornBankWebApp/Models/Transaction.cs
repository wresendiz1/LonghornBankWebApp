using System;
using System.ComponentModel.DataAnnotations;


namespace LonghornBankWebApp.Models
{
    // NOTE: Savings can only contain standard transaction (deposits/withdrawals/transfers)
    public enum TransactionTypes { Deposit, Withdrawal, Transfer, Interest, Fee, Other, Bonus }
    
    public enum TransactionStatuses { Pending, Approved, Rejected, Scheduled, Deleted }

    public class Transaction
    {
        // ASK: how to do the 'enter by search string' do we need a seperate transaction number
        public Int32 TransactionID { get; set; }

        // ASK: does transaction number start anywhere specific? why does our db have id starting at 101 and num starting at 1
        [Display(Name = "Transaction Number")]
        public Int32 TransactionNumber { get; set; }

        [Required]
        [Display(Name = "Transaction Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(1.0, (double)Decimal.MaxValue, ErrorMessage = "Transaction amount must be greater than 0")]
        public Decimal TransactionAmount { get; set; }

        [Required]
        [Display(Name = "Transaction Type")]
        public TransactionTypes TransactionType { get; set; }


        [Display(Name = "Transaction Date")]
        //[DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Today;

        //ASK: Might need to add a transaction comments property, are they the same or different
        [Display(Name = "Transaction Notes")]
        public String TransactionNotes { get; set; }

        // Change seeding to reflect boolean to enum change
        [Display(Name = "Transaction Status")]
        public TransactionStatuses TransactionStatus { get; set; }

        // Navigational Properties

        [Display(Name = "Bank Account")]
        public BankAccount BankAccount { get; set; }

        public List<Dispute> Disputes { get; set; }

        // ADDED: In order to deposit and withdraw from stock portfolio
        public StockPortfolio StockPortfolio { get; set; }



        public Transaction()
        {
            if (Disputes == null)
            {
                Disputes = new List<Dispute>();
            }
        }

    }
}
