using Microsoft.EntityFrameworkCore;
using StoreWeb.Models;

namespace StoreWeb.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly RepositoryContext _context;

    public OrderRepository(RepositoryContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task<Order?> GetByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
