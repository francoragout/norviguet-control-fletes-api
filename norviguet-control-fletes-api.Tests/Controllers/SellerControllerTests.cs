using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Controllers;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Profiles;
using norviguet_control_fletes_api.Models.Seller;
using norviguet_control_fletes_api.Models.Common;
using norviguet_control_fletes_api.Entities;
using Xunit;

namespace norviguet_control_fletes_api.Tests.Controllers
{
    public class SellerControllerTests
    {
        private readonly SellerController _controller;
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public SellerControllerTests()
        {
            var options = new DbContextOptionsBuilder<NorviguetDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            _context = new NorviguetDbContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SellerProfile>();
            });
            _mapper = config.CreateMapper();

            _controller = new SellerController(_context, _mapper);
        }

        [Fact]
        public async Task GetSellers_ReturnsOk_WithListOfSellers()
        {
            _context.Sellers.AddRange(
            new Seller { Id = 1, Name = "Seller A", Zone = "North" },
            new Seller { Id = 2, Name = "Seller B", Zone = "South" }
            );
            await _context.SaveChangesAsync();
            var result = await _controller.GetSellers();
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var sellers = Assert.IsType<List<SellerDto>>(okResult.Value);
            Assert.Equal(2, sellers.Count);
        }

        [Fact]
        public async Task GetSeller_ReturnsNotFound_WhenSellerDoesNotExist()
        {
            var result = await _controller.GetSeller(999);
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSeller_ReturnsOk_WithSeller()
        {
            var seller = new Seller { Id = 1, Name = "Seller A", Zone = "North" };
            _context.Sellers.Add(seller);
            await _context.SaveChangesAsync();
            var result = await _controller.GetSeller(1);
            var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
            var returnedSeller = Assert.IsType<SellerDto>(okResult.Value);
            Assert.Equal(seller.Id, returnedSeller.Id);
            Assert.Equal(seller.Name, returnedSeller.Name);
            Assert.Equal(seller.Zone, returnedSeller.Zone);
        }

        [Fact]
        public async Task CreateSeller_AddsSellerToDatabase()
        {
            var dto = new CreateSellerDto
            {
                Name = "New Seller",
                Zone = "East"
            };
            var result = await _controller.CreateSeller(dto);
            Assert.IsType<Microsoft.AspNetCore.Mvc.OkResult>(result);
            var sellerInDb = await _context.Sellers.FirstOrDefaultAsync(s => s.Name == "New Seller");
            Assert.NotNull(sellerInDb);
            Assert.Equal("East", sellerInDb.Zone);
        }

        [Fact]
        public async Task UpdateSeller_UpdatesExistingSeller()
        {
            var seller = new Seller { Id = 1, Name = "Seller A", Zone = "North" };
            _context.Sellers.Add(seller);
            await _context.SaveChangesAsync();
            var dto = new UpdateSellerDto
            {
                Name = "Updated Seller",
                Zone = "West"
            };
            var result = await _controller.UpdateSeller(1, dto);
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            var updatedSeller = await _context.Sellers.FindAsync(1);
            Assert.NotNull(updatedSeller);
            Assert.Equal("Updated Seller", updatedSeller.Name);
            Assert.Equal("West", updatedSeller.Zone);
        }

        [Fact]
        public async Task UpdateSeller_ReturnsNotFound_WhenSellerDoesNotExist()
        {
            var dto = new UpdateSellerDto
            {
                Name = "Nonexistent Seller",
                Zone = "West"
            };
            var result = await _controller.UpdateSeller(999, dto);
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteSeller_DeleteExistingSeller()
        {
            var seller = new Seller { Id = 1, Name = "Seller A", Zone = "North" };
            _context.Sellers.Add(seller);
            await _context.SaveChangesAsync();
            var result = await _controller.DeleteSeller(1);
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            var deletedSeller = await _context.Sellers.FindAsync(1);
            Assert.Null(deletedSeller);
        }

        [Fact]
        public async Task DeleteSeller_ReturnsNotFound_WhenSellerDoesNotExist()
        {
            var result = await _controller.DeleteSeller(999);
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteSeller_ReturnsConflict_WhenSellerHasAssociatedOrders()
        {
            var seller = new Seller
            {
                Id = 1,
                Name = "Seller with orders",
                Zone = "North",
                Orders = new List<Order>()
            };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order
            {
                SellerId = 1,
                Seller = seller,
                CustomerId = 1,
                Customer = customer
            };
            seller.Orders.Add(order);
            _context.Sellers.Add(seller);
            _context.Customers.Add(customer);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var result = await _controller.DeleteSeller(1);
            var conflictResult = Assert.IsType<Microsoft.AspNetCore.Mvc.ConflictObjectResult>(result);
            var value = conflictResult.Value?.ToString();
            Assert.Contains("ASSOCIATED_RECORDS", value);
            Assert.Contains("Seller cannot be deleted because it has associated orders.", value);
        }

        [Fact]
        public async Task DeleteSellersBulk_ReturnsNoContent()
        {
            var seller1 = new Seller { Id = 1, Name = "Seller A", Zone = "North" };
            var seller2 = new Seller { Id = 2, Name = "Seller B", Zone = "South" };
            _context.Sellers.AddRange(seller1, seller2);
            await _context.SaveChangesAsync();
            var idsToDelete = new List<int> { 1, 2 };
            var dto = new DeleteEntitiesDto { Ids = idsToDelete };
            var result = await _controller.DeleteSellers(dto);
            Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
            var sellersInDb = await _context.Sellers.Where(s => idsToDelete.Contains(s.Id)).ToListAsync();
            Assert.Empty(sellersInDb);
        }

        [Fact]
        public async Task DeleteSellersBulk_ReturnsConflict_WhenAnySellerHasAssociatedOrders()
        {
            var seller1 = new Seller { Id = 1, Name = "Seller A", Zone = "North" };
            var seller2 = new Seller { Id = 2, Name = "Seller B", Zone = "South" };
            var customer = new Customer { Id = 1, Name = "Customer1", CUIT = "20-12345678-9" };
            var order = new Order
            {
                SellerId = 2,
                Seller = seller2,
                CustomerId = 1,
                Customer = customer
            };
            seller2.Orders.Add(order);
            _context.Sellers.AddRange(seller1, seller2);
            _context.Customers.Add(customer);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var idsToDelete = new List<int> { 1, 2 };
            var dto = new DeleteEntitiesDto { Ids = idsToDelete };
            var result = await _controller.DeleteSellers(dto);
            var conflictResult = Assert.IsType<Microsoft.AspNetCore.Mvc.ConflictObjectResult>(result);
            var value = conflictResult.Value?.ToString();
            Assert.Contains("ASSOCIATED_RECORDS", value);
            Assert.Contains("Some sellers could not be deleted because they have associated orders.", value);
        }

        [Fact]
        public async Task DeleteSellersBulk_ReturnsNotFound_WhenNoSellersExist()
        {
            var idsToDelete = new List<int> { 999, 1000 };
            var dto = new DeleteEntitiesDto { Ids = idsToDelete };
            var result = await _controller.DeleteSellers(dto);
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
        }
    }
}
