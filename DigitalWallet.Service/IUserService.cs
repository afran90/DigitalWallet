using DigitalWallet.Common.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalWallet.Service
{
    public interface IUserService
    {

        Task<UserRegitration?> CreateAsync(UserRegitration dto);
        Task<AuthResponse?> LoginAsync(LoginUserDto dto);
    }
    
}
