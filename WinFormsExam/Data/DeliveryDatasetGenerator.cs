using WinFormsExam.Domain;
using WinFormsExam.Infrastructure;

namespace WinFormsExam.Data;

public sealed class DeliveryDatasetGenerator
{
    private readonly IDeliveryRepository repository;
    private readonly TraceLogger traceLogger;

    public DeliveryDatasetGenerator(IDeliveryRepository repository, TraceLogger traceLogger)
    {
        this.repository = repository;
        this.traceLogger = traceLogger;
    }

    public int Generate(int minimumRows = 100)
    {
        if (minimumRows < 100)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumRows), "The dataset must contain at least 100 rows.");
        }

        var random = new Random();
        var generated = 0;
        for (var index = 0; index < minimumRows; index++)
        {
            // Randomly assign priority
            var priority = index < 30 ? 1 : index < 60 ? 2 : index < 90 ? 3 : random.Next(1, 4);
            Delivery delivery = priority == 3
                ? new ExpressDelivery(0, $"Generated Recipient {index + 1}")
                : new RegularDelivery(0, $"Generated Recipient {index + 1}", priority);
            var id = repository.Insert(delivery);
            traceLogger.DeliveryAdded(id);
            generated++;
        }

        return generated;
    }
}
