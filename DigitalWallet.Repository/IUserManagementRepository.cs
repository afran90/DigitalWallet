using DigitalWallet.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Repository
{
    public interface IUserManagementRepository
    {
        Task<User> AddAsync(User User);
        Task<User?> GetByEmailAsync(string email);
    }
}
