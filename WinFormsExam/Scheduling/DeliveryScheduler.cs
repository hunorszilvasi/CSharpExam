using WinFormsExam.Domain;

namespace WinFormsExam.Scheduling;

public sealed class DeliveryScheduler
{
    public Delivery? SelectNext(IReadOnlyList<Delivery> deliveries, int consecutiveExpressCount)
    {
        // Filter the deliveries to only include those that are pending
        var pending = deliveries
            .Where(delivery => delivery.Status == DeliveryStatus.Pending)
            .ToList();

        if (pending.Count == 0)
        {
            return null;
        }
        // Select the next delivery based on priority
        if (consecutiveExpressCount >= 2)
        {
            var oldestLowerPriority = pending
                .Where(delivery => delivery.Priority is 1 or 2)
                .OrderBy(delivery => delivery.Id)
                .FirstOrDefault();
            if (oldestLowerPriority is not null)
            {
                // Select the oldest lower priority delivery
                return oldestLowerPriority;
            }
        }

        return pending
            .OrderByDescending(delivery => delivery.Priority)
            .ThenBy(delivery => delivery.Id)
            .First();
    }
}
