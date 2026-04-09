using StoreWeb.Models;

namespace StoreWeb.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(int orderId);
    Task<int> SaveChangesAsync();
}
