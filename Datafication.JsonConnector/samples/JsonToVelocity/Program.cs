using System.Diagnostics;
using Datafication.Core.Data;
using Datafication.Connectors.JsonConnector;
using Datafication.Extensions.Connectors.JsonConnector;
using Datafication.Storage.Velocity;

Console.WriteLine("=== Datafication.JsonConnector to VelocityDataBlock Sample ===\n");

// Get paths
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var jsonPath = Path.Combine(dataPath, "sales_large.json");
var velocityPath = Path.Combine(Path.GetTempPath(), "velocity_json_samples");

// Clean up previous runs
if (Directory.Exists(velocityPath))
{
    Directory.Delete(velocityPath, recursive: true);
}
Directory.CreateDirectory(velocityPath);

// ============================================================================
// 1. Check source JSON file
// ============================================================================
Console.WriteLine("1. Source JSON file information...");
var fileInfo = new FileInfo(jsonPath);
Console.WriteLine($"   File: {fileInfo.Name}");
Console.WriteLine($"   Size: {fileInfo.Length:N0} bytes\n");

// ============================================================================
// 2. Baseline: Load JSON to in-memory DataBlock
// ============================================================================
Console.WriteLine("2. Baseline: Loading JSON to DataBlock (in-memory)...");
var stopwatch = Stopwatch.StartNew();
var memoryData = await DataBlock.Connector.LoadJsonAsync(jsonPath);
stopwatch.Stop();
var loadTime = stopwatch.ElapsedMilliseconds;
Console.WriteLine($"   Loaded {memoryData.RowCount} rows to memory in {loadTime}ms");
Console.WriteLine($"   Schema columns: {string.Join(", ", memoryData.Schema.GetColumnNames())}\n");

// ============================================================================
// 3. Configure JSON connector for streaming
// ============================================================================
Console.WriteLine("3. Configuring JSON connector...");

var jsonConfig = new JsonConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(jsonPath)),
    Id = "sales-json-connector"
};

var jsonConnector = new JsonDataConnector(jsonConfig);
Console.WriteLine($"   Connector ID: {jsonConnector.GetConnectorId()}\n");

// ============================================================================
// 4. Create VelocityDataBlock for high-performance storage
// ============================================================================
Console.WriteLine("4. Creating VelocityDataBlock...");

var dfcPath = Path.Combine(velocityPath, "sales.dfc");
var velocityOptions = VelocityOptions.CreateHighThroughput();

using var velocityBlock = new VelocityDataBlock(dfcPath, velocityOptions);
Console.WriteLine($"   Storage path: {dfcPath}");
Console.WriteLine($"   Options: High Throughput mode\n");

// ============================================================================
// 5. Stream JSON data to VelocityDataBlock
// ============================================================================
Console.WriteLine("5. Streaming JSON to VelocityDataBlock...");
Console.WriteLine("   (Using GetStorageDataAsync for batch processing)\n");

stopwatch.Restart();

// Stream with batch size - schema is auto-detected from JSON
const int batchSize = 500;
await jsonConnector.GetStorageDataAsync(velocityBlock, batchSize);
await velocityBlock.FlushAsync();

stopwatch.Stop();

Console.WriteLine($"   Batch size: {batchSize:N0} rows");
Console.WriteLine($"   Total rows loaded: {velocityBlock.RowCount:N0}");
Console.WriteLine($"   Load time: {stopwatch.ElapsedMilliseconds:N0} ms");
Console.WriteLine($"   Throughput: {velocityBlock.RowCount / Math.Max(stopwatch.Elapsed.TotalSeconds, 0.001):N0} rows/sec\n");

// ============================================================================
// 6. Storage statistics
// ============================================================================
Console.WriteLine("6. Storage statistics...");

var stats = await velocityBlock.GetStorageStatsAsync();
Console.WriteLine($"   Total rows: {stats.TotalRows:N0}");
Console.WriteLine($"   Active rows: {stats.ActiveRows:N0}");
Console.WriteLine($"   Storage files: {stats.StorageFiles}");

// Compare file sizes
var dfcFileInfo = new FileInfo(dfcPath);
Console.WriteLine($"   JSON file size: {fileInfo.Length:N0} bytes");
Console.WriteLine($"   DFC file size:  {dfcFileInfo.Length:N0} bytes");
if (dfcFileInfo.Length > 0)
{
    Console.WriteLine($"   Compression:    {(double)dfcFileInfo.Length / fileInfo.Length:P1}");
}
Console.WriteLine();

// ============================================================================
// 7. Query the VelocityDataBlock (SIMD-accelerated)
// ============================================================================
Console.WriteLine("7. Querying VelocityDataBlock (SIMD-accelerated)...");

// Filter by region
stopwatch.Restart();
var northSales = velocityBlock.Where("Region", "North").Execute();
stopwatch.Stop();
Console.WriteLine($"   North region sales: {northSales.RowCount:N0} rows (query: {stopwatch.ElapsedMilliseconds}ms)");

// Filter by another region
stopwatch.Restart();
var westSales = velocityBlock.Where("Region", "West").Execute();
stopwatch.Stop();
Console.WriteLine($"   West region sales:  {westSales.RowCount:N0} rows (query: {stopwatch.ElapsedMilliseconds}ms)");

// Sort by price
stopwatch.Restart();
var topByPrice = velocityBlock.Sort(SortDirection.Descending, "UnitPrice").Head(5).Execute();
stopwatch.Stop();
Console.WriteLine($"   Top 5 by price: {topByPrice.RowCount} rows (sort: {stopwatch.ElapsedMilliseconds}ms)\n");

// ============================================================================
// 8. Aggregation example
// ============================================================================
Console.WriteLine("8. Aggregation: Sales by region...");

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
// 9. Sample data preview
// ============================================================================
Console.WriteLine("9. Sample data (first 5 rows):");
Console.WriteLine("   " + new string('-', 95));
Console.WriteLine($"   {"TxnId",-8} {"Date",-12} {"Product",-10} {"Qty",-5} {"Price",-10} {"Customer",-10} {"Region",-8} {"Payment",-15}");
Console.WriteLine("   " + new string('-', 95));

var sampleCursor = velocityBlock.GetRowCursor("TransactionId", "Date", "ProductId", "Quantity", "UnitPrice", "CustomerId", "Region", "PaymentMethod");
int count = 0;
while (sampleCursor.MoveNext() && count < 5)
{
    Console.WriteLine($"   {sampleCursor.GetValue("TransactionId"),-8} {sampleCursor.GetValue("Date"),-12} {sampleCursor.GetValue("ProductId"),-10} {sampleCursor.GetValue("Quantity"),-5} {sampleCursor.GetValue("UnitPrice"),-10:C2} {sampleCursor.GetValue("CustomerId"),-10} {sampleCursor.GetValue("Region"),-8} {sampleCursor.GetValue("PaymentMethod"),-15}");
    count++;
}
Console.WriteLine("   " + new string('-', 95));
Console.WriteLine();

// ============================================================================
// 10. Batch size recommendations
// ============================================================================
Console.WriteLine("10. Batch size recommendations:");
Console.WriteLine("   ┌─────────────────────────┬────────────────────────────────────┐");
Console.WriteLine("   │ Data Characteristics    │ Recommended Batch Size             │");
Console.WriteLine("   ├─────────────────────────┼────────────────────────────────────┤");
Console.WriteLine("   │ Small JSON files        │ 1,000 - 5,000 rows                 │");
Console.WriteLine("   │ Medium JSON files       │ 5,000 - 10,000 rows                │");
Console.WriteLine("   │ Large JSON files        │ 10,000 - 50,000 rows               │");
Console.WriteLine("   │ Limited memory          │ 500 - 2,000 rows                   │");
Console.WriteLine("   └─────────────────────────┴────────────────────────────────────┘");
Console.WriteLine();

// ============================================================================
// 11. Data integrity check
// ============================================================================
Console.WriteLine("11. Data Integrity Check:");
Console.WriteLine($"    Original JSON rows:     {memoryData.RowCount}");
Console.WriteLine($"    VelocityDataBlock rows: {velocityBlock.RowCount}");
Console.WriteLine($"    Match: {memoryData.RowCount == velocityBlock.RowCount}");
Console.WriteLine();

// ============================================================================
// Summary
// ============================================================================
Console.WriteLine("=== Summary ===");
Console.WriteLine($"   JSON file: {Path.GetFileName(jsonPath)}");
Console.WriteLine($"   JSON size: {fileInfo.Length:N0} bytes");
Console.WriteLine($"   DFC size:  {dfcFileInfo.Length:N0} bytes");
Console.WriteLine($"   Final rows: {velocityBlock.RowCount:N0}");
Console.WriteLine();
Console.WriteLine("   Benefits of streaming JSON to VelocityDataBlock:");
Console.WriteLine("   - Memory efficient (processes in batches)");
Console.WriteLine("   - Disk-backed storage (handles datasets larger than memory)");
Console.WriteLine("   - SIMD-accelerated queries (10-30x faster filtering)");
Console.WriteLine("   - Automatic compression (typically 50-70% size reduction)");
Console.WriteLine("   - Persistent storage (data survives application restarts)");

// Cleanup
velocityBlock.Dispose();
Directory.Delete(velocityPath, recursive: true);

Console.WriteLine("\n=== Sample Complete ===");
