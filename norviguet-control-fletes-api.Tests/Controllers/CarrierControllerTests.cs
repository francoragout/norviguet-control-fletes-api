using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Carrier;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Models.Profiles;
using norviguet_control_fletes_api.Repositories;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Tests.Controllers
{
    public class CarrierControllerTests
    {
        private readonly CarrierController _controller;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICarrierService _carrierService;
        private readonly ICarrierRepository _carrierRepository;

        public CarrierControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarrierProfile>();
            });
            _mapper = config.CreateMapper();

            var logger = new Mock<ILogger<CarrierService>>().Object;
            
            _carrierRepository = new CarrierRepository(_context);
            _carrierService = new CarrierService(_carrierRepository, _mapper, logger);
            _controller = new CarrierController(_carrierService);
        }

        [Fact]
        public async Task GetCarriers_ReturnsOk_WithListOfCarriers()
        {
            // Arrange
            _context.Carriers.Add(new Entities.Carrier { Id = 1, Name = "Test Carrier" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetCarriers();

            // Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var carriers = Assert.IsType<List<CarrierDto>>(okResult.Value);
            Assert.Single(carriers);
            Assert.Equal("Test Carrier", carriers[0].Name);
        }

        [Fact]
        public async Task GetCarrier_ReturnsOk_WithCarrier()
        {
            // Arrange
            _context.Carriers.Add(new Entities.Carrier { Id = 1, Name = "Test Carrier" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetCarrier(1);

            // Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var carrier = Assert.IsType<CarrierDto>(okResult.Value);
            Assert.Equal("Test Carrier", carrier.Name);
        }

        [Fact]
        public async Task GetCarrier_ReturnsNotFound_WhenCarrierDoesNotExist()
        {
            // Act
            var result = await _controller.GetCarrier(999);
            
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateCarrier_ReturnsCreated()
        {
            // Arrange
            var dto = new Models.Carrier.CreateCarrierDto
            {
                Name = "New Carrier"
            };

            // Act
            var result = await _controller.CreateCarrier(dto);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>(result);
            var carrierInDb = await _context.Carriers.FirstOrDefaultAsync(c => c.Name == "New Carrier");
            Assert.NotNull(carrierInDb);
        }

        [Fact]
        public async Task UpdateCarrier_ReturnsOk_WhenCarrierExists()
        {
            // Arrange
            _context.Carriers.Add(new Entities.Carrier { Id = 1, Name = "Old Carrier" });
            await _context.SaveChangesAsync();
            var dto = new Models.Carrier.UpdateCarrierDto
            {
                Name = "Updated Carrier"
            };

            // Act
            var result = await _controller.UpdateCarrier(1, dto);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
            var carrierInDb = await _context.Carriers.FindAsync(1);
            Assert.NotNull(carrierInDb);
            Assert.Equal("Updated Carrier", carrierInDb.Name);
        }

        [Fact]
        public async Task UpdateCarrier_ReturnsNotFound_WhenCarrierDoesNotExist()
        {
            // Arrange
            var dto = new Models.Carrier.UpdateCarrierDto
            {
                Name = "Updated Carrier"
            };

            // Act
            var result = await _controller.UpdateCarrier(999, dto);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCarrier_ReturnsNoContent_WhenCarrierExists()
        {
            // Arrange
            _context.Carriers.Add(new Entities.Carrier { Id = 1, Name = "Carrier to Delete" });
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _controller.DeleteCarrier(1);
            
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            var carrierInDb = await _context.Carriers.FindAsync(1);
            Assert.Null(carrierInDb);
        }

        [Fact]
        public async Task DeleteCarrier_ReturnsNotFound_WhenCarrierDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteCarrier(999);
            
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCarrier_ReturnsConflict_WhenCarrierHasAssociatedRecords()
        {
            // Arrange
            var seller = new Entities.Seller { Id = 1, Name = "Test Seller" };
            var customer = new Entities.Customer { Id = 1, Name = "Test Customer", CUIT = "20-12345678-9" };

            var order = new Entities.Order
            {
                Id = 1,
                Seller = seller,
                Customer = customer,
            };

            var carrier = new Entities.Carrier
            {
                Id = 1,
                Name = "Carrier with records",
                DeliveryNotes = new List<DeliveryNote>(),
                PaymentOrders = new List<PaymentOrder>(),
                Invoices = new List<Invoice>()
            };

            var deliveryNote = new Entities.DeliveryNote
            {
                Id = 1,
                CarrierId = 1,
                OrderId = 1,
                Carrier = carrier,
                Order = order
            };
            var paymentOrder = new Entities.PaymentOrder
            {
                PaymentOrderNumber = "PO1",
                CarrierId = 1,
                OrderId = 1,
                Carrier = carrier,
                Order = order
            };
            var invoice = new Entities.Invoice
            {
                Id = 1,
                CarrierId = 1,
                OrderId = 1,
                Price = 100,
                Carrier = carrier,
                Order = order
            };

            carrier.DeliveryNotes.Add(deliveryNote);
            carrier.PaymentOrders.Add(paymentOrder);
            carrier.Invoices.Add(invoice);

            _context.Carriers.Add(carrier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteCarrier(1);

            // Assert
            var conflictResult = Assert.IsType<Microsoft.AspNetCore.Mvc.ConflictObjectResult>(result);
            var value = conflictResult.Value?.ToString();
            Assert.Contains("ASSOCIATED_RECORDS", value);
        }

        [Fact]
        public async Task DeleteCarriersBulk_ReturnsNotFound_WhenNoCarriersFound()
        {
            // Arrange
            var dto = new Models.Common.DeleteEntitiesDto { Ids = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.DeleteCarriersBulk(dto);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCarriersBulk_ReturnsNoContent_WhenAllCarriersCanBeDeleted()
        {
            // Arrange
            _context.Carriers.Add(new Entities.Carrier { Id = 1, Name = "Carrier 1" });
            _context.Carriers.Add(new Entities.Carrier { Id = 2, Name = "Carrier 2" });
            await _context.SaveChangesAsync();

            var dto = new Models.Common.DeleteEntitiesDto { Ids = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.DeleteCarriersBulk(dto);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            Assert.Null(await _context.Carriers.FindAsync(1));
            Assert.Null(await _context.Carriers.FindAsync(2));
        }

        [Fact]
        public async Task DeleteCarriersBulk_ReturnsConflict_WhenSomeCarriersHaveAssociatedRecords()
        {
            // Arrange
            var seller = new Entities.Seller { Id = 1, Name = "Test Seller" };
            var customer = new Entities.Customer { Id = 1, Name = "Test Customer", CUIT = "20-12345678-9" };
            var order = new Entities.Order { Id = 1, Seller = seller, Customer = customer };

            var carrierWithRecords = new Entities.Carrier
            {
                Id = 1,
                Name = "Carrier with records",
                DeliveryNotes = new List<DeliveryNote>(),
                PaymentOrders = new List<PaymentOrder>(),
                Invoices = new List<Invoice>()
            };
            var deliveryNote = new Entities.DeliveryNote
            {
                Id = 1,
                CarrierId = 1,
                OrderId = 1,
                Carrier = carrierWithRecords,
                Order = order
            };
            carrierWithRecords.DeliveryNotes.Add(deliveryNote);

            var carrierWithoutRecords = new Entities.Carrier { Id = 2, Name = "Carrier without records" };

            _context.Carriers.Add(carrierWithRecords);
            _context.Carriers.Add(carrierWithoutRecords);
            await _context.SaveChangesAsync();

            var dto = new Models.Common.DeleteEntitiesDto { Ids = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.DeleteCarriersBulk(dto);

            // Assert
            var conflictResult = Assert.IsType<Microsoft.AspNetCore.Mvc.ConflictObjectResult>(result);
            var value = conflictResult.Value?.ToString();
            Assert.Contains("ASSOCIATED_RECORDS", value);
            Assert.NotNull(await _context.Carriers.FindAsync(1));
            Assert.NotNull(await _context.Carriers.FindAsync(2));
        }
    }
}
