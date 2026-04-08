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
}
