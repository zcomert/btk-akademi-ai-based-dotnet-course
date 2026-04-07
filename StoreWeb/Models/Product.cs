namespace StoreWeb.Repositories;

public class Product
{
    public int ProductId { get; set; }
    public String ProductName { get; set; }
    public Decimal Price { get; set; }
    public String? ImageURL { get; set; }
}