using WinFormsExam.Data;
using WinFormsExam.Domain;
using WinFormsExam.Infrastructure;
using WinFormsExam.Scheduling;

namespace WinFormsExam.Processing;

public sealed class DeliveryProcessor
{
    private readonly IDeliveryRepository repository;
    private readonly TraceLogger traceLogger;
    private readonly DeliveryScheduler scheduler;
    private readonly object syncRoot = new();
    private Thread? workerThread;
    private volatile bool stopRequested;

    public DeliveryProcessor(IDeliveryRepository repository, TraceLogger traceLogger, DeliveryScheduler scheduler)
    {
        this.repository = repository;
        this.traceLogger = traceLogger;
        this.scheduler = scheduler;
    }

    public bool IsRunning
    {
        get
        {
            lock (syncRoot)
            {
                return workerThread?.IsAlive == true;
            }
        }
    }
    // Starts the delivery processing
    public bool Start(Action refreshCallback, Action completedCallback, Action<Exception>? failedCallback = null)
    {
        lock (syncRoot)
        {
            if (workerThread?.IsAlive == true)
            {
                return false;
            }

            stopRequested = false;
            // Start a new worker thread to process deliveries
            workerThread = new Thread(() => ProcessDeliveries(refreshCallback, completedCallback, failedCallback))
            {
                IsBackground = true,
                Name = "PonyExpressDeliveryWorker"
            };
            // Start the worker thread
            workerThread.Start();
            return true;
        }
    }

    public void RequestStop()
    {
        stopRequested = true;
    }

    // Waits for the worker thread to finish processing deliveries
    public void StopAndWait()
    {
        Thread? thread;
        lock (syncRoot)
        {
            stopRequested = true;
            thread = workerThread;
        }

        if (thread is not null && thread != Thread.CurrentThread)
        {
            thread.Join();
        }
    }

    private void ProcessDeliveries(
        Action refreshCallback,
        Action completedCallback,
        Action<Exception>? failedCallback)
    {
        // Track the number of consecutive express deliveries processed
        var consecutiveExpressCount = 0;
        Delivery? currentDelivery = null;
        try
        {
            while (!stopRequested)
            {
                // Select the next delivery
                currentDelivery = scheduler.SelectNext(repository.LoadAll(), consecutiveExpressCount);
                if (currentDelivery is null)
                {
                    break;
                }
                // Update the status to Processing
                repository.UpdateStatus(currentDelivery.Id, DeliveryStatus.Processing);
                currentDelivery.SetStatus(DeliveryStatus.Processing);
                RefreshOnWorker(refreshCallback);

                Thread.Sleep(currentDelivery.GetProcessingDelayMs());

                // Update the status to Completed
                repository.UpdateStatus(currentDelivery.Id, DeliveryStatus.Completed);
                currentDelivery.SetStatus(DeliveryStatus.Completed);
                traceLogger.DeliveryCompleted(currentDelivery);
                consecutiveExpressCount = currentDelivery.Priority == 3 ? consecutiveExpressCount + 1 : 0;
                RefreshOnWorker(refreshCallback);
                currentDelivery = null;
            }
        }
        catch (Exception exception)
        {
            if (currentDelivery is not null)
            {
                try
                {
                    repository.ResetProcessingDelivery(currentDelivery.Id);
                }
                catch (Exception recoveryException)
                {
                    traceLogger.ProcessingFailed(
                        currentDelivery.Id,
                        new AggregateException(exception, recoveryException));
                    currentDelivery = null;
                }

                if (currentDelivery is not null)
                {
                    traceLogger.ProcessingFailed(currentDelivery.Id, exception);
                }
            }

            RefreshOnWorker(() => failedCallback?.Invoke(exception));
        }
        finally
        {
            lock (syncRoot)
            {
                workerThread = null;
            }

            RefreshOnWorker(completedCallback);
        }
    }

    // Refreshes the UI from the worker thread
    private static void RefreshOnWorker(Action callback)
    {
        try
        {
            callback();
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }
}
