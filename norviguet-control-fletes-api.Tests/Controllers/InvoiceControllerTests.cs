using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Common;
using norviguet_control_fletes_api.Models.Invoice;
using norviguet_control_fletes_api.Profiles;
using norviguet_control_fletes_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace norviguet_control_fletes_api.Tests.Controllers
{
    public class InvoiceControllerTests
    {
        private readonly InvoiceController _controller;
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public InvoiceControllerTests()
        {
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            _context = new NorviguetDbContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<InvoiceProfile>();
            });
            _mapper = config.CreateMapper();

            _controller = new InvoiceController(_context, _mapper);
        }

        [Fact]
        public async Task GetInvoicesByOrderId_ReturnsOk_WhenOrderExists()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetInvoicesByOrderId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var invoices = Assert.IsType<List<InvoiceDto>>(okResult.Value);
            Assert.Single(invoices);
        }

        [Fact]
        public async Task GetInvoicesByOrderId_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Act
            var result = await _controller.GetInvoicesByOrderId(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetInvoice_ReturnsOk_WhenInvoiceExists()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 2, CarrierId = 1, OrderId = 1, InvoiceNumber = "54321-12345678", Price = 200, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetInvoice(2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var invoice = Assert.IsType<InvoiceDto>(okResult.Value);
            Assert.Equal(2, invoice.Id);
        }

        [Fact]
        public async Task GetInvoice_ReturnsNotFound_WhenInvoiceDoesNotExist()
        {
            // Act
            var result = await _controller.GetInvoice(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateInvoice_ReturnsNoContent_WhenValid()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            var user = new User { Id = 1, Role = UserRole.Admin };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new CreateInvoiceDto
            {
                InvoiceNumber = "12345-12345678",
                Price = 100,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.A
            };

            // Act
            var result = await _controller.CreateInvoice(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CreateInvoice_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var dto = new CreateInvoiceDto
            {
                InvoiceNumber = "12345-12345678",
                Price = 100,
                OrderId = 99,
                CarrierId = 1,
                Type = InvoiceType.A
            };
            // Act
            var result = await _controller.CreateInvoice(dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateInvoice_ReturnsConflict_WhenOrderClosedOrRejected()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Closed, Seller = seller, Customer = customer };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var dto = new CreateInvoiceDto
            {
                InvoiceNumber = "12345-12345678",
                Price = 100,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.A
            };
            // Act
            var result = await _controller.CreateInvoice(dto);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("CLOSED_OR_REJECTED_ORDER", conflict.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task CreateInvoice_ReturnsConflict_WhenCarrierHasPendingDeliveryNotes()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.DeliveryNotes.Add(new DeliveryNote { Id = 1, CarrierId = 1, OrderId = 1, Status = DeliveryNoteStatus.Pending, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            var dto = new CreateInvoiceDto
            {
                InvoiceNumber = "12345-12345678",
                Price = 100,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.A
            };
            // Act
            var result = await _controller.CreateInvoice(dto);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("CARRIER_HAS_PENDING_DELIVERY_NOTES", conflict.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task CreateInvoice_ReturnsConflict_WhenInvoiceNumberExists()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            var dto = new CreateInvoiceDto
            {
                InvoiceNumber = "12345-12345678",
                Price = 100,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.A
            };
            // Act
            var result = await _controller.CreateInvoice(dto);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("INVOICE_NUMBER_ALREADY_EXISTS", conflict.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task CreateInvoice_ReturnsConflict_WhenCarrierOrderExists()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "54321-12345678", Price = 100, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            var dto = new CreateInvoiceDto
            {
                InvoiceNumber = "12345-12345678",
                Price = 100,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.A
            };
            // Act
            var result = await _controller.CreateInvoice(dto);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("INVOICE_CARRIER_ORDER_ALREADY_EXISTS", conflict.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task UpdateInvoice_ReturnsNoContent_WhenValid()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            var dto = new UpdateInvoiceDto
            {
                InvoiceNumber = "54321-12345678",
                Price = 200,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.B
            };
            // Act
            var result = await _controller.UpdateInvoice(1, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateInvoice_ReturnsNotFound_WhenInvoiceDoesNotExist()
        {
            // Arrange
            var dto = new UpdateInvoiceDto
            {
                InvoiceNumber = "54321-12345678",
                Price = 200,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.B
            };
            // Act
            var result = await _controller.UpdateInvoice(999, dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateInvoice_ReturnsConflict_WhenInvoiceNumberExists()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order, Carrier = carrier });
            _context.Invoices.Add(new Invoice { Id = 2, CarrierId = 1, OrderId = 1, InvoiceNumber = "54321-12345678", Price = 200, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            var dto = new UpdateInvoiceDto
            {
                InvoiceNumber = "54321-12345678",
                Price = 200,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.B
            };
            // Act
            var result = await _controller.UpdateInvoice(1, dto);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("INVOICE_NUMBER_ALREADY_EXISTS", conflict.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task UpdateInvoice_ReturnsConflict_WhenCarrierOrderExists()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order, Carrier = carrier });
            _context.Invoices.Add(new Invoice { Id = 2, CarrierId = 1, OrderId = 1, InvoiceNumber = "54321-12345678", Price = 200, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            var dto = new UpdateInvoiceDto
            {
                InvoiceNumber = "99999-12345678",
                Price = 200,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.B
            };
            // Act
            var result = await _controller.UpdateInvoice(1, dto);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("INVOICE_CARRIER_ORDER_ALREADY_EXISTS", conflict.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task UpdateInvoice_ReturnsConflict_WhenCarrierHasPendingDeliveryNotes()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order, Carrier = carrier });
            _context.DeliveryNotes.Add(new DeliveryNote { Id = 1, CarrierId = 1, OrderId = 1, Status = DeliveryNoteStatus.Pending, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            var dto = new UpdateInvoiceDto
            {
                InvoiceNumber = "54321-12345678",
                Price = 200,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.B
            };
            // Act
            var result = await _controller.UpdateInvoice(1, dto);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("CARRIER_HAS_PENDING_DELIVERY_NOTES", conflict.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task UpdateInvoice_ReturnsConflict_WhenOrderClosedOrRejected()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Closed, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            var dto = new UpdateInvoiceDto
            {
                InvoiceNumber = "54321-12345678",
                Price = 200,
                OrderId = 1,
                CarrierId = 1,
                Type = InvoiceType.B
            };
            // Act
            var result = await _controller.UpdateInvoice(1, dto);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("CLOSED_OR_REJECTED_ORDER", conflict.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task DeleteInvoice_ReturnsNoContent_WhenValid()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            // Act
            var result = await _controller.DeleteInvoice(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteInvoice_ReturnsNotFound_WhenInvoiceDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteInvoice(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteInvoice_ReturnsConflict_WhenOrderClosedOrRejected()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Closed, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            // Act
            var result = await _controller.DeleteInvoice(1);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("CLOSED_OR_REJECTED_ORDER", conflict.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task DeleteInvoicesBulk_ReturnsNoContent_WhenValid()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order { Id = 1, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order, Carrier = carrier });
            _context.Invoices.Add(new Invoice { Id = 2, CarrierId = 1, OrderId = 1, InvoiceNumber = "54321-12345678", Price = 200, Order = order, Carrier = carrier });
            await _context.SaveChangesAsync();
            var dto = new DeleteEntitiesDto { Ids = new List<int> { 1, 2 } };
            // Act
            var result = await _controller.DeleteInvoicesBulk(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteInvoicesBulk_ReturnsNotFound_WhenNoneExist()
        {
            // Arrange
            var dto = new DeleteEntitiesDto { Ids = new List<int> { 99, 100 } };
            // Act
            var result = await _controller.DeleteInvoicesBulk(dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteInvoicesBulk_ReturnsConflict_WhenAnyOrderClosedOrRejected()
        {
            // Arrange
            var seller = new Seller { Id = 1, Name = "Seller1" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order1 = new Order { Id = 1, Status = OrderStatus.Closed, Seller = seller, Customer = customer };
            var order2 = new Order { Id = 2, Status = OrderStatus.Pending, Seller = seller, Customer = customer };
            var carrier = new Carrier { Id = 1, Name = "Carrier1" };
            _context.Orders.Add(order1);
            _context.Orders.Add(order2);
            _context.Carriers.Add(carrier);
            _context.Invoices.Add(new Invoice { Id = 1, CarrierId = 1, OrderId = 1, InvoiceNumber = "12345-12345678", Price = 100, Order = order1, Carrier = carrier });
            _context.Invoices.Add(new Invoice { Id = 2, CarrierId = 1, OrderId = 2, InvoiceNumber = "54321-12345678", Price = 200, Order = order2, Carrier = carrier });
            await _context.SaveChangesAsync();
            var dto = new DeleteEntitiesDto { Ids = new List<int> { 1, 2 } };
            // Act
            var result = await _controller.DeleteInvoicesBulk(dto);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("CLOSED_OR_REJECTED_ORDER", conflict.Value?.ToString() ?? string.Empty);
        }
    }
}
