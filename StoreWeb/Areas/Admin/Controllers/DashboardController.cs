using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreWeb.Models.Identity;
using StoreWeb.Repositories;

namespace StoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class DashboardController : Controller
{
    private readonly RepositoryContext _context;

    public DashboardController(RepositoryContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var productCount = await _context.Products.CountAsync();
        var totalInventoryValue = await _context.Products.SumAsync(p => p.Price);
        var latestProducts = await _context.Products
            .AsNoTracking()
            .OrderByDescending(p => p.ProductId)
            .Take(5)
            .ToListAsync();

        ViewData["Title"] = "Admin Dashboard";
        ViewData["ProductCount"] = productCount;
        ViewData["TotalInventoryValue"] = totalInventoryValue;

        return View(latestProducts);
    }
}
