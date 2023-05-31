using IronXL;
using LonghornBankWebApp.DAL;
using LonghornBankWebApp.Models;
using LonghornBankWebApp.Utilities;
using Microsoft.AspNetCore.Identity;

namespace LonghornBankWebApp.Seeding
{

    public static class SeedUsers
    {
        public async static Task<IdentityResult> SeedAllUsers(UserManager<AppUser> userManager, AppDbContext context)
        {
            //ADDED: Read in data from Excel file
            String path = @"C:\Fall 2022\LonghornBankWebApp\LonghornBankWebApp\wwwroot\Files\BankData.xlsx";
            var workBook = new WorkBook(path);
            var workSheet1 = workBook.WorkSheets.First();

            //Create a list of AddUserModels
            List<AddUserModel> AllUsers = new List<AddUserModel>();

            //ADDED: Iterate through every customer case in the Excel file
            for (var y = 2; y <= 52; y++)
            {
                // Gets all the fields from customer
                var cells = workSheet1[$"A{y}:K{y}"].ToList();
                AllUsers.Add(new AddUserModel()
                {
                    User = new AppUser()
                    {
                        UserName = cells[0].Value.ToString(),
                        Email = cells[0].Value.ToString(),
                        PhoneNumber = cells[9].Value.ToString(),
                        FirstName = cells[2].Value.ToString(),
                        MidIntName = cells[4].Value.ToString(),
                        LastName = cells[3].Value.ToString(),
                        Street = cells[5].Value.ToString(),
                        City = cells[6].Value.ToString(),
                        State = cells[7].Value.ToString(),
                        ZipCode = cells[8].Value.ToString(),
                        DOB = Convert.ToDateTime(cells[10].Value),
                        IsActive = true
                    },
                    Password = cells[1].Value.ToString(),
                    RoleName = "Customer"
                }); ;

            }
            //ADDED: Iterate through every employee case in the Excel file
            // employees need SSN & EmpType and lack DOB

            var workSheet2 = workBook.WorkSheets[1];

            for (var y = 2; y <= 23; y++)
            {
                // Gets all the fields from customer
                var cells = workSheet2[$"A{y}:L{y}"].ToList();
                AllUsers.Add(new AddUserModel()
                {
                    User = new AppUser()
                    {
                        UserName = cells[0].Value.ToString(),
                        Email = cells[0].Value.ToString(),
                        PhoneNumber = cells[11].Value.ToString(),
                        FirstName = cells[1].Value.ToString(),
                        MidIntName = cells[2].Value.ToString(),
                        LastName = cells[3].Value.ToString(),
                        Street = cells[7].Value.ToString(),
                        City = cells[8].Value.ToString(),
                        State = cells[9].Value.ToString(),
                        ZipCode = cells[10].Value.ToString(),
                        SSN = cells[5].Value.ToString(),
                        IsActive = true
                    },
                    Password = cells[4].Value.ToString(),
                    RoleName = cells[6].Value.ToString() == "Employee" ?
                            "Employee" : "Admin"
                });

            }

            //create flag to help with errors
            String errorFlag = "Start";

            //create an identity result
            IdentityResult result = new IdentityResult();
            //call the method to seed the user
            try
            {
                foreach (AddUserModel aum in AllUsers)
                {
                    errorFlag = aum.User.Email;
                    result = await Utilities.AddUser.AddUserWithRoleAsync(aum, userManager, context);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("There was a problem adding the user with email: "
                    + errorFlag, ex);
            }

            return result;
        }
    }
}
