using Microsoft.EntityFrameworkCore;

namespace StoreWeb.Repositories;

public class RepositoryContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public RepositoryContext(DbContextOptions<RepositoryContext> options) 
        : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { ProductId = 1, ProductName = "Laptop", Price = 45999.90m },
            new Product { ProductId = 2, ProductName = "Wireless Mouse", Price = 899.50m },
            new Product { ProductId = 3, ProductName = "Mechanical Keyboard", Price = 2499.00m },
            new Product { ProductId = 4, ProductName = "27-inch Monitor", Price = 12999.00m },
            new Product { ProductId = 5, ProductName = "Bluetooth Headphones", Price = 3199.90m }
        );

        base.OnModelCreating(modelBuilder);
    }
}
