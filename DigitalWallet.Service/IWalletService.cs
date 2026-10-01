using DigitalWallet.Common.DTO;
using DigitalWallet.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Service
{
    public interface IWalletService
    {
        Task<SumittedTransactionResponse> SubmitTransaction(SumittedTransaction transaction);
        Task<WalletSummary?> GetSummary(string walletId, DateTime? from, DateTime? to);
    }
}
