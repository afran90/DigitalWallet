using DigitalWallet.Repository.Data.EmployeeManagementAPI.Repository.Data;
using DigitalWallet.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Repository
{
    public class UserManagementRepository : IUserManagementRepository
    {
        private readonly AppDbContext _appDbContext;
        public UserManagementRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<User> AddAsync(User User)
        {
            await _appDbContext.AddAsync(User);
            return User;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _appDbContext.Users
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}
