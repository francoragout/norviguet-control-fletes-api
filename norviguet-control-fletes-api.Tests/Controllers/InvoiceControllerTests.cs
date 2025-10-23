using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Invoice;
using norviguet_control_fletes_api.Profiles;

namespace norviguet_control_fletes_api.Tests
{
    public class InvoiceControllerTests
    {
        private readonly InvoiceController _controller;
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public InvoiceControllerTests()
        {
            // Configurar DB en memoria
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // GUID para evitar interferencia entre tests
            .Options;

            _context = new NorviguetDbContext(options);

            // Configurar AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<InvoiceProfile>();
            });
            _mapper = config.CreateMapper();

            // Instanciar el controlador
            _controller = new InvoiceController(_context, _mapper);
        }

        [Fact]
        public async Task GetInvoices_ReturnsOk_WithListOfInvoices()
        {
            // Arrange
            _context.Invoices.Add(new Invoice { Id = 1, Type = InvoiceType.A, PointOfSale = "0001", Number = "00000001" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetInvoices();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var invoices = Assert.IsType<List<InvoiceDto>>(okResult.Value);
            Assert.Single(invoices);
            Assert.Equal("0001", invoices[0].PointOfSale);
        }

        [Fact]
        public async Task CreateInvoice_AddsInvoiceToDatabase()
        {
            // Arrange
            var dto = new CreateInvoiceDto { Type = InvoiceType.B, PointOfSale = "0002", Number = "00000002" };

            // Act
            var result = await _controller.CreateInvoice(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Single(_context.Invoices);
            Assert.Equal("0002", _context.Invoices.First().PointOfSale);
        }

        [Fact]
        public async Task UpdateInvoice_UpdatesExistingInvoice()
        {
            // Arrange
            var invoice = new Invoice { Id = 1, Type = InvoiceType.A, PointOfSale = "0001", Number = "00000001" };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            var dto = new UpdateInvoiceDto { Type = InvoiceType.C, PointOfSale = "0003", Number = "00000003" };

            // Act
            var result = await _controller.UpdateInvoice(1, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updated = await _context.Invoices.FindAsync(1);
            Assert.Equal("0003", updated!.PointOfSale);
            Assert.Equal(InvoiceType.C, updated.Type);
        }

        [Fact]
        public async Task DeleteInvoice_RemovesInvoiceFromDatabase()
        {
            // Arrange
            var invoice = new Invoice { Id = 1, Type = InvoiceType.A, PointOfSale = "0001", Number = "00000001" };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteInvoice(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Invoices);
        }

        [Fact]
        public async Task DeleteInvoicesBulk_RemovesMultipleInvoices()
        {
            // Arrange
            _context.Invoices.AddRange(
            new Invoice { Id = 1, Type = InvoiceType.A, PointOfSale = "0001", Number = "00000001" },
            new Invoice { Id = 2, Type = InvoiceType.B, PointOfSale = "0002", Number = "00000002" }
            );
            await _context.SaveChangesAsync();

            var dto = new DeleteInvoicesDto { Ids = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.DeleteInvoicesBulk(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Invoices);
        }
    }
}
