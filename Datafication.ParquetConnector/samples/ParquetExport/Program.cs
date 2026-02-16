using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ParquetConnector;
using Datafication.Sinks.Connectors.ParquetConnector;

Console.WriteLine("=== Datafication.ParquetConnector Export Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var parquetPath = Path.GetFullPath(Path.Combine(dataPath, "SalesRecords50k.parquet"));
var outputPath = Path.Combine(Path.GetTempPath(), "ParquetExportSample");
Directory.CreateDirectory(outputPath);

// 1. Create a sample DataBlock with various types
Console.WriteLine("1. Creating sample DataBlock with various types...");
var sampleData = new DataBlock();
sampleData.AddColumn(new DataColumn("Id", typeof(int)));
sampleData.AddColumn(new DataColumn("Name", typeof(string)));
sampleData.AddColumn(new DataColumn("Salary", typeof(double)));
sampleData.AddColumn(new DataColumn("StartDate", typeof(DateTime)));
sampleData.AddColumn(new DataColumn("IsActive", typeof(bool)));
sampleData.AddColumn(new DataColumn("Score", typeof(long)));

sampleData.AddRow(new object[] { 1, "Alice", 75000.50, new DateTime(2020, 1, 15), true, 95L });
sampleData.AddRow(new object[] { 2, "Bob", 82000.75, new DateTime(2019, 6, 1), true, 87L });
sampleData.AddRow(new object[] { 3, "Charlie", 68000.25, new DateTime(2021, 3, 10), false, 92L });
sampleData.AddRow(new object[] { 4, "Diana", 91000.00, new DateTime(2018, 11, 20), true, 88L });
sampleData.AddRow(new object[] { 5, "Eve", 73500.50, new DateTime(2022, 2, 5), true, 91L });

Console.WriteLine($"   Created DataBlock with {sampleData.RowCount} rows and {sampleData.Schema.Count} columns");
Console.WriteLine($"   Columns: {string.Join(", ", sampleData.Schema.GetColumnNames())}\n");

// 2. Basic async export
Console.WriteLine("2. Exporting to Parquet (async)...");
var asyncBytes = await sampleData.ParquetSinkAsync();
Console.WriteLine($"   Exported {asyncBytes.Length:N0} bytes\n");

// 3. Sync export
Console.WriteLine("3. Exporting to Parquet (sync)...");
var syncBytes = sampleData.ParquetSink();
Console.WriteLine($"   Exported {syncBytes.Length:N0} bytes\n");

// 4. Export with skipped columns tracking
Console.WriteLine("4. Export with skipped columns tracking...");

// Create DataBlock with a nested DataBlock (which will be skipped)
var nestedData = new DataBlock();
nestedData.AddColumn(new DataColumn("ParentId", typeof(int)));
nestedData.AddColumn(new DataColumn("Value", typeof(double)));
nestedData.AddRow(new object[] { 1, 100.0 });
nestedData.AddRow(new object[] { 2, 200.0 });
nestedData.AddRow(new object[] { 3, 300.0 });

// Add nested DataBlock column (will be skipped during export)
var parentData = new DataBlock();
parentData.AddColumn(new DataColumn("Id", typeof(int)));
parentData.AddColumn(new DataColumn("Name", typeof(string)));
parentData.AddColumn(new DataColumn("Details", typeof(DataBlock)));
parentData.AddRow(new object[] { 1, "Parent1", nestedData });
parentData.AddRow(new object[] { 2, "Parent2", nestedData });
parentData.AddRow(new object[] { 3, "Parent3", nestedData });

var (exportedBytes, skippedColumns) = await parentData.ParquetSinkWithSkippedColumnsAsync();
Console.WriteLine($"   Exported {exportedBytes.Length:N0} bytes");
Console.WriteLine($"   Skipped columns: {(skippedColumns.Count > 0 ? string.Join(", ", skippedColumns) : "None")}");
if (skippedColumns.Count > 0)
{
    Console.WriteLine("   Note: Nested DataBlock columns are not supported in Parquet format");
}
Console.WriteLine();

// 5. Save to file
Console.WriteLine("5. Saving to file...");
var exportFilePath = Path.Combine(outputPath, "employees_export.parquet");
await File.WriteAllBytesAsync(exportFilePath, asyncBytes);
Console.WriteLine($"   Saved to: {exportFilePath}");
Console.WriteLine($"   File size: {new FileInfo(exportFilePath).Length:N0} bytes\n");

// 6. Round-trip verification
Console.WriteLine("6. Round-trip verification (export then re-import)...");
var reloaded = await DataBlock.Connector.LoadParquetAsync(new Uri("file://" + exportFilePath));

// Remove RowGroup column for comparison
reloaded.RemoveColumn("RowGroup");

Console.WriteLine($"   Original rows: {sampleData.RowCount}");
Console.WriteLine($"   Reloaded rows: {reloaded.RowCount}");
Console.WriteLine($"   Original columns: {sampleData.Schema.Count}");
Console.WriteLine($"   Reloaded columns: {reloaded.Schema.Count}");

// Compare data
Console.WriteLine("   Data comparison:");
Console.WriteLine("   " + new string('-', 65));
Console.WriteLine($"   {"Id",-5} {"Name",-12} {"Salary",-12} {"IsActive",-10} {"Match",-8}");
Console.WriteLine("   " + new string('-', 65));

var origCursor = sampleData.GetRowCursor("Id", "Name", "Salary", "IsActive");
var reloadCursor = reloaded.GetRowCursor("Id", "Name", "Salary", "IsActive");
while (origCursor.MoveNext() && reloadCursor.MoveNext())
{
    var origId = origCursor.GetValue("Id");
    var reloadId = reloadCursor.GetValue("Id");
    var origName = origCursor.GetValue("Name");
    var reloadName = reloadCursor.GetValue("Name");
    var origSalary = origCursor.GetValue("Salary");
    var reloadSalary = reloadCursor.GetValue("Salary");
    var origActive = origCursor.GetValue("IsActive");
    var reloadActive = reloadCursor.GetValue("IsActive");

    var match = origId?.Equals(reloadId) == true &&
                origName?.Equals(reloadName) == true &&
                origSalary?.Equals(reloadSalary) == true &&
                origActive?.Equals(reloadActive) == true;

    Console.WriteLine($"   {origId,-5} {origName,-12} {origSalary,-12:N2} {origActive,-10} {(match ? "Yes" : "No"),-8}");
}
Console.WriteLine("   " + new string('-', 65));
Console.WriteLine();

// 7. Export large dataset
Console.WriteLine("7. Exporting larger dataset...");
var largeData = await DataBlock.Connector.LoadParquetAsync(new Uri("file://" + parquetPath));
largeData.RemoveColumn("RowGroup"); // Remove auto-added column

var largeExportBytes = await largeData.ParquetSinkAsync();
var largeExportPath = Path.Combine(outputPath, "sales_export.parquet");
await File.WriteAllBytesAsync(largeExportPath, largeExportBytes);

Console.WriteLine($"   Rows exported: {largeData.RowCount:N0}");
Console.WriteLine($"   File size: {largeExportBytes.Length:N0} bytes");
Console.WriteLine($"   Saved to: {largeExportPath}\n");

// 8. Supported types reference
Console.WriteLine("8. Supported Parquet Types:");
Console.WriteLine("   " + new string('-', 50));
Console.WriteLine($"   {"C# Type",-20} {"Parquet Type",-20}");
Console.WriteLine("   " + new string('-', 50));
Console.WriteLine($"   {"int / Int32",-20} {"INT32",-20}");
Console.WriteLine($"   {"long / Int64",-20} {"INT64",-20}");
Console.WriteLine($"   {"float",-20} {"FLOAT",-20}");
Console.WriteLine($"   {"double",-20} {"DOUBLE",-20}");
Console.WriteLine($"   {"decimal",-20} {"DECIMAL",-20}");
Console.WriteLine($"   {"bool",-20} {"BOOLEAN",-20}");
Console.WriteLine($"   {"string",-20} {"BYTE_ARRAY (UTF8)",-20}");
Console.WriteLine($"   {"DateTime",-20} {"INT64 (TIMESTAMP)",-20}");
Console.WriteLine($"   {"byte[]",-20} {"BYTE_ARRAY",-20}");
Console.WriteLine("   " + new string('-', 50));
Console.WriteLine("   Note: Nested DataBlock columns are automatically skipped");

// Cleanup
Console.WriteLine($"\n   Temporary files saved to: {outputPath}");

Console.WriteLine("\n=== Sample Complete ===");
