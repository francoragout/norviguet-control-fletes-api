using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Carrier;
using AutoMapper;
using Xunit;

public class CarrierControllerTests
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
        mapperMock.Setup(m => m.Map<List<CarrierDto>>(It.IsAny<List<Carrier>>()))
            .Returns((List<Carrier> carriers) =>
                carriers.Select(c => new CarrierDto { Id = c.Id, Name = c.Name }).ToList());
        mapperMock.Setup(m => m.Map<CarrierDto>(It.IsAny<Carrier>()))
            .Returns((Carrier c) => new CarrierDto { Id = c.Id, Name = c.Name });
        mapperMock.Setup(m => m.Map<Carrier>(It.IsAny<CreateCarrierDto>()))
            .Returns((CreateCarrierDto dto) => new Carrier { Name = dto.Name });
        mapperMock.Setup(m => m.Map(It.IsAny<UpdateCarrierDto>(), It.IsAny<Carrier>()))
            .Callback((UpdateCarrierDto dto, Carrier carrier) => carrier.Name = dto.Name);
        return mapperMock.Object;
    }

    [Fact]
    public async Task GetCarriers_ReturnsListOfCarrierDto()
    {
        var context = GetDbContext("GetCarriersDb");
        context.Carriers.AddRange(new Carrier { Id = 1, Name = "Carrier 1" }, new Carrier { Id = 2, Name = "Carrier 2" });
        context.SaveChanges();

        var controller = new CarrierController(context, GetMapper());
        var result = await controller.GetCarriers();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<CarrierDto>>(okResult.Value);
        Assert.Equal(2, returnedList.Count);
    }

    [Fact]
    public async Task GetCarrier_ReturnsCarrierDto_WhenExists()
    {
        var context = GetDbContext("GetCarrierExistsDb");
        context.Carriers.Add(new Carrier { Id = 1, Name = "Carrier 1" });
        context.SaveChanges();

        var controller = new CarrierController(context, GetMapper());
        var result = await controller.GetCarrier(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var carrierDto = Assert.IsType<CarrierDto>(okResult.Value);
        Assert.Equal(1, carrierDto.Id);
        Assert.Equal("Carrier 1", carrierDto.Name);
    }

    [Fact]
    public async Task GetCarrier_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext("GetCarrierNotExistsDb");
        var controller = new CarrierController(context, GetMapper());
        var result = await controller.GetCarrier(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateCarrier_ReturnsOk()
    {
        var context = GetDbContext("CreateCarrierDb");
        var controller = new CarrierController(context, GetMapper());
        var dto = new CreateCarrierDto { Name = "New Carrier" };

        var result = await controller.CreateCarrier(dto);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateCarrier_ReturnsNoContent_WhenExists()
    {
        var context = GetDbContext("UpdateCarrierExistsDb");
        context.Carriers.Add(new Carrier { Id = 1, Name = "Old Name" });
        context.SaveChanges();

        var controller = new CarrierController(context, GetMapper());
        var dto = new UpdateCarrierDto { Name = "Updated Name" };

        var result = await controller.UpdateCarrier(1, dto);

        Assert.IsType<NoContentResult>(result);
        var updatedCarrier = await context.Carriers.FindAsync(1);
        Assert.NotNull(updatedCarrier);
        Assert.Equal("Updated Name", updatedCarrier.Name);
    }

    [Fact]
    public async Task UpdateCarrier_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext("UpdateCarrierNotExistsDb");
        var controller = new CarrierController(context, GetMapper());
        var dto = new UpdateCarrierDto { Name = "Updated Name" };

        var result = await controller.UpdateCarrier(99, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteCarrier_ReturnsNoContent_WhenExists()
    {
        var context = GetDbContext("DeleteCarrierExistsDb");
        context.Carriers.Add(new Carrier { Id = 1, Name = "Carrier" });
        context.SaveChanges();

        var controller = new CarrierController(context, GetMapper());
        var result = await controller.DeleteCarrier(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await context.Carriers.FindAsync(1));
    }

    [Fact]
    public async Task DeleteCarrier_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext("DeleteCarrierNotExistsDb");
        var controller = new CarrierController(context, GetMapper());
        var result = await controller.DeleteCarrier(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteCarriersBulk_ReturnsNoContent_WhenCarriersExist()
    {
        var context = GetDbContext("DeleteCarriersBulkExistsDb");
        context.Carriers.AddRange(new Carrier { Id = 1, Name = "Carrier 1" }, new Carrier { Id = 2, Name = "Carrier 2" });
        context.SaveChanges();

        var controller = new CarrierController(context, GetMapper());
        var dto = new DeleteCarriersDto { Ids = new List<int> { 1, 2 } };

        var result = await controller.DeleteCarriersBulk(dto);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Carriers.ToList());
    }

    [Fact]
    public async Task DeleteCarriersBulk_ReturnsNotFound_WhenNoCarriersExist()
    {
        var context = GetDbContext("DeleteCarriersBulkNotExistsDb");
        var controller = new CarrierController(context, GetMapper());
        var dto = new DeleteCarriersDto { Ids = new List<int> { 99, 100 } };

        var result = await controller.DeleteCarriersBulk(dto);

        Assert.IsType<NotFoundResult>(result);
    }
}
