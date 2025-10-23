using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.User;
using norviguet_control_fletes_api.Profiles;
using Xunit;

namespace norviguet_control_fletes_api.Tests
{
    public class UserControllerTests
    {
        private readonly UserController _controller;
        private readonly NorviguetDbContext _context;
        private readonly Mock<IBlobStorageService> _blobMock;
        private readonly IMapper _mapper;

        public UserControllerTests()
        {
            // Configuración de la base de datos en memoria
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new NorviguetDbContext(options);

            // Mock del servicio de blobs
            _blobMock = new Mock<IBlobStorageService>();
            _blobMock.Setup(b => b.GetBlobSasUrl(It.IsAny<string>(), It.IsAny<int>()))
                     .Returns<string, int>((file, _) => $"https://mock.blob/{file}");

            // Configuración de AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UserProfile>();
            });
            _mapper = config.CreateMapper();

            // Instanciación del controlador
            _controller = new UserController(_context, _mapper, _blobMock.Object);
        }

        // 🧩 1. GetUsers()
        [Fact]
        public async Task GetUsers_ReturnsOk_WithMappedUsers_AndImageUrlsResolved()
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
            var result = await _controller.GetUsers();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var users = Assert.IsType<List<UserDto>>(ok.Value);
            Assert.Single(users);
            Assert.Equal("Franco", users[0].Name);
            Assert.StartsWith("https://mock.blob/", users[0].ImageUrl);
        }

        // 🧩 2. UpdateUserRole() — actualiza rol correctamente
        [Fact]
        public async Task UpdateUserRole_UpdatesRoleSuccessfully()
        {
            // Arrange
            var user = new User { Id = 1, Name = "Pepe", Role = UserRole.Pending };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new UpdateUserRoleDto { Role = "Admin" };

            // Act
            var result = await _controller.UpdateUserRole(1, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updated = await _context.Users.FindAsync(1);
            Assert.Equal(UserRole.Admin, updated!.Role);
        }

        // 🧩 3. UpdateUserRole() — intenta cambiar el rol del único admin
        [Fact]
        public async Task UpdateUserRole_ReturnsBadRequest_WhenChangingLastAdmin()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Root", Role = UserRole.Admin });
            await _context.SaveChangesAsync();

            var dto = new UpdateUserRoleDto { Role = "Pending" };

            // Act
            var result = await _controller.UpdateUserRole(1, dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var response = bad.Value!.ToString();
            Assert.Contains("CANNOT_EDIT_LAST_ADMIN_ROLE", response);
        }

        // 🧩 4. DeleteUser() — elimina correctamente un usuario normal
        [Fact]
        public async Task DeleteUser_RemovesUserSuccessfully()
        {
            // Arrange
            var user = new User { Id = 1, Name = "Carlos", Role = UserRole.Pending };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteUser(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Users);
        }

        // 🧩 5. DeleteUser() — no puede eliminar el último admin
        [Fact]
        public async Task DeleteUser_ReturnsBadRequest_WhenDeletingLastAdmin()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Admin", Role = UserRole.Admin });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteUser(1);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var response = bad.Value!.ToString();
            Assert.Contains("CANNOT_DELETE_ALL_ADMINS", response);
        }

        // 🧩 6. DeleteUsers() — elimina usuarios por lote
        [Fact]
        public async Task DeleteUsers_DeletesMultipleUsersSuccessfully()
        {
            // Arrange
            _context.Users.AddRange(
                new User { Id = 1, Name = "U1", Role = UserRole.Pending },
                new User { Id = 2, Name = "U2", Role = UserRole.Pending },
                new User { Id = 3, Name = "Admin", Role = UserRole.Admin }
            );
            await _context.SaveChangesAsync();

            var dto = new DeleteUsersDto { Ids = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.DeleteUsers(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var remaining = await _context.Users.ToListAsync();
            Assert.Single(remaining);
            Assert.Equal("Admin", remaining[0].Name);
        }

        // 🧩 7. DeleteUsers() — no permite eliminar todos los admins
        [Fact]
        public async Task DeleteUsers_ReturnsBadRequest_WhenDeletingAllAdmins()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Admin", Role = UserRole.Admin });
            await _context.SaveChangesAsync();

            var dto = new DeleteUsersDto { Ids = new List<int> { 1 } };

            // Act
            var result = await _controller.DeleteUsers(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var response = bad.Value!.ToString();
            Assert.Contains("CANNOT_DELETE_ALL_ADMINS", response);
        }
    }
}
