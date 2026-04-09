using Microsoft.AspNetCore.Http;
using StoreWeb.Models;
using StoreWeb.Repositories;
using StoreWeb.ViewModels;

namespace StoreWeb.Services;

public class OrderManager : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartService _cartService;

    public OrderManager(IOrderRepository orderRepository, ICartService cartService)
    {
        _orderRepository = orderRepository;
        _cartService = cartService;
    }

    public async Task<Order> PlaceOrderAsync(string userId, CheckoutViewModel vm, ISession session)
    {
        var cartItems = _cartService.GetCart(session);

        var order = new Order
        {
            UserId = userId,
            FullName = vm.FullName,
            Address = vm.Address,
            CreatedAt = DateTimeOffset.UtcNow,
            OrderItems = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = item.Price,
                Quantity = item.Quantity
            }).ToList()
        };

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();
        _cartService.ClearCart(session);

        return order;
    }

    public async Task<Order?> GetOrderAsync(int orderId, string userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null || order.UserId != userId)
            return null;
        return order;
    }
}
