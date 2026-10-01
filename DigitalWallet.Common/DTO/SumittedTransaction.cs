using DigitalWallet.Common.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Common.DTO
{
    public class SumittedTransaction
    {
        public string FromWalletId { get; set; }
        public string? ToWalletId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Currency { get; set; }
        public string Reference { get; set; }
        public string IdempotencyKey { get; set; }
    }
}
