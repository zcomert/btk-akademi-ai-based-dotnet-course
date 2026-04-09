using Microsoft.AspNetCore.Mvc;
using StoreWeb.Services;

namespace StoreWeb.Controllers;

public class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Urunler";
        var products = await _productService.GetAllAsync(asNoTracking: true);
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetByIdAsync(id, asNoTracking: true);
        
        if (product is null)
        {
            return NotFound();
        }

        ViewData["Title"] = product.ProductName;
        return View(product);
    }
}
