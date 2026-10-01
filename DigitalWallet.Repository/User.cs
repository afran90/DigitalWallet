using DigitalWallet.Common.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Repository.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public UserStatus Status { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string WalletId { get; set; }
        public Wallet Wallet { get; set; }
    }
}
