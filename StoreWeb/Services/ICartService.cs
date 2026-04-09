using Microsoft.AspNetCore.Http;
using StoreWeb.Models;

namespace StoreWeb.Services;

public interface ICartService
{
    void AddItem(ISession session, Product product);
    void RemoveItem(ISession session, int productId);
    List<CartItem> GetCart(ISession session);
    void ClearCart(ISession session);
    decimal GetTotal(ISession session);
}
