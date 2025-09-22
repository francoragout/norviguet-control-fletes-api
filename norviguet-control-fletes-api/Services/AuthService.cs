using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace norviguet_control_fletes_api.Services
{
    public class AuthService(NorviguetDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<TokenResponseDto?> LoginAsync(LoginDto request)
        {
            var user = await context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null) return null;

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponse(user, request.RememberMe);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user, bool rememberMe = false)
        {
            // Revocar todos los tokens activos previos (filtrado en memoria)
            var activeTokens = user.RefreshTokens
                .ToList()
                .Where(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToList();

            foreach (var token in activeTokens)
                token.RevokedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Generar y guardar nuevo refresh token
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user, rememberMe);
            var refreshTokenEntity = await context.RefreshTokens
                .OrderByDescending(rt => rt.CreatedAt)
                .FirstAsync(rt => rt.Token == refreshToken);

            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
            };
        }

        public async Task<User?> RegisterAsync(RegisterDto request)
        {
            if (await context.Users.AnyAsync(u => u.Email == request.Email))
                return null;

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = new PasswordHasher<User>().HashPassword(null!, request.Password)
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(string refreshToken)
        {
            var tokenEntity = await context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken &&
                                           rt.RevokedAt == null &&
                                           rt.ExpiresAt > DateTime.UtcNow);

            if (tokenEntity == null) return null;

            // Revocar el token actual
            tokenEntity.RevokedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return await CreateTokenResponse(tokenEntity.User);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user, bool rememberMe = false)
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7)
            };

            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();

            return refreshToken.Token;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("email", user.Email),
                new Claim("id", user.Id.ToString()),
                new Claim("role", user.Role.ToString()),
                new Claim("name", user.Name)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken &&
                                           rt.RevokedAt == null &&
                                           rt.ExpiresAt > DateTime.UtcNow);

            return tokenEntity?.User;
        }

        public async Task InvalidateRefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (tokenEntity != null && tokenEntity.RevokedAt == null && tokenEntity.ExpiresAt > DateTime.UtcNow)
            {
                tokenEntity.RevokedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task<RefreshToken?> GetLatestActiveRefreshTokenAsync(User user)
        {
            var refreshToken = user.RefreshTokens
                .ToList() // cargar en memoria
                .Where(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt)
                .FirstOrDefault();

            return await Task.FromResult(refreshToken);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            // Puedes reutilizar PasswordHasher de Microsoft.AspNetCore.Identity
            var result = new PasswordHasher<User>().VerifyHashedPassword(null!, passwordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public string HashPassword(string password)
        {
            return new PasswordHasher<User>().HashPassword(null!, password);
        }
    }
}
