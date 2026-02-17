using Microsoft.EntityFrameworkCore;
using FastMvcAPI.REslava.Result.Demo.Models;

namespace FastMvcAPI.REslava.Result.Demo.Data;

public class DemoDbContext : DbContext
{
    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Email = "john@example.com", Name = "John Doe", Role = "Admin", IsActive = true, CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Email = "jane@example.com", Name = "Jane Smith", Role = "User", IsActive = true, CreatedAt = DateTime.UtcNow },
            new User { Id = 3, Email = "bob@example.com", Name = "Bob Johnson", Role = "User", IsActive = false, CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 1299.99m, StockQuantity = 50, Category = "Electronics", IsAvailable = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, Name = "Mouse", Description = "Wireless mouse", Price = 29.99m, StockQuantity = 200, Category = "Electronics", IsAvailable = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, Name = "Keyboard", Description = "Mechanical keyboard", Price = 79.99m, StockQuantity = 100, Category = "Electronics", IsAvailable = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 4, Name = "Monitor", Description = "27-inch 4K monitor", Price = 399.99m, StockQuantity = 0, Category = "Electronics", IsAvailable = false, CreatedAt = DateTime.UtcNow },
            new Product { Id = 5, Name = "Headphones", Description = "Noise-cancelling headphones", Price = 199.99m, StockQuantity = 75, Category = "Electronics", IsAvailable = true, CreatedAt = DateTime.UtcNow }
        );
    }
}
