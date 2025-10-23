using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Carrier;
using norviguet_control_fletes_api.Profiles;

namespace norviguet_control_fletes_api.Tests
{
    public class CarrierControllerTests
    {
        private readonly CarrierController _controller;
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public CarrierControllerTests()
        {
            // Configurar DB en memoria
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // GUID para evitar interferencia entre tests
                .Options;

            _context = new NorviguetDbContext(options);

            // Configurar AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarrierProfile>();
            });
            _mapper = config.CreateMapper();

            // Instanciar el controlador
            _controller = new CarrierController(_context, _mapper);
        }

        [Fact]
        public async Task GetCarriers_ReturnsOk_WithListOfCarriers()
        {
            // Arrange
            _context.Carriers.Add(new Carrier { Id = 1, Name = "Test Carrier" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetCarriers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var carriers = Assert.IsType<List<CarrierDto>>(okResult.Value);
            Assert.Single(carriers);
            Assert.Equal("Test Carrier", carriers[0].Name);
        }

        [Fact]
        public async Task CreateCarrier_AddsCarrierToDatabase()
        {
            // Arrange
            var dto = new CreateCarrierDto { Name = "New Carrier" };

            // Act
            var result = await _controller.CreateCarrier(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Single(_context.Carriers);
            Assert.Equal("New Carrier", _context.Carriers.First().Name);
        }

        [Fact]
        public async Task UpdateCarrier_UpdatesExistingCarrier()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Old Name" };
            _context.Carriers.Add(carrier);
            await _context.SaveChangesAsync();

            var dto = new UpdateCarrierDto { Name = "Updated Name" };

            // Act
            var result = await _controller.UpdateCarrier(1, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updated = await _context.Carriers.FindAsync(1);
            Assert.Equal("Updated Name", updated!.Name);
        }

        [Fact]
        public async Task DeleteCarrier_RemovesCarrierFromDatabase()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "ToDelete" };
            _context.Carriers.Add(carrier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteCarrier(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Carriers);
        }

        [Fact]
        public async Task DeleteCarriersBulk_RemovesMultipleCarriers()
        {
            // Arrange
            _context.Carriers.AddRange(
                new Carrier { Id = 1, Name = "Carrier1" },
                new Carrier { Id = 2, Name = "Carrier2" }
            );
            await _context.SaveChangesAsync();

            var dto = new DeleteCarriersDto { Ids = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.DeleteCarriersBulk(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Carriers);
        }
    }
}
