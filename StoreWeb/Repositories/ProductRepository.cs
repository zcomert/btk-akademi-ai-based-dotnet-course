using Microsoft.EntityFrameworkCore;
using StoreWeb.Models;

namespace StoreWeb.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly RepositoryContext _context;

    public ProductRepository(RepositoryContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(bool asNoTracking = true)
    {
        IQueryable<Product> query = _context.Products.OrderBy(p => p.ProductId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id, bool asNoTracking = true)
    {
        IQueryable<Product> query = _context.Products;
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(p => p.ProductId == id);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Remove(Product product)
    {
        _context.Products.Remove(product);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(e => e.ProductId == id);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Products.CountAsync();
    }

    public async Task<decimal> GetTotalInventoryValueAsync()
    {
        return await _context.Products.SumAsync(p => p.Price);
    }

    public async Task<IReadOnlyList<Product>> GetLatestAsync(int count)
    {
        return await _context.Products
            .AsNoTracking()
            .OrderByDescending(p => p.ProductId)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
