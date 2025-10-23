using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Account;
using norviguet_control_fletes_api.Models.User;
using norviguet_control_fletes_api.Profiles;
using norviguet_control_fletes_api.Services;
using System.Security.Claims;

namespace norviguet_control_fletes_api.Tests
{
    public class AccountControllerTests
    {
        private readonly AccountController _controller;
        private readonly NorviguetDbContext _context;
        private readonly Mock<IAuthService> _authMock;
        private readonly Mock<IBlobStorageService> _blobMock;
        private readonly IMapper _mapper;

        public AccountControllerTests()
        {
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new NorviguetDbContext(options);

            _authMock = new Mock<IAuthService>();
            _blobMock = new Mock<IBlobStorageService>();
            _blobMock.Setup(b => b.GetBlobSasUrl(It.IsAny<string>(), It.IsAny<int>()))
                     .Returns<string, int>((file, _) => $"https://mock.blob/{file}");

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UserProfile>();
            });
            _mapper = config.CreateMapper();

            _controller = new AccountController(_authMock.Object, _context, _mapper, _blobMock.Object);

            // Simular usuario autenticado con Claim "id"
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim("id", "1")
        }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAccount_ReturnsOk_WithMappedUser_AndImageUrlResolved()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Id = 1,
                Name = "Franco",
                Role = UserRole.Admin,
                ImageUrl = "avatar1.png"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAccount();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var userDto = Assert.IsType<UserDto>(ok.Value);
            Assert.Equal("Franco", userDto.Name);
            Assert.StartsWith("https://mock.blob/", userDto.ImageUrl);
        }

        [Fact]
        public async Task UpdateAccountName_UpdatesNameSuccessfully()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Pepe", Role = UserRole.Pending });
            await _context.SaveChangesAsync();

            var dto = new UpdateAccountNameDto { Name = "NuevoNombre" };

            // Act
            var result = await _controller.UpdateAccountName(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updated = await _context.Users.FindAsync(1);
            Assert.Equal("NuevoNombre", updated!.Name);
        }

        [Fact]
        public async Task ChangeAccountPassword_ChangesPassword_WhenCurrentIsValid()
        {
            // Arrange
            var user = new User { Id = 1, Name = "Carlos", PasswordHash = "oldhash" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _authMock.Setup(a => a.VerifyPassword("actual", "oldhash")).Returns(true);
            _authMock.Setup(a => a.HashPassword("nuevo")).Returns("newhash");

            var dto = new ChangeAccountPasswordDto
            {
                CurrentPassword = "actual",
                NewPassword = "nuevo",
                ConfirmNewPassword = "nuevo"
            };

            // Act
            var result = await _controller.ChangeAccountPassword(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updated = await _context.Users.FindAsync(1);
            Assert.Equal("newhash", updated!.PasswordHash);
        }

        [Fact]
        public async Task ChangeAccountPassword_ReturnsBadRequest_WhenCurrentIsInvalid()
        {
            // Arrange
            var user = new User { Id = 1, Name = "Carlos", PasswordHash = "oldhash" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _authMock.Setup(a => a.VerifyPassword("actual", "oldhash")).Returns(false);

            var dto = new ChangeAccountPasswordDto
            {
                CurrentPassword = "actual",
                NewPassword = "nuevo",
                ConfirmNewPassword = "nuevo"
            };

            // Act
            var result = await _controller.ChangeAccountPassword(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var response = bad.Value!.ToString();
            Assert.Contains("INVALID_PASSWORD", response);
        }

        [Fact]
        public async Task UpdateAccountImage_UpdatesImageSuccessfully()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Franco", ImageUrl = null });
            await _context.SaveChangesAsync();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("avatar.png");
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));
            fileMock.Setup(f => f.ContentType).Returns("image/png");

            _blobMock.Setup(b => b.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), "image/png"))
                     .ReturnsAsync("https://mock.blob/avatar.png");

            // Act
            var result = await _controller.UpdateAccountImage(fileMock.Object);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var userDto = Assert.IsType<UserDto>(ok.Value);
            Assert.Equal("https://mock.blob/avatar.png", userDto.ImageUrl);
        }

        [Fact]
        public async Task UpdateAccountImage_ReturnsBadRequest_WhenNoFile()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Franco", ImageUrl = null });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.UpdateAccountImage(null);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file was received.", bad.Value);
        }

        [Fact]
        public async Task DeleteAccountImage_DeletesImageSuccessfully()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Franco", ImageUrl = "avatar1.png" });
            await _context.SaveChangesAsync();

            _blobMock.Setup(b => b.DeleteAsync("avatar1.png")).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAccountImage();

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updated = await _context.Users.FindAsync(1);
            Assert.Null(updated!.ImageUrl);
        }

        [Fact]
        public async Task DeleteAccount_RemovesUserSuccessfully()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Carlos", Role = UserRole.Pending });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteAccount();

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Users);
        }

        [Fact]
        public async Task DeleteAccount_ReturnsBadRequest_WhenDeletingLastAdmin()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Admin", Role = UserRole.Admin });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteAccount();

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var response = bad.Value!.ToString();
            Assert.Contains("CANNOT_DELETE_ALL_ADMINS", response);
        }
    }
}