using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreWeb.Models;
using StoreWeb.Models.Identity;
using StoreWeb.Services;
using System.IO;

namespace StoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly IWebHostEnvironment _environment;
    private const long MaxImageSizeBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public ProductController(IProductService productService, IWebHostEnvironment environment)
    {
        _productService = productService;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Admin Product";
        var products = await _productService.GetAllAsync(asNoTracking: true);

        return View(products);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(id.Value, asNoTracking: true);

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
    public async Task<IActionResult> Create([Bind("ProductName,Price")] Product product, IFormFile? imageFile)
    {
        ValidateProduct(product);
        ValidateImageFile(imageFile);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Admin Product Create";
            return View(product);
        }

        product.ImageURL = await SaveProductImageAsync(imageFile);
        await _productService.AddAsync(product);
        await _productService.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(id.Value, asNoTracking: true);
        if (product is null)
        {
            return NotFound();
        }

        ViewData["Title"] = $"Admin Product Edit - {product.ProductName}";
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Price,ImageURL")] Product product, IFormFile? imageFile)
    {
        if (id != product.ProductId)
        {
            return NotFound();
        }

        ValidateProduct(product);
        ValidateImageFile(imageFile);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = $"Admin Product Edit - {product.ProductName}";
            return View(product);
        }

        try
        {
            var existingProduct = await _productService.GetByIdAsync(id, asNoTracking: false);
            if (existingProduct is null)
            {
                return NotFound();
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.Price = product.Price;

            if (imageFile is not null)
            {
                var oldImagePath = existingProduct.ImageURL;
                existingProduct.ImageURL = await SaveProductImageAsync(imageFile);
                DeleteProductImage(oldImagePath);
            }

            await _productService.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExistsAsync(product.ProductId))
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

        var product = await _productService.GetByIdAsync(id.Value, asNoTracking: true);

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
        var product = await _productService.GetByIdAsync(id, asNoTracking: false);
        if (product is not null)
        {
            DeleteProductImage(product.ImageURL);
            _productService.Remove(product);
            await _productService.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ProductExistsAsync(int id)
    {
        return await _productService.ExistsAsync(id);
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

    private void ValidateImageFile(IFormFile? imageFile)
    {
        if (imageFile is null)
        {
            return;
        }

        if (imageFile.Length == 0)
        {
            ModelState.AddModelError("ImageFile", "Please select a valid image file.");
            return;
        }

        if (imageFile.Length > MaxImageSizeBytes)
        {
            ModelState.AddModelError("ImageFile", "Image size must be 10 MB or smaller.");
        }

        var extension = Path.GetExtension(imageFile.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            ModelState.AddModelError("ImageFile", "Only JPG, PNG, and WEBP image files are allowed.");
        }

        if (string.IsNullOrWhiteSpace(imageFile.ContentType) || !AllowedContentTypes.Contains(imageFile.ContentType))
        {
            ModelState.AddModelError("ImageFile", "Unsupported image content type.");
        }
    }

    private async Task<string?> SaveProductImageAsync(IFormFile? imageFile)
    {
        if (imageFile is null)
        {
            return null;
        }

        var imagesDirectory = Path.Combine(_environment.WebRootPath, "images", "products");
        Directory.CreateDirectory(imagesDirectory);

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(imagesDirectory, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await imageFile.CopyToAsync(stream);

        return $"/images/products/{fileName}";
    }

    private void DeleteProductImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("/images/products/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

        if (System.IO.File.Exists(absolutePath))
        {
            System.IO.File.Delete(absolutePath);
        }
    }
}
