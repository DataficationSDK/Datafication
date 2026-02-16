using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ParquetConnector;

Console.WriteLine("=== Datafication.ParquetConnector Basic Load Sample ===\n");

// Get path to data directory (relative to build output)
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var parquetPath = Path.Combine(dataPath, "SalesRecords50k.parquet");

Console.WriteLine($"Parquet file path: {Path.GetFullPath(parquetPath)}\n");

// 1. Load Parquet asynchronously (recommended for I/O operations)
Console.WriteLine("1. Loading Parquet asynchronously...");
var salesData = await DataBlock.Connector.LoadParquetAsync(new Uri("file://" + Path.GetFullPath(parquetPath)));
Console.WriteLine($"   Loaded {salesData.RowCount} rows with {salesData.Schema.Count} columns\n");

// 2. Display schema information
Console.WriteLine("2. Schema Information:");
foreach (var colName in salesData.Schema.GetColumnNames())
{
    var column = salesData.GetColumn(colName);
    Console.WriteLine($"   - {colName}: {column.DataType.GetClrType().Name}");
}
Console.WriteLine();

// 3. Load Parquet synchronously (alternative for simpler scenarios)
Console.WriteLine("3. Loading Parquet synchronously...");
var salesSync = DataBlock.Connector.LoadParquet(new Uri("file://" + Path.GetFullPath(parquetPath)));
Console.WriteLine($"   Loaded {salesSync.RowCount} rows\n");

// 4. Display sample data using row cursor
Console.WriteLine("4. First 10 sales records:");
Console.WriteLine("   " + new string('-', 90));
Console.WriteLine($"   {"Region",-15} {"Country",-20} {"ItemType",-15} {"UnitsSold",-12} {"TotalRevenue",-15}");
Console.WriteLine("   " + new string('-', 90));

var cursor = salesData.GetRowCursor("Region", "Country", "ItemType", "UnitsSold", "TotalRevenue");
int rowCount = 0;
while (cursor.MoveNext() && rowCount < 10)
{
    var region = cursor.GetValue("Region");
    var country = cursor.GetValue("Country");
    var itemType = cursor.GetValue("ItemType");
    var unitsSold = cursor.GetValue("UnitsSold");
    var revenue = cursor.GetValue("TotalRevenue");
    Console.WriteLine($"   {region,-15} {country,-20} {itemType,-15} {unitsSold,-12} {revenue,-15:C0}");
    rowCount++;
}
Console.WriteLine("   " + new string('-', 90));
Console.WriteLine($"   ... and {salesData.RowCount - 10} more rows\n");

// 5. Basic filtering example
Console.WriteLine("5. Filtering: Sales in Europe...");
var europeSales = salesData.Where("Region", "Europe");
Console.WriteLine($"   Found {europeSales.RowCount} sales records in Europe\n");

// 6. Basic sorting example
Console.WriteLine("6. Sorting: Top 5 highest revenue transactions...");
var topRevenue = salesData
    .Sort(SortDirection.Descending, "TotalRevenue")
    .Head(5);

Console.WriteLine("   " + new string('-', 60));
Console.WriteLine($"   {"Country",-25} {"TotalRevenue",-20}");
Console.WriteLine("   " + new string('-', 60));

var topCursor = topRevenue.GetRowCursor("Country", "TotalRevenue");
while (topCursor.MoveNext())
{
    var country = topCursor.GetValue("Country");
    var revenue = topCursor.GetValue("TotalRevenue");
    Console.WriteLine($"   {country,-25} {revenue,-20:C0}");
}
Console.WriteLine("   " + new string('-', 60));

Console.WriteLine("\n=== Sample Complete ===");
