using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Customer;
using AutoMapper;
using Xunit;

public class CustomerControllerTests
{
    private NorviguetDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<NorviguetDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new NorviguetDbContext(options);
    }

    private IMapper GetMapper()
    {
        var mapperMock = new Mock<IMapper>();
        mapperMock.Setup(m => m.Map<List<CustomerDto>>(It.IsAny<List<Customer>>()))
            .Returns((List<Customer> customers) =>
                customers.Select(c => new CustomerDto { Id = c.Id, Name = c.Name, Location = c.Location }).ToList());
        mapperMock.Setup(m => m.Map<CustomerDto>(It.IsAny<Customer>()))
            .Returns((Customer c) => new CustomerDto { Id = c.Id, Name = c.Name, Location = c.Location });
        mapperMock.Setup(m => m.Map<Customer>(It.IsAny<CreateCustomerDto>()))
            .Returns((CreateCustomerDto dto) => new Customer { Name = dto.Name, Location = dto.Location });
        mapperMock.Setup(m => m.Map(It.IsAny<UpdateCustomerDto>(), It.IsAny<Customer>()))
            .Callback((UpdateCustomerDto dto, Customer customer) => {
                customer.Name = dto.Name;
                customer.Location = dto.Location;
            });
        return mapperMock.Object;
    }

    [Fact]
    public async Task GetCustomers_ReturnsListOfCustomerDto()
    {
        var context = GetDbContext("GetCustomersDb");
        context.Customers.AddRange(
            new Customer { Id = 1, Name = "Customer 1", Location = "Loc 1" },
            new Customer { Id = 2, Name = "Customer 2", Location = "Loc 2" }
        );
        context.SaveChanges();

        var controller = new CustomerController(context, GetMapper());
        var result = await controller.GetCustomers();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<CustomerDto>>(okResult.Value);
        Assert.Equal(2, returnedList.Count);
    }

    [Fact]
    public async Task GetCustomer_ReturnsCustomerDto_WhenExists()
    {
        var context = GetDbContext("GetCustomerExistsDb");
        context.Customers.Add(new Customer { Id = 1, Name = "Customer 1", Location = "Loc 1" });
        context.SaveChanges();

        var controller = new CustomerController(context, GetMapper());
        var result = await controller.GetCustomer(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var customerDto = Assert.IsType<CustomerDto>(okResult.Value);
        Assert.Equal(1, customerDto.Id);
        Assert.Equal("Customer 1", customerDto.Name);
        Assert.Equal("Loc 1", customerDto.Location);
    }

    [Fact]
    public async Task GetCustomer_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext("GetCustomerNotExistsDb");
        var controller = new CustomerController(context, GetMapper());
        var result = await controller.GetCustomer(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateCustomer_ReturnsOk()
    {
        var context = GetDbContext("CreateCustomerDb");
        var controller = new CustomerController(context, GetMapper());
        var dto = new CreateCustomerDto { Name = "New Customer", Location = "New Location" };

        var result = await controller.CreateCustomer(dto);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsOk_WhenExists()
    {
        var context = GetDbContext("UpdateCustomerExistsDb");
        context.Customers.Add(new Customer { Id = 1, Name = "Old Name", Location = "Old Location" });
        context.SaveChanges();

        var controller = new CustomerController(context, GetMapper());
        var dto = new UpdateCustomerDto { Name = "Updated Name", Location = "Updated Location" };

        var result = await controller.UpdateCustomer(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedDto = Assert.IsType<CustomerDto>(okResult.Value);
        Assert.Equal("Updated Name", updatedDto.Name);
        Assert.Equal("Updated Location", updatedDto.Location);

        var updatedCustomer = await context.Customers.FindAsync(1);
        Assert.NotNull(updatedCustomer);
        Assert.Equal("Updated Name", updatedCustomer.Name);
        Assert.Equal("Updated Location", updatedCustomer.Location);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext("UpdateCustomerNotExistsDb");
        var controller = new CustomerController(context, GetMapper());
        var dto = new UpdateCustomerDto { Name = "Updated Name", Location = "Updated Location" };

        var result = await controller.UpdateCustomer(99, dto);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteCustomer_ReturnsNoContent_WhenExists()
    {
        var context = GetDbContext("DeleteCustomerExistsDb");
        context.Customers.Add(new Customer { Id = 1, Name = "Customer", Location = "Loc" });
        context.SaveChanges();

        var controller = new CustomerController(context, GetMapper());
        var result = await controller.DeleteCustomer(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await context.Customers.FindAsync(1));
    }

    [Fact]
    public async Task DeleteCustomer_ReturnsNotFound_WhenNotExists()
    {
        var context = GetDbContext("DeleteCustomerNotExistsDb");
        var controller = new CustomerController(context, GetMapper());
        var result = await controller.DeleteCustomer(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteCustomersBulk_ReturnsNoContent_WhenCustomersExist()
    {
        var context = GetDbContext("DeleteCustomersBulkExistsDb");
        context.Customers.AddRange(
            new Customer { Id = 1, Name = "Customer 1", Location = "Loc 1" },
            new Customer { Id = 2, Name = "Customer 2", Location = "Loc 2" }
        );
        context.SaveChanges();

        var controller = new CustomerController(context, GetMapper());
        var dto = new DeleteCustomersDto { Ids = new List<int> { 1, 2 } };

        var result = await controller.DeleteCustomersBulk(dto);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Customers.ToList());
    }

    [Fact]
    public async Task DeleteCustomersBulk_ReturnsNotFound_WhenNoCustomersExist()
    {
        var context = GetDbContext("DeleteCustomersBulkNotExistsDb");
        var controller = new CustomerController(context, GetMapper());
        var dto = new DeleteCustomersDto { Ids = new List<int> { 99, 100 } };

        var result = await controller.DeleteCustomersBulk(dto);

        Assert.IsType<NotFoundResult>(result);
    }
}