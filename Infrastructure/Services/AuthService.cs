using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using EcommerceInventoryApi.Core.DTOs;
using EcommerceInventoryApi.Core.Entities;
using EcommerceInventoryApi.Core.Interfaces;

namespace EcommerceInventoryApi.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());
            if (existingUsers.Any())
            {
                throw new InvalidOperationException("El correo electrónico ya se encuentra registrado.");
            }

            var user = new User
            {
                Email = registerDto.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName.Trim(),
                LastName = registerDto.LastName.Trim(),
                Role = registerDto.Role
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == loginDto.Email.ToLower().Trim());
            var user = users.FirstOrDefault();

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas. Verifique el correo o contraseña.");
            }

            return GenerateAuthResponse(user);
        }

        private AuthResponseDto GenerateAuthResponse(User user)
        {
            var jwtSecret = _configuration["Jwt:SecretKey"] ?? "SUPER_SECRET_KEY_ECOMMERCE_INVENTORY_API_2026_VERY_SECURE_KEY!";
            var issuer = _configuration["Jwt:Issuer"] ?? "EcommerceInventoryApi";
            var audience = _configuration["Jwt:Audience"] ?? "EcommerceInventoryApiUsers";
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "120");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = user.Role.ToString(),
                Token = tokenString,
                ExpiresAt = expiresAt
            };
        }
    }
}
