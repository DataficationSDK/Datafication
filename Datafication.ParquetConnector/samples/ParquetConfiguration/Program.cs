using Datafication.Core.Data;
using Datafication.Connectors.ParquetConnector;
using Datafication.Extensions.Connectors.ParquetConnector;

Console.WriteLine("=== Datafication.ParquetConnector Configuration Sample ===\n");

// Get path to data directory (relative to build output)
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var parquetPath = Path.GetFullPath(Path.Combine(dataPath, "SalesRecords50k.parquet"));

// 1. Full configuration with all options
Console.WriteLine("1. Creating full configuration with all options...");
var errorLog = new List<(DateTime Timestamp, string Message)>();

var fullConfig = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath),
    Id = "sales-data-2024",
    ErrorHandler = (ex) =>
    {
        errorLog.Add((DateTime.UtcNow, ex.Message));
        Console.WriteLine($"   [ErrorHandler] {ex.GetType().Name}: {ex.Message}");
    }
};

Console.WriteLine($"   Source: {fullConfig.Source}");
Console.WriteLine($"   Id: {fullConfig.Id}");
Console.WriteLine($"   ErrorHandler: {(fullConfig.ErrorHandler != null ? "Configured" : "Not set")}\n");

// 2. Load data using the configuration
Console.WriteLine("2. Loading data with full configuration...");
var connector = new ParquetDataConnector(fullConfig);
var salesData = await connector.GetDataAsync();
Console.WriteLine($"   Loaded {salesData.RowCount} rows");
Console.WriteLine($"   Connector ID: {connector.GetConnectorId()}\n");

// 3. Minimal configuration (using defaults)
Console.WriteLine("3. Minimal configuration (defaults)...");
var minimalConfig = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath)
};

Console.WriteLine($"   Source: {minimalConfig.Source}");
Console.WriteLine($"   Id: {minimalConfig.Id ?? "(auto-generated)"}");
Console.WriteLine($"   ErrorHandler: {(minimalConfig.ErrorHandler != null ? "Configured" : "Not set")}\n");

// 4. Configuration with custom Id only
Console.WriteLine("4. Configuration with custom Id...");
var idConfig = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath),
    Id = "quarterly-sales-report"
};

var idConnector = new ParquetDataConnector(idConfig);
Console.WriteLine($"   Connector ID: {idConnector.GetConnectorId()}\n");

// 5. Using extension method with configuration
Console.WriteLine("5. Using extension method with configuration...");
var configForExtension = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath),
    Id = "extension-load"
};

var dataViaExtension = await DataBlock.Connector.LoadParquetAsync(configForExtension);
Console.WriteLine($"   Loaded {dataViaExtension.RowCount} rows via extension method\n");

// 6. Configuration reuse pattern
Console.WriteLine("6. Configuration reuse pattern...");
var reusableConfig = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath),
    Id = "reusable-config"
};

// Create multiple connectors from same configuration
var connector1 = new ParquetDataConnector(reusableConfig);
var connector2 = new ParquetDataConnector(reusableConfig);

Console.WriteLine($"   Connector 1 ID: {connector1.GetConnectorId()}");
Console.WriteLine($"   Connector 2 ID: {connector2.GetConnectorId()}");
Console.WriteLine($"   Same ID: {connector1.GetConnectorId() == connector2.GetConnectorId()}\n");

// 7. Display sample data to verify loading
Console.WriteLine("7. Sample data from configured load:");
Console.WriteLine("   " + new string('-', 70));
Console.WriteLine($"   {"Region",-15} {"Country",-25} {"TotalRevenue",-15}");
Console.WriteLine("   " + new string('-', 70));

var cursor = salesData.GetRowCursor("Region", "Country", "TotalRevenue");
int rowCount = 0;
while (cursor.MoveNext() && rowCount < 5)
{
    var region = cursor.GetValue("Region");
    var country = cursor.GetValue("Country");
    var revenue = cursor.GetValue("TotalRevenue");
    Console.WriteLine($"   {region,-15} {country,-25} {revenue,-15:C0}");
    rowCount++;
}
Console.WriteLine("   " + new string('-', 70));

// 8. Configuration reference
Console.WriteLine("\n8. Configuration Properties Reference:");
Console.WriteLine("   " + new string('-', 70));
Console.WriteLine($"   {"Property",-20} {"Type",-25} {"Required",-10}");
Console.WriteLine("   " + new string('-', 70));
Console.WriteLine($"   {"Source",-20} {"Uri",-25} {"Yes",-10}");
Console.WriteLine($"   {"Id",-20} {"string?",-25} {"No",-10}");
Console.WriteLine($"   {"ErrorHandler",-20} {"Action<Exception>?",-25} {"No",-10}");
Console.WriteLine("   " + new string('-', 70));

Console.WriteLine("\n=== Sample Complete ===");
