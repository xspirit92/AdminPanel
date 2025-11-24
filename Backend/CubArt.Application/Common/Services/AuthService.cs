using CubArt.Application.Users.DTOs;
using CubArt.Domain.Entities;
using CubArt.Domain.Enums;
using CubArt.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CubArt.Application.Common.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(UserDto user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
        Task<Guid> GetCurrentUserIdAsync();
        Task<bool> HasPermissionAsync(PermissionEnum permission);
        Task<bool> HasAnyPermissionAsync(params PermissionEnum[] permissions);
        Task<bool> HasAllPermissionsAsync(params PermissionEnum[] permissions);
        Task<IEnumerable<PermissionEnum>> GetUserPermissionsAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public AuthService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }
        public Guid? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return null;

                return userId;
            }
        }

        public string GenerateJwtToken(UserDto user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    //new Claim("firstName", user.FirstName),
                    //new Claim("lastName", user.LastName)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
        }

        public async Task<Guid> GetCurrentUserIdAsync()
        {
            if (!UserId.HasValue)
                throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

            var user = await _userRepository.GetByIdAsync(UserId.Value);
            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException("Пользователь не найден или деактивирован");

            return UserId.Value;
        }

        public async Task<bool> HasPermissionAsync(PermissionEnum permission)
        {
            var userId = await GetCurrentUserIdAsync();
            var user = await _userRepository.GetByIdWithRolesAsync(userId);

            return user?.HasPermission(permission) ?? false;
        }

        public async Task<bool> HasAnyPermissionAsync(params PermissionEnum[] permissions)
        {
            var userPermissions = await GetUserPermissionsAsync();
            return permissions.Any(p => userPermissions.Contains(p));
        }

        public async Task<bool> HasAllPermissionsAsync(params PermissionEnum[] permissions)
        {
            var userPermissions = await GetUserPermissionsAsync();
            return permissions.All(p => userPermissions.Contains(p));
        }

        public async Task<IEnumerable<PermissionEnum>> GetUserPermissionsAsync()
        {
            var userId = await GetCurrentUserIdAsync();
            var user = await _userRepository.GetByIdWithRolesAsync(userId);

            return user?.GetPermissions() ?? Enumerable.Empty<PermissionEnum>();
        }
    }
}
