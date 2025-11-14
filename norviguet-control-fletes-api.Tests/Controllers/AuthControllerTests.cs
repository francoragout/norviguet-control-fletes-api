using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Auth;
using norviguet_control_fletes_api.Profiles;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Tests
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly Mock<IAuthService> _authMock;
        private readonly IMapper _mapper;
        private readonly NorviguetDbContext _context;

        public AuthControllerTests()
        {
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new NorviguetDbContext(options);

            _authMock = new Mock<IAuthService>();

            var config = new MapperConfiguration(cfg =>
            {
                // Si tienes un perfil de AutoMapper, agrégalo aquí
                cfg.AddProfile<UserProfile>();
            });
            _mapper = config.CreateMapper();

            _controller = new AuthController(_authMock.Object, _mapper, _context);
        }

        // Utilidad para simular cookies en HttpContext.Request
        private static void SetRequestCookie(HttpContext httpContext, string key, string value)
        {
            var cookies = new Dictionary<string, string> { { key, value } };
            httpContext.Request.Headers["Cookie"] = string.Join("; ", cookies.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }

        // 🧩1. Register() — usuario nuevo
        [Fact]
        public async Task Register_CreatesUser()
        {
            // Arrange
            var request = new RegisterDto { Name = "Test", Email = "test@mail.com", Password = "Password1!", ConfirmPassword = "Password1!" };
            var user = new User { Id =2, Name = "Test", Email = "test@mail.com", Role = UserRole.Pending };
            _authMock.Setup(a => a.RegisterAsync(request)).ReturnsAsync(user);
            _context.Users.Add(new User { Id =1, Name = "Admin", Role = UserRole.Admin });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Register(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var response = ok.Value!.ToString();
            Assert.Contains("USER_CREATED", response);
        }

        // 🧩2. Register() — usuario ya existe
        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUserExists()
        {
            // Arrange
            var request = new RegisterDto { Name = "Test", Email = "test@mail.com", Password = "Password1!", ConfirmPassword = "Password1!" };
            _authMock.Setup(a => a.RegisterAsync(request)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var response = bad.Value!.ToString();
            Assert.Contains("USER_EXISTS", response);
        }

        // 🧩3. Login() — credenciales válidas y usuario aprobado
        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsValid_AndUserApproved()
        {
            // Arrange
            var request = new LoginDto { Email = "test@mail.com", Password = "Password1!", RememberMe = false };
            var tokenResponse = new TokenResponseDto { AccessToken = "access", RefreshToken = "refresh", RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1) };
            _authMock.Setup(a => a.LoginAsync(request)).ReturnsAsync(tokenResponse);
            _authMock.Setup(a => a.GetUserByEmailAsync(request.Email)).ReturnsAsync(new User { Id =1, Email = request.Email, Role = UserRole.Admin });
            // Ensure ControllerContext is set with a valid HttpContext
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = await _controller.Login(request);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TokenResponseDto>(ok.Value);
            Assert.Equal("access", response.AccessToken);
        }

        // 🧩4. Login() — credenciales inválidas
        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsInvalid()
        {
            // Arrange
            var request = new LoginDto { Email = "test@mail.com", Password = "wrong", RememberMe = false };
            _authMock.Setup(a => a.LoginAsync(request)).ReturnsAsync((TokenResponseDto?)null);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = unauthorized.Value!.ToString();
            Assert.Contains("INVALID_CREDENTIALS", response);
        }

        // 🧩5. Login() — usuario pendiente
        [Fact]
        public async Task Login_ReturnsForbidden_WhenUserPending()
        {
            // Arrange
            var request = new LoginDto { Email = "pending@mail.com", Password = "Password1!", RememberMe = false };
            var tokenResponse = new TokenResponseDto { AccessToken = "access", RefreshToken = "refresh", RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1) };
            _authMock.Setup(a => a.LoginAsync(request)).ReturnsAsync(tokenResponse);
            _authMock.Setup(a => a.GetUserByEmailAsync(request.Email)).ReturnsAsync(new User { Id =2, Email = request.Email, Role = UserRole.Pending });

            // Act
            var result = await _controller.Login(request);

            // Assert
            var forbidden = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(403, forbidden.StatusCode);
            var response = forbidden.Value!.ToString();
            Assert.Contains("USER_PENDING", response);
        }

        // 🧩6. RefreshToken() — token válido
        [Fact]
        public async Task RefreshToken_ReturnsOk_WhenTokenValid()
        {
            // Arrange
            var tokenResponse = new TokenResponseDto { AccessToken = "access", RefreshToken = "refresh", RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1) };
            _authMock.Setup(a => a.RefreshTokensAsync("validtoken")).ReturnsAsync(tokenResponse);
            var httpContext = new DefaultHttpContext();
            SetRequestCookie(httpContext, "refreshToken", "validtoken");
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.RefreshToken();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TokenResponseDto>(ok.Value);
            Assert.Equal("access", response.AccessToken);
        }

        // 🧩7. RefreshToken() — sin cookie
        [Fact]
        public async Task RefreshToken_ReturnsUnauthorized_WhenNoCookie()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.RefreshToken();

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("No refresh token cookie found.", unauthorized.Value);
        }

        // 🧩8. RefreshToken() — token inválido
        [Fact]
        public async Task RefreshToken_ReturnsUnauthorized_WhenTokenInvalid()
        {
            // Arrange
            _authMock.Setup(a => a.RefreshTokensAsync("invalidtoken")).ReturnsAsync((TokenResponseDto?)null);
            var httpContext = new DefaultHttpContext();
            SetRequestCookie(httpContext, "refreshToken", "invalidtoken");
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.RefreshToken();

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Invalid or expired refresh token.", unauthorized.Value);
        }

        // 🧩9. Logout() — elimina cookie y revoca token
        [Fact]
        public async Task Logout_RevokesTokenAndRemovesCookie()
        {
            // Arrange
            _authMock.Setup(a => a.InvalidateRefreshTokenAsync("refreshTokenValue")).Returns(Task.CompletedTask);
            var httpContext = new DefaultHttpContext();
            SetRequestCookie(httpContext, "refreshToken", "refreshTokenValue");
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
