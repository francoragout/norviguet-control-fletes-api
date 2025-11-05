using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Common;
using norviguet_control_fletes_api.Models.Customer;
using norviguet_control_fletes_api.Profiles;

namespace norviguet_control_fletes_api.Tests
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
            _context.Customers.Add(new Customer { Id = 1, Name = "Test Customer" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetCustomers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var customers = Assert.IsType<List<CustomerDto>>(okResult.Value);
            Assert.Single(customers);
            Assert.Equal("Test Customer", customers[0].Name);
        }

        [Fact]
        public async Task CreateCustomer_AddsCustomerToDatabase()
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                Name = "New Customer",
                CUIT = "20-12345678-1" // Valor válido requerido
            };

            // Act
            var result = await _controller.CreateCustomer(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Single(_context.Customers);
            Assert.Equal("New Customer", _context.Customers.First().Name);
            Assert.Equal("20-12345678-1", _context.Customers.First().CUIT);
        }

        [Fact]
        public async Task UpdateCustomer_UpdatesExistingCustomer()
        {
            // Arrange
            var customer = new Customer { Id = 1, Name = "Old Name", CUIT = "20-12345678-1" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            var dto = new UpdateCustomerDto
            {
                Name = "Updated Name",
                CUIT = "20-87654321-0" // Valor válido requerido
            };
            // Act
            var result = await _controller.UpdateCustomer(1, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedCustomer = _context.Customers.First();
            Assert.Equal("Updated Name", updatedCustomer.Name);
            Assert.Equal("20-87654321-0", updatedCustomer.CUIT);
        }

        [Fact]
        public async Task DeleteCustomer_RemovesCustomerFromDatabase()
        {
            // Arrange
            var customer = new Customer { Id = 1, Name = "ToDelete" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteCustomer(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Customers);
        }

        [Fact]
        public async Task DeleteCustomersBulk_RemovesMultipleCustomers()
        {
            // Arrange
            _context.Customers.AddRange(
            new Customer { Id = 1, Name = "Customer1" },
            new Customer { Id = 2, Name = "Customer2" }
            );
            await _context.SaveChangesAsync();

            var dto = new DeleteEntitiesDto { Ids = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.DeleteCustomersBulk(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Customers);
        }
    }
}
