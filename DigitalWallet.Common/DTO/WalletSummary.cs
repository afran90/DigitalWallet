using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace DigitalWallet.Common.DTO
{
    public class WalletSummary
    {
        public string Wallet { get; set; }
        public string Period { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalWithDrawn { get; set; }
        public decimal TotalSent { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal ClosingBalance { get; set; }
        public int TotalPending { get; set; }
        public int TotalRejected { get; set; }




    }
}
