using Microsoft.AspNetCore.Mvc;
using StoreWeb.Repositories;

namespace StoreWeb.Controllers;

public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;

    public ProductController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Urunler";
        var products = await _productRepository.GetAllAsync(asNoTracking: true);
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productRepository.GetByIdAsync(id, asNoTracking: true);
        
        if (product is null)
        {
            return NotFound();
        }

        ViewData["Title"] = product.ProductName;
        return View(product);
    }
}
