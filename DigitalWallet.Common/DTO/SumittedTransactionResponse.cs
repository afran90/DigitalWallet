using DigitalWallet.Common.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Common.DTO
{
    public class SumittedTransactionResponse
    {
        public int TransactionId { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string FromWalletId { get; set; }
        public string ToWalletId { get; set; }
        public string FailureReason { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime CreatedAT { get; set; }
        public DateTime CompletedAT { get; set; }
    }
}
