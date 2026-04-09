using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StoreWeb.Models.Identity;
using StoreWeb.Services;
using StoreWeb.ViewModels;

namespace StoreWeb.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderController(
        IOrderService orderService,
        ICartService cartService,
        UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _cartService = cartService;
        _userManager = userManager;
    }

    public IActionResult Checkout()
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        if (!cart.Any())
            return RedirectToAction("Index", "Cart");

        return View(new CheckoutViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel vm)
    {
        if (!ModelState.IsValid)
            return View("Checkout", vm);

        var cart = _cartService.GetCart(HttpContext.Session);
        if (!cart.Any())
            return RedirectToAction("Index", "Cart");

        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.PlaceOrderAsync(userId, vm, HttpContext.Session);

        return RedirectToAction(nameof(Confirmation), new { id = order.OrderId });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.GetOrderAsync(id, userId);

        if (order is null)
            return NotFound();

        return View(order);
    }
}
