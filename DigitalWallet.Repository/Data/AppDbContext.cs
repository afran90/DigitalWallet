using DigitalWallet.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Repository.Data
{
    namespace EmployeeManagementAPI.Repository.Data
    {
        public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
        {
            public DbSet<User> Users => Set<User>();
            public DbSet<Wallet> Wallets => Set<Wallet>();
            public DbSet<Transaction> Transactions => Set<Transaction>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<User>(entity =>
                {
                    entity.HasIndex(e => e.Email).IsUnique();
                });

                modelBuilder.Entity<Wallet>(entity =>
                {
                    entity.Property(e => e.balance).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.RowVersion).IsRowVersion();
                    entity.HasOne(e => e.User).WithOne(w => w.Wallet);
                });

                modelBuilder.Entity<Transaction>(entity =>
                {
                    entity.HasOne(e => e.Wallet)
                        .WithMany(d => d.Transactions)
                        .HasForeignKey(e => e.WalletId)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.BalanceAfter).HasColumnType("decimal(18,2)");

                    entity.HasIndex(e => e.IdempotencyKey)
                        .IsUnique()
                        .HasFilter("[IdempotencyKey] IS NOT NULL");
                });

            }
        }
    }

}
