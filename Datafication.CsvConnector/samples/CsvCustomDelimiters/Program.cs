using Datafication.Core.Data;
using Datafication.Connectors.CsvConnector;

Console.WriteLine("=== Datafication.CsvConnector Custom Delimiters Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");

// ============================================================================
// 1. Semicolon-separated values (common in European locales)
// ============================================================================
Console.WriteLine("1. Loading semicolon-separated CSV (European format)...");
Console.WriteLine("   File: products_euro.csv (separator: ';')\n");

var semicolonConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "products_euro.csv"))),
    Separator = ';',
    HeaderRow = true
};

var semicolonConnector = new CsvDataConnector(semicolonConfig);
var products = await semicolonConnector.GetDataAsync();

Console.WriteLine($"   Loaded {products.RowCount} products");
Console.WriteLine("   Columns: " + string.Join(", ", products.Schema.GetColumnNames()));
Console.WriteLine();

// Display first 5 products
Console.WriteLine("   First 5 products:");
Console.WriteLine("   " + new string('-', 70));
Console.WriteLine($"   {"ProductId",-10} {"Name",-25} {"Category",-15} {"Price",-10}");
Console.WriteLine("   " + new string('-', 70));

var productCursor = products.GetRowCursor("ProductId", "Name", "Category", "Price");
int count = 0;
while (productCursor.MoveNext() && count < 5)
{
    Console.WriteLine($"   {productCursor.GetValue("ProductId"),-10} {productCursor.GetValue("Name"),-25} {productCursor.GetValue("Category"),-15} {productCursor.GetValue("Price"),-10:C}");
    count++;
}
Console.WriteLine();

// ============================================================================
// 2. Tab-separated values (TSV)
// ============================================================================
Console.WriteLine("2. Loading tab-separated values (TSV)...");
Console.WriteLine("   File: server_logs.tsv (separator: '\\t')\n");

var tsvConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "server_logs.tsv"))),
    Separator = '\t',
    HeaderRow = true
};

var tsvConnector = new CsvDataConnector(tsvConfig);
var logs = await tsvConnector.GetDataAsync();

Console.WriteLine($"   Loaded {logs.RowCount} log entries");
Console.WriteLine("   Columns: " + string.Join(", ", logs.Schema.GetColumnNames()));
Console.WriteLine();

// Display first 5 log entries
Console.WriteLine("   First 5 log entries:");
Console.WriteLine("   " + new string('-', 90));
Console.WriteLine($"   {"Timestamp",-24} {"Level",-8} {"Source",-15} {"Message",-40}");
Console.WriteLine("   " + new string('-', 90));

var logCursor = logs.GetRowCursor("Timestamp", "Level", "Source", "Message");
count = 0;
while (logCursor.MoveNext() && count < 5)
{
    var msg = logCursor.GetValue("Message")?.ToString() ?? "";
    if (msg.Length > 38) msg = msg.Substring(0, 38) + "..";
    Console.WriteLine($"   {logCursor.GetValue("Timestamp"),-24} {logCursor.GetValue("Level"),-8} {logCursor.GetValue("Source"),-15} {msg,-40}");
    count++;
}
Console.WriteLine();

// Filter by log level
var warnings = logs.Where("Level", "WARN");
var errors = logs.Where("Level", "ERROR");
Console.WriteLine($"   Summary: {warnings.RowCount} warnings, {errors.RowCount} errors\n");

// ============================================================================
// 3. Pipe-delimited values
// ============================================================================
Console.WriteLine("3. Loading pipe-delimited values...");
Console.WriteLine("   File: sensor_data.csv (separator: '|')\n");

var pipeConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "sensor_data.csv"))),
    Separator = '|',
    HeaderRow = true
};

var pipeConnector = new CsvDataConnector(pipeConfig);
var sensors = await pipeConnector.GetDataAsync();

Console.WriteLine($"   Loaded {sensors.RowCount} sensor readings");
Console.WriteLine("   Columns: " + string.Join(", ", sensors.Schema.GetColumnNames()));
Console.WriteLine();

// Display first 5 sensor readings
Console.WriteLine("   First 5 sensor readings:");
Console.WriteLine("   " + new string('-', 75));
Console.WriteLine($"   {"SensorId",-10} {"Location",-25} {"Temp",-8} {"Humidity",-10} {"Timestamp",-20}");
Console.WriteLine("   " + new string('-', 75));

var sensorCursor = sensors.GetRowCursor("SensorId", "Location", "Temperature", "Humidity", "Timestamp");
count = 0;
while (sensorCursor.MoveNext() && count < 5)
{
    Console.WriteLine($"   {sensorCursor.GetValue("SensorId"),-10} {sensorCursor.GetValue("Location"),-25} {sensorCursor.GetValue("Temperature"),-8} {sensorCursor.GetValue("Humidity"),-10} {sensorCursor.GetValue("Timestamp"),-20}");
    count++;
}
Console.WriteLine();

// ============================================================================
// Summary
// ============================================================================
Console.WriteLine("=== Summary ===");
Console.WriteLine($"   Semicolon-separated (;): {products.RowCount} products loaded");
Console.WriteLine($"   Tab-separated (\\t):      {logs.RowCount} log entries loaded");
Console.WriteLine($"   Pipe-separated (|):      {sensors.RowCount} sensor readings loaded");

Console.WriteLine("\n=== Sample Complete ===");
