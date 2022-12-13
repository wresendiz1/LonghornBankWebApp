using System;
namespace LonghornBankWebApp.Models.ViewModels
{
    public class TransactionDispute
    {
        public Transaction transaction { get; set; }
        public Dispute dispute { get; set; }
    }
}

