using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Auth;
using norviguet_control_fletes_api.Models.DTOs.User;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Models.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace norviguet_control_fletes_api.Services
{
    public class AuthService(ApplicationDbContext context, IMapper mapper, IConfiguration configuration) : IAuthService
    {
        public async Task<LoginResponseDto> LoginAsync(LoginDto dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);

            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiresAt = dto.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7);

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = refreshTokenExpiresAt,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.RefreshTokens.Add(refreshTokenEntity);
            await context.SaveChangesAsync(cancellationToken);

            SetRefreshTokenCookie(httpContext, refreshToken, refreshTokenExpiresAt);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken)
        {
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new ConflictException("Email already exists.");
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Role = UserRole.Pending,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            return mapper.Map<UserDto>(user);
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(HttpContext httpContext, CancellationToken cancellationToken)
        {
            var refreshToken = httpContext.Request.Cookies["refreshToken"];
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new UnauthorizedException("Refresh token not found in cookies.");
            }

            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                var storedToken = await context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsActive, cancellationToken);

                if (storedToken == null)
                {
                    throw new UnauthorizedException("Invalid or expired refresh token.");
                }

                userId = storedToken.UserId;
            }

            var user = await context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var existingRefreshToken = user.RefreshTokens
                .FirstOrDefault(rt => rt.Token == refreshToken && rt.IsActive);

            if (existingRefreshToken == null)
            {
                throw new UnauthorizedException("Invalid or expired refresh token.");
            }

            var accessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var refreshTokenExpiresAt = existingRefreshToken.ExpiresAt;

            existingRefreshToken.RevokedAt = DateTime.UtcNow;

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                ExpiresAt = refreshTokenExpiresAt,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.RefreshTokens.Add(newRefreshTokenEntity);
            await context.SaveChangesAsync(cancellationToken);

            SetRefreshTokenCookie(httpContext, newRefreshToken, refreshTokenExpiresAt);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task RevokeTokenAsync(HttpContext httpContext, CancellationToken cancellationToken)
        {
            var refreshToken = httpContext.Request.Cookies["refreshToken"];
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new UnauthorizedException("Refresh token not found in cookies.");
            }

            var storedToken = await context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsActive, cancellationToken);

            if (storedToken == null)
            {
                throw new NotFoundException("Refresh token not found or already revoked.");
            }

            storedToken.RevokedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            DeleteRefreshTokenCookie(httpContext);
        }

        private string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["AppSettings:Token"] ?? throw new InvalidOperationException("Token key not configured"));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = configuration["AppSettings:Issuer"],
                Audience = configuration["AppSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private static void SetRefreshTokenCookie(HttpContext httpContext, string refreshToken, DateTime expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt
            };

            httpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private static void DeleteRefreshTokenCookie(HttpContext httpContext)
        {
            httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}

