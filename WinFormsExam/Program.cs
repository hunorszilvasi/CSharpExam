using WinFormsExam.Data;
using WinFormsExam.Infrastructure;

namespace WinFormsExam
{
    internal static class Program
    {
        ///  The main entry point for the application
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            if (Environment.GetCommandLineArgs().Contains("--generate-dataset", StringComparer.OrdinalIgnoreCase))
            {
                // Database path is set to the application's base directory
                var databasePath = Path.Combine(AppContext.BaseDirectory, "ponyexpress.db");
                using var traceLogger = new TraceLogger(Path.Combine(AppContext.BaseDirectory, "trace.log"));
                var repository = new SqliteDeliveryRepository(databasePath);
                // Generate a dataset with at least 100 deliveries
                var generator = new DeliveryDatasetGenerator(repository, traceLogger);
                Console.WriteLine($"Generated {generator.Generate()} deliveries in {databasePath}");
                return;
            }

            Application.Run(new Form1());
        }
    }
}