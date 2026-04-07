using Microsoft.EntityFrameworkCore;

namespace StoreWeb.Repositories;

public class RepositoryContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public RepositoryContext(DbContextOptions<RepositoryContext> options) 
        : base(options)
    {
        
    }
}