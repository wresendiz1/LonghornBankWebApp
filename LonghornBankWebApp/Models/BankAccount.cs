using System.ComponentModel.DataAnnotations;



namespace LonghornBankWebApp.Models
{
    // User can have unlimited checking and savings but only one IRA account

    // This model class refers to the bank accounts made by users and not website account


    public enum BankAccountTypes { Checking, Savings, IRA }
    public class BankAccount
    {

        public Int32 BankAccountID { get; set; }

        [Display(Name = "Account Number")]
        public UInt32 BankAccountNumber { get; set; }


        // This has a default value if an active account and can be changed to customer preffered nickname
        [Display(Name = "Account Nickname")]
        public String BankAccountName { get; set; }

        // NOTE: Savings can only contain standard transaction (deposits/withdrawals/transfers)
        // Only account type and inital deposit required when making an account
        [Required]
        [Display(Name = "Account Type")]
        public BankAccountTypes BankAccountType { get; set; }



        [Display(Name = "Account Balance")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal BankAccountBalance { get; set; }


        public Boolean isActive { get; set; }

        public String TransferDropDown { get; set; }

        //// NOTE: which admin deactivated this account
        //public AppUser WhoDeactivated { get; set; }

        //CONSIDER: For IRA Might need to add a current contribution account to check if it max contribution has been met
        [Display(Name = "Total contribution")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal IRAContribution { get; set; }

        //CONSIDER: For IRA, might need method or property to see if qualified
        public Boolean IRAQualified { get; set; }


        //CONSIDER: This should just be a property in the view model when creating an account
        [Display(Name = "Initial Deposit")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public Decimal InitialDeposit { get; set; }



        // Navigational Properties


        public AppUser User { get; set; }


        public List<Transaction> Transactions { get; set; }



        //ADDED: prevent null value

        public BankAccount()
        {
            if (Transactions == null)
            {

                Transactions = new List<Transaction>();

            }
        }






    }
}
