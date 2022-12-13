﻿// <auto-generated />
using System;
using LonghornBankWebApp.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LonghornBankWebApp.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20221202173930_Final")]
    partial class Final
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);
            SqlServerModelBuilderExtensions.HasServiceTierSql(modelBuilder, "'Basic'");
            SqlServerModelBuilderExtensions.HasPerformanceLevelSql(modelBuilder, "'Basic'");

            modelBuilder.Entity("AppUserMessage", b =>
                {
                    b.Property<string>("AdminsId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("MessagesMessageID")
                        .HasColumnType("int");

                    b.HasKey("AdminsId", "MessagesMessageID");

                    b.HasIndex("MessagesMessageID");

                    b.ToTable("AppUserMessage");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DOB")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("MidIntName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SSN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("State")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Street")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("UserHasAccount")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("ZipCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.BankAccount", b =>
                {
                    b.Property<int>("BankAccountID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("BankAccountID"), 1L, 1);

                    b.Property<decimal>("BankAccountBalance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("BankAccountName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("BankAccountNumber")
                        .HasColumnType("bigint");

                    b.Property<int>("BankAccountType")
                        .HasColumnType("int");

                    b.Property<decimal>("IRAContribution")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("IRAQualified")
                        .HasColumnType("bit");

                    b.Property<decimal>("InitialDeposit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("TransferDropDown")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("isActive")
                        .HasColumnType("bit");

                    b.HasKey("BankAccountID");

                    b.HasIndex("UserId");

                    b.ToTable("BankAccounts");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.Dispute", b =>
                {
                    b.Property<int>("DisputeID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DisputeID"), 1L, 1);

                    b.Property<string>("AdminNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("CorrectAmount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("DeleteTransaction")
                        .HasColumnType("bit");

                    b.Property<string>("DisputeNotes")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DisputeStatus")
                        .HasColumnType("int");

                    b.Property<int?>("TransactionID")
                        .HasColumnType("int");

                    b.Property<string>("adminEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("adminMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("dateCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("dateResolved")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("oldAmount")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("DisputeID");

                    b.HasIndex("TransactionID");

                    b.ToTable("Disputes");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.Message", b =>
                {
                    b.Property<int>("MessageID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MessageID"), 1L, 1);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Info")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.Property<string>("Receiver")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Sender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Subject")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MessageID");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.PortfolioProcess", b =>
                {
                    b.Property<int>("PortfolioProcessID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PortfolioProcessID"), 1L, 1);

                    b.Property<DateTime>("DateProcessed")
                        .HasColumnType("datetime2");

                    b.Property<string>("ProcessedBy")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PortfolioProcessID");

                    b.ToTable("PortfolioProcesses");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.Stock", b =>
                {
                    b.Property<int>("StockID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StockID"), 1L, 1);

                    b.Property<decimal>("CurrentPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("StockName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("StockTypeID")
                        .HasColumnType("int");

                    b.Property<string>("TickerSymbol")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("StockID");

                    b.HasIndex("StockTypeID");

                    b.ToTable("Stocks");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.StockPortfolio", b =>
                {
                    b.Property<int>("StockPortfolioID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StockPortfolioID"), 1L, 1);

                    b.Property<string>("AppUserForeignKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<decimal>("CashValuePortion")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("IsBalanced")
                        .HasColumnType("bit");

                    b.Property<string>("PortfolioName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("PortfolioNumber")
                        .HasColumnType("bigint");

                    b.Property<decimal>("PortfolioValue")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalBonuses")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalFees")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalGains")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("TransferDropDown")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("isActive")
                        .HasColumnType("bit");

                    b.HasKey("StockPortfolioID");

                    b.HasIndex("AppUserForeignKey")
                        .IsUnique()
                        .HasFilter("[AppUserForeignKey] IS NOT NULL");

                    b.ToTable("StockPortfolios");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.StockTransaction", b =>
                {
                    b.Property<int>("StockTransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StockTransactionID"), 1L, 1);

                    b.Property<decimal>("GainLoss")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("NumberOfShares")
                        .HasColumnType("int");

                    b.Property<decimal>("PricePerShare")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("SellingTransactionNumber")
                        .HasColumnType("int");

                    b.Property<int?>("StockID")
                        .HasColumnType("int");

                    b.Property<int?>("StockPortfolioID")
                        .HasColumnType("int");

                    b.Property<string>("StockTransactionNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StockTransactionNumber")
                        .HasColumnType("int");

                    b.Property<int>("StockTransactionType")
                        .HasColumnType("int");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("datetime2");

                    b.HasKey("StockTransactionID");

                    b.HasIndex("StockID");

                    b.HasIndex("StockPortfolioID");

                    b.ToTable("StockTransactions");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.StockType", b =>
                {
                    b.Property<int>("StockTypeID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StockTypeID"), 1L, 1);

                    b.Property<string>("StockTypeName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("StockTypeID");

                    b.ToTable("StockTypes");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.Transaction", b =>
                {
                    b.Property<int>("TransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TransactionID"), 1L, 1);

                    b.Property<int?>("BankAccountID")
                        .HasColumnType("int");

                    b.Property<int?>("StockPortfolioID")
                        .HasColumnType("int");

                    b.Property<decimal>("TransactionAmount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("TransactionNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TransactionNumber")
                        .HasColumnType("int");

                    b.Property<int>("TransactionStatus")
                        .HasColumnType("int");

                    b.Property<int>("TransactionType")
                        .HasColumnType("int");

                    b.HasKey("TransactionID");

                    b.HasIndex("BankAccountID");

                    b.HasIndex("StockPortfolioID");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("AppUserMessage", b =>
                {
                    b.HasOne("LonghornBankWebApp.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("AdminsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LonghornBankWebApp.Models.Message", null)
                        .WithMany()
                        .HasForeignKey("MessagesMessageID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.BankAccount", b =>
                {
                    b.HasOne("LonghornBankWebApp.Models.AppUser", "User")
                        .WithMany("BankAccounts")
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.Dispute", b =>
                {
                    b.HasOne("LonghornBankWebApp.Models.Transaction", "Transaction")
                        .WithMany("Disputes")
                        .HasForeignKey("TransactionID");

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.Stock", b =>
                {
                    b.HasOne("LonghornBankWebApp.Models.StockType", "StockType")
                        .WithMany("Stocks")
                        .HasForeignKey("StockTypeID");

                    b.Navigation("StockType");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.StockPortfolio", b =>
                {
                    b.HasOne("LonghornBankWebApp.Models.AppUser", "User")
                        .WithOne("StockPortfolio")
                        .HasForeignKey("LonghornBankWebApp.Models.StockPortfolio", "AppUserForeignKey");

                    b.Navigation("User");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.StockTransaction", b =>
                {
                    b.HasOne("LonghornBankWebApp.Models.Stock", "Stock")
                        .WithMany("StockTransactions")
                        .HasForeignKey("StockID");

                    b.HasOne("LonghornBankWebApp.Models.StockPortfolio", "StockPortfolio")
                        .WithMany("StockTransactions")
                        .HasForeignKey("StockPortfolioID");

                    b.Navigation("Stock");

                    b.Navigation("StockPortfolio");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.Transaction", b =>
                {
                    b.HasOne("LonghornBankWebApp.Models.BankAccount", "BankAccount")
                        .WithMany("Transactions")
                        .HasForeignKey("BankAccountID");

                    b.HasOne("LonghornBankWebApp.Models.StockPortfolio", "StockPortfolio")
                        .WithMany("Transactions")
                        .HasForeignKey("StockPortfolioID");

                    b.Navigation("BankAccount");

                    b.Navigation("StockPortfolio");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("LonghornBankWebApp.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("LonghornBankWebApp.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LonghornBankWebApp.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("LonghornBankWebApp.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.AppUser", b =>
                {
                    b.Navigation("BankAccounts");

                    b.Navigation("StockPortfolio");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.BankAccount", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.Stock", b =>
                {
                    b.Navigation("StockTransactions");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.StockPortfolio", b =>
                {
                    b.Navigation("StockTransactions");

                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.StockType", b =>
                {
                    b.Navigation("Stocks");
                });

            modelBuilder.Entity("LonghornBankWebApp.Models.Transaction", b =>
                {
                    b.Navigation("Disputes");
                });
#pragma warning restore 612, 618
        }
    }
}
