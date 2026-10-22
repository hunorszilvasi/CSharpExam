using Microsoft.Data.Sqlite;
using WinFormsExam.Domain;

namespace WinFormsExam.Data;

public sealed class SqliteDeliveryRepository : IDeliveryRepository
{
    private readonly string connectionString;

    // Initializes a new instance of the SqliteDeliveryRepository
    public SqliteDeliveryRepository(string databasePath)
    {
        // Ensure the directory for the database exist
        var fullPath = Path.GetFullPath(databasePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath
        }.ToString();

        InitializeDatabase();
    }

    // Inserts a new delivery into the database
    public int Insert(Delivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Deliveries (Recipient, Priority, Status)
            VALUES ($recipient, $priority, $status);
            SELECT last_insert_rowid();
            """;
        // Add parameters to prevent SQL injection
        command.Parameters.AddWithValue("$recipient", delivery.Recipient);
        command.Parameters.AddWithValue("$priority", delivery.Priority);
        command.Parameters.AddWithValue("$status", delivery.Status.ToString());

        // Execute the command and return the newly generated Id
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // Loads all deliveries from the database
    public IReadOnlyList<Delivery> LoadAll()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Recipient, Priority, Status
            FROM Deliveries
            ORDER BY Id;
            """;

        using var reader = command.ExecuteReader();
        var deliveries = new List<Delivery>();
        while (reader.Read())
        {
            var id = reader.GetInt32(0);
            var recipient = reader.GetString(1);
            var priority = reader.GetInt32(2);
            var status = ParseStatus(reader.GetString(3));
            deliveries.Add(CreateDelivery(id, recipient, priority, status));
        }
        return deliveries;
    }

    // Updates the status of a delivery
    public void UpdateStatus(int id, DeliveryStatus status)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Deliveries
            SET Status = $status
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$status", status.ToString());
        command.Parameters.AddWithValue("$id", id);

        if (command.ExecuteNonQuery() == 0)
        {
            throw new InvalidOperationException($"Delivery {id} does not exist.");
        }
    }

    // Resets the status of all deliveries in case of an application crash
    public void ResetProcessingDeliveries()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Deliveries
            SET Status = 'Pending'
            WHERE Status = 'Processing';
            """;
        command.ExecuteNonQuery();
    }
    // Resets the status of a specific delivery in case of an application crash
    public void ResetProcessingDelivery(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Deliveries
            SET Status = 'Pending'
            WHERE Id = $id AND Status = 'Processing';
            """;
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private void InitializeDatabase()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        // Create the Deliveries table
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Deliveries
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Recipient TEXT NOT NULL,
                Priority INTEGER NOT NULL CHECK (Priority BETWEEN 1 AND 3),
                Status TEXT NOT NULL CHECK (Status IN ('Pending', 'Processing', 'Completed'))
            );
            """;
        command.ExecuteNonQuery();
    }

    private SqliteConnection OpenConnection()
    {
        // Open a new SQLite connection
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }

    private static Delivery CreateDelivery(int id, string recipient, int priority, DeliveryStatus status)
    {
        // Create a Delivery instance based on the priority
        return priority == 3
            ? new ExpressDelivery(id, recipient, priority, status)
            : new RegularDelivery(id, recipient, priority, status);
    }

    private static DeliveryStatus ParseStatus(string status)
    {
        // Parse the delivery status from string to enum
        return Enum.TryParse<DeliveryStatus>(status, ignoreCase: false, out var parsed)
            ? parsed
            : throw new InvalidDataException($"Unknown delivery status: {status}");
    }
}
