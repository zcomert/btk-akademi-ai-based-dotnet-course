using Microsoft.AspNetCore.Mvc;
using StoreWeb.Services;

namespace StoreWeb.Api;

/// <summary>
/// Provides read-only product endpoints for external consumers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Returns all products.
    /// </summary>
    /// <returns>List of products.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAllAsync(asNoTracking: true);

        var result = products
            .Select(p => new ProductDto(
                p.ProductId,
                p.ProductName,
                p.Price,
                p.ImageURL))
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Returns a single product by id.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <returns>The requested product.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id, asNoTracking: true);
        if (product is null)
        {
            return NotFound();
        }

        var result = new ProductDto(
            product.ProductId,
            product.ProductName,
            product.Price,
            product.ImageURL);

        return Ok(result);
    }
}
