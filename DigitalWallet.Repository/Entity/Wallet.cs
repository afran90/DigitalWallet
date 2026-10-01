using DigitalWallet.Common.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Repository.Entity
{
    public class Wallet
    {
        public string Id { get; set; }
        public int UserId { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal balance { get; set; }
        public WalletStatus Status { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public User User { get; set; }
    }
}
