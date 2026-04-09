using System.Text.Json;
using Microsoft.AspNetCore.Http;
using StoreWeb.Models;

namespace StoreWeb.Services;

public class CartManager : ICartService
{
    private const string CartKey = "StoreWeb_Cart";

    public void AddItem(ISession session, Product product)
    {
        var cart = GetCart(session);
        var existing = cart.FirstOrDefault(x => x.ProductId == product.ProductId);
        if (existing is not null)
        {
            existing.Quantity++;
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                Quantity = 1
            });
        }
        SaveCart(session, cart);
    }

    public void RemoveItem(ISession session, int productId)
    {
        var cart = GetCart(session);
        var item = cart.FirstOrDefault(x => x.ProductId == productId);
        if (item is null) return;
        cart.Remove(item);
        SaveCart(session, cart);
    }

    public List<CartItem> GetCart(ISession session)
    {
        var json = session.GetString(CartKey);
        if (json is null)
            return new List<CartItem>();
        try
        {
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }
        catch (JsonException)
        {
            return new List<CartItem>();
        }
    }

    public void ClearCart(ISession session)
    {
        session.Remove(CartKey);
    }

    public decimal GetTotal(ISession session)
    {
        return GetCart(session).Sum(x => x.Price * x.Quantity);
    }

    private static void SaveCart(ISession session, List<CartItem> cart)
    {
        session.SetString(CartKey, JsonSerializer.Serialize(cart));
    }
}
