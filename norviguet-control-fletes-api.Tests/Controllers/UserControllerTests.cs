using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.User;
using AutoMapper;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserControllerTests
{
    private NorviguetDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<NorviguetDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new NorviguetDbContext(options);
    }

    private IMapper GetMapper()
    {
        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<List<UserDto>>(It.IsAny<List<User>>()))
            .Returns((List<User> users) =>
                users.Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email, Role = u.Role, ImageUrl = u.ImageUrl }).ToList());
        mapperMock.Setup(m => m.Map(It.IsAny<UpdateUserRoleDto>(), It.IsAny<User>()))
            .Callback((object src, object dest) =>
            {
                var dto = src as UpdateUserRoleDto;
                var user = dest as User;
                if (dto != null && user != null)
                {
                    if (Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
                        user.Role = parsedRole;
                }
            });
        return mapperMock.Object;
    }

    [Fact]
    public async Task GetUsers_ReturnsListOfUserDto_WithImageUrlTransformed()
    {
        var context = GetDbContext("GetUsersDb");
        context.Users.AddRange(
            new User { Id = 1, Name = "User 1", Email = "user1@mail.com", Role = UserRole.Admin, ImageUrl = "image1.png" },
            new User { Id = 2, Name = "User 2", Email = "user2@mail.com", Role = UserRole.Pending, ImageUrl = null }
        );
        context.SaveChanges();

        var blobServiceMock = new Mock<IBlobStorageService>();
        blobServiceMock.Setup(b => b.GetBlobSasUrl(It.IsAny<string>(), It.IsAny<int>())).Returns((string fileName, int _) => $"https://sasurl/{fileName}");

        var controller = new UserController(context, GetMapper(), blobServiceMock.Object);
        var result = await controller.GetUsers();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<UserDto>>(okResult.Value);
        Assert.Equal(2, returnedList.Count);
        Assert.Equal("https://sasurl/image1.png", returnedList[0].ImageUrl);
        Assert.Null(returnedList[1].ImageUrl);
    }

    [Fact]
    public async Task UpdateUserRole_ChangesRole_WhenValid()
    {
        var context = GetDbContext("UpdateUserRoleDb");
        context.Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@mail.com", Role = UserRole.Admin });
        context.Users.Add(new User { Id = 2, Name = "Admin2", Email = "admin2@mail.com", Role = UserRole.Admin });
        context.SaveChanges();
        var controller = new UserController(context, GetMapper(), new Mock<IBlobStorageService>().Object);
        var dto = new UpdateUserRoleDto { Role = "Pending" };
        var result = await controller.UpdateUserRole(1, dto);
        Assert.IsType<NoContentResult>(result);
        var user = context.Users.Find(1);
        Assert.NotNull(user);
        Assert.Equal(UserRole.Pending, user!.Role);
    }

    [Fact]
    public async Task UpdateUserRole_CannotEditLastAdmin()
    {
        var context = GetDbContext("UpdateUserRoleLastAdminDb");
        context.Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@mail.com", Role = UserRole.Admin });
        context.SaveChanges();
        var controller = new UserController(context, GetMapper(), new Mock<IBlobStorageService>().Object);
        var dto = new UpdateUserRoleDto { Role = "Pending" };
        var result = await controller.UpdateUserRole(1, dto);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("CANNOT_EDIT_LAST_ADMIN_ROLE", badRequest.Value.ToString());
    }

    [Fact]
    public async Task DeleteUser_RemovesUser_WhenValid()
    {
        var context = GetDbContext("DeleteUserDb");
        context.Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@mail.com", Role = UserRole.Pending });
        context.SaveChanges();
        var controller = new UserController(context, GetMapper(), new Mock<IBlobStorageService>().Object);
        var result = await controller.DeleteUser(1);
        Assert.IsType<NoContentResult>(result);
        var user = context.Users.Find(1);
        Assert.Null(user);
        Assert.Empty(context.Users);
    }

    [Fact]
    public async Task DeleteUser_CannotDeleteLastAdmin()
    {
        var context = GetDbContext("DeleteUserLastAdminDb");
        context.Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@mail.com", Role = UserRole.Admin });
        context.SaveChanges();
        var controller = new UserController(context, GetMapper(), new Mock<IBlobStorageService>().Object);
        var result = await controller.DeleteUser(1);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("CANNOT_DELETE_ALL_ADMINS", badRequest.Value.ToString());
    }

    [Fact]
    public async Task DeleteUsers_RemovesUsers_WhenValid()
    {
        var context = GetDbContext("DeleteUsersDb");
        context.Users.AddRange(
            new User { Id = 1, Name = "Admin", Email = "admin@mail.com", Role = UserRole.Admin },
            new User { Id = 2, Name = "User", Email = "user@mail.com", Role = UserRole.Pending }
        );
        context.SaveChanges();
        var controller = new UserController(context, GetMapper(), new Mock<IBlobStorageService>().Object);
        var dto = new DeleteUsersDto { Ids = new List<int> { 2 } };
        var result = await controller.DeleteUsers(dto);
        Assert.IsType<NoContentResult>(result);
        Assert.Single(context.Users);
        var remainingUser = context.Users.FirstOrDefault();
        Assert.NotNull(remainingUser);
        Assert.Equal(1, remainingUser!.Id);
    }

    [Fact]
    public async Task DeleteUsers_CannotDeleteAllAdmins()
    {
        var context = GetDbContext("DeleteUsersAllAdminsDb");
        context.Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@mail.com", Role = UserRole.Admin });
        context.SaveChanges();
        var controller = new UserController(context, GetMapper(), new Mock<IBlobStorageService>().Object);
        var dto = new DeleteUsersDto { Ids = new List<int> { 1 } };
        var result = await controller.DeleteUsers(dto);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("CANNOT_DELETE_ALL_ADMINS", badRequest.Value.ToString());
        var user = context.Users.Find(1);
        Assert.NotNull(user);
    }
}
