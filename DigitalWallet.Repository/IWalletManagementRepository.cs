using DigitalWallet.Common.Enum;
using DigitalWallet.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Repository
{
    public interface IWalletManagementRepository
    {
        Task<Transaction> SubmitTransaction(Transaction transaction);
        Task<Wallet?> GetById(string id);
        Task<Transaction?> GetTransactionByIdempotencyKey(string idempotencyKey);
        Task<List<Transaction>> GetTransactionsUpTo(string walletId, DateTime upTo);
    }
}
