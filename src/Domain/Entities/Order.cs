using Domain.Enums;

namespace Domain.Entities;

public class Order
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }

    private Order() { }

    public Order(int userId, decimal totalAmount)
    {
        UserId = userId;
        TotalAmount = totalAmount;
        Status = OrderStatus.Pending;
        OrderDate = DateTime.UtcNow;
    }

    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.Pending)
            throw new Exceptions.DomainException("Only pending orders can be marked as processing");
        Status = OrderStatus.Processing;
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Processing)
            throw new Exceptions.DomainException("Only processing orders can be marked as shipped");
        Status = OrderStatus.Shipped;
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new Exceptions.DomainException("Only shipped orders can be marked as delivered");
        Status = OrderStatus.Delivered;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new Exceptions.DomainException("Cannot cancel a delivered order");
        Status = OrderStatus.Cancelled;
    }
}
