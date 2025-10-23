using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Seller;
using norviguet_control_fletes_api.Profiles;

namespace norviguet_control_fletes_api.Tests
{
    public class SellerControllerTests
    {
        private readonly SellerController _controller;
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public SellerControllerTests()
        {
            // Configurar DB en memoria
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // GUID para evitar interferencia entre tests
                .Options;
            _context = new NorviguetDbContext(options);

            // Configurar AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SellerProfile>();
            });
            _mapper = config.CreateMapper();

            // Instanciar el controlador
            _controller = new SellerController(_context, _mapper);
        }

        [Fact]
        public async Task GetSellers_ReturnsOk_WithListOfSellers()
        {
            // Arrange
            _context.Sellers.Add(new Seller { Id = 1, Name = "Test Seller" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetSellers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var sellers = Assert.IsType<List<SellerDto>>(okResult.Value);
            Assert.Single(sellers);
            Assert.Equal("Test Seller", sellers[0].Name);
        }

        [Fact]
        public async Task CreateSeller_AddsSellerToDatabase()
        {
            // Arrange
            var dto = new CreateSellerDto { Name = "New Seller" };

            // Act
            var result = await _controller.CreateSeller(dto);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Single(_context.Sellers);
            Assert.Equal("New Seller", _context.Sellers.First().Name);
        }

        [Fact]
        public async Task UpdateSeller_UpdatesExistingSeller()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Old Name" };
            _context.Sellers.Add(seller);
            await _context.SaveChangesAsync();

            var dto = new UpdateSellerDto { Name = "Updated Name" };

            // Act
            var result = await _controller.UpdateSeller(1, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updated = await _context.Sellers.FindAsync(1);
            Assert.Equal("Updated Name", updated!.Name);
        }

        [Fact]
        public async Task DeleteSeller_RemovesSellerFromDatabase()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "ToDelete" };
            _context.Sellers.Add(seller);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteSeller(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Sellers);
        }

        [Fact]
        public async Task DeleteSellers_RemovesMultipleSellers()
        {
            // Arrange
            _context.Sellers.AddRange(
                new Seller { Id = 1, Name = "Seller1" },
                new Seller { Id = 2, Name = "Seller2" }
            );
            await _context.SaveChangesAsync();

            var dto = new DeleteSellersDto { Ids = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.DeleteSellers(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Sellers);
        }
    }
}
