using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreWeb.Models;
using StoreWeb.Repositories;

namespace StoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly RepositoryContext _context;

    public ProductController(RepositoryContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Admin Product";
        var products = await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.ProductId)
            .ToListAsync();

        return View(products);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product is null)
        {
            return NotFound();
        }

        ViewData["Title"] = $"Admin Product Detail - {product.ProductName}";
        return View(product);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Admin Product Create";
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProductName,Price,ImageURL")] Product product)
    {
        ValidateProduct(product);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Admin Product Create";
            return View(product);
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        ViewData["Title"] = $"Admin Product Edit - {product.ProductName}";
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Price,ImageURL")] Product product)
    {
        if (id != product.ProductId)
        {
            return NotFound();
        }

        ValidateProduct(product);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = $"Admin Product Edit - {product.ProductName}";
            return View(product);
        }

        try
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(product.ProductId))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product is null)
        {
            return NotFound();
        }

        ViewData["Title"] = $"Admin Product Delete - {product.ProductName}";
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is not null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.ProductId == id);
    }

    private void ValidateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.ProductName))
        {
            ModelState.AddModelError(nameof(Product.ProductName), "Product name is required.");
        }

        if (product.Price <= 0)
        {
            ModelState.AddModelError(nameof(Product.Price), "Price must be greater than zero.");
        }
    }
}
