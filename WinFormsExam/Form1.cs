using WinFormsExam.Data;
using WinFormsExam.Domain;
using WinFormsExam.Infrastructure;
using WinFormsExam.Processing;
using WinFormsExam.Scheduling;

namespace WinFormsExam;

public partial class Form1 : Form
{
    private readonly IDeliveryRepository repository;
    private readonly TraceLogger traceLogger;
    private readonly DeliveryDatasetGenerator datasetGenerator;
    private readonly DeliveryProcessor deliveryProcessor;
    private readonly TextBox recipientTextBox = new();
    private readonly ComboBox priorityComboBox = new();
    private readonly DataGridView deliveriesGrid = new();
    private readonly Label countsLabel = new();
    private readonly Button addButton = new();
    private readonly Button generateButton = new();
    private readonly Button startButton = new();
    private readonly Button stopButton = new();

    public Form1()
    {
        InitializeComponent();

        var databasePath = Path.Combine(AppContext.BaseDirectory, "ponyexpress.db");
        repository = new SqliteDeliveryRepository(databasePath);
        repository.ResetProcessingDeliveries();
        traceLogger = new TraceLogger(Path.Combine(AppContext.BaseDirectory, "trace.log"));
        datasetGenerator = new DeliveryDatasetGenerator(repository, traceLogger);
        deliveryProcessor = new DeliveryProcessor(repository, traceLogger, new DeliveryScheduler());

        InitializeUserInterface();
        RefreshDeliveries();
    }

    private void InitializeUserInterface()
    {
        Text = "PonyExpress Delivery Manager";
        ClientSize = new Size(900, 600);
        MinimumSize = new Size(700, 450);

        var recipientLabel = new Label { AutoSize = true, Text = "Recipient:", Location = new Point(12, 17) };
        recipientTextBox.Location = new Point(85, 12);
        recipientTextBox.Size = new Size(220, 23);

        var priorityLabel = new Label { AutoSize = true, Text = "Priority:", Location = new Point(325, 17) };
        priorityComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        priorityComboBox.Items.AddRange(new object[] { 1, 2, 3 });
        priorityComboBox.SelectedIndex = 0;
        priorityComboBox.Location = new Point(380, 12);
        priorityComboBox.Size = new Size(60, 23);

        ConfigureButton(addButton, "Add delivery", new Point(455, 11), AddDeliveryClick);
        ConfigureButton(generateButton, "Generate dataset", new Point(560, 11), GenerateDatasetClick);
        ConfigureButton(startButton, "Start processing", new Point(12, 52), StartProcessingClick);
        ConfigureButton(stopButton, "Stop processing", new Point(145, 52), StopProcessingClick);
        stopButton.Enabled = false;

        countsLabel.AutoSize = true;
        countsLabel.Location = new Point(310, 59);

        deliveriesGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        deliveriesGrid.Location = new Point(12, 90);
        deliveriesGrid.Size = new Size(860, 450);
        deliveriesGrid.ReadOnly = true;
        deliveriesGrid.AllowUserToAddRows = false;
        deliveriesGrid.AllowUserToDeleteRows = false;
        deliveriesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        deliveriesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        deliveriesGrid.Columns.Add("Id", "Id");
        deliveriesGrid.Columns.Add("Recipient", "Recipient");
        deliveriesGrid.Columns.Add("Priority", "Priority");
        deliveriesGrid.Columns.Add("Status", "Status");

        Controls.AddRange(new Control[]
        {
            recipientLabel, recipientTextBox, priorityLabel, priorityComboBox,
            addButton, generateButton, startButton, stopButton, countsLabel, deliveriesGrid
        });
    }

    private static void ConfigureButton(Button button, string text, Point location, EventHandler clickHandler)
    {
        button.Text = text;
        button.Location = location;
        button.AutoSize = true;
        button.Click += clickHandler;
    }

    private void AddDeliveryClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(recipientTextBox.Text))
        {
            MessageBox.Show("Recipient is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (priorityComboBox.SelectedItem is not int priority)
        {
            MessageBox.Show("Select a valid priority.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Delivery delivery = priority == 3
            ? new ExpressDelivery(0, recipientTextBox.Text)
            : new RegularDelivery(0, recipientTextBox.Text, priority);
        var id = repository.Insert(delivery);
        traceLogger.DeliveryAdded(id);
        recipientTextBox.Clear();
        RefreshDeliveries();
    }

    private void GenerateDatasetClick(object? sender, EventArgs e)
    {
        var generated = datasetGenerator.Generate();
        RefreshDeliveries();
        MessageBox.Show($"Generated {generated} deliveries in SQLite.", "Dataset generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void StartProcessingClick(object? sender, EventArgs e)
    {
        if (deliveryProcessor.Start(RefreshFromWorker, ProcessingCompleted, ProcessingFailed))
        {
            startButton.Enabled = false;
            stopButton.Enabled = true;
        }
    }

    private void StopProcessingClick(object? sender, EventArgs e)
    {
        deliveryProcessor.RequestStop();
        stopButton.Enabled = false;
    }

    private void RefreshDeliveries()
    {
        var deliveries = repository.LoadAll();
        deliveriesGrid.Rows.Clear();
        foreach (var delivery in deliveries)
        {
            deliveriesGrid.Rows.Add(delivery.Id, delivery.Recipient, delivery.Priority, delivery.Status);
        }

        var counts = deliveries
            .GroupBy(delivery => delivery.Status)
            .ToDictionary(group => group.Key, group => group.Count());
        countsLabel.Text = $"Pending: {GetCount(counts, DeliveryStatus.Pending)}   " +
                           $"Processing: {GetCount(counts, DeliveryStatus.Processing)}   " +
                           $"Completed: {GetCount(counts, DeliveryStatus.Completed)}";
    }

    private static int GetCount(IReadOnlyDictionary<DeliveryStatus, int> counts, DeliveryStatus status)
    {
        return counts.TryGetValue(status, out var count) ? count : 0;
    }

    private void RefreshFromWorker()
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(RefreshFromWorker);
            return;
        }

        RefreshDeliveries();
    }

    private void ProcessingFailed(Exception exception)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(() => ProcessingFailed(exception));
            return;
        }

        MessageBox.Show(
            $"Delivery processing stopped and the active delivery was returned to Pending.\n\n{exception.Message}",
            "Processing error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private void ProcessingCompleted()
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(ProcessingCompleted);
            return;
        }

        startButton.Enabled = true;
        stopButton.Enabled = false;
        RefreshDeliveries();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        deliveryProcessor.StopAndWait();
        traceLogger.Dispose();
        base.OnFormClosed(e);
    }
}
