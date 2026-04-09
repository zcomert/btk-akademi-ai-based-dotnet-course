# Sepet & Sipariş Sistemi — Tasarım Dokümanı

**Tarih:** 2026-04-09  
**Proje:** StoreWeb (ASP.NET Core MVC)  
**Konu:** Ürünleri sepete ekleme, sepet yönetimi ve siparişi tamamlama

---

## Gereksinimler

- Anonim kullanıcılar ürünleri sepete ekleyebilir.
- Siparişi tamamlamak için kullanıcı giriş yapmış olmalıdır.
- Sepet, Session'da saklanır (kalıcı değil, tarayıcı kapanınca sıfırlanır).
- Sipariş formu minimal bilgi toplar: **Ad-Soyad** ve **Adres**.
- Sipariş tamamlandıktan sonra teşekkür + sipariş özeti sayfası gösterilir.
- Sepet sayfası tek sütun düzeninde, mobil dostu bir tasarıma sahiptir.

---

## Mimari Yaklaşım

Mevcut `IProductService` → `ProductManager` → `ProductRepository` zincirini izleyen **servis katmanlı** mimari.  
Cart için servis katmanı yeterli (repository yok — session tabanlı).  
Order için yeni DB tablosu, repository ve servis eklenir.

---

## Veri Modelleri

### CartItem (`Models/CartItem.cs`)
```csharp
public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
```

### Order (`Models/Order.cs`)
```csharp
public class Order
{
    public int OrderId { get; set; }
    public string UserId { get; set; }
    public string FullName { get; set; }
    public string Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}
```

### OrderItem (`Models/OrderItem.cs`)
```csharp
public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }  // Kopyalanır — ürün silinse kayıt bozulmaz
    public decimal Price { get; set; }        // Kopyalanır — fiyat değişse geçmiş etkilenmez
    public int Quantity { get; set; }
}
```

### CheckoutViewModel (`ViewModels/CheckoutViewModel.cs`)
```csharp
public class CheckoutViewModel
{
    [Required] public string FullName { get; set; }
    [Required] public string Address { get; set; }
}
```

---

## Yeni Dosyalar

```
Models/
  CartItem.cs
  Order.cs
  OrderItem.cs

ViewModels/
  CheckoutViewModel.cs

Repositories/
  IOrderRepository.cs
  OrderRepository.cs        — AddAsync, GetByIdAsync, GetByUserIdAsync

Services/
  ICartService.cs
  CartManager.cs            — Session tabanlı implementasyon
  IOrderService.cs
  OrderManager.cs           — Login kontrolü, sipariş kaydı, sepet temizleme

Controllers/
  CartController.cs         — AddToCart, ViewCart, RemoveItem
  OrderController.cs        — Checkout (GET), PlaceOrder (POST), Confirmation (GET)

Views/
  Cart/Index.cshtml         — Tek sütun sepet sayfası
  Order/Checkout.cshtml     — Ad-soyad + adres formu + sipariş özeti
  Order/Confirmation.cshtml — Teşekkür + sipariş özeti
```

---

## Servis Arayüzleri

### ICartService
```csharp
void AddItem(ISession session, Product product);   // Varsa miktarı artır, yoksa ekle
void RemoveItem(ISession session, int productId);  // Listeden çıkar
List<CartItem> GetCart(ISession session);          // Mevcut sepeti döner
void ClearCart(ISession session);                  // Siparişten sonra çağrılır
decimal GetTotal(ISession session);                // Toplam tutarı hesaplar
```

### IOrderService
```csharp
Task<Order> PlaceOrderAsync(string userId, CheckoutViewModel vm, ISession session);
Task<Order?> GetOrderAsync(int orderId, string userId);
```

### IOrderRepository
```csharp
Task AddAsync(Order order);
Task<Order?> GetByIdAsync(int orderId);
Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId);
Task<int> SaveChangesAsync();
```

---

## Akış

### Sepete Ekleme
1. Kullanıcı ürün detay sayfasında "Sepete Ekle" butonuna tıklar.
2. `POST /Cart/AddToCart` → `CartManager.AddItem()` → Session güncellenir.
3. Sepet sayfasına yönlendirilir (`/Cart/Index`).

### Checkout
1. Kullanıcı sepet sayfasında "Siparişi Tamamla" butonuna tıklar.
2. `GET /Order/Checkout` — `[Authorize]` attribute devreye girer; giriş yapılmamışsa `/Auth/Login`'e yönlendirilir.
3. Login sonrası form gösterilir (FullName + Address).
4. `POST /Order/PlaceOrder` → `OrderManager.PlaceOrderAsync()`:
   - Session'dan sepet alınır.
   - `Order` + `OrderItem`'lar oluşturulur ve DB'ye kaydedilir.
   - Session temizlenir.
5. `/Order/Confirmation/{orderId}` sayfasına yönlendirilir.

### Confirmation
- Sipariş ID, tarih, teslimat adresi, ürün listesi ve genel toplam gösterilir.
- "Alışverişe devam et" linki ile ürün listesine dönülür.

---

## Hata Yönetimi

| Durum | Davranış |
|---|---|
| Sepet boşken Checkout'a gidilirse | `/Cart/Index`'e yönlendir |
| `PlaceOrderAsync` başarısız olursa | `ModelState` hatası, form tekrar gösterilir |
| Geçersiz `orderId` ile Confirmation | `NotFound()` döner |
| Başkasının siparişi görüntülenmeye çalışılırsa | `NotFound()` döner (userId kontrolü) |

---

## Session Kurulumu (`Program.cs`)

```csharp
builder.Services.AddSession();
// ...
app.UseSession();  // UseAuthentication'dan önce eklenmelidir
```

## IoC Kayıtları (`ServiceExtensions.ConfigureIoC`)

```csharp
services.AddScoped<ICartService, CartManager>();
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IOrderService, OrderManager>();
```

---

## Nav Badge

`_Layout.cshtml`'e sepetteki ürün sayısını gösteren bir badge eklenir. Session'dan okunur, partial view veya ViewComponent ile render edilir.

---

## DB Migrasyonu

`Order` ve `OrderItem` tabloları için `RepositoryContext`'e `DbSet` eklenir, ardından EF Core migration çalıştırılır.
