using Datafication.Core.Data;
using Datafication.Connectors.CsvConnector;

Console.WriteLine("=== Datafication.CsvConnector Full Configuration Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");

// ============================================================================
// 1. Complete Configuration with All Options
// ============================================================================
Console.WriteLine("1. Creating a fully configured CsvConnectorConfiguration...\n");

var errorCount = 0;
var fullConfig = new CsvConnectorConfiguration
{
    // Required: Source URI
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "employees.csv"))),

    // Optional: Field separator (default is comma)
    Separator = ',',

    // Optional: Whether first row contains headers (default is true)
    HeaderRow = true,

    // Optional: Custom identifier for this connector
    Id = "employees-connector-001",

    // Optional: Global error handler
    ErrorHandler = (ex) =>
    {
        errorCount++;
        Console.WriteLine($"   [Error #{errorCount}] {ex.GetType().Name}: {ex.Message}");
    }
};

Console.WriteLine("   Configuration Properties:");
Console.WriteLine($"   - Source: {fullConfig.Source}");
Console.WriteLine($"   - Separator: '{fullConfig.Separator}'");
Console.WriteLine($"   - HeaderRow: {fullConfig.HeaderRow}");
Console.WriteLine($"   - Id: {fullConfig.Id}");
Console.WriteLine($"   - ErrorHandler: {(fullConfig.ErrorHandler != null ? "Configured" : "Not set")}");
Console.WriteLine();

// ============================================================================
// 2. Create Connector and Load Data
// ============================================================================
Console.WriteLine("2. Creating connector and loading data...");

var connector = new CsvDataConnector(fullConfig);
Console.WriteLine($"   Connector ID: {connector.GetConnectorId()}");

var employees = await connector.GetDataAsync();
Console.WriteLine($"   Loaded {employees.RowCount} rows with {employees.Schema.Count} columns\n");

// ============================================================================
// 3. Inspect Configuration from Connector
// ============================================================================
Console.WriteLine("3. Accessing configuration from connector instance...");
Console.WriteLine($"   connector.Configuration.Source: {connector.Configuration.Source}");
Console.WriteLine($"   connector.Configuration.Separator: '{connector.Configuration.Separator}'");
Console.WriteLine($"   connector.Configuration.HeaderRow: {connector.Configuration.HeaderRow}");
Console.WriteLine();

// ============================================================================
// 4. Different Configuration Patterns
// ============================================================================
Console.WriteLine("4. Common configuration patterns...\n");

// Pattern A: Minimal configuration (uses defaults)
Console.WriteLine("   Pattern A: Minimal (defaults)");
var minimalConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "employees.csv")))
};
Console.WriteLine($"   - Uses default separator: ','");
Console.WriteLine($"   - Uses default HeaderRow: true");
Console.WriteLine($"   - Auto-generated Id: {minimalConfig.Id}");
Console.WriteLine();

// Pattern B: European CSV format
Console.WriteLine("   Pattern B: European format (semicolon separator)");
var europeanConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "products_euro.csv"))),
    Separator = ';',
    HeaderRow = true,
    Id = "european-products"
};
var euroConnector = new CsvDataConnector(europeanConfig);
var euroData = await euroConnector.GetDataAsync();
Console.WriteLine($"   - Loaded {euroData.RowCount} rows with semicolon separator");
Console.WriteLine();

// Pattern C: Headerless data
Console.WriteLine("   Pattern C: No header row");
var noHeaderConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "raw_numbers.csv"))),
    HeaderRow = false,
    Id = "raw-data"
};
var noHeaderConnector = new CsvDataConnector(noHeaderConfig);
var noHeaderData = await noHeaderConnector.GetDataAsync();
Console.WriteLine($"   - Loaded {noHeaderData.RowCount} rows");
Console.WriteLine($"   - Auto-generated columns: {string.Join(", ", noHeaderData.Schema.GetColumnNames())}");
Console.WriteLine();

// Pattern D: Production configuration with monitoring
Console.WriteLine("   Pattern D: Production with monitoring");
var prodErrors = new List<(DateTime Time, string Message)>();
var productionConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "employees.csv"))),
    Separator = ',',
    HeaderRow = true,
    Id = $"prod-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
    ErrorHandler = (ex) =>
    {
        prodErrors.Add((DateTime.UtcNow, ex.Message));
        // In production: log to file, send to monitoring system, etc.
    }
};
Console.WriteLine($"   - Timestamped Id: {productionConfig.Id}");
Console.WriteLine($"   - Error tracking: Configured");
Console.WriteLine();

// ============================================================================
// 5. Configuration Reuse
// ============================================================================
Console.WriteLine("5. Reusing configuration for multiple loads...");

var reusableConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "employees.csv"))),
    HeaderRow = true,
    Id = "reusable-config"
};

// Create multiple connectors with same config
var connector1 = new CsvDataConnector(reusableConfig);
var data1 = await connector1.GetDataAsync();

var connector2 = new CsvDataConnector(reusableConfig);
var data2 = await connector2.GetDataAsync();

Console.WriteLine($"   First load: {data1.RowCount} rows");
Console.WriteLine($"   Second load: {data2.RowCount} rows");
Console.WriteLine($"   Both use same config Id: {reusableConfig.Id}\n");

// ============================================================================
// 6. Display Sample Data
// ============================================================================
Console.WriteLine("6. Sample data from full configuration load:");
Console.WriteLine("   " + new string('-', 75));
Console.WriteLine($"   {"Id",-5} {"Name",-22} {"Department",-15} {"Salary",-12} {"Active",-8}");
Console.WriteLine("   " + new string('-', 75));

var cursor = employees.GetRowCursor("Id", "Name", "Department", "Salary", "IsActive");
int count = 0;
while (cursor.MoveNext() && count < 5)
{
    Console.WriteLine($"   {cursor.GetValue("Id"),-5} {cursor.GetValue("Name"),-22} {cursor.GetValue("Department"),-15} {cursor.GetValue("Salary"),-12:C0} {cursor.GetValue("IsActive"),-8}");
    count++;
}
Console.WriteLine("   " + new string('-', 75));
Console.WriteLine();

// ============================================================================
// Summary
// ============================================================================
Console.WriteLine("=== Configuration Reference ===");
Console.WriteLine("   CsvConnectorConfiguration Properties:");
Console.WriteLine("   ┌─────────────────┬─────────────────────────────────────────┐");
Console.WriteLine("   │ Property        │ Description                             │");
Console.WriteLine("   ├─────────────────┼─────────────────────────────────────────┤");
Console.WriteLine("   │ Source (Uri)    │ File path or HTTP URL (required)        │");
Console.WriteLine("   │ Separator (char)│ Field delimiter (default: ',')          │");
Console.WriteLine("   │ HeaderRow (bool)│ First row is headers (default: true)    │");
Console.WriteLine("   │ Id (string)     │ Unique identifier (auto-generated)      │");
Console.WriteLine("   │ ErrorHandler    │ Global exception handler (optional)     │");
Console.WriteLine("   └─────────────────┴─────────────────────────────────────────┘");

Console.WriteLine("\n=== Sample Complete ===");
