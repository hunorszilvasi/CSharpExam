namespace WinFormsExam.Domain;

public enum DeliveryStatus
{
    Pending,
    Processing,
    Completed
}

public abstract class Delivery
{
    protected Delivery(int id, string recipient, int priority, DeliveryStatus status = DeliveryStatus.Pending)
    {
        if (id < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        if (string.IsNullOrWhiteSpace(recipient))
        {
            throw new ArgumentException("Recipient is required.", nameof(recipient));
        }

        if (priority is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be 1, 2, or 3.");
        }

        Id = id;
        Recipient = recipient.Trim();
        Priority = priority;
        Status = status;
    }

    public int Id { get; }

    public string Recipient { get; }

    public int Priority { get; }

    public DeliveryStatus Status { get; private set; }

    public abstract int GetProcessingDelayMs();

    public void SetStatus(DeliveryStatus status)
    {
        Status = status;
    }
}

public sealed class RegularDelivery : Delivery
{
    public RegularDelivery(int id, string recipient, int priority, DeliveryStatus status = DeliveryStatus.Pending)
        : base(id, recipient, priority, status)
    {
        if (priority is not (1 or 2))
        {
            throw new ArgumentOutOfRangeException(nameof(priority), "Regular deliveries must have priority 1 or 2.");
        }
    }
    // Returns the processing delay
    public override int GetProcessingDelayMs() => 600;
}

public sealed class ExpressDelivery : Delivery
{
    public ExpressDelivery(int id, string recipient, int priority = 3, DeliveryStatus status = DeliveryStatus.Pending)
        : base(id, recipient, priority, status)
    {
        if (priority != 3)
        {
            throw new ArgumentOutOfRangeException(nameof(priority), "Express deliveries must have priority 3.");
        }
    }

    public override int GetProcessingDelayMs() => 300;
}
