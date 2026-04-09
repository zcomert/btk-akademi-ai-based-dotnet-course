using Microsoft.AspNetCore.Mvc;
using StoreWeb.Services;

namespace StoreWeb.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IProductService _productService;

    public CartController(ICartService cartService, IProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
    }

    public IActionResult Index()
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        ViewData["Total"] = _cartService.GetTotal(HttpContext.Session);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId)
    {
        var product = await _productService.GetByIdAsync(productId);
        if (product is null)
            return NotFound();

        _cartService.AddItem(HttpContext.Session, product);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveItem(int productId)
    {
        _cartService.RemoveItem(HttpContext.Session, productId);
        return RedirectToAction(nameof(Index));
    }
}
