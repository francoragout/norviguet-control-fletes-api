using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
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
            _context.Customers.Add(new Customer { Id = 1, Name = "Test Customer", Location = "Test Location" });
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
            var dto = new CreateCustomerDto { Name = "New Customer", Location = "New Location" };

            // Act
            var result = await _controller.CreateCustomer(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Single(_context.Customers);
            Assert.Equal("New Customer", _context.Customers.First().Name);
        }

        [Fact]
        public async Task UpdateCustomer_UpdatesExistingCustomer()
        {
            // Arrange
            var customer = new Customer { Id = 1, Name = "Old Name", Location = "Old Location" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var dto = new UpdateCustomerDto { Name = "Updated Name", Location = "Updated Location" };

            // Act
            var result = await _controller.UpdateCustomer(1, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var updated = await _context.Customers.FindAsync(1);
            Assert.Equal("Updated Name", updated!.Name);
            Assert.Equal("Updated Location", updated.Location);
        }

        [Fact]
        public async Task DeleteCustomer_RemovesCustomerFromDatabase()
        {
            // Arrange
            var customer = new Customer { Id = 1, Name = "ToDelete", Location = "Loc" };
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
            new Customer { Id = 1, Name = "Customer1", Location = "Loc1" },
            new Customer { Id = 2, Name = "Customer2", Location = "Loc2" }
            );
            await _context.SaveChangesAsync();

            var dto = new DeleteCustomersDto { Ids = new List<int> { 1, 2 } };

            // Act
            var result = await _controller.DeleteCustomersBulk(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(_context.Customers);
        }
    }
}
