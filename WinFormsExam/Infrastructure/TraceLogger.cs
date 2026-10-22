using System.Diagnostics;
using WinFormsExam.Domain;

namespace WinFormsExam.Infrastructure;

public sealed class TraceLogger : IDisposable
{
    private readonly TextWriterTraceListener listener;

    public TraceLogger(string filePath)
    {
        listener = new TextWriterTraceListener(filePath);
        Trace.Listeners.Add(listener);
        Trace.AutoFlush = true;
    }

    // Logs the addition of a new delivery
    public void DeliveryAdded(int deliveryId)
    {
        Trace.WriteLine($"{DateTime.Now:O} Delivery added: Id={deliveryId}");
        listener.Flush();
    }
    // Logs the completion of a delivery
    public void DeliveryCompleted(Delivery delivery)
    {
        Trace.WriteLine($"{DateTime.Now:O} Delivery completed: Id={delivery.Id}");
        listener.Flush();
    }
    // in case of processing failure, logs the delivery id and the exception details
    public void ProcessingFailed(int deliveryId, Exception exception)
    {
        Trace.WriteLine($"{DateTime.Now:O} Delivery processing failed: Id={deliveryId}, Error={exception}");
        listener.Flush();
    }

    // Logs the start of the delivery processing
    public void Dispose()
    {
        listener.Flush();
        Trace.Listeners.Remove(listener);
        listener.Dispose();
    }
}
