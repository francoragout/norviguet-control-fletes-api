using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Payment;
using norviguet_control_fletes_api.Profiles;

namespace norviguet_control_fletes_api.Tests
{
    public class PaymentControllerTests
    {
        private readonly PaymentController _controller;
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public PaymentControllerTests()
        {
            // Configurar DB en memoria
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // GUID para evitar interferencia entre tests
            .Options;

            _context = new NorviguetDbContext(options);

            // Configurar AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentProfile>();
            });
            _mapper = config.CreateMapper();

            // Instanciar el controlador
            _controller = new PaymentController(_context, _mapper);
        }

        [Fact]
        public async Task GetPayments_ReturnsOk_WithListOfPayments()
        {
            // Arrange
            _context.Payments.Add(new Payment { Id = 1, PointOfSale = "0001" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetPayments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var payments = Assert.IsType<List<PaymentDto>>(okResult.Value);
            Assert.Single(payments);
            Assert.Equal("0001", payments[0].PointOfSale);
        }

        [Fact]
        public async Task CreatePayment_AddsPaymentToDatabase()
        {
            // Arrange
            var dto = new CreatePaymentDto { PointOfSale = "0002" };

            // Act
            var result = await _controller.CreatePayment(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Single(_context.Payments);
            Assert.Equal("0002", _context.Payments.First().PointOfSale);
        }

        [Fact]
        public async Task UpdatePayment_UpdatesExistingPayment()
        {
            // Arrange
            var payment = new Payment { Id = 1, PointOfSale = "0001" };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var dto = new UpdatePaymentDto { PointOfSale = "0003" };

            // Act
            var result = await _controller.UpdatePayment(1, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updated = await _context.Payments.FindAsync(1);
            Assert.Equal("0003", updated!.PointOfSale);
        }

        [Fact]
        public async Task DeletePayment_RemovesPaymentFromDatabase()
        {
            // Arrange
            var payment = new Payment { Id = 1, PointOfSale = "0001" };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeletePayment(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Payments);
        }

        [Fact]
        public async Task DeletePaymentsBulk_RemovesMultiplePayments()
        {
            // Arrange
            _context.Payments.AddRange(
            new Payment { Id = 1, PointOfSale = "0001" },
            new Payment { Id = 2, PointOfSale = "0002" }
            );
            await _context.SaveChangesAsync();

            var dto = new DeletePaymentsDto { Ids = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.DeletePaymentsBulk(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Payments);
        }
    }
}
