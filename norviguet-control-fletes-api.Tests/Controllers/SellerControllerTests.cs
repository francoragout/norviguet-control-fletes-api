using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Seller;
using AutoMapper;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SellerControllerTests
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
        mapperMock.Setup(m => m.Map<List<SellerDto>>(It.IsAny<List<Seller>>()))
            .Returns((List<Seller> sellers) =>
                sellers.Select(s => new SellerDto { Id = s.Id, Name = s.Name }).ToList());
        mapperMock.Setup(m => m.Map<SellerDto>(It.IsAny<Seller>()))
            .Returns((Seller s) => new SellerDto { Id = s.Id, Name = s.Name });
        mapperMock.Setup(m => m.Map<Seller>(It.IsAny<CreateSellerDto>()))
            .Returns((CreateSellerDto dto) => new Seller { Name = dto.Name });
        mapperMock.Setup(m => m.Map(It.IsAny<UpdateSellerDto>(), It.IsAny<Seller>()))
            .Callback((UpdateSellerDto dto, Seller seller) => seller.Name = dto.Name);
        return mapperMock.Object;
    }

    [Fact]
    public async Task GetSellers_ReturnsListOfSellerDto()
    {
        var context = GetDbContext("GetSellersDb");
        context.Sellers.AddRange(
            new Seller { Id = 1, Name = "Seller 1" },
            new Seller { Id = 2, Name = "Seller 2" }
        );
        context.SaveChanges();

        var controller = new SellerController(context, GetMapper());
        var result = await controller.GetSellers();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<SellerDto>>(okResult.Value);
        Assert.Equal(2, returnedList.Count);
    }

    [Fact]
    public async Task GetSeller_ReturnsSellerDto_WhenExists()
    {
        var context = GetDbContext("GetSellerExistsDb");
        context.Sellers.Add(new Seller { Id = 1, Name = "Seller 1" });
        context.SaveChanges();

        var controller = new SellerController(context, GetMapper());
        var result = await controller.GetSeller(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var sellerDto = Assert.IsType<SellerDto>(okResult.Value);
        Assert.Equal(1, sellerDto.Id);
        Assert.Equal("Seller 1", sellerDto.Name);
    }

    [Fact]
    public async Task GetSeller_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext("GetSellerNotExistsDb");
        var controller = new SellerController(context, GetMapper());
        var result = await controller.GetSeller(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateSeller_ReturnsCreatedSellerDto()
    {
        var context = GetDbContext("CreateSellerDb");
        var controller = new SellerController(context, GetMapper());
        var dto = new CreateSellerDto { Name = "New Seller" };

        var result = await controller.CreateSeller(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var sellerDto = Assert.IsType<SellerDto>(createdResult.Value);
        Assert.Equal("New Seller", sellerDto.Name);
        Assert.True(sellerDto.Id > 0);
    }

    [Fact]
    public async Task UpdateSeller_ReturnsNoContent_WhenExists()
    {
        var context = GetDbContext("UpdateSellerExistsDb");
        context.Sellers.Add(new Seller { Id = 1, Name = "Old Name" });
        context.SaveChanges();

        var controller = new SellerController(context, GetMapper());
        var dto = new UpdateSellerDto { Name = "Updated Name" };

        var result = await controller.UpdateSeller(1, dto);

        Assert.IsType<NoContentResult>(result);
        var updatedSeller = await context.Sellers.FindAsync(1);
        Assert.Equal("Updated Name", updatedSeller.Name);
    }

    [Fact]
    public async Task UpdateSeller_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext("UpdateSellerNotExistsDb");
        var controller = new SellerController(context, GetMapper());
        var dto = new UpdateSellerDto { Name = "Updated Name" };

        var result = await controller.UpdateSeller(99, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteSeller_ReturnsNoContent_WhenExists()
    {
        var context = GetDbContext("DeleteSellerExistsDb");
        context.Sellers.Add(new Seller { Id = 1, Name = "Seller" });
        context.SaveChanges();

        var controller = new SellerController(context, GetMapper());
        var result = await controller.DeleteSeller(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await context.Sellers.FindAsync(1));
    }

    [Fact]
    public async Task DeleteSeller_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext("DeleteSellerNotExistsDb");
        var controller = new SellerController(context, GetMapper());
        var result = await controller.DeleteSeller(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteSellers_ReturnsNoContent_WhenSellersExist()
    {
        var context = GetDbContext("DeleteSellersBulkExistsDb");
        context.Sellers.AddRange(
            new Seller { Id = 1, Name = "Seller 1" },
            new Seller { Id = 2, Name = "Seller 2" }
        );
        context.SaveChanges();

        var controller = new SellerController(context, GetMapper());
        var dto = new DeleteSellersDto { Ids = new List<int> { 1, 2 } };

        var result = await controller.DeleteSellers(dto);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Sellers.ToList());
    }

    [Fact]
    public async Task DeleteSellers_ReturnsNotFound_WhenNoSellersExist()
    {
        var context = GetDbContext("DeleteSellersBulkNotExistsDb");
        var controller = new SellerController(context, GetMapper());
        var dto = new DeleteSellersDto { Ids = new List<int> { 99, 100 } };

        var result = await controller.DeleteSellers(dto);

        Assert.IsType<NotFoundResult>(result);
    }
}
