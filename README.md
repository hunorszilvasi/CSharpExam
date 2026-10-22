# PonyExpress Delivery Manager

A .NET 10 Windows Forms application for registering and processing emergency deliveries.

## Run

1. Open `WinFormsExam.slnx` in Visual Studio 2026.
2. Restore NuGet packages and build the solution.
3. Start the `WinFormsExam` project.

The application uses the SQLite file at:

`WinFormsExam/bin/Debug/net10.0-windows/ponyexpress.db`

The file is copied from `WinFormsExam/ponyexpress.db` when it is newer. The application creates the database and `Deliveries` table automatically if they do not exist.

## Generate the required dataset

The Generate dataset button inserts 100 real SQLite rows. It creates at least 30 records at each priority and all generated records start with `Pending` status.

For reproducible command-line seeding, run from the solution directory:

```powershell
dotnet run --project .\WinFormsExam\WinFormsExam.csproj -- --generate-dataset
```

The generated database and trace are written to the runtime directory shown above. The application processes records without deleting them; only their persisted statuses change.

## Requirements implemented

- Abstract `Delivery`, `RegularDelivery`, and `ExpressDelivery` with polymorphic processing delays.
- `IDeliveryRepository` and parameterized `SqliteDeliveryRepository` operations.
- WinForms input validation, SQLite-backed grid, dataset generation, and status counts.
- LINQ scheduling with priority ordering and the two-consecutive-express fairness rule.
- LINQ `GroupBy` status counts.
- Exactly one cooperative `System.Threading.Thread` for processing; no `Task`, `async`, or `Thread.Abort`.
- Persisted `Pending`, `Processing`, and `Completed` transitions.
- `trace.log` records delivery-added and delivery-completed events with timestamps and IDs.
