namespace Domain.Exceptions;

public class ProductNotFoundException : DomainException
{
    public int ProductId { get; }

    public ProductNotFoundException(int productId)
        : base($"Product with ID {productId} was not found.")
    {
        ProductId = productId;
    }
}
