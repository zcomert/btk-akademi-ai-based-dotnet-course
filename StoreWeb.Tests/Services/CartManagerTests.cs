using StoreWeb.Models;
using StoreWeb.Services;

namespace StoreWeb.Tests.Services;

public class CartManagerTests
{
    private readonly ICartService _sut = new CartManager();
    private readonly MockSession _session = new();

    [Fact]
    public void AddItem_NewProduct_AddsToCart()
    {
        var product = new Product { ProductId = 1, ProductName = "Laptop", Price = 100m };

        _sut.AddItem(_session, product);

        var cart = _sut.GetCart(_session);
        Assert.Single(cart);
        Assert.Equal(1, cart[0].Quantity);
        Assert.Equal("Laptop", cart[0].ProductName);
    }

    [Fact]
    public void AddItem_ExistingProduct_IncrementsQuantity()
    {
        var product = new Product { ProductId = 1, ProductName = "Laptop", Price = 100m };

        _sut.AddItem(_session, product);
        _sut.AddItem(_session, product);

        var cart = _sut.GetCart(_session);
        Assert.Single(cart);
        Assert.Equal(2, cart[0].Quantity);
    }

    [Fact]
    public void RemoveItem_ExistingProduct_RemovesFromCart()
    {
        var product = new Product { ProductId = 1, ProductName = "Laptop", Price = 100m };
        _sut.AddItem(_session, product);

        _sut.RemoveItem(_session, 1);

        var cart = _sut.GetCart(_session);
        Assert.Empty(cart);
    }

    [Fact]
    public void RemoveItem_NonExistingProduct_DoesNotThrow()
    {
        _sut.RemoveItem(_session, 999);

        var cart = _sut.GetCart(_session);
        Assert.Empty(cart);
    }

    [Fact]
    public void GetTotal_MultipleItems_ReturnsCorrectSum()
    {
        var p1 = new Product { ProductId = 1, ProductName = "A", Price = 100m };
        var p2 = new Product { ProductId = 2, ProductName = "B", Price = 200m };
        _sut.AddItem(_session, p1); // qty 1 → 100
        _sut.AddItem(_session, p1); // qty 2 → 200
        _sut.AddItem(_session, p2); // qty 1 → 200

        var total = _sut.GetTotal(_session);

        Assert.Equal(400m, total);
    }

    [Fact]
    public void ClearCart_AfterAddingItems_EmptiesCart()
    {
        var product = new Product { ProductId = 1, ProductName = "Laptop", Price = 100m };
        _sut.AddItem(_session, product);

        _sut.ClearCart(_session);

        var cart = _sut.GetCart(_session);
        Assert.Empty(cart);
    }

    [Fact]
    public void GetCart_EmptySession_ReturnsEmptyList()
    {
        var cart = _sut.GetCart(_session);

        Assert.Empty(cart);
    }
}
