using DigitalWallet.Common.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Repository.Entity
{
    public class Transaction
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public string WalletId { get; set; }
        public decimal Amount { get; set; }
        public string CounterParty { get; set; }
        public TransactionStatus Status { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string? IdempotencyKey { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Wallet Wallet { get; set; }

    }
}
