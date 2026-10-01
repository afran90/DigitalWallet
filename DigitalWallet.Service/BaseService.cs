using DigitalWallet.Repository.Data;
using DigitalWallet.Repository.Data.EmployeeManagementAPI.Repository.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Service
{
    public abstract class BaseService
    {
        protected readonly AppDbContext Context;

        protected BaseService(AppDbContext context)
        {
            Context = context;
        }

        protected Task<int> SaveChangesAsync() => Context.SaveChangesAsync();
    }
}
