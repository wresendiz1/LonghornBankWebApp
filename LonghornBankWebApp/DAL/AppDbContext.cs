using LonghornBankWebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.Emit;

namespace LonghornBankWebApp.DAL
{
    //NOTE: This class definition references the user class for this project.  
    //If your User class is called something other than AppUser, you will need
    //to change it in the line below
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            /// Specify the precision and scale for decimal types (VALIDATION 30000)
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(decimal));
                foreach (var property in properties)
                {
                    builder.Entity(entityType.Name).Property(property.Name).HasColumnType("decimal(18,2)");
                }
            }
            // Add the shadow property to the model
            builder.Entity<StockPortfolio>().Property<String>("AppUserForeignKey");

            //this code configures the 1:1 relationship between AppUser and StockPortfolio
            builder.Entity<AppUser>().HasOne(sp => sp.StockPortfolio).WithOne(u => u.User).HasForeignKey<StockPortfolio>("AppUserForeignKey");

            // builder.Entity<BankAccount>().Property(ba => ba.BankAccountBalance).HasPrecision(18, 2).HasColumnType("decimal(18,2)");

            base.OnModelCreating(builder);
        }

        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<StockPortfolio> StockPortfolios { get; set; }

        public DbSet<Dispute> Disputes { get; set; }

        public DbSet<StockTransaction> StockTransactions { get; set; }

        public DbSet<Stock> Stocks { get; set; }

        public DbSet<StockType> StockTypes { get; set; }

        public DbSet<PortfolioProcess> PortfolioProcesses { get; set; }

        public DbSet<Message> Messages { get; set; }

        //ADDED: This is the line that adds the StockPrice table to the database
        public DbSet<StockPrice> StockPrices { get; set; }

    }
}
