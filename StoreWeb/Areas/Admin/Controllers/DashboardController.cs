using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreWeb.Models.Identity;
using StoreWeb.Repositories;

namespace StoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class DashboardController : Controller
{
    private readonly IProductRepository _productRepository;

    public DashboardController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IActionResult> Index()
    {
        var productCount = await _productRepository.CountAsync();
        var totalInventoryValue = await _productRepository.GetTotalInventoryValueAsync();
        var latestProducts = await _productRepository.GetLatestAsync(5);

        ViewData["Title"] = "Admin Dashboard";
        ViewData["ProductCount"] = productCount;
        ViewData["TotalInventoryValue"] = totalInventoryValue;

        return View(latestProducts);
    }
}
