using System.Diagnostics;
using Datafication.Core.Data;
using Datafication.Connectors.CsvConnector;
using Datafication.Storage.Velocity;

Console.WriteLine("=== Datafication.CsvConnector to VelocityDataBlock Sample ===\n");

// Get paths
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var csvPath = Path.Combine(dataPath, "sales_large.csv");
var velocityPath = Path.Combine(Path.GetTempPath(), "csv_velocity_sample");

// Clean up previous runs
if (Directory.Exists(velocityPath))
{
    Directory.Delete(velocityPath, recursive: true);
}
Directory.CreateDirectory(velocityPath);

// ============================================================================
// 1. Check source CSV file
// ============================================================================
Console.WriteLine("1. Source CSV file information...");
var fileInfo = new FileInfo(csvPath);
Console.WriteLine($"   File: {fileInfo.Name}");
Console.WriteLine($"   Size: {fileInfo.Length:N0} bytes");

// Quick row count
var lineCount = File.ReadLines(csvPath).Count() - 1; // Subtract header
Console.WriteLine($"   Rows: {lineCount:N0}\n");

// ============================================================================
// 2. Configure CSV connector
// ============================================================================
Console.WriteLine("2. Configuring CSV connector...");

var csvConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(csvPath)),
    Separator = ',',
    HeaderRow = true,
    Id = "sales-data-connector"
};

var csvConnector = new CsvDataConnector(csvConfig);
Console.WriteLine($"   Connector ID: {csvConnector.GetConnectorId()}\n");

// ============================================================================
// 3. Create VelocityDataBlock for high-performance storage
// ============================================================================
Console.WriteLine("3. Creating VelocityDataBlock...");

var dfcPath = Path.Combine(velocityPath, "sales.dfc");
var velocityOptions = VelocityOptions.CreateHighThroughput();

using var velocityBlock = new VelocityDataBlock(dfcPath, velocityOptions);
Console.WriteLine($"   Storage path: {dfcPath}");
Console.WriteLine($"   Options: High Throughput mode\n");

// ============================================================================
// 4. Stream CSV data to VelocityDataBlock
// ============================================================================
Console.WriteLine("4. Streaming CSV to VelocityDataBlock...");
Console.WriteLine("   (Using batch processing for memory efficiency)\n");

var stopwatch = Stopwatch.StartNew();

// Stream with batch size of 10000 rows
const int batchSize = 10000;
await csvConnector.GetStorageDataAsync(velocityBlock, batchSize);
await velocityBlock.FlushAsync();

stopwatch.Stop();

Console.WriteLine($"   Batch size: {batchSize:N0} rows");
Console.WriteLine($"   Total rows loaded: {velocityBlock.RowCount:N0}");
Console.WriteLine($"   Load time: {stopwatch.ElapsedMilliseconds:N0} ms");
Console.WriteLine($"   Throughput: {velocityBlock.RowCount / stopwatch.Elapsed.TotalSeconds:N0} rows/sec\n");

// ============================================================================
// 5. Display storage statistics
// ============================================================================
Console.WriteLine("5. Storage statistics...");

var stats = await velocityBlock.GetStorageStatsAsync();
Console.WriteLine($"   Total rows: {stats.TotalRows:N0}");
Console.WriteLine($"   Active rows: {stats.ActiveRows:N0}");
Console.WriteLine($"   Deleted rows: {stats.DeletedRows:N0}");
Console.WriteLine($"   Storage files: {stats.StorageFiles}");
Console.WriteLine($"   Estimated size: {stats.EstimatedSizeBytes:N0} bytes\n");

// ============================================================================
// 6. Query the VelocityDataBlock
// ============================================================================
Console.WriteLine("6. Querying VelocityDataBlock (SIMD-accelerated)...");

// Query: Get sales from "West" region
var westSales = velocityBlock
    .Where("Region", "West")
    .Execute();

Console.WriteLine($"   West region sales: {westSales.RowCount:N0} orders\n");

// ============================================================================
// 7. Aggregation example
// ============================================================================
Console.WriteLine("7. Aggregation: Sales by region...");

var regionStats = velocityBlock
    .GroupByAggregate("Region", "Quantity", AggregationType.Sum, "TotalQuantity")
    .Execute();

Console.WriteLine("   " + new string('-', 35));
Console.WriteLine($"   {"Region",-15} {"Total Quantity",-15}");
Console.WriteLine("   " + new string('-', 35));

var regionCursor = regionStats.GetRowCursor("Region", "TotalQuantity");
while (regionCursor.MoveNext())
{
    Console.WriteLine($"   {regionCursor.GetValue("Region"),-15} {regionCursor.GetValue("TotalQuantity"),-15:N0}");
}
Console.WriteLine("   " + new string('-', 35));
Console.WriteLine();

// ============================================================================
// 8. Sample data preview
// ============================================================================
Console.WriteLine("8. Sample data (first 5 rows):");
Console.WriteLine("   " + new string('-', 90));
Console.WriteLine($"   {"OrderId",-12} {"Date",-12} {"Product",-18} {"Qty",-5} {"Price",-10} {"Region",-10}");
Console.WriteLine("   " + new string('-', 90));

var sampleCursor = velocityBlock.GetRowCursor("OrderId", "Date", "Product", "Quantity", "UnitPrice", "Region");
int count = 0;
while (sampleCursor.MoveNext() && count < 5)
{
    var product = sampleCursor.GetValue("Product")?.ToString() ?? "";
    if (product.Length > 16) product = product.Substring(0, 16) + "..";
    Console.WriteLine($"   {sampleCursor.GetValue("OrderId"),-12} {sampleCursor.GetValue("Date"),-12} {product,-18} {sampleCursor.GetValue("Quantity"),-5} {sampleCursor.GetValue("UnitPrice"),-10:C} {sampleCursor.GetValue("Region"),-10}");
    count++;
}
Console.WriteLine("   " + new string('-', 90));
Console.WriteLine();

// ============================================================================
// 9. Batch size recommendations
// ============================================================================
Console.WriteLine("9. Batch size recommendations:");
Console.WriteLine("   ┌─────────────────────┬──────────────────────────────────────┐");
Console.WriteLine("   │ Data Characteristics│ Recommended Batch Size               │");
Console.WriteLine("   ├─────────────────────┼──────────────────────────────────────┤");
Console.WriteLine("   │ Narrow rows (<10)   │ 50,000 - 100,000 rows                │");
Console.WriteLine("   │ Medium rows (10-50) │ 10,000 - 25,000 rows                 │");
Console.WriteLine("   │ Wide rows (50+)     │ 5,000 - 10,000 rows                  │");
Console.WriteLine("   │ Limited memory      │ 1,000 - 5,000 rows                   │");
Console.WriteLine("   └─────────────────────┴──────────────────────────────────────┘");
Console.WriteLine();

// ============================================================================
// Summary
// ============================================================================
Console.WriteLine("=== Summary ===");
Console.WriteLine($"   CSV rows processed: {lineCount:N0}");
Console.WriteLine($"   VelocityDataBlock rows: {velocityBlock.RowCount:N0}");
Console.WriteLine($"   Processing time: {stopwatch.ElapsedMilliseconds:N0} ms");
Console.WriteLine($"   Storage location: {velocityPath}");
Console.WriteLine();
Console.WriteLine("   Benefits of streaming to VelocityDataBlock:");
Console.WriteLine("   - Memory efficient (processes in batches)");
Console.WriteLine("   - Disk-backed storage (handles large datasets)");
Console.WriteLine("   - SIMD-accelerated queries (10-30x faster)");
Console.WriteLine("   - Persistent storage (data survives restarts)");

Console.WriteLine("\n=== Sample Complete ===");
