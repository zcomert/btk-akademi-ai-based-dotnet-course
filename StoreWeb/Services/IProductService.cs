using StoreWeb.Models;

namespace StoreWeb.Services;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync(bool asNoTracking = true);
    Task<Product?> GetByIdAsync(int id, bool asNoTracking = true);
    Task AddAsync(Product product);
    void Update(Product product);
    void Remove(Product product);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
    Task<decimal> GetTotalInventoryValueAsync();
    Task<IReadOnlyList<Product>> GetLatestAsync(int count);
    Task<int> SaveChangesAsync();
}
