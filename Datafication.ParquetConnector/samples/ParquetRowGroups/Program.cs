using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ParquetConnector;

Console.WriteLine("=== Datafication.ParquetConnector Row Groups Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var parquetPath = Path.GetFullPath(Path.Combine(dataPath, "SalesRecords50k.parquet"));

// 1. Load Parquet file (RowGroup column is automatically added)
Console.WriteLine("1. Loading Parquet file...");
var salesData = await DataBlock.Connector.LoadParquetAsync(new Uri("file://" + parquetPath));
Console.WriteLine($"   Loaded {salesData.RowCount} rows with {salesData.Schema.Count} columns\n");

// 2. Explain RowGroup column
Console.WriteLine("2. Understanding the RowGroup column:");
Console.WriteLine("   The ParquetConnector automatically adds a 'RowGroup' column to track");
Console.WriteLine("   which Parquet row group each row originated from. This is useful for:");
Console.WriteLine("   - Understanding data partitioning");
Console.WriteLine("   - Analyzing data distribution");
Console.WriteLine("   - Parallel processing strategies\n");

// 3. Display schema showing RowGroup column
Console.WriteLine("3. Schema (note RowGroup as first column):");
Console.WriteLine("   " + new string('-', 40));
foreach (var colName in salesData.Schema.GetColumnNames())
{
    var column = salesData.GetColumn(colName);
    var marker = colName == "RowGroup" ? " <-- Auto-added" : "";
    Console.WriteLine($"   {colName}: {column.DataType.GetClrType().Name}{marker}");
}
Console.WriteLine("   " + new string('-', 40));
Console.WriteLine();

// 4. Analyze row group distribution using GroupBy
Console.WriteLine("4. Row group distribution:");
var groupedByRowGroup = salesData.GroupBy("RowGroup");
var groupInfo = groupedByRowGroup.Info();

Console.WriteLine("   " + new string('-', 40));
Console.WriteLine($"   {"RowGroup",-15} {"Row Count",-15}");
Console.WriteLine("   " + new string('-', 40));

var infoCursor = groupInfo.GetRowCursor("Group", "Count");
int totalGroups = 0;
while (infoCursor.MoveNext())
{
    var rowGroup = infoCursor.GetValue("Group");
    var count = infoCursor.GetValue("Count");
    Console.WriteLine($"   {rowGroup,-15} {count,-15}");
    totalGroups++;
}
Console.WriteLine("   " + new string('-', 40));
Console.WriteLine($"   Total row groups: {totalGroups}\n");

// 5. Filter to specific row group
Console.WriteLine("5. Filtering to specific row group (RowGroup = 0)...");
var group0 = salesData.Where("RowGroup", 0);
Console.WriteLine($"   Rows in RowGroup 0: {group0.RowCount}");

// Show sample from this group
Console.WriteLine("   Sample data from RowGroup 0:");
Console.WriteLine("   " + new string('-', 60));
var group0Cursor = group0.GetRowCursor("Region", "Country", "TotalRevenue");
int count0 = 0;
while (group0Cursor.MoveNext() && count0 < 3)
{
    var region = group0Cursor.GetValue("Region");
    var country = group0Cursor.GetValue("Country");
    var revenue = group0Cursor.GetValue("TotalRevenue");
    Console.WriteLine($"   {region,-15} {country,-20} {revenue:C0}");
    count0++;
}
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine();

// 6. Aggregate statistics per row group using GroupByAggregate
Console.WriteLine("6. Statistics per row group:");

// Get total revenue per row group
var revenueByGroup = salesData.GroupByAggregate("RowGroup", "TotalRevenue", AggregationType.Sum, "TotalRevenue");

Console.WriteLine("   " + new string('-', 50));
Console.WriteLine($"   {"RowGroup",-12} {"Total Revenue",-20}");
Console.WriteLine("   " + new string('-', 50));

var revCursor = revenueByGroup.GetRowCursor("RowGroup", "TotalRevenue");
while (revCursor.MoveNext())
{
    var rowGroup = revCursor.GetValue("RowGroup");
    var revenue = revCursor.GetValue("TotalRevenue");
    Console.WriteLine($"   {rowGroup,-12} {revenue,-20:C0}");
}
Console.WriteLine("   " + new string('-', 50));
Console.WriteLine();

// 7. Working without RowGroup column
Console.WriteLine("7. Removing RowGroup column for clean output...");
var originalColumnCount = salesData.Schema.Count;
salesData.RemoveColumn("RowGroup");
Console.WriteLine($"   Original columns: {originalColumnCount}");
Console.WriteLine($"   After removal: {salesData.Schema.Count}");
Console.WriteLine($"   Columns now: {string.Join(", ", salesData.Schema.GetColumnNames().Take(5))}...\n");

// 8. Sample cleaned data
Console.WriteLine("8. Sample data without RowGroup column:");
Console.WriteLine("   " + new string('-', 80));
Console.WriteLine($"   {"Region",-15} {"Country",-20} {"ItemType",-15} {"TotalRevenue",-15}");
Console.WriteLine("   " + new string('-', 80));

var cleanCursor = salesData.GetRowCursor("Region", "Country", "ItemType", "TotalRevenue");
int cleanCount = 0;
while (cleanCursor.MoveNext() && cleanCount < 5)
{
    var region = cleanCursor.GetValue("Region");
    var country = cleanCursor.GetValue("Country");
    var itemType = cleanCursor.GetValue("ItemType");
    var revenue = cleanCursor.GetValue("TotalRevenue");
    Console.WriteLine($"   {region,-15} {country,-20} {itemType,-15} {revenue,-15:C0}");
    cleanCount++;
}
Console.WriteLine("   " + new string('-', 80));

Console.WriteLine("\n=== Sample Complete ===");
