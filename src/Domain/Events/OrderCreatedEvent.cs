namespace Domain.Events;

public class OrderCreatedEvent
{
    public int OrderId { get; }
    public int UserId { get; }
    public DateTime OccurredOn { get; }

    public OrderCreatedEvent(int orderId, int userId)
    {
        OrderId = orderId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}
