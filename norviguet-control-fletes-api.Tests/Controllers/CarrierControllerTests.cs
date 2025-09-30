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
    [Fact]
    public async Task GetCarriers_ReturnsListOfCarrierDto()
    {
        // Arrange
        var carrierDtos = new List<CarrierDto>
        {
            new CarrierDto { Id = 1, Name = "Carrier 1" },
            new CarrierDto { Id = 2, Name = "Carrier 2" }
        };

        // Configuramos DbContext InMemory
        var options = new DbContextOptionsBuilder<NorviguetDbContext>()
                          .UseInMemoryDatabase(databaseName: "TestDb")
                          .Options;

        using var context = new NorviguetDbContext(options);

        // Agregamos datos de prueba
        context.Carriers.AddRange(new List<Carrier>
        {
            new Carrier { Id = 1, Name = "Carrier 1" },
            new Carrier { Id = 2, Name = "Carrier 2" }
        });
        context.SaveChanges();

        // Mock del mapper
        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<List<CarrierDto>>(It.IsAny<List<Carrier>>()))
                  .Returns((List<Carrier> carriers) =>
                      carriers.Select(c => new CarrierDto
                      {
                          Id = c.Id,
                          Name = c.Name
                      }).ToList()
                  );

        var controller = new CarrierController(context, mapperMock.Object);

        // Act
        var result = await controller.GetCarriers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<CarrierDto>>(okResult.Value);
        Assert.Equal(2, returnedList.Count);
        Assert.Equal("Carrier 1", returnedList[0].Name);
        Assert.Equal("Carrier 2", returnedList[1].Name);
    }
}
