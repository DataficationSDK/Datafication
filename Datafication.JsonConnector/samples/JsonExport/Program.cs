using Datafication.Core.Data;
using Datafication.Extensions.Connectors.JsonConnector;
using Datafication.Sinks.Connectors.JsonConnector;

Console.WriteLine("=== Datafication.JsonConnector Export Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var jsonPath = Path.Combine(dataPath, "employees.json");

// 1. Load source data
Console.WriteLine("1. Loading source JSON data...");
var employees = await DataBlock.Connector.LoadJsonAsync(jsonPath);
Console.WriteLine($"   Loaded {employees.RowCount} employees\n");

// 2. Export full dataset asynchronously
Console.WriteLine("2. Exporting full dataset to JSON (async)...");
var fullJson = await employees.JsonStringSinkAsync();
Console.WriteLine($"   Generated JSON with {fullJson.Length:N0} characters");
Console.WriteLine($"   Preview (first 200 chars): {fullJson.Substring(0, Math.Min(200, fullJson.Length))}...\n");

// 3. Export single record (produces JSON object, not array)
Console.WriteLine("3. Exporting single record...");
var singleEmployee = employees.Head(1);
var singleJson = singleEmployee.JsonStringSink();
Console.WriteLine($"   Single record JSON:\n{singleJson}\n");

// 4. Transform then export workflow
Console.WriteLine("4. Transform then export: Engineering department only...");
var engineersOnly = employees
    .Where("Department", "Engineering")
    .Sort(SortDirection.Descending, "Salary");

var engineersJson = await engineersOnly.JsonStringSinkAsync();
Console.WriteLine($"   Exported {engineersOnly.RowCount} engineers");
Console.WriteLine($"   JSON length: {engineersJson.Length:N0} characters\n");

// 5. Select specific columns before export
Console.WriteLine("5. Export with column selection...");
var summary = employees
    .Select("Name", "Department", "Salary")
    .Head(5);

var summaryJson = summary.JsonStringSink();
Console.WriteLine($"   Summary JSON (5 employees, 3 columns):\n{summaryJson}\n");

// 6. Roundtrip demonstration: Load -> Modify -> Export
Console.WriteLine("6. Roundtrip: Load -> Filter -> Sort -> Export...");
var topEarners = employees
    .Sort(SortDirection.Descending, "Salary")
    .Head(3)
    .Select("Name", "Department", "Salary");

var topEarnersJson = await topEarners.JsonStringSinkAsync();
Console.WriteLine($"   Top 3 earners:\n{topEarnersJson}\n");

// 7. Save to file (optional demonstration)
Console.WriteLine("7. Saving exported JSON to file...");
var outputPath = Path.Combine(dataPath, "output_employees.json");
await File.WriteAllTextAsync(outputPath, fullJson);
Console.WriteLine($"   Saved to: {outputPath}");

Console.WriteLine("\n=== Sample Complete ===");
