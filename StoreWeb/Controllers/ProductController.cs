using Microsoft.AspNetCore.Mvc;
using StoreWeb.Repositories;

namespace StoreWeb.Controllers;

public class ProductController : Controller
{
    private readonly RepositoryContext _context;

    public ProductController(RepositoryContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Urunler";
        var products = _context.Products.ToList();
        return View(products);
    }

    public IActionResult Details(int id)
    {
        var product = _context
            .Products
            .FirstOrDefault(p => p.ProductId == id);
        
        if (product is null)
        {
            return NotFound();
        }
        return View(product);
    }
}
