using DigitalWallet.Common.DTO;
using DigitalWallet.Common.Enum;
using DigitalWallet.Repository;
using DigitalWallet.Repository.Data.EmployeeManagementAPI.Repository.Data;
using DigitalWallet.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalWallet.Service
{
    public class WalletService : BaseService, IWalletService
    {
        private const int MaxConcurrencyRetries = 3;

        private readonly IWalletManagementRepository _walletManagementRepository;

        public WalletService(AppDbContext context, IWalletManagementRepository walletManagementRepository) : base(context)
        {
            _walletManagementRepository = walletManagementRepository;
        }

        public async Task<SumittedTransactionResponse> SubmitTransaction(SumittedTransaction request)
        {
            var validationError = Validate(request);
            if (validationError is not null)
            {
                return Rejected(request, validationError);
            }

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existing = await _walletManagementRepository.GetTransactionByIdempotencyKey(request.IdempotencyKey);
                if (existing is not null)
                {
                    return Map(existing);
                }
            }

            for (var attempt = 1; attempt <= MaxConcurrencyRetries; attempt++)
            {
                try
                {
                    return await ProcessAsync(request);
                }
                catch (DbUpdateConcurrencyException ex) when (attempt < MaxConcurrencyRetries)
                {
                    foreach (var entry in ex.Entries)
                    {
                        await entry.ReloadAsync();
                    }

                    foreach (var staged in Context.ChangeTracker.Entries<Transaction>()
                                 .Where(e => e.State == EntityState.Added)
                                 .ToList())
                    {
                        staged.State = EntityState.Detached;
                    }
                }
            }

            return Failed(request, "Transaction could not be completed due to concurrent updates. Please retry.");
        }

        private async Task<SumittedTransactionResponse> ProcessAsync(SumittedTransaction request)
        {
            var fromWallet = await _walletManagementRepository.GetById(request.FromWalletId);
            if (fromWallet is null)
            {
                return Rejected(request, $"Wallet '{request.FromWalletId}' was not found.");
            }

            if (fromWallet.Status != WalletStatus.Active)
            {
                return Rejected(request, "Source wallet is not active.");
            }

            if (!string.Equals(fromWallet.Currency, request.Currency, StringComparison.OrdinalIgnoreCase))
            {
                return Rejected(request, "Currency does not match the source wallet currency.");
            }

            var isTransfer = request.TransactionType == TransactionType.TransferOut;
            Wallet? toWallet = null;

            if (isTransfer)
            {
                if (string.IsNullOrWhiteSpace(request.ToWalletId))
                {
                    return Rejected(request, "Destination wallet is required for a transfer.");
                }

                if (string.Equals(request.ToWalletId, request.FromWalletId, StringComparison.OrdinalIgnoreCase))
                {
                    return Rejected(request, "Source and destination wallets must be different.");
                }

                toWallet = await _walletManagementRepository.GetById(request.ToWalletId);
                if (toWallet is null)
                {
                    return Rejected(request, $"Wallet '{request.ToWalletId}' was not found.");
                }

                if (toWallet.Status != WalletStatus.Active)
                {
                    return Rejected(request, "Destination wallet is not active.");
                }

                if (!string.Equals(toWallet.Currency, request.Currency, StringComparison.OrdinalIgnoreCase))
                {
                    return Rejected(request, "Currency does not match the destination wallet currency.");
                }
            }

            var isDebit = request.TransactionType is TransactionType.WithDrawal or TransactionType.TransferOut;
            if (isDebit && fromWallet.balance < request.Amount)
            {
                return Rejected(request, "Insufficient balance.");
            }

            var now = DateTime.UtcNow;

            fromWallet.balance = isDebit
                ? fromWallet.balance - request.Amount
                : fromWallet.balance + request.Amount;

            var transaction = new Transaction
            {
                Type = request.TransactionType,
                WalletId = fromWallet.Id,
                Amount = request.Amount,
                CounterParty = request.ToWalletId ?? string.Empty,
                Status = TransactionStatus.Completed,
                Reference = request.Reference ?? string.Empty,
                IdempotencyKey = request.IdempotencyKey,
                BalanceAfter = fromWallet.balance,
                CreatedAt = now,
                CompletedAt = now,
            };

            await _walletManagementRepository.SubmitTransaction(transaction);

            if (isTransfer && toWallet is not null)
            {
                toWallet.balance += request.Amount;

                var counterpartTransaction = new Transaction
                {
                    Type = TransactionType.TransferIn,
                    WalletId = toWallet.Id,
                    Amount = request.Amount,
                    CounterParty = fromWallet.Id,
                    Status = TransactionStatus.Completed,
                    Reference = request.Reference ?? string.Empty,
                    IdempotencyKey = request.IdempotencyKey is null ? null : $"{request.IdempotencyKey}:in",
                    BalanceAfter = toWallet.balance,
                    CreatedAt = now,
                    CompletedAt = now,
                };

                await _walletManagementRepository.SubmitTransaction(counterpartTransaction);
            }

            await SaveChangesAsync();

            return new SumittedTransactionResponse
            {
                TransactionId = transaction.Id,
                Type = transaction.Type,
                Status = transaction.Status,
                Amount = transaction.Amount,
                Currency = request.Currency,
                FromWalletId = fromWallet.Id,
                ToWalletId = toWallet?.Id ?? string.Empty,
                BalanceAfter = transaction.BalanceAfter,
                CreatedAT = transaction.CreatedAt,
                CompletedAT = transaction.CompletedAt ?? now,
            };
        }

        public async Task<WalletSummary?> GetSummary(string walletId, DateTime? from, DateTime? to)
        {
            if (string.IsNullOrWhiteSpace(walletId))
            {
                return null;
            }

            var wallet = await _walletManagementRepository.GetById(walletId);
            if (wallet is null)
            {
                return null;
            }

            var periodEnd = to ?? DateTime.UtcNow;
            var periodStart = from ?? new DateTime(periodEnd.Year, periodEnd.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var transactions = await _walletManagementRepository.GetTransactionsUpTo(walletId, periodEnd);

            var openingBalance = transactions
                .Where(t => t.CreatedAt < periodStart)
                .Select(t => (decimal?)t.BalanceAfter)
                .LastOrDefault() ?? 0m;

            var periodTransactions = transactions
                .Where(t => t.CreatedAt >= periodStart && t.CreatedAt <= periodEnd)
                .ToList();

            decimal SumCompleted(Func<Transaction, bool> predicate) => periodTransactions
                .Where(t => t.Status == TransactionStatus.Completed && predicate(t))
                .Sum(t => t.Amount);

            var closingBalance = periodTransactions.Count > 0
                ? periodTransactions[^1].BalanceAfter
                : openingBalance;

            return new WalletSummary
            {
                Wallet = wallet.Id,
                Period = $"{periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}",
                OpeningBalance = openingBalance,
                TotalWithDrawn = SumCompleted(t => t.Type == TransactionType.WithDrawal),
                TotalSent = SumCompleted(t => t.Type == TransactionType.TransferOut),
                TotalReceived = SumCompleted(t => t.Type is TransactionType.Deposit or TransactionType.Credit or TransactionType.TransferIn),
                ClosingBalance = closingBalance,
                TotalPending = periodTransactions.Count(t => t.Status == TransactionStatus.Pending),
                TotalRejected = periodTransactions.Count(t => t.Status == TransactionStatus.Rejected),
            };
        }

        private static string? Validate(SumittedTransaction request)
        {
            if (string.IsNullOrWhiteSpace(request.FromWalletId))
            {
                return "FromWalletId is required.";
            }

            if (request.Amount <= 0)
            {
                return "Amount must be greater than zero.";
            }

            if (request.TransactionType is TransactionType.None or TransactionType.TransferIn)
            {
                return "A valid transaction type is required.";
            }

            if (string.IsNullOrWhiteSpace(request.Currency))
            {
                return "Currency is required.";
            }

            return null;
        }

        private static SumittedTransactionResponse Rejected(SumittedTransaction request, string reason) => new()
        {
            Type = request.TransactionType,
            Status = TransactionStatus.Rejected,
            Amount = request.Amount,
            Currency = request.Currency,
            FromWalletId = request.FromWalletId,
            ToWalletId = request.ToWalletId ?? string.Empty,
            FailureReason = reason,
            CreatedAT = DateTime.UtcNow,
        };

        private static SumittedTransactionResponse Failed(SumittedTransaction request, string reason) => new()
        {
            Type = request.TransactionType,
            Status = TransactionStatus.Failed,
            Amount = request.Amount,
            Currency = request.Currency,
            FromWalletId = request.FromWalletId,
            ToWalletId = request.ToWalletId ?? string.Empty,
            FailureReason = reason,
            CreatedAT = DateTime.UtcNow,
        };

        private static SumittedTransactionResponse Map(Transaction transaction) => new()
        {
            TransactionId = transaction.Id,
            Type = transaction.Type,
            Status = transaction.Status,
            Amount = transaction.Amount,
            Currency = transaction.Wallet?.Currency ?? string.Empty,
            FromWalletId = transaction.WalletId,
            ToWalletId = transaction.Type == TransactionType.TransferOut ? transaction.CounterParty : string.Empty,
            FailureReason = transaction.FailureReason ?? string.Empty,
            BalanceAfter = transaction.BalanceAfter,
            CreatedAT = transaction.CreatedAt,
            CompletedAT = transaction.CompletedAt ?? transaction.CreatedAt,
        };
    }
}
