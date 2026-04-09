using StoreWeb.Models;
using StoreWeb.Repositories;

namespace StoreWeb.Services;

public class ProductManager : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductManager(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(bool asNoTracking = true)
    {
        return await _productRepository.GetAllAsync(asNoTracking);
    }

    public async Task<Product?> GetByIdAsync(int id, bool asNoTracking = true)
    {
        return await _productRepository.GetByIdAsync(id, asNoTracking);
    }

    public async Task AddAsync(Product product)
    {
        await _productRepository.AddAsync(product);
    }

    public void Update(Product product)
    {
        _productRepository.Update(product);
    }

    public void Remove(Product product)
    {
        _productRepository.Remove(product);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _productRepository.ExistsAsync(id);
    }

    public async Task<int> CountAsync()
    {
        return await _productRepository.CountAsync();
    }

    public async Task<decimal> GetTotalInventoryValueAsync()
    {
        return await _productRepository.GetTotalInventoryValueAsync();
    }

    public async Task<IReadOnlyList<Product>> GetLatestAsync(int count)
    {
        return await _productRepository.GetLatestAsync(count);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _productRepository.SaveChangesAsync();
    }
}
