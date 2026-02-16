using Datafication.Core.Data;
using Datafication.Connectors.CsvConnector;
using Datafication.Extensions.Connectors.CsvConnector;

Console.WriteLine("=== Datafication.CsvConnector No Headers Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var csvPath = Path.Combine(dataPath, "raw_numbers.csv");

// ============================================================================
// 1. Loading CSV without headers (auto-generated column names)
// ============================================================================
Console.WriteLine("1. Loading CSV without header row...");
Console.WriteLine($"   File: raw_numbers.csv\n");

var noHeaderConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(csvPath)),
    HeaderRow = false  // No header row - columns will be auto-named
};

var connector = new CsvDataConnector(noHeaderConfig);
var data = await connector.GetDataAsync();

Console.WriteLine($"   Loaded {data.RowCount} rows with {data.Schema.Count} columns");
Console.WriteLine("   Auto-generated column names:");
foreach (var colName in data.Schema.GetColumnNames())
{
    var col = data.GetColumn(colName);
    Console.WriteLine($"   - {colName}: {col.DataType.GetClrType().Name}");
}
Console.WriteLine();

// ============================================================================
// 2. Display data with auto-generated column names
// ============================================================================
Console.WriteLine("2. First 5 rows with auto-generated column names:");
Console.WriteLine("   " + new string('-', 50));
Console.WriteLine($"   {"Column_1",-10} {"Column_2",-10} {"Column_3",-10} {"Column_4",-10}");
Console.WriteLine("   " + new string('-', 50));

var cursor = data.GetRowCursor("Column_1", "Column_2", "Column_3", "Column_4");
int count = 0;
while (cursor.MoveNext() && count < 5)
{
    Console.WriteLine($"   {cursor.GetValue("Column_1"),-10} {cursor.GetValue("Column_2"),-10} {cursor.GetValue("Column_3"),-10} {cursor.GetValue("Column_4"),-10}");
    count++;
}
Console.WriteLine();

// ============================================================================
// 3. Working with auto-generated column names
// ============================================================================
Console.WriteLine("3. Filtering and calculations with auto-generated names...");

// Filter where Column_4 is true
var filtered = data.Where("Column_4", true);
Console.WriteLine($"   Rows where Column_4 is true: {filtered.RowCount}");

// Compute a new column based on existing auto-named columns
var computed = data.Compute("Calculated", "Column_2 * Column_3");
Console.WriteLine($"   Added computed column 'Calculated' = Column_2 * Column_3");
Console.WriteLine();

// Display computed results
Console.WriteLine("   First 5 rows with calculated column:");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine($"   {"Column_1",-10} {"Column_2",-10} {"Column_3",-10} {"Calculated",-15}");
Console.WriteLine("   " + new string('-', 60));

var compCursor = computed.GetRowCursor("Column_1", "Column_2", "Column_3", "Calculated");
count = 0;
while (compCursor.MoveNext() && count < 5)
{
    Console.WriteLine($"   {compCursor.GetValue("Column_1"),-10} {compCursor.GetValue("Column_2"),-10} {compCursor.GetValue("Column_3"),-10} {compCursor.GetValue("Calculated"),-15:F2}");
    count++;
}
Console.WriteLine();

// ============================================================================
// 4. Comparison: Same file WITH headers (incorrect interpretation)
// ============================================================================
Console.WriteLine("4. Comparison: Loading same file WITH HeaderRow=true...");
Console.WriteLine("   (First row would be incorrectly treated as headers)\n");

var withHeaderConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(csvPath)),
    HeaderRow = true  // Incorrectly treating first row as headers
};

var connectorWithHeaders = new CsvDataConnector(withHeaderConfig);
var dataWithHeaders = await connectorWithHeaders.GetDataAsync();

Console.WriteLine($"   Row count: {dataWithHeaders.RowCount} (one less - first row became headers!)");
Console.WriteLine("   Column names (from first data row - incorrect!):");
foreach (var colName in dataWithHeaders.Schema.GetColumnNames())
{
    Console.WriteLine($"   - {colName}");
}
Console.WriteLine();

// ============================================================================
// Summary
// ============================================================================
Console.WriteLine("=== Summary ===");
Console.WriteLine("   HeaderRow=false:");
Console.WriteLine($"     - {data.RowCount} rows loaded");
Console.WriteLine("     - Columns named: Column_1, Column_2, Column_3, Column_4");
Console.WriteLine();
Console.WriteLine("   HeaderRow=true (incorrect for this file):");
Console.WriteLine($"     - {dataWithHeaders.RowCount} rows loaded (first row lost)");
Console.WriteLine("     - First row values became column names");

Console.WriteLine("\n=== Sample Complete ===");
