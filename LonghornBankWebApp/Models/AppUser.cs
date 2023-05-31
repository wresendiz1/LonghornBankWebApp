using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace LonghornBankWebApp.Models
{


    public class AppUser : IdentityUser
    {
        //CONSIDER: Katie said we should set what is required
        // in the RegisterViewModel instead of here
        //https://piazza.com/class/l3p02vtw1t675g/post/411


        [Display(Name = "First Name")]
        public String FirstName { get; set; }

        // Optional
        [Display(Name = "Middle Initials")]
        public String MidIntName { get; set; }


        [Display(Name = "Last Name")]
        public String LastName { get; set; }


        [Display(Name = "Street")]
        public String Street { get; set; }


        [Display(Name = "State")]
        public String State { get; set; }


        [Display(Name = "City")]
        public String City { get; set; }


        [Display(Name = "Zip Code")]
        public String ZipCode { get; set; }

        //ADDED: DOB is not necessary for all users
        [Display(Name = "Date of Birth")]
        public DateTime DOB { get; set; }

        public Boolean IsActive { get; set; }

        public String SSN { get; set; }




        // Used to restrict features for certain users
        public Boolean UserHasAccount { get; set; }


        // Read-only properties

        [Display(Name = "Full Name")]
        public String FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }


        // Navigational Properties

        public StockPortfolio StockPortfolio { get; set; }


        public List<BankAccount> BankAccounts { get; set; }

        public List<Message> Messages { get; set; }

        public AppUser()
        {
            if (BankAccounts == null)
            {
                BankAccounts = new List<BankAccount>();
            }
            if (Messages == null)
            {
                Messages = new List<Message>();
            }
        }

    }
}
