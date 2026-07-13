namespace FCG.Shared.Events;

public class PaymentProcessedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
