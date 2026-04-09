using StoreWeb.Models;

namespace StoreWeb.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(int orderId);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId);
    Task<int> SaveChangesAsync();
}
