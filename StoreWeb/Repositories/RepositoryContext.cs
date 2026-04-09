using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StoreWeb.Models.Identity;
using StoreWeb.Models;

namespace StoreWeb.Repositories;

public class RepositoryContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public RepositoryContext(DbContextOptions<RepositoryContext> options) 
        : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(user => user.NormalizedEmail)
            .HasDatabaseName("EmailIndex")
            .IsUnique();

        modelBuilder.Entity<Product>().HasData(
            new Product { ProductId = 1, ProductName = "Laptop", Price = 45999.90m, ImageURL = "images/1.jpg" },
            new Product { ProductId = 2, ProductName = "Wireless Mouse", Price = 899.50m, ImageURL = "images/2.jpg" },
            new Product { ProductId = 3, ProductName = "Mechanical Keyboard", Price = 2499.00m, ImageURL = "images/3.jpg" },
            new Product { ProductId = 4, ProductName = "27-inch Monitor", Price = 12999.00m, ImageURL = "images/4.jpg" },
            new Product { ProductId = 5, ProductName = "Bluetooth Headphones", Price = 3199.90m, ImageURL = "images/5.jpg" }
        );
    }
}
