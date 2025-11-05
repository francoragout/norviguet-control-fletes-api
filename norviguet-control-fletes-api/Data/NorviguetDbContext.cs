using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Entities;

namespace norviguet_control_fletes_api.Data
{
    public class NorviguetDbContext(DbContextOptions<NorviguetDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<DeliveryNote> DeliveryNotes { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<PaymentOrder> PaymentOrders { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Seller> Sellers { get; set; } = null!;
        public virtual DbSet<Carrier> Carriers { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enum conversions
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.Incoterm)
                .HasConversion<string>();

            modelBuilder.Entity<DeliveryNote>()
               .Property(dn => dn.Status)
               .HasConversion<string>();

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Type)
                .HasConversion<string>();

            // Unique constraints
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<DeliveryNote>()
                .HasIndex(dn => dn.DeliveryNoteNumber)
                .IsUnique();

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<PaymentOrder>()
                .HasIndex(po => po.PaymentOrderNumber)
                .IsUnique();

            // Unique combined constraints
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.OrderId, i.CarrierId })
                .IsUnique();

            modelBuilder.Entity<PaymentOrder>()
                .HasIndex(po => new { po.OrderId, po.CarrierId })
                .IsUnique();

            // Precision settings
            modelBuilder.Entity<Invoice>()
                .Property(o => o.Price)
                .HasPrecision(18, 2);

            // Relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.DeliveryNotes)
                .WithOne(dn => dn.Order)
                .HasForeignKey(dn => dn.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Invoices)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.PaymentOrders)
                .WithOne(po => po.Order)
                .HasForeignKey(po => po.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Carrier>()
                .HasMany(c => c.DeliveryNotes)
                .WithOne(dn => dn.Carrier)
                .HasForeignKey(dn => dn.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Carrier>()
                .HasMany(c => c.Invoices)
                .WithOne(i => i.Carrier)
                .HasForeignKey(i => i.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Carrier>()
                .HasMany(c => c.PaymentOrders)
                .WithOne(po => po.Carrier)
                .HasForeignKey(po => po.CarrierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Seller>()
                .HasMany(s => s.Orders)
                .WithOne(o => o.Seller)
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
