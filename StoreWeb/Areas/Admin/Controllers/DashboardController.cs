using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreWeb.Models.Identity;
using StoreWeb.Services;

namespace StoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class DashboardController : Controller
{
    private readonly IProductService _productService;

    public DashboardController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var productCount = await _productService.CountAsync();
        var totalInventoryValue = await _productService.GetTotalInventoryValueAsync();
        var latestProducts = await _productService.GetLatestAsync(5);

        ViewData["Title"] = "Admin Dashboard";
        ViewData["ProductCount"] = productCount;
        ViewData["TotalInventoryValue"] = totalInventoryValue;

        return View(latestProducts);
    }
}
