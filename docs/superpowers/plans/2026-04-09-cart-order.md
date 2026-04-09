# Sepet & Sipariş Sistemi — Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Anonim kullanıcıların ürünleri session tabanlı sepete ekleyebildiği, giriş yaptıktan sonra minimal form (ad-soyad + adres) ile siparişi tamamlayabildiği ve sipariş özeti + teşekkür sayfasının gösterildiği bir sistem inşa etmek.

**Architecture:** `ICartService`/`CartManager` session'da `List<CartItem>` saklar. `IOrderService`/`OrderManager` siparişi DB'ye yazar ve sepeti temizler. `CartController` [Authorize] gerektirmez; `OrderController` tamamen `[Authorize]` ile korunur.

**Tech Stack:** ASP.NET Core MVC (.NET 10), SQLite + Entity Framework Core 10, ASP.NET Core Session, xUnit (CartManager birim testleri)

---

## Dosya Haritası

| Durum | Dosya | Sorumluluk |
|---|---|---|
| YENİ | `Models/CartItem.cs` | Session'da saklanan sepet kalemi |
| YENİ | `Models/Order.cs` | DB'de saklanan sipariş başlığı |
| YENİ | `Models/OrderItem.cs` | Siparişe ait kalem (fiyat/isim kopyalanır) |
| YENİ | `ViewModels/CheckoutViewModel.cs` | Checkout form model |
| YENİ | `Services/ICartService.cs` | Sepet servis arayüzü |
| YENİ | `Services/CartManager.cs` | Session tabanlı sepet implementasyonu |
| YENİ | `Services/IOrderService.cs` | Sipariş servis arayüzü |
| YENİ | `Services/OrderManager.cs` | Sipariş kaydetme + sepet temizleme |
| YENİ | `Repositories/IOrderRepository.cs` | Sipariş veri erişim arayüzü |
| YENİ | `Repositories/OrderRepository.cs` | EF Core sipariş repository |
| YENİ | `Controllers/CartController.cs` | Sepet: ekle / listele / kaldır |
| YENİ | `Controllers/OrderController.cs` | Checkout GET/POST, Confirmation |
| YENİ | `Views/Cart/Index.cshtml` | Tek sütun sepet sayfası |
| YENİ | `Views/Order/Checkout.cshtml` | Teslimat bilgileri formu |
| YENİ | `Views/Order/Confirmation.cshtml` | Teşekkür + sipariş özeti |
| YENİ | `StoreWeb.Tests/StoreWeb.Tests.csproj` | xUnit test projesi |
| YENİ | `StoreWeb.Tests/Services/CartManagerTests.cs` | CartManager birim testleri |
| DEĞİŞİR | `Program.cs` | AddSession + UseSession ekle |
| DEĞİŞİR | `Infrastructure/Extensions/ServiceExtensions.cs` | IoC kayıtları |
| DEĞİŞİR | `Repositories/RepositoryContext.cs` | DbSet<Order>, DbSet<OrderItem> ekle |
| DEĞİŞİR | `Views/Product/Details.cshtml` | "Sepete Ekle" POST form |
| DEĞİŞİR | `Views/Shared/_Layout.cshtml` | Nav sepet badge |
| DEĞİŞİR | `Views/_ViewImports.cshtml` | `@using StoreWeb.ViewModels` ekle |

---

## Task 1: CartItem modeli + Session kurulumu

**Files:**
- Create: `StoreWeb/Models/CartItem.cs`
- Modify: `StoreWeb/Program.cs`

- [ ] **Step 1: CartItem modelini oluştur**

`StoreWeb/Models/CartItem.cs`:
```csharp
namespace StoreWeb.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Total => Price * Quantity;
}
```

- [ ] **Step 2: Session servislerini ve middleware'i ekle**

`Program.cs`'in `builder.Services` bloğuna şunu ekle (satır 5, `AddControllersWithViews`'dan sonra):
```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

Middleware pipeline'da `app.UseRouting();`'dan sonra, `app.UseAuthentication();`'dan önce ekle:
```csharp
app.UseSession();
```

- [ ] **Step 3: Derleme kontrolü**

```bash
cd C:/btk-akademi
dotnet build StoreWeb/StoreWeb.csproj
```

Beklenen: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add StoreWeb/Models/CartItem.cs StoreWeb/Program.cs
git commit -m "feat: add CartItem model and session configuration"
```

---

## Task 2: ICartService arayüzü + Test projesi + CartManager (TDD)

**Files:**
- Create: `StoreWeb/Services/ICartService.cs`
- Create: `StoreWeb.Tests/StoreWeb.Tests.csproj`
- Create: `StoreWeb.Tests/Services/CartManagerTests.cs`
- Create: `StoreWeb/Services/CartManager.cs`

- [ ] **Step 1: ICartService arayüzünü oluştur**

`StoreWeb/Services/ICartService.cs`:
```csharp
using Microsoft.AspNetCore.Http;
using StoreWeb.Models;

namespace StoreWeb.Services;

public interface ICartService
{
    void AddItem(ISession session, Product product);
    void RemoveItem(ISession session, int productId);
    List<CartItem> GetCart(ISession session);
    void ClearCart(ISession session);
    decimal GetTotal(ISession session);
}
```

- [ ] **Step 2: Test projesini oluştur**

```bash
cd C:/btk-akademi
dotnet new xunit -n StoreWeb.Tests -o StoreWeb.Tests --framework net10.0
dotnet add StoreWeb.Tests/StoreWeb.Tests.csproj reference StoreWeb/StoreWeb.csproj
```

`Store.slnx` dosyasını güncelle — `<Project Path="StoreWeb.Tests/StoreWeb.Tests.csproj" />` satırını ekle:
```xml
<Solution>
  <Project Path="ConsoleExtensionApp/ConsoleExtensionApp.csproj" />
  <Project Path="StoreWeb/StoreWeb.csproj" />
  <Project Path="StoreWeb.Tests/StoreWeb.Tests.csproj" />
</Solution>
```

- [ ] **Step 3: MockSession yardımcı sınıfını oluştur**

`StoreWeb.Tests/MockSession.cs`:
```csharp
using Microsoft.AspNetCore.Http;

namespace StoreWeb.Tests;

public class MockSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();
    public bool IsAvailable => true;
    public string Id => "test-session";
    public IEnumerable<string> Keys => _store.Keys;
    public void Clear() => _store.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _store.Remove(key);
    public void Set(string key, byte[] value) => _store[key] = value;
    public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
}
```

- [ ] **Step 4: Başarısız testleri yaz**

`StoreWeb.Tests/Services/CartManagerTests.cs`:
```csharp
using StoreWeb.Models;
using StoreWeb.Services;

namespace StoreWeb.Tests.Services;

public class CartManagerTests
{
    private readonly CartManager _sut = new();
    private readonly MockSession _session = new();

    [Fact]
    public void AddItem_NewProduct_AddsToCart()
    {
        var product = new Product { ProductId = 1, ProductName = "Laptop", Price = 100m };

        _sut.AddItem(_session, product);

        var cart = _sut.GetCart(_session);
        Assert.Single(cart);
        Assert.Equal(1, cart[0].Quantity);
        Assert.Equal("Laptop", cart[0].ProductName);
    }

    [Fact]
    public void AddItem_ExistingProduct_IncrementsQuantity()
    {
        var product = new Product { ProductId = 1, ProductName = "Laptop", Price = 100m };

        _sut.AddItem(_session, product);
        _sut.AddItem(_session, product);

        var cart = _sut.GetCart(_session);
        Assert.Single(cart);
        Assert.Equal(2, cart[0].Quantity);
    }

    [Fact]
    public void RemoveItem_ExistingProduct_RemovesFromCart()
    {
        var product = new Product { ProductId = 1, ProductName = "Laptop", Price = 100m };
        _sut.AddItem(_session, product);

        _sut.RemoveItem(_session, 1);

        var cart = _sut.GetCart(_session);
        Assert.Empty(cart);
    }

    [Fact]
    public void RemoveItem_NonExistingProduct_DoesNotThrow()
    {
        _sut.RemoveItem(_session, 999);

        var cart = _sut.GetCart(_session);
        Assert.Empty(cart);
    }

    [Fact]
    public void GetTotal_MultipleItems_ReturnsCorrectSum()
    {
        var p1 = new Product { ProductId = 1, ProductName = "A", Price = 100m };
        var p2 = new Product { ProductId = 2, ProductName = "B", Price = 200m };
        _sut.AddItem(_session, p1); // qty 1 → 100
        _sut.AddItem(_session, p1); // qty 2 → 200
        _sut.AddItem(_session, p2); // qty 1 → 200

        var total = _sut.GetTotal(_session);

        Assert.Equal(400m, total);
    }

    [Fact]
    public void ClearCart_AfterAddingItems_EmptiesCart()
    {
        var product = new Product { ProductId = 1, ProductName = "Laptop", Price = 100m };
        _sut.AddItem(_session, product);

        _sut.ClearCart(_session);

        var cart = _sut.GetCart(_session);
        Assert.Empty(cart);
    }

    [Fact]
    public void GetCart_EmptySession_ReturnsEmptyList()
    {
        var cart = _sut.GetCart(_session);

        Assert.Empty(cart);
    }
}
```

- [ ] **Step 5: Testlerin başarısız olduğunu doğrula**

```bash
cd C:/btk-akademi
dotnet test StoreWeb.Tests/StoreWeb.Tests.csproj
```

Beklenen: derleme hatası — `CartManager` henüz yok.

- [ ] **Step 6: CartManager implementasyonunu yaz**

`StoreWeb/Services/CartManager.cs`:
```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using StoreWeb.Models;

namespace StoreWeb.Services;

public class CartManager : ICartService
{
    private const string CartKey = "cart";

    public void AddItem(ISession session, Product product)
    {
        var cart = GetCart(session);
        var existing = cart.FirstOrDefault(x => x.ProductId == product.ProductId);
        if (existing is not null)
        {
            existing.Quantity++;
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                Quantity = 1
            });
        }
        SaveCart(session, cart);
    }

    public void RemoveItem(ISession session, int productId)
    {
        var cart = GetCart(session);
        var item = cart.FirstOrDefault(x => x.ProductId == productId);
        if (item is not null)
            cart.Remove(item);
        SaveCart(session, cart);
    }

    public List<CartItem> GetCart(ISession session)
    {
        var json = session.GetString(CartKey);
        return json is null
            ? new List<CartItem>()
            : JsonSerializer.Deserialize<List<CartItem>>(json)!;
    }

    public void ClearCart(ISession session)
    {
        session.Remove(CartKey);
    }

    public decimal GetTotal(ISession session)
    {
        return GetCart(session).Sum(x => x.Price * x.Quantity);
    }

    private static void SaveCart(ISession session, List<CartItem> cart)
    {
        session.SetString(CartKey, JsonSerializer.Serialize(cart));
    }
}
```

- [ ] **Step 7: Testlerin geçtiğini doğrula**

```bash
cd C:/btk-akademi
dotnet test StoreWeb.Tests/StoreWeb.Tests.csproj --verbosity normal
```

Beklenen: `Passed! - Failed: 0, Passed: 7`

- [ ] **Step 8: Commit**

```bash
git add StoreWeb/Services/ICartService.cs StoreWeb/Services/CartManager.cs
git add StoreWeb.Tests/ Store.slnx
git commit -m "feat: add CartManager with session-based cart logic (TDD)"
```

---

## Task 3: CartController + IoC kaydı

**Files:**
- Create: `StoreWeb/Controllers/CartController.cs`
- Modify: `StoreWeb/Infrastructure/Extensions/ServiceExtensions.cs`

- [ ] **Step 1: IoC kaydını ekle**

`ServiceExtensions.cs`'in `ConfigureIoC` metoduna şunları ekle:
```csharp
services.AddScoped<ICartService, CartManager>();
```

Güncel `ConfigureIoC`:
```csharp
public static void ConfigureIoC(this IServiceCollection services)
{
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<IProductService, ProductManager>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<ICartService, CartManager>();   // YENİ
}
```

- [ ] **Step 2: CartController'ı oluştur**

`StoreWeb/Controllers/CartController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using StoreWeb.Services;

namespace StoreWeb.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IProductService _productService;

    public CartController(ICartService cartService, IProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
    }

    public IActionResult Index()
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        ViewData["Total"] = _cartService.GetTotal(HttpContext.Session);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId)
    {
        var product = await _productService.GetByIdAsync(productId);
        if (product is null)
            return NotFound();

        _cartService.AddItem(HttpContext.Session, product);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveItem(int productId)
    {
        _cartService.RemoveItem(HttpContext.Session, productId);
        return RedirectToAction(nameof(Index));
    }
}
```

- [ ] **Step 3: Derleme kontrolü**

```bash
cd C:/btk-akademi
dotnet build StoreWeb/StoreWeb.csproj
```

Beklenen: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add StoreWeb/Controllers/CartController.cs StoreWeb/Infrastructure/Extensions/ServiceExtensions.cs
git commit -m "feat: add CartController and register ICartService in IoC"
```

---

## Task 4: Cart/Index view + Product/Details "Sepete Ekle" formu

**Files:**
- Create: `StoreWeb/Views/Cart/Index.cshtml`
- Modify: `StoreWeb/Views/Product/Details.cshtml`

- [ ] **Step 1: Cart/Index view'ını oluştur**

Önce klasörü oluştur:
```bash
mkdir StoreWeb/Views/Cart
```

`StoreWeb/Views/Cart/Index.cshtml`:
```html
@model List<CartItem>

@{
    ViewData["Title"] = "Sepetim";
    var total = (decimal)(ViewData["Total"] ?? 0m);
}

<section class="product-hero reveal" data-reveal>
    <p class="product-eyebrow">Alışveriş</p>
    <h1 class="product-hero-title">Sepetim</h1>
</section>

@if (!Model.Any())
{
    <section class="card card-body reveal" data-reveal>
        <h2 class="card-title">Sepetiniz boş</h2>
        <p class="muted">Ürünlere göz atarak sepetinize ekleyebilirsiniz.</p>
        <a asp-controller="Product" asp-action="Index" class="btn btn-primary" style="margin-top:1rem;">
            Ürünlere Göz At
        </a>
    </section>
}
else
{
    <section class="reveal" data-reveal>
        @foreach (var item in Model)
        {
            <article class="card card-body" style="margin-bottom:1rem; display:flex; align-items:center; gap:1rem; flex-wrap:wrap;">
                <div style="flex:1; min-width:160px;">
                    <h2 class="card-title" style="margin-bottom:.25rem;">@item.ProductName</h2>
                    <p class="muted">@item.Price.ToString("C") × @item.Quantity</p>
                </div>
                <strong>@item.Total.ToString("C")</strong>
                <form asp-controller="Cart" asp-action="RemoveItem" method="post">
                    @Html.AntiForgeryToken()
                    <input type="hidden" name="productId" value="@item.ProductId" />
                    <button type="submit" class="btn btn-secondary">Kaldır</button>
                </form>
            </article>
        }

        <div class="card card-body" style="display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:1rem;">
            <strong style="font-size:1.1rem;">Toplam: @total.ToString("C")</strong>
            <div style="display:flex; gap:.75rem;">
                <a asp-controller="Product" asp-action="Index" class="btn btn-secondary">Alışverişe Devam</a>
                <a asp-controller="Order" asp-action="Checkout" class="btn btn-primary">Siparişi Tamamla</a>
            </div>
        </div>
    </section>
}
```

- [ ] **Step 2: Product/Details "Sepete Ekle" butonunu form'a dönüştür**

`Views/Product/Details.cshtml`'de şu satırı:
```html
<a href="#" class="btn btn-primary">Sepete Ekle</a>
```

Şununla değiştir:
```html
<form asp-controller="Cart" asp-action="AddToCart" method="post">
    @Html.AntiForgeryToken()
    <input type="hidden" name="productId" value="@Model.ProductId" />
    <button type="submit" class="btn btn-primary">Sepete Ekle</button>
</form>
```

- [ ] **Step 3: Manuel test**

```bash
cd C:/btk-akademi
dotnet run --project StoreWeb/StoreWeb.csproj
```

Tarayıcıda:
1. `/Product` → bir ürün detayına git → "Sepete Ekle" tıkla
2. `/Cart` sayfasına yönlendirilmeli, ürün listede görünmeli
3. "Kaldır" butonuna tıkla → ürün listeden silinmeli
4. Sepet boşken `/Cart` → "Sepetiniz boş" mesajı görünmeli

- [ ] **Step 4: Commit**

```bash
git add StoreWeb/Views/Cart/ StoreWeb/Views/Product/Details.cshtml
git commit -m "feat: add Cart/Index view and wire up AddToCart button on product details"
```

---

## Task 5: Order ve OrderItem modelleri

**Files:**
- Create: `StoreWeb/Models/Order.cs`
- Create: `StoreWeb/Models/OrderItem.cs`

- [ ] **Step 1: Order modelini oluştur**

`StoreWeb/Models/Order.cs`:
```csharp
namespace StoreWeb.Models;

public class Order
{
    public int OrderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}
```

- [ ] **Step 2: OrderItem modelini oluştur**

`StoreWeb/Models/OrderItem.cs`:
```csharp
namespace StoreWeb.Models;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
```

- [ ] **Step 3: Commit**

```bash
git add StoreWeb/Models/Order.cs StoreWeb/Models/OrderItem.cs
git commit -m "feat: add Order and OrderItem models"
```

---

## Task 6: RepositoryContext güncellemesi + DB migration

**Files:**
- Modify: `StoreWeb/Repositories/RepositoryContext.cs`
- Create: `StoreWeb/Migrations/<timestamp>_AddOrderTables.cs` (EF Core otomatik oluşturur)

- [ ] **Step 1: RepositoryContext'e DbSet'leri ekle**

`StoreWeb/Repositories/RepositoryContext.cs`'de `DbSet<Product>` satırından sonra ekle:
```csharp
public DbSet<Product> Products { get; set; }
public DbSet<Order> Orders { get; set; }      // YENİ
public DbSet<OrderItem> OrderItems { get; set; }  // YENİ
```

- [ ] **Step 2: Migration oluştur**

```bash
cd C:/btk-akademi
dotnet ef migrations add AddOrderTables --project StoreWeb/StoreWeb.csproj --startup-project StoreWeb/StoreWeb.csproj
```

Beklenen: `Done. To undo this action, use 'ef migrations remove'`  
`Migrations/` klasöründe `<timestamp>_AddOrderTables.cs` dosyası oluşmalı.

- [ ] **Step 3: Veritabanını güncelle**

```bash
dotnet ef database update --project StoreWeb/StoreWeb.csproj --startup-project StoreWeb/StoreWeb.csproj
```

Beklenen: `Done.`

- [ ] **Step 4: Derleme kontrolü**

```bash
dotnet build StoreWeb/StoreWeb.csproj
```

Beklenen: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add StoreWeb/Repositories/RepositoryContext.cs StoreWeb/Migrations/
git commit -m "feat: add Orders and OrderItems tables via EF Core migration"
```

---

## Task 7: IOrderRepository + OrderRepository + IoC kaydı

**Files:**
- Create: `StoreWeb/Repositories/IOrderRepository.cs`
- Create: `StoreWeb/Repositories/OrderRepository.cs`
- Modify: `StoreWeb/Infrastructure/Extensions/ServiceExtensions.cs`

- [ ] **Step 1: IOrderRepository arayüzünü oluştur**

`StoreWeb/Repositories/IOrderRepository.cs`:
```csharp
using StoreWeb.Models;

namespace StoreWeb.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(int orderId);
    Task<int> SaveChangesAsync();
}
```

- [ ] **Step 2: OrderRepository implementasyonunu oluştur**

`StoreWeb/Repositories/OrderRepository.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using StoreWeb.Models;

namespace StoreWeb.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly RepositoryContext _context;

    public OrderRepository(RepositoryContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task<Order?> GetByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
```

- [ ] **Step 3: IoC kaydını ekle**

`ServiceExtensions.cs`'in `ConfigureIoC` metoduna ekle:
```csharp
public static void ConfigureIoC(this IServiceCollection services)
{
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<IProductService, ProductManager>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<ICartService, CartManager>();
    services.AddScoped<IOrderRepository, OrderRepository>();   // YENİ
}
```

- [ ] **Step 4: Derleme kontrolü**

```bash
cd C:/btk-akademi
dotnet build StoreWeb/StoreWeb.csproj
```

Beklenen: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add StoreWeb/Repositories/IOrderRepository.cs StoreWeb/Repositories/OrderRepository.cs
git add StoreWeb/Infrastructure/Extensions/ServiceExtensions.cs
git commit -m "feat: add OrderRepository and register in IoC"
```

---

## Task 8: CheckoutViewModel + IOrderService + OrderManager

**Files:**
- Create: `StoreWeb/ViewModels/CheckoutViewModel.cs`
- Create: `StoreWeb/Services/IOrderService.cs`
- Create: `StoreWeb/Services/OrderManager.cs`
- Modify: `StoreWeb/Infrastructure/Extensions/ServiceExtensions.cs`

- [ ] **Step 1: CheckoutViewModel'i oluştur**

`StoreWeb/ViewModels/CheckoutViewModel.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace StoreWeb.ViewModels;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Teslimat adresi zorunludur.")]
    [Display(Name = "Teslimat Adresi")]
    public string Address { get; set; } = string.Empty;
}
```

- [ ] **Step 2: IOrderService arayüzünü oluştur**

`StoreWeb/Services/IOrderService.cs`:
```csharp
using Microsoft.AspNetCore.Http;
using StoreWeb.Models;
using StoreWeb.ViewModels;

namespace StoreWeb.Services;

public interface IOrderService
{
    Task<Order> PlaceOrderAsync(string userId, CheckoutViewModel vm, ISession session);
    Task<Order?> GetOrderAsync(int orderId, string userId);
}
```

- [ ] **Step 3: OrderManager implementasyonunu oluştur**

`StoreWeb/Services/OrderManager.cs`:
```csharp
using Microsoft.AspNetCore.Http;
using StoreWeb.Models;
using StoreWeb.Repositories;
using StoreWeb.ViewModels;

namespace StoreWeb.Services;

public class OrderManager : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartService _cartService;

    public OrderManager(IOrderRepository orderRepository, ICartService cartService)
    {
        _orderRepository = orderRepository;
        _cartService = cartService;
    }

    public async Task<Order> PlaceOrderAsync(string userId, CheckoutViewModel vm, ISession session)
    {
        var cartItems = _cartService.GetCart(session);

        var order = new Order
        {
            UserId = userId,
            FullName = vm.FullName,
            Address = vm.Address,
            CreatedAt = DateTime.UtcNow,
            OrderItems = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = item.Price,
                Quantity = item.Quantity
            }).ToList()
        };

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();
        _cartService.ClearCart(session);

        return order;
    }

    public async Task<Order?> GetOrderAsync(int orderId, string userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null || order.UserId != userId)
            return null;
        return order;
    }
}
```

- [ ] **Step 4: OrderManager'ı IoC'e kaydet**

`ServiceExtensions.cs`'in `ConfigureIoC` metodunu güncelle:
```csharp
public static void ConfigureIoC(this IServiceCollection services)
{
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<IProductService, ProductManager>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<ICartService, CartManager>();
    services.AddScoped<IOrderRepository, OrderRepository>();
    services.AddScoped<IOrderService, OrderManager>();   // YENİ
}
```

- [ ] **Step 5: Derleme kontrolü**

```bash
cd C:/btk-akademi
dotnet build StoreWeb/StoreWeb.csproj
```

Beklenen: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add StoreWeb/ViewModels/CheckoutViewModel.cs
git add StoreWeb/Services/IOrderService.cs StoreWeb/Services/OrderManager.cs
git add StoreWeb/Infrastructure/Extensions/ServiceExtensions.cs
git commit -m "feat: add CheckoutViewModel, OrderManager and register IOrderService in IoC"
```

---

## Task 9: OrderController

**Files:**
- Create: `StoreWeb/Controllers/OrderController.cs`

- [ ] **Step 1: OrderController'ı oluştur**

`StoreWeb/Controllers/OrderController.cs`:
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StoreWeb.Models.Identity;
using StoreWeb.Services;
using StoreWeb.ViewModels;

namespace StoreWeb.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderController(
        IOrderService orderService,
        ICartService cartService,
        UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _cartService = cartService;
        _userManager = userManager;
    }

    public IActionResult Checkout()
    {
        var cart = _cartService.GetCart(HttpContext.Session);
        if (!cart.Any())
            return RedirectToAction("Index", "Cart");

        return View(new CheckoutViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel vm)
    {
        if (!ModelState.IsValid)
            return View("Checkout", vm);

        var cart = _cartService.GetCart(HttpContext.Session);
        if (!cart.Any())
            return RedirectToAction("Index", "Cart");

        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.PlaceOrderAsync(userId, vm, HttpContext.Session);

        return RedirectToAction(nameof(Confirmation), new { id = order.OrderId });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var order = await _orderService.GetOrderAsync(id, userId);

        if (order is null)
            return NotFound();

        return View(order);
    }
}
```

- [ ] **Step 2: Derleme kontrolü**

```bash
cd C:/btk-akademi
dotnet build StoreWeb/StoreWeb.csproj
```

Beklenen: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add StoreWeb/Controllers/OrderController.cs
git commit -m "feat: add OrderController with Checkout, PlaceOrder and Confirmation actions"
```

---

## Task 10: Checkout + Confirmation view'ları + _ViewImports güncellemesi

**Files:**
- Create: `StoreWeb/Views/Order/Checkout.cshtml`
- Create: `StoreWeb/Views/Order/Confirmation.cshtml`
- Modify: `StoreWeb/Views/_ViewImports.cshtml`

- [ ] **Step 1: _ViewImports'a ViewModels namespace'ini ekle**

`StoreWeb/Views/_ViewImports.cshtml`'e şu satırı ekle (mevcut `@using StoreWeb.ViewModels.Auth` satırından önce):
```razor
@using StoreWeb.ViewModels
```

Güncel `_ViewImports.cshtml`:
```razor
@using StoreWeb
@using StoreWeb.Models
@using StoreWeb.Models.Identity
@using StoreWeb.ViewModels
@using StoreWeb.ViewModels.Auth
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

- [ ] **Step 2: Order klasörünü oluştur**

```bash
mkdir StoreWeb/Views/Order
```

- [ ] **Step 3: Checkout view'ını oluştur**

`StoreWeb/Views/Order/Checkout.cshtml`:
```html
@model CheckoutViewModel

@{
    ViewData["Title"] = "Siparişi Tamamla";
}

<section class="product-hero reveal" data-reveal>
    <p class="product-eyebrow">Son Adım</p>
    <h1 class="product-hero-title">Teslimat Bilgileri</h1>
</section>

<section class="card card-body reveal" data-reveal style="max-width:560px;">
    <form asp-controller="Order" asp-action="PlaceOrder" method="post">
        @Html.AntiForgeryToken()
        <div asp-validation-summary="ModelOnly" class="muted" style="margin-bottom:1rem;color:#dc2626;"></div>

        <div style="margin-bottom:1.25rem;">
            <label asp-for="FullName" style="display:block;font-weight:600;margin-bottom:.4rem;"></label>
            <input asp-for="FullName" class="mock-input" style="width:100%;" placeholder="Örn: Ahmet Yılmaz" />
            <span asp-validation-for="FullName" style="color:#dc2626;font-size:.875rem;"></span>
        </div>

        <div style="margin-bottom:1.75rem;">
            <label asp-for="Address" style="display:block;font-weight:600;margin-bottom:.4rem;"></label>
            <textarea asp-for="Address" class="mock-input" rows="3"
                      style="width:100%;resize:vertical;"
                      placeholder="Mahalle, sokak, bina no, daire..."></textarea>
            <span asp-validation-for="Address" style="color:#dc2626;font-size:.875rem;"></span>
        </div>

        <div style="display:flex;gap:.75rem;flex-wrap:wrap;">
            <button type="submit" class="btn btn-primary">Siparişi Onayla</button>
            <a asp-controller="Cart" asp-action="Index" class="btn btn-secondary">Sepete Dön</a>
        </div>
    </form>
</section>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

- [ ] **Step 4: Confirmation view'ını oluştur**

`StoreWeb/Views/Order/Confirmation.cshtml`:
```html
@model Order

@{
    ViewData["Title"] = "Sipariş Onayı";
    var total = Model.OrderItems.Sum(item => item.Price * item.Quantity);
}

<section class="product-hero reveal" data-reveal>
    <p class="product-eyebrow">Teşekkürler!</p>
    <h1 class="product-hero-title">Siparişiniz Alındı</h1>
    <p class="product-hero-copy">Siparişiniz başarıyla oluşturuldu. En kısa sürede kargoya verilecektir.</p>
</section>

<section class="card card-body reveal" data-reveal>
    <div style="margin-bottom:1rem;">
        <p><strong>Sipariş No:</strong> #@Model.OrderId</p>
        <p><strong>Tarih:</strong> @Model.CreatedAt.ToString("dd.MM.yyyy HH:mm")</p>
        <p><strong>Teslimat Adresi:</strong> @Model.Address</p>
    </div>

    <hr style="border-color:#e2e8f0;margin:1rem 0;" />

    <h2 class="card-title" style="margin-bottom:1rem;">Sipariş Özeti</h2>

    @foreach (var item in Model.OrderItems)
    {
        <div style="display:flex;justify-content:space-between;padding:.5rem 0;border-bottom:1px solid #f1f5f9;">
            <span>@item.ProductName <span class="muted">× @item.Quantity</span></span>
            <strong>@((item.Price * item.Quantity).ToString("C"))</strong>
        </div>
    }

    <div style="display:flex;justify-content:space-between;padding:1rem 0;font-size:1.1rem;font-weight:700;">
        <span>Toplam</span>
        <span>@total.ToString("C")</span>
    </div>

    <a asp-controller="Product" asp-action="Index" class="btn btn-primary" style="margin-top:.5rem;">
        Alışverişe Devam Et
    </a>
</section>
```

- [ ] **Step 5: Derleme kontrolü**

```bash
cd C:/btk-akademi
dotnet build StoreWeb/StoreWeb.csproj
```

Beklenen: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add StoreWeb/Views/Order/ StoreWeb/Views/_ViewImports.cshtml
git commit -m "feat: add Checkout and Confirmation views, update _ViewImports"
```

---

## Task 11: Nav badge (_Layout.cshtml)

**Files:**
- Modify: `StoreWeb/Views/Shared/_Layout.cshtml`

- [ ] **Step 1: @inject ve isCart değişkenini ekle**

`_Layout.cshtml`'in en üstüne (mevcut `@{` bloğundan önce) ekle:
```razor
@using StoreWeb.Services
@inject ICartService CartService
```

Mevcut `@{` bloğuna şu iki satırı ekle (diğer `var is...` değişkenlerinin yanına):
```razor
var cartCount = CartService.GetCart(Context.Session).Sum(x => x.Quantity);
var isCart = currentController == "Cart";
```

- [ ] **Step 2: Desktop nav'a sepet linkini ekle**

`_Layout.cshtml`'deki desktop nav (`<nav class="hidden items-center gap-2 md:flex"...>`) içinde, `@if (!isAuthenticated)` bloğundan hemen önce şunu ekle:
```razor
<a asp-controller="Cart" asp-action="Index"
   class="nav-link @(isCart ? "is-active" : string.Empty)">
    Sepet@(cartCount > 0 ? $" ({cartCount})" : "")
</a>
```

- [ ] **Step 3: Mobile nav'a sepet linkini ekle**

`_Layout.cshtml`'deki mobil nav (`<nav id="mobile-menu"...>`) içinde de, `@if (!isAuthenticated)` bloğundan hemen önce şunu ekle:
```razor
<a asp-controller="Cart" asp-action="Index"
   class="mobile-link @(isCart ? "is-active" : string.Empty)">
    Sepet@(cartCount > 0 ? $" ({cartCount})" : "")
</a>
```

- [ ] **Step 4: Derleme kontrolü**

```bash
cd C:/btk-akademi
dotnet build StoreWeb/StoreWeb.csproj
```

Beklenen: `Build succeeded.`

- [ ] **Step 5: Uçtan uca manuel test**

```bash
dotnet run --project StoreWeb/StoreWeb.csproj
```

Test senaryoları:

| Adım | Beklenen |
|---|---|
| `/Product` → ürün detayı → "Sepete Ekle" | `/Cart`'a yönlendirilir, nav'da `Sepet (1)` görünür |
| Aynı ürüne tekrar "Sepete Ekle" | `Sepet (2)` olur |
| `/Cart` → "Kaldır" | Ürün listeden silinir |
| `/Cart` → "Siparişi Tamamla" (giriş yapılmamış) | Login sayfasına yönlendirilir |
| Login sonrası checkout | Form gösterilir |
| Boş form submit | Validasyon hataları görünür |
| Dolu form submit | `/Order/Confirmation/1` sayfası açılır, sepet temizlenir, nav `Sepet` olur |
| Confirmation sayfasında başka kullanıcının order ID'si | `404 Not Found` döner |

- [ ] **Step 6: Final commit**

```bash
git add StoreWeb/Views/Shared/_Layout.cshtml
git commit -m "feat: add cart badge to navigation"
```

---

## Özet

Tüm görevler tamamlandığında sistem şu şekilde çalışır:

1. Ziyaretçi herhangi bir ürün detayından "Sepete Ekle" butonuyla session'a ürün ekler.
2. Nav'daki "Sepet (N)" linki her sayfada güncel adedi gösterir.
3. "Siparişi Tamamla" tıklandığında giriş yapılmamışsa `/Auth/Login`'e yönlendirilir.
4. Giriş sonrası ad-soyad + adres formu doldurulur, sipariş DB'ye yazılır, session temizlenir.
5. Confirmation sayfasında sipariş no, tarih, adres ve kalem özeti gösterilir.
