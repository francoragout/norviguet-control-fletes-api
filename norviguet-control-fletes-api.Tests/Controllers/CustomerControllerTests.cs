using AutoMapper;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Profiles;

namespace norviguet_control_fletes_api.Tests.Controllers
{
    public class CustomerControllerTests
    {
        private readonly CustomerController _controller;
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public CustomerControllerTests()
        {
            // Configurar DB en memoria
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            _context = new NorviguetDbContext(options);

            // Configurar AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CustomerProfile>();
            });
            _mapper = config.CreateMapper();

            // Instanciar el controlador
            _controller = new CustomerController(_context, _mapper);
        }

        [Fact]
        public async Task GetCustomers_ReturnsOk_WithListOfCustomers()
        {
            // Arrange
            _context.Customers.AddRange(
                new Entities.Customer { Id = 1, Name = "Customer A", CUIT = "20-39575327-20" },
                new Entities.Customer { Id = 2, Name = "Customer B", CUIT = "27-12345678-9" }
            );
            await _context.SaveChangesAsync();
            // Act
            var result = await _controller.GetCustomers();
            // Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var customers = Assert.IsType<List<Models.Customer.CustomerDto>>(okResult.Value);
            Assert.Equal(2, customers.Count);
        }

        [Fact]
        public async Task GetCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Act
            var result = await _controller.GetCustomer(999);
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetCustomer_ReturnsOk_WithCustomer()
        {
            // Arrange
            var customer = new Entities.Customer { Id = 1, Name = "Customer A", CUIT = "20-39575327-20" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            // Act
            var result = await _controller.GetCustomer(1);
            // Assert
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var returnedCustomer = Assert.IsType<Models.Customer.CustomerDto>(okResult.Value);
            Assert.Equal(customer.Id, returnedCustomer.Id);
            Assert.Equal(customer.Name, returnedCustomer.Name);
            Assert.Equal(customer.CUIT, returnedCustomer.CUIT);
        }

        [Fact]
        public async Task CreateCustomer_AddsCustomerToDatabase()
        {
            // Arrange
            var dto = new Models.Customer.CreateCustomerDto
            {
                Name = "New Customer",
                CUIT = "30-12345678-9"
            };
            // Act
            var result = await _controller.CreateCustomer(dto);
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            var customerInDb = await _context.Customers.FirstOrDefaultAsync(c => c.Name == "New Customer");
            Assert.NotNull(customerInDb);
            Assert.Equal("30-12345678-9", customerInDb.CUIT);
        }

        [Fact]
        public async Task UpdateCustomer_UpdatesExistingCustomer()
        {
            // Arrange
            var customer = new Entities.Customer { Id = 1, Name = "Customer A", CUIT = "20-39575327-20" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            var dto = new Models.Customer.UpdateCustomerDto
            {
                Name = "Updated Customer",
                CUIT = "23-87654321-0"
            };
            // Act
            var result = await _controller.UpdateCustomer(1, dto);
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            var updatedCustomer = await _context.Customers.FindAsync(1);
            Assert.NotNull(updatedCustomer);
            Assert.Equal("Updated Customer", updatedCustomer.Name);
            Assert.Equal("23-87654321-0", updatedCustomer.CUIT);
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            var dto = new Models.Customer.UpdateCustomerDto
            {
                Name = "Nonexistent Customer",
                CUIT = "23-87654321-0"
            };
            // Act
            var result = await _controller.UpdateCustomer(999, dto);
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCustomer_DeleteExistingCustomer()
        {
            // Arrange
            var customer = new Entities.Customer { Id = 1, Name = "Customer A", CUIT = "20-39575327-20" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            // Act
            var result = await _controller.DeleteCustomer(1);
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            var deletedCustomer = await _context.Customers.FindAsync(1);
            Assert.Null(deletedCustomer);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteCustomer(999);
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsConflict_WhenCustomerHasAssociatedOrders()
        {
            // Arrange
            var customer = new Entities.Customer
            {
                Id = 1,
                Name = "Customer with orders",
                CUIT = "20-12345678-9",
                Orders = new List<Entities.Order>()
            };

            var seller = new Entities.Seller
            {
                Id = 1,
                Name = "Seller 1",
            };

            var order = new Entities.Order
            {
                CustomerId = 1,
                Customer = customer,
                Seller = seller,
            };

            customer.Orders.Add(order);

            _context.Customers.Add(customer);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteCustomer(1);

            // Assert
            var conflictResult = Assert.IsType<Microsoft.AspNetCore.Mvc.ConflictObjectResult>(result);
            var value = conflictResult.Value?.ToString();
            Assert.Contains("ASSOCIATED_RECORDS", value);
            Assert.Contains("Customer cannot be deleted because it has associated orders.", value);
        }

        [Fact]
        public async Task DeleteCustomersBulk_ReturnsNoContent()
        {
            // Arrange
            var customer1 = new Entities.Customer { Id = 1, Name = "Customer A", CUIT = "20-39575327-20" };
            var customer2 = new Entities.Customer { Id = 2, Name = "Customer B", CUIT = "27-12345678-9" };
            _context.Customers.AddRange(customer1, customer2);
            await _context.SaveChangesAsync();
            var idsToDelete = new List<int> { 1, 2 };
            var dto = new norviguet_control_fletes_api.Models.Common.DeleteEntitiesDto { Ids = idsToDelete };

            // Act
            var result = await _controller.DeleteCustomersBulk(dto);
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            var customersInDb = await _context.Customers.Where(c => idsToDelete.Contains(c.Id)).ToListAsync();
            Assert.Empty(customersInDb);
        }

        [Fact]
        public async Task DeleteCustomersBulk_ReturnsConflict_WhenAnyCustomerHasAssociatedOrders()
        {
            // Arrange
            var customer1 = new Entities.Customer { Id = 1, Name = "Customer A", CUIT = "20-39575327-20" };
            var customer2 = new Entities.Customer { Id = 2, Name = "Customer B", CUIT = "27-12345678-9" };
            var seller = new Entities.Seller
            {
                Id = 1,
                Name = "Seller 1",
            };
            var order = new Entities.Order
            {
                CustomerId = 2,
                Customer = customer2,
                Seller = seller,
            };
            _context.Customers.AddRange(customer1, customer2);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var idsToDelete = new List<int> { 1, 2 };
            var dto = new norviguet_control_fletes_api.Models.Common.DeleteEntitiesDto { Ids = idsToDelete };
            // Act
            var result = await _controller.DeleteCustomersBulk(dto);
            // Assert
            var conflictResult = Assert.IsType<Microsoft.AspNetCore.Mvc.ConflictObjectResult>(result);
            var value = conflictResult.Value?.ToString();
        }

        [Fact]
        public async Task DeleteCustomersBulk_ReturnsNotFound_WhenNoCustomersExist()
        {
            // Arrange
            var idsToDelete = new List<int> { 999, 1000 };
            var dto = new norviguet_control_fletes_api.Models.Common.DeleteEntitiesDto { Ids = idsToDelete };
            // Act
            var result = await _controller.DeleteCustomersBulk(dto);
            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
        }
    }
}
