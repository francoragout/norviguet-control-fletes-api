using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Carrier;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services;

namespace Tests.Unit.Services
{
    public class CarrierServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly CarrierService _service;

        public CarrierServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite($"DataSource=:memory:")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Carrier, CarrierDto>();
                cfg.CreateMap<CarrierCreateDto, Carrier>();
                cfg.CreateMap<CarrierUpdateDto, Carrier>();
            });

            _mapper = mapperConfig.CreateMapper();
            _service = new CarrierService(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.CloseConnection();
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithValidRequest_ReturnsPagedResult()
        {
            // Arrange
            var carriers = new List<Carrier>
            {
                new() { Id = 1, Name = "Carrier 1", CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new() { Id = 2, Name = "Carrier 2", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new() { Id = 3, Name = "Carrier 3", CreatedAt = DateTime.UtcNow.AddDays(-1) }
            };

            await _context.Carriers.AddRangeAsync(carriers);
            await _context.SaveChangesAsync();

            var request = new PagedRequestDto { Page = 1, PageSize = 10 };

            // Act
            var result = await _service.GetAllAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal("Carrier 3", result.Items[0].Name); // Most recent first
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            for (int i = 1; i <= 15; i++)
            {
                await _context.Carriers.AddAsync(new Carrier
                {
                    Id = i,
                    Name = $"Carrier {i}",
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                });
            }
            await _context.SaveChangesAsync();

            var request = new PagedRequestDto { Page = 2, PageSize = 5 };

            // Act
            var result = await _service.GetAllAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(15, result.TotalItems);
            Assert.Equal(5, result.Items.Count);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var request = new PagedRequestDto { Page = 1, PageSize = 10 };

            // Act
            var result = await _service.GetAllAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalItems);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetAllAsync_WithNullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.GetAllAsync(null!, CancellationToken.None));
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsCarrier()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Test Carrier" };
            await _context.Carriers.AddAsync(carrier);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetByIdAsync(1, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Carrier", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetByIdAsync(999, CancellationToken.None));

            Assert.Equal("Carrier not found", exception.Message);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidDto_CreatesCarrier()
        {
            // Arrange
            var createDto = new CarrierCreateDto { Name = "New Carrier" };

            // Act
            var result = await _service.CreateAsync(createDto, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Carrier", result.Name);
            Assert.True(result.Id > 0);

            var carrierInDb = await _context.Carriers.FindAsync(result.Id);
            Assert.NotNull(carrierInDb);
            Assert.Equal("New Carrier", carrierInDb.Name);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateName_ThrowsConflictException()
        {
            // Arrange
            var existingCarrier = new Carrier { Id = 1, Name = "Existing Carrier" };
            await _context.Carriers.AddAsync(existingCarrier);
            await _context.SaveChangesAsync();

            var createDto = new CarrierCreateDto { Name = "Existing Carrier" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _service.CreateAsync(createDto, CancellationToken.None));

            Assert.Equal("A carrier with the name 'Existing Carrier' already exists", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_WithNullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreateAsync(null!, CancellationToken.None));
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidDto_UpdatesCarrier()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Original Name" };
            await _context.Carriers.AddAsync(carrier);
            await _context.SaveChangesAsync();

            var rowVersion = _context.Entry(carrier).Property(x => x.RowVersion).CurrentValue;

            var updateDto = new CarrierUpdateDto
            {
                Name = "Updated Name",
                RowVersion = rowVersion!
            };

            // Act
            var result = await _service.UpdateAsync(1, updateDto, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);

            var updatedCarrier = await _context.Carriers.FindAsync(1);
            Assert.NotNull(updatedCarrier);
            Assert.Equal("Updated Name", updatedCarrier.Name);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidId_ThrowsNotFoundException()
        {
            // Arrange
            var updateDto = new CarrierUpdateDto
            {
                Name = "Updated Name",
                RowVersion = new byte[] { 1, 2, 3, 4 }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.UpdateAsync(999, updateDto, CancellationToken.None));

            Assert.Equal("Carrier not found", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateName_ThrowsConflictException()
        {
            // Arrange
            var carrier1 = new Carrier { Id = 1, Name = "Carrier 1" };
            var carrier2 = new Carrier { Id = 2, Name = "Carrier 2" };
            await _context.Carriers.AddRangeAsync(carrier1, carrier2);
            await _context.SaveChangesAsync();

            var rowVersion = _context.Entry(carrier2).Property(x => x.RowVersion).CurrentValue;

            var updateDto = new CarrierUpdateDto
            {
                Name = "Carrier 1",
                RowVersion = rowVersion!
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _service.UpdateAsync(2, updateDto, CancellationToken.None));

            Assert.Equal("A carrier with the name 'Carrier 1' already exists", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithSameName_UpdatesSuccessfully()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Carrier Name" };
            await _context.Carriers.AddAsync(carrier);
            await _context.SaveChangesAsync();

            var rowVersion = _context.Entry(carrier).Property(x => x.RowVersion).CurrentValue;

            var updateDto = new CarrierUpdateDto
            {
                Name = "Carrier Name",
                RowVersion = rowVersion!
            };

            // Act
            var result = await _service.UpdateAsync(1, updateDto, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Carrier Name", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidRowVersion_ThrowsConflictException()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Original Name" };
            await _context.Carriers.AddAsync(carrier);
            await _context.SaveChangesAsync();

            var updateDto = new CarrierUpdateDto
            {
                Name = "Updated Name",
                RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _service.UpdateAsync(1, updateDto, CancellationToken.None));

            Assert.Equal("The record was modified by another user. Please reload and try again.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithNullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.UpdateAsync(1, null!, CancellationToken.None));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidIds_DeletesCarriers()
        {
            // Arrange
            var carriers = new List<Carrier>
            {
                new() { Id = 1, Name = "Carrier 1" },
                new() { Id = 2, Name = "Carrier 2" },
                new() { Id = 3, Name = "Carrier 3" }
            };

            await _context.Carriers.AddRangeAsync(carriers);
            await _context.SaveChangesAsync();

            var idsToDelete = new[] { 1, 2 };

            // Act
            await _service.DeleteAsync(idsToDelete, CancellationToken.None);

            // Assert
            var remainingCarriers = await _context.Carriers.ToListAsync();
            Assert.Single(remainingCarriers);
            Assert.Equal(3, remainingCarriers[0].Id);
        }

        [Fact]
        public async Task DeleteAsync_WithDuplicateIds_DeletesOnce()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Carrier 1" };
            await _context.Carriers.AddAsync(carrier);
            await _context.SaveChangesAsync();

            var idsToDelete = new[] { 1, 1, 1 };

            // Act
            await _service.DeleteAsync(idsToDelete, CancellationToken.None);

            // Assert
            var remainingCarriers = await _context.Carriers.ToListAsync();
            Assert.Empty(remainingCarriers);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentIds_ThrowsNotFoundException()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Carrier 1" };
            await _context.Carriers.AddAsync(carrier);
            await _context.SaveChangesAsync();

            var idsToDelete = new[] { 1, 999 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.DeleteAsync(idsToDelete, CancellationToken.None));

            Assert.Equal("Some of the specified carriers were not found", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithEmptyIds_DoesNothing()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Carrier 1" };
            await _context.Carriers.AddAsync(carrier);
            await _context.SaveChangesAsync();

            var idsToDelete = Array.Empty<int>();

            // Act
            await _service.DeleteAsync(idsToDelete, CancellationToken.None);

            // Assert
            var remainingCarriers = await _context.Carriers.ToListAsync();
            Assert.Single(remainingCarriers);
        }

        [Fact]
        public async Task DeleteAsync_WithDeliveryNoteAssociation_ThrowsConflictException()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Carrier 1" };
            var customer = new Customer { Id = 1, Name = "Customer 1" };
            var seller = new Seller { Id = 1, Name = "Seller 1" };
            await _context.Carriers.AddAsync(carrier);
            await _context.Customers.AddAsync(customer);
            await _context.Sellers.AddAsync(seller);
            await _context.SaveChangesAsync();

            var order = new Order
            {
                Id = 1,
                OrderNumber = "ORD-001",
                SellerId = 1,
                CustomerId = 1,
                Seller = seller,
                Customer = customer
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var deliveryNote = new DeliveryNote
            {
                Id = 1,
                DeliveryNoteNumber = "DN-001",
                CarrierId = 1,
                OrderId = 1,
                Order = order,
                Carrier = carrier
            };
            await _context.DeliveryNotes.AddAsync(deliveryNote);
            await _context.SaveChangesAsync();

            var idsToDelete = new[] { 1 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _service.DeleteAsync(idsToDelete, CancellationToken.None));

            Assert.Contains("carrier(s) cannot be deleted due to existing associations", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithInvoiceAssociation_ThrowsConflictException()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Carrier 1" };
            var customer = new Customer { Id = 1, Name = "Customer 1" };
            var seller = new Seller { Id = 1, Name = "Seller 1" };
            await _context.Carriers.AddAsync(carrier);
            await _context.Customers.AddAsync(customer);
            await _context.Sellers.AddAsync(seller);
            await _context.SaveChangesAsync();

            var order = new Order
            {
                Id = 1,
                OrderNumber = "ORD-001",
                SellerId = 1,
                CustomerId = 1,
                Seller = seller,
                Customer = customer
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var invoice = new Invoice
            {
                Id = 1,
                InvoiceNumber = "INV-001",
                CarrierId = 1,
                OrderId = 1,
                Order = order,
                Carrier = carrier
            };
            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();

            var idsToDelete = new[] { 1 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _service.DeleteAsync(idsToDelete, CancellationToken.None));

            Assert.Contains("carrier(s) cannot be deleted due to existing associations", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithPaymentOrderAssociation_ThrowsConflictException()
        {
            // Arrange
            var carrier = new Carrier { Id = 1, Name = "Carrier 1" };
            var customer = new Customer { Id = 1, Name = "Customer 1" };
            var seller = new Seller { Id = 1, Name = "Seller 1" };
            await _context.Carriers.AddAsync(carrier);
            await _context.Customers.AddAsync(customer);
            await _context.Sellers.AddAsync(seller);
            await _context.SaveChangesAsync();

            var order = new Order
            {
                Id = 1,
                OrderNumber = "ORD-001",
                SellerId = 1,
                CustomerId = 1,
                Seller = seller,
                Customer = customer
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var paymentOrder = new PaymentOrder
            {
                Id = 1,
                PaymentOrderNumber = "PO-001",
                CarrierId = 1,
                OrderId = 1,
                Order = order,
                Carrier = carrier
            };
            await _context.PaymentOrders.AddAsync(paymentOrder);
            await _context.SaveChangesAsync();

            var idsToDelete = new[] { 1 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _service.DeleteAsync(idsToDelete, CancellationToken.None));

            Assert.Contains("carrier(s) cannot be deleted due to existing associations", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_WithNullIds_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.DeleteAsync(null!, CancellationToken.None));
        }

        #endregion
    }
}
