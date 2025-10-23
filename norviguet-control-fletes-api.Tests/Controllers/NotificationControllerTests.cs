using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Notification;
using norviguet_control_fletes_api.Profiles;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Tests
{
    public class NotificationControllerTests
    {
        private readonly NotificationController _controller;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly IMapper _mapper;
        private readonly NorviguetDbContext _context;

        public NotificationControllerTests()
        {
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
            _context = new NorviguetDbContext(options);

            _notificationServiceMock = new Mock<INotificationService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<NotificationProfile>(); // <-- Agrega esta línea
            });
            _mapper = config.CreateMapper();

            _controller = new NotificationController(_context, _mapper, _notificationServiceMock.Object);
        }

        private void SetUserContext(int userId)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] {
            new System.Security.Claims.Claim("id", userId.ToString())
            })
            );
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Fact]
        public async Task GetNotifications_ReturnsOk_WithNotifications()
        {
            // Arrange
            var userId = 1;
            SetUserContext(userId);
            _context.Notifications.Add(new Notification
            {
                Id = 1,
                Title = "Test",
                Message = "Msg",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Link = "",
                UserId = userId
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetNotifications();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var notifications = Assert.IsType<List<NotificationDto>>(ok.Value);
            Assert.Single(notifications);
        }

        [Fact]
        public async Task GetNotifications_ReturnsUnauthorized_IfNoUser()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = await _controller.GetNotifications();

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task CreateNotification_ReturnsCreated()
        {
            // Arrange
            var dto = new CreateNotificationDto
            {
                Title = "Test",
                Message = "Msg",
                IsRead = false,
                Link = "",
                UserId = 1
            };
            var notification = new Notification
            {
                Id = 1,
                Title = dto.Title,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = dto.IsRead,
                Link = dto.Link,
                UserId = dto.UserId
            };
            _notificationServiceMock.Setup(s => s.CreateNotificationAsync(dto)).ReturnsAsync(notification);

            // Act
            var result = await _controller.CreateNotification(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var notifDto = Assert.IsType<NotificationDto>(created.Value);
            Assert.Equal(dto.Title, notifDto.Title);
        }

        [Fact]
        public async Task UpdateNotifications_MarksAsRead_AndReturnsNoContent()
        {
            // Arrange
            var userId = 2;
            SetUserContext(userId);
            _context.Notifications.Add(new Notification
            {
                Id = 2,
                Title = "Test",
                Message = "Msg",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Link = "",
                UserId = userId
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.UpdateNotifications();

            // Assert
            Assert.IsType<NoContentResult>(result);
            var notif = await _context.Notifications.FirstAsync(n => n.Id == 2);
            Assert.True(notif.IsRead);
        }

        [Fact]
        public async Task UpdateNotifications_ReturnsNotFound_IfNoUnread()
        {
            // Arrange
            var userId = 3;
            SetUserContext(userId);
            _context.Notifications.Add(new Notification
            {
                Id = 3,
                Title = "Test",
                Message = "Msg",
                CreatedAt = DateTime.UtcNow,
                IsRead = true,
                Link = "",
                UserId = userId
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.UpdateNotifications();

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No hay notificaciones no leídas para este usuario.", notFound.Value);
        }
    }
}
