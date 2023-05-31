using LonghornBankWebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
            //this code makes sure the database is re-created on the $5/month Azure tier
            builder.HasPerformanceLevel("Basic");
            builder.HasServiceTier("Basic");
            // Add the shadow property to the model
            builder.Entity<StockPortfolio>().Property<String>("AppUserForeignKey");

            //this code configures the 1:1 relationship between AppUser and StockPortfolio
            builder.Entity<AppUser>().HasOne(sp => sp.StockPortfolio).WithOne(u => u.User).HasForeignKey<StockPortfolio>("AppUserForeignKey");
            //builder.Entity<AppUser>().HasMany(b => b.BankAccounts).WithOne(u => u.User);

            // builder.Entity<BankAccount>().Property(ba => ba.BankAccountBalance).HasPrecision(18, 2).HasColumnType("decimal(18,2)");


            base.OnModelCreating(builder);
        }
        // NOTE: Avoid performance issues when returning query with (JOINS) navigational properties by configuring split queries
        // https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(
                    @"Server=tcp:longhornbanktrust.database.windows.net,1433;Initial Catalog=longhornbank;Persist Security Info=False;User ID=MISAdmin;Password=Passkey123;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
                    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
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
