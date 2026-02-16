using Datafication.Core.Data;
using Datafication.Connectors.JsonConnector;
using Datafication.Extensions.Connectors.JsonConnector;

Console.WriteLine("=== Datafication.JsonConnector Configuration Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var jsonPath = Path.Combine(dataPath, "employees.json");
var fileUri = new Uri("file://" + jsonPath.Replace('\\', '/'));

// 1. Minimal configuration - just the source
Console.WriteLine("1. Minimal Configuration (Source only)...");
var minimalConfig = new JsonConnectorConfiguration
{
    Source = fileUri
};
var minimalConnector = new JsonDataConnector(minimalConfig);
var minimalData = await minimalConnector.GetDataAsync();
Console.WriteLine($"   Config ID: {minimalConfig.Id}");
Console.WriteLine($"   Source: {minimalConfig.Source}");
Console.WriteLine($"   Loaded: {minimalData.RowCount} rows\n");

// 2. Configuration with custom ID
Console.WriteLine("2. Configuration with Custom ID...");
var namedConfig = new JsonConnectorConfiguration
{
    Id = "employee-data-source",
    Source = fileUri
};
var namedConnector = new JsonDataConnector(namedConfig);
Console.WriteLine($"   Config ID: {namedConfig.Id}");
Console.WriteLine($"   Connector ID: {namedConnector.GetConnectorId()}\n");

// 3. Configuration with error handler
Console.WriteLine("3. Configuration with Error Handler...");
var errorsLogged = new List<string>();
var errorHandlingConfig = new JsonConnectorConfiguration
{
    Id = "error-handling-demo",
    Source = fileUri,
    ErrorHandler = (ex) =>
    {
        errorsLogged.Add($"[{DateTime.Now:HH:mm:ss}] Error: {ex.Message}");
        Console.WriteLine($"   ErrorHandler invoked: {ex.Message}");
    }
};
var errorHandlingConnector = new JsonDataConnector(errorHandlingConfig);
var errorHandlingData = await errorHandlingConnector.GetDataAsync();
Console.WriteLine($"   Loaded: {errorHandlingData.RowCount} rows");
Console.WriteLine($"   Errors logged: {errorsLogged.Count}\n");

// 4. Production-ready configuration
Console.WriteLine("4. Production-Ready Configuration...");
var productionConfig = new JsonConnectorConfiguration
{
    Id = $"json-connector-{DateTime.Now:yyyyMMdd-HHmmss}",
    Source = fileUri,
    ErrorHandler = (ex) =>
    {
        // In production, this could log to a file, monitoring system, etc.
        Console.WriteLine($"   [PROD LOG] {ex.GetType().Name}: {ex.Message}");
    }
};

Console.WriteLine($"   ID: {productionConfig.Id}");
Console.WriteLine($"   Source: {productionConfig.Source}");
Console.WriteLine($"   ErrorHandler: {(productionConfig.ErrorHandler != null ? "Configured" : "None")}\n");

// 5. Using configuration with extension methods
Console.WriteLine("5. Using Configuration with Extension Methods...");
var extensionConfig = new JsonConnectorConfiguration
{
    Id = "extension-method-demo",
    Source = fileUri,
    ErrorHandler = (ex) => Console.WriteLine($"   Extension error: {ex.Message}")
};

// Load using the configuration overload of the extension method
var extensionData = await DataBlock.Connector.LoadJsonAsync(extensionConfig);
Console.WriteLine($"   Loaded via extension: {extensionData.RowCount} rows\n");

// 6. Reusing configurations
Console.WriteLine("6. Reusing Configuration for Multiple Loads...");
var reusableConfig = new JsonConnectorConfiguration
{
    Id = "reusable-config",
    Source = fileUri
};

// First load
var load1 = await new JsonDataConnector(reusableConfig).GetDataAsync();
Console.WriteLine($"   First load: {load1.RowCount} rows");

// Change source and load again (demonstrating flexibility)
var productsPath = Path.Combine(dataPath, "products.json");
reusableConfig.Source = new Uri("file://" + productsPath.Replace('\\', '/'));

var load2 = await new JsonDataConnector(reusableConfig).GetDataAsync();
Console.WriteLine($"   Second load (products): {load2.RowCount} rows\n");

// 7. Display all configuration properties
Console.WriteLine("7. JsonConnectorConfiguration Properties Summary:");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine($"   {"Property",-20} {"Description",-40}");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine($"   {"Id",-20} {"Unique identifier for the connector",-40}");
Console.WriteLine($"   {"Source",-20} {"URI to JSON file or HTTP endpoint",-40}");
Console.WriteLine($"   {"ErrorHandler",-20} {"Action<Exception> callback for errors",-40}");
Console.WriteLine("   " + new string('-', 60));

Console.WriteLine("\n=== Sample Complete ===");
