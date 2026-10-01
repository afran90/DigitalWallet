using DigitalWallet.Repository.Data.EmployeeManagementAPI.Repository.Data;
using DigitalWallet.Repository.Entity;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Repository
{
    public class WalletManagementRepository : IWalletManagementRepository
    {
        private readonly AppDbContext _appDbContext;
        public WalletManagementRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Transaction> SubmitTransaction(Transaction transaction)
        {
            await _appDbContext.AddAsync(transaction);
            return transaction;
        }

        public async Task<Wallet?> GetById(string id)
        {
            return await _appDbContext.Wallets.Where(_ => _.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Transaction?> GetTransactionByIdempotencyKey(string idempotencyKey)
        {
            return await _appDbContext.Transactions
                .Include(t => t.Wallet)
                .Where(t => t.IdempotencyKey == idempotencyKey)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Transaction>> GetTransactionsUpTo(string walletId, DateTime upTo)
        {
            return await _appDbContext.Transactions
                .AsNoTracking()
                .Where(t => t.WalletId == walletId && t.CreatedAt <= upTo)
                .OrderBy(t => t.CreatedAt)
                .ThenBy(t => t.Id)
                .ToListAsync();
        }
    }
}
