namespace ProductCatalog.Domain.Entities;

public class Product : IComparable<Product>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int CompareTo(Product? other)
    {
        if (other == null)
            return 1;

        int nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
        if (nameComparison != 0)
            return nameComparison;

        return Price.CompareTo(other.Price);
    }
}
