using Microsoft.AspNetCore.Http;
using StoreWeb.Models;
using StoreWeb.ViewModels;

namespace StoreWeb.Services;

public interface IOrderService
{
    Task<Order> PlaceOrderAsync(string userId, CheckoutViewModel vm, ISession session);
    Task<Order?> GetOrderAsync(int orderId, string userId);
}
