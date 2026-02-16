using System.Diagnostics;
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ParquetConnector;
using Datafication.Sinks.Connectors.ParquetConnector;
using Parquet;

Console.WriteLine("=== Datafication.ParquetConnector Compression Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var parquetPath = Path.GetFullPath(Path.Combine(dataPath, "SalesRecords50k.parquet"));
var outputPath = Path.Combine(Path.GetTempPath(), "ParquetCompressionSample");
Directory.CreateDirectory(outputPath);

// 1. Load source data
Console.WriteLine("1. Loading source data...");
var salesData = await DataBlock.Connector.LoadParquetAsync(new Uri("file://" + parquetPath));
salesData.RemoveColumn("RowGroup"); // Remove auto-added column for clean export
Console.WriteLine($"   Loaded {salesData.RowCount:N0} rows with {salesData.Schema.Count} columns\n");

// 2. Export with Snappy compression (default)
Console.WriteLine("2. Exporting with Snappy compression (default)...");
var swSnappy = Stopwatch.StartNew();
var snappyBytes = await salesData.ParquetSinkAsync(CompressionMethod.Snappy);
swSnappy.Stop();
Console.WriteLine($"   Size: {snappyBytes.Length:N0} bytes");
Console.WriteLine($"   Time: {swSnappy.ElapsedMilliseconds} ms\n");

// 3. Export with Gzip compression
Console.WriteLine("3. Exporting with Gzip compression...");
var swGzip = Stopwatch.StartNew();
var gzipBytes = await salesData.ParquetSinkAsync(CompressionMethod.Gzip);
swGzip.Stop();
Console.WriteLine($"   Size: {gzipBytes.Length:N0} bytes");
Console.WriteLine($"   Time: {swGzip.ElapsedMilliseconds} ms\n");

// 4. Export with no compression
Console.WriteLine("4. Exporting with no compression...");
var swNone = Stopwatch.StartNew();
var noneBytes = await salesData.ParquetSinkAsync(CompressionMethod.None);
swNone.Stop();
Console.WriteLine($"   Size: {noneBytes.Length:N0} bytes");
Console.WriteLine($"   Time: {swNone.ElapsedMilliseconds} ms\n");

// 5. Comparison table
Console.WriteLine("5. Compression Comparison:");
Console.WriteLine("   " + new string('-', 75));
Console.WriteLine($"   {"Method",-12} {"Size (bytes)",-18} {"Time (ms)",-12} {"Ratio",-12} {"Notes",-20}");
Console.WriteLine("   " + new string('-', 75));

var baseSize = (double)noneBytes.Length;
Console.WriteLine($"   {"None",-12} {noneBytes.Length,-18:N0} {swNone.ElapsedMilliseconds,-12} {"1.00x",-12} {"Fastest export",-20}");
Console.WriteLine($"   {"Snappy",-12} {snappyBytes.Length,-18:N0} {swSnappy.ElapsedMilliseconds,-12} {(snappyBytes.Length / baseSize):F2}x{"",-7} {"Best balance",-20}");
Console.WriteLine($"   {"Gzip",-12} {gzipBytes.Length,-18:N0} {swGzip.ElapsedMilliseconds,-12} {(gzipBytes.Length / baseSize):F2}x{"",-7} {"Smallest size",-20}");
Console.WriteLine("   " + new string('-', 75));

var spaceSavedSnappy = (1 - (double)snappyBytes.Length / noneBytes.Length) * 100;
var spaceSavedGzip = (1 - (double)gzipBytes.Length / noneBytes.Length) * 100;
Console.WriteLine($"\n   Space saved with Snappy: {spaceSavedSnappy:F1}%");
Console.WriteLine($"   Space saved with Gzip: {spaceSavedGzip:F1}%\n");

// 6. Save files for verification
Console.WriteLine("6. Saving compressed files...");
var snappyPath = Path.Combine(outputPath, "sales_snappy.parquet");
var gzipPath = Path.Combine(outputPath, "sales_gzip.parquet");
var nonePath = Path.Combine(outputPath, "sales_none.parquet");

await File.WriteAllBytesAsync(snappyPath, snappyBytes);
await File.WriteAllBytesAsync(gzipPath, gzipBytes);
await File.WriteAllBytesAsync(nonePath, noneBytes);

Console.WriteLine($"   Snappy: {snappyPath}");
Console.WriteLine($"   Gzip: {gzipPath}");
Console.WriteLine($"   None: {nonePath}\n");

// 7. Verify all exports can be re-loaded
Console.WriteLine("7. Verifying all exports can be re-loaded...");

async Task<bool> VerifyExport(string path, string method)
{
    try
    {
        var data = await DataBlock.Connector.LoadParquetAsync(new Uri("file://" + path));
        var originalRowCount = salesData.RowCount;
        var loadedRowCount = data.RowCount;
        var match = originalRowCount == loadedRowCount;
        Console.WriteLine($"   {method,-10}: {loadedRowCount:N0} rows loaded - {(match ? "OK" : "MISMATCH")}");
        return match;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   {method,-10}: FAILED - {ex.Message}");
        return false;
    }
}

var allValid = await VerifyExport(snappyPath, "Snappy");
allValid &= await VerifyExport(gzipPath, "Gzip");
allValid &= await VerifyExport(nonePath, "None");

Console.WriteLine($"\n   All exports valid: {(allValid ? "Yes" : "No")}\n");

// 8. Recommendations
Console.WriteLine("8. Compression Recommendations:");
Console.WriteLine("   " + new string('-', 70));
Console.WriteLine("   Use Case                          | Recommended Method");
Console.WriteLine("   " + new string('-', 70));
Console.WriteLine("   General purpose / Default         | Snappy");
Console.WriteLine("   Maximum compression               | Gzip");
Console.WriteLine("   Fastest export speed              | None");
Console.WriteLine("   Network transfer                  | Gzip (smaller = faster transfer)");
Console.WriteLine("   Local storage with SSD            | Snappy (fast decompress)");
Console.WriteLine("   Archival / Long-term storage      | Gzip (best compression ratio)");
Console.WriteLine("   Real-time processing              | None or Snappy");
Console.WriteLine("   " + new string('-', 70));

// 9. Using sync API with compression
Console.WriteLine("\n9. Sync API with compression:");
var syncSnappy = salesData.ParquetSink(CompressionMethod.Snappy);
var syncGzip = salesData.ParquetSink(CompressionMethod.Gzip);
Console.WriteLine($"   Sync Snappy: {syncSnappy.Length:N0} bytes");
Console.WriteLine($"   Sync Gzip: {syncGzip.Length:N0} bytes");

Console.WriteLine($"\n   Temporary files saved to: {outputPath}");

Console.WriteLine("\n=== Sample Complete ===");
