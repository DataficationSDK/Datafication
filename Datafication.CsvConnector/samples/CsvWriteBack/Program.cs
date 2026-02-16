using Datafication.Core.Data;
using Datafication.Extensions.Connectors.CsvConnector;
using Datafication.Sinks.Connectors.CsvConnector;

Console.WriteLine("=== Datafication.CsvConnector Write Back Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var csvPath = Path.Combine(dataPath, "employees.csv");

// ============================================================================
// 1. Load original CSV data
// ============================================================================
Console.WriteLine("1. Loading original CSV data...");
var employees = await DataBlock.Connector.LoadCsvAsync(csvPath);
Console.WriteLine($"   Loaded {employees.RowCount} employees\n");

// ============================================================================
// 2. Transform the data
// ============================================================================
Console.WriteLine("2. Transforming data...");

// Filter to Engineering department and add computed columns
var transformed = employees
    .Where("Department", "Engineering")
    .Compute("AnnualBonus", "Salary * 0.15")
    .Compute("TotalCompensation", "Salary + (Salary * 0.15)")
    .Select("Id", "Name", "Salary", "AnnualBonus", "TotalCompensation", "Email");

Console.WriteLine($"   Filtered to Engineering: {transformed.RowCount} employees");
Console.WriteLine("   Added computed columns: AnnualBonus, TotalCompensation\n");

// ============================================================================
// 3. Convert to CSV string (async)
// ============================================================================
Console.WriteLine("3. Converting to CSV string (async)...");
var csvStringAsync = await transformed.CsvStringSinkAsync();

Console.WriteLine("   First 500 characters of output:");
Console.WriteLine("   " + new string('-', 60));
var preview = csvStringAsync.Length > 500 ? csvStringAsync.Substring(0, 500) + "..." : csvStringAsync;
foreach (var line in preview.Split('\n').Take(8))
{
    Console.WriteLine($"   {line}");
}
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine();

// ============================================================================
// 4. Convert to CSV string (sync)
// ============================================================================
Console.WriteLine("4. Converting to CSV string (sync)...");
var csvStringSync = transformed.CsvStringSink();
Console.WriteLine($"   Generated {csvStringSync.Length} characters\n");

// ============================================================================
// 5. Write to file
// ============================================================================
Console.WriteLine("5. Writing transformed data to file...");
var outputPath = Path.Combine(Path.GetTempPath(), "engineering_compensation.csv");
await File.WriteAllTextAsync(outputPath, csvStringAsync);
Console.WriteLine($"   Written to: {outputPath}");

// Verify the file
var fileInfo = new FileInfo(outputPath);
Console.WriteLine($"   File size: {fileInfo.Length} bytes\n");

// ============================================================================
// 6. Roundtrip verification
// ============================================================================
Console.WriteLine("6. Roundtrip verification (load the exported file)...");
var reloaded = await DataBlock.Connector.LoadCsvAsync(outputPath);
Console.WriteLine($"   Reloaded {reloaded.RowCount} rows with {reloaded.Schema.Count} columns");
Console.WriteLine("   Columns: " + string.Join(", ", reloaded.Schema.GetColumnNames()));
Console.WriteLine();

// Display reloaded data
Console.WriteLine("   First 5 rows from reloaded file:");
Console.WriteLine("   " + new string('-', 85));
Console.WriteLine($"   {"Id",-5} {"Name",-20} {"Salary",-12} {"Bonus",-12} {"Total",-12}");
Console.WriteLine("   " + new string('-', 85));

var cursor = reloaded.GetRowCursor("Id", "Name", "Salary", "AnnualBonus", "TotalCompensation");
int count = 0;
while (cursor.MoveNext() && count < 5)
{
    Console.WriteLine($"   {cursor.GetValue("Id"),-5} {cursor.GetValue("Name"),-20} {cursor.GetValue("Salary"),-12:C0} {cursor.GetValue("AnnualBonus"),-12:C0} {cursor.GetValue("TotalCompensation"),-12:C0}");
    count++;
}
Console.WriteLine("   " + new string('-', 85));
Console.WriteLine();

// ============================================================================
// 7. ETL Pipeline example: Load -> Transform -> Export
// ============================================================================
Console.WriteLine("7. Complete ETL Pipeline example...");

// Extract
var rawData = await DataBlock.Connector.LoadCsvAsync(csvPath);

// Transform: Get top 10 earners with department stats
var topEarners = rawData
    .Sort(SortDirection.Descending, "Salary")
    .Head(10)
    .Select("Name", "Department", "Salary");

// Load (export)
var topEarnersCsv = await topEarners.CsvStringSinkAsync();
var topEarnersPath = Path.Combine(Path.GetTempPath(), "top_earners.csv");
await File.WriteAllTextAsync(topEarnersPath, topEarnersCsv);

Console.WriteLine($"   ETL complete: Top 10 earners exported to {topEarnersPath}\n");

// ============================================================================
// Cleanup
// ============================================================================
Console.WriteLine("=== Summary ===");
Console.WriteLine($"   Original records: {employees.RowCount}");
Console.WriteLine($"   Transformed records: {transformed.RowCount}");
Console.WriteLine($"   Output files created:");
Console.WriteLine($"   - {outputPath}");
Console.WriteLine($"   - {topEarnersPath}");

Console.WriteLine("\n=== Sample Complete ===");
