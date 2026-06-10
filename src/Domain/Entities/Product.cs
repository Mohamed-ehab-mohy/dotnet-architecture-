namespace Domain.Entities;

public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }

    private Product() { }

    public Product(string name, string description, decimal price)
    {
        Name = name;
        Description = description;
        SetPrice(price);
    }

    public void UpdateDetails(string name, string description, decimal price)
    {
        Name = name;
        Description = description;
        SetPrice(price);
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new Exceptions.DomainException("Price must be greater than zero");
        Price = price;
    }
}
