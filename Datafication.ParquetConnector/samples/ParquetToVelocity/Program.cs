using System.Diagnostics;
using Datafication.Core.Data;
using Datafication.Connectors.ParquetConnector;
using Datafication.Storage.Velocity;

Console.WriteLine("=== Datafication.ParquetConnector to VelocityDataBlock Sample ===\n");

// Get paths
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var parquetPath = Path.GetFullPath(Path.Combine(dataPath, "SalesRecords50k.parquet"));
var velocityPath = Path.Combine(Path.GetTempPath(), "parquet_velocity_sample");

// Clean up previous runs
if (Directory.Exists(velocityPath))
{
    Directory.Delete(velocityPath, recursive: true);
}
Directory.CreateDirectory(velocityPath);

// ============================================================================
// 1. Check source Parquet file
// ============================================================================
Console.WriteLine("1. Source Parquet file information...");
var fileInfo = new FileInfo(parquetPath);
Console.WriteLine($"   File: {fileInfo.Name}");
Console.WriteLine($"   Size: {fileInfo.Length:N0} bytes\n");

// ============================================================================
// 2. Configure Parquet connector
// ============================================================================
Console.WriteLine("2. Configuring Parquet connector...");

var parquetConfig = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath),
    Id = "sales-parquet-connector"
};

var parquetConnector = new ParquetDataConnector(parquetConfig);
Console.WriteLine($"   Connector ID: {parquetConnector.GetConnectorId()}\n");

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
// 4. Stream Parquet data to VelocityDataBlock
// ============================================================================
Console.WriteLine("4. Streaming Parquet to VelocityDataBlock...");
Console.WriteLine("   (Using batch processing for memory efficiency)\n");

var stopwatch = Stopwatch.StartNew();

// Stream with batch size of 10000 rows
const int batchSize = 10000;
await parquetConnector.GetStorageDataAsync(velocityBlock, batchSize);
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
// 6. Query the VelocityDataBlock (SIMD-accelerated)
// ============================================================================
Console.WriteLine("6. Querying VelocityDataBlock (SIMD-accelerated)...");

// Query: Get sales from "Europe" region
var europeSales = velocityBlock
    .Where("Region", "Europe")
    .Execute();

Console.WriteLine($"   Europe region sales: {europeSales.RowCount:N0} orders\n");

// ============================================================================
// 7. Aggregation example
// ============================================================================
Console.WriteLine("7. Aggregation: Total revenue by region...");

var regionStats = velocityBlock
    .GroupByAggregate("Region", "TotalRevenue", AggregationType.Sum, "SumRevenue")
    .Execute();

Console.WriteLine("   " + new string('-', 45));
Console.WriteLine($"   {"Region",-25} {"Total Revenue",-18}");
Console.WriteLine("   " + new string('-', 45));

var regionCursor = regionStats.GetRowCursor("Region", "SumRevenue");
while (regionCursor.MoveNext())
{
    var region = regionCursor.GetValue("Region");
    var revenue = regionCursor.GetValue("SumRevenue");
    Console.WriteLine($"   {region,-25} {revenue,-18:C0}");
}
Console.WriteLine("   " + new string('-', 45));
Console.WriteLine();

// ============================================================================
// 8. Sample data preview
// ============================================================================
Console.WriteLine("8. Sample data (first 5 rows):");
Console.WriteLine("   " + new string('-', 85));
Console.WriteLine($"   {"Region",-15} {"Country",-20} {"ItemType",-15} {"UnitsSold",-10} {"TotalRevenue",-15}");
Console.WriteLine("   " + new string('-', 85));

var sampleCursor = velocityBlock.GetRowCursor("Region", "Country", "ItemType", "UnitsSold", "TotalRevenue");
int count = 0;
while (sampleCursor.MoveNext() && count < 5)
{
    var region = sampleCursor.GetValue("Region");
    var country = sampleCursor.GetValue("Country");
    var itemType = sampleCursor.GetValue("ItemType");
    var unitsSold = sampleCursor.GetValue("UnitsSold");
    var revenue = sampleCursor.GetValue("TotalRevenue");
    Console.WriteLine($"   {region,-15} {country,-20} {itemType,-15} {unitsSold,-10} {revenue,-15:C0}");
    count++;
}
Console.WriteLine("   " + new string('-', 85));
Console.WriteLine();

// ============================================================================
// 9. Batch size recommendations
// ============================================================================
Console.WriteLine("9. Batch size recommendations:");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   Data Characteristics     | Recommended Batch Size");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   Narrow rows (<10 cols)   | 50,000 - 100,000 rows");
Console.WriteLine("   Medium rows (10-50 cols) | 10,000 - 25,000 rows");
Console.WriteLine("   Wide rows (50+ cols)     | 5,000 - 10,000 rows");
Console.WriteLine("   Limited memory           | 1,000 - 5,000 rows");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine();

// ============================================================================
// Summary
// ============================================================================
Console.WriteLine("=== Summary ===");
Console.WriteLine($"   Parquet rows processed: {velocityBlock.RowCount:N0}");
Console.WriteLine($"   Processing time: {stopwatch.ElapsedMilliseconds:N0} ms");
Console.WriteLine($"   Storage location: {velocityPath}");
Console.WriteLine();
Console.WriteLine("   Benefits of streaming Parquet to VelocityDataBlock:");
Console.WriteLine("   - Memory efficient (processes in batches)");
Console.WriteLine("   - Disk-backed storage (handles large datasets)");
Console.WriteLine("   - SIMD-accelerated queries (10-30x faster)");
Console.WriteLine("   - Persistent storage (data survives restarts)");
Console.WriteLine("   - Combines Parquet's columnar format with Velocity's speed");

Console.WriteLine("\n=== Sample Complete ===");
