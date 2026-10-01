using DigitalWallet.Common.DTO;
using DigitalWallet.Repository;
using DigitalWallet.Repository.Data.EmployeeManagementAPI.Repository.Data;
using DigitalWallet.Repository.Entity;
using DigitalWallet.Service.Security;

namespace DigitalWallet.Service
{
    public class UserService : BaseService, IUserService
    {
        private readonly IUserManagementRepository _userRepository;
        private readonly JwtTokenGenerator _tokenGenerator;

        public UserService(AppDbContext context, IUserManagementRepository userRepository, JwtTokenGenerator tokenGenerator) : base(context)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<UserRegitration?> CreateAsync(UserRegitration dto)
        {
            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing is not null)
            {
                return null;
            }

            var wallet = new Wallet
            {
                Currency = "BDT",
                balance = 0,
                Status = Common.Enum.WalletStatus.Active
            };

            var user = new User
            {
                Email = dto.Email,
                FullName = dto.FullName,
                Password = PasswordHasher.Hash(dto.Password),
                Status = Common.Enum.UserStatus.Active,
                RegisteredAt = DateTime.UtcNow,
                Wallet = wallet,
            };

            await _userRepository.AddAsync(user);
            await SaveChangesAsync();

            return dto;
        }

        public async Task<AuthResponse?> LoginAsync(LoginUserDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user is null || !PasswordHasher.Verify(dto.PassWord, user.Password))
            {
                return null;
            }

            var (token, expiresAt) = _tokenGenerator.GenerateToken(user.Id, user.Email);

            return new AuthResponse
            {
                Token = token,
                ExpiresAtUtc = expiresAt,
                Email = user.Email,
                FullName = user.FullName,
            };
        }
    }
}
