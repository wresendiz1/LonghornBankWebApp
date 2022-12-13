using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace LonghornBankWebApp.Models
{ 
    //NOTE: This is the view model used to allow the user to login
    //The user only needs teh email and password to login
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    //NOTE: This is the view model used to register a user
    //When the user registers, they only need to specify the
    //properties listed in this model
    public class RegisterViewModel
    {   
        //NOTE: Here is the property for email
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //NOTE: Here is the property for phone number
        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }


        //First name is provided as an example
        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        public String FirstName { get; set; }

        [Display(Name = "Middle Initial")]
        [RegularExpression(@"^[a-zA-Z]{1}$", ErrorMessage = "Middle initial must be 1 valid character long.")]
        public string? MidIntName { get; set; }

        
        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        public String LastName { get; set; }

        //ADDED: DOB & Full Address necessary for users to register.
        // MI optional

        [Required(ErrorMessage = "Street is required.")]
        [Display(Name = "Street")]
        public String Street { get; set; }

        [Required(ErrorMessage = "State is required.")]
        [Display(Name = "State")]
        public String State { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [Display(Name = "City")]
        public String City { get; set; }

      
        [Required(ErrorMessage = "Zip Code is required.")]
        [RegularExpression(@"^(?!00000)[0-9]{5,5}$", ErrorMessage = "Must be a valid 5 digit zip code.")]
        [DataType(DataType.PostalCode)]
        [Display(Name = "Zip Code")]
        public String ZipCode { get; set; }
        
        [Required(ErrorMessage = "Date of birth is required.")]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        //NOTE: Here is the logic for putting in a password
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }



    public class EmployeeRegisterViewModel
    {
        //NOTE: Here is the property for email
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //NOTE: Here is the property for phone number
        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }


        //First name is provided as an example
        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        public String FirstName { get; set; }

        [Display(Name = "Middle Initial")]
        [RegularExpression(@"^[a-zA-Z]{1,1}$", ErrorMessage = "Middle initial must be 1 valid character long.")]
        public String MidIntName { get; set; }


        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        public String LastName { get; set; }


        [Required(ErrorMessage = "Street is required.")]
        [Display(Name = "Street")]
        public String Street { get; set; }

        [Required(ErrorMessage = "State is required.")]
        [Display(Name = "State")]
        public String State { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [Display(Name = "City")]
        public String City { get; set; }

        [Required(ErrorMessage = "Zip Code is required.")]
        [RegularExpression(@"^(?!00000)[0-9]{5,5}$", ErrorMessage = "Must be a valid 5 digit zip code.")]
        [DataType(DataType.PostalCode)]
        [Display(Name = "Zip Code")]
        public String ZipCode { get; set; }

        //[Required(ErrorMessage = "Date of birth is required.")]
        //[Display(Name = "Date of Birth")]
        //[DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        //[DataType(DataType.Date)]
        //public DateTime DOB { get; set; }

        //NOTE: Here is the logic for putting in a password
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    //NOTE: This is the view model used to allow the user to 
    //change their password
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class EmpChangePasswordViewModel
    {

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    //NOTE: This is the view model used to display basic user information
    //on the index page
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public String UserName { get; set; }
        public String Email { get; set; }
        public String UserID { get; set; }

        // ADDED: Phone number & Full Address necessary for employees to modify
        public String Street { get; set; }

        public String City { get; set; }

        public String State { get; set; }

        public String ZipCode { get; set; }

        public String PhoneNumber { get; set; }

        // Concatenate full address
        public String FullAddress { get { return Street + ", " + City + ", " + State + " " + ZipCode; } }

        public String FirstName { get; set; }

        public String MidIntName { get; set; }

        public String LastName { get; set; }

        // Concatenate full name
        public String FullName { get { return FirstName + " " + MidIntName + " " + LastName; } }

    }

    public class ModifyViewModel
    {
        [Display(Name = "Street")]
        public String Street { get; set; }

        [Display(Name = "City")]
        public String City { get; set; }

        [Display(Name = "State")]
        public String State { get; set; }

        [Display(Name = "Zip Code")]
        public String ZipCode { get; set; }

        [Display(Name = "Phone Number")]
        [Phone]
        public String PhoneNumber { get; set; }

        [Display(Name = "First Name")]
        public String FirstName { get; set; }

        [Display(Name = "Middle Initial")]
        [RegularExpression(@"^[a-zA-Z]{1,1}$", ErrorMessage = "Middle initial must be 1 valid character long.")]
        public String MidIntName { get; set; }

        [Display(Name = "Last Name")]
        public String LastName { get; set; }



        public String Email { get; set; }
    }

    public class Messages
    {
        public IEnumerable<Message> UnreadMessages { get; set; }
        public IEnumerable<Message> ReadMessages { get; set; }

    }
}
