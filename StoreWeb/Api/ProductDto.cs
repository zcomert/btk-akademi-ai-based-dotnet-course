namespace StoreWeb.Api;

public record ProductDto(
    int ProductId,
    string ProductName,
    decimal Price,
    string? ImageURL
);
