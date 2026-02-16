using Datafication.Core.Data;
using Datafication.Extensions.Connectors.JsonConnector;

Console.WriteLine("=== Datafication.JsonConnector Basic Load Sample ===\n");

// Get path to data directory (relative to build output)
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var jsonPath = Path.Combine(dataPath, "employees.json");

Console.WriteLine($"JSON file path: {Path.GetFullPath(jsonPath)}\n");

// 1. Load JSON asynchronously (recommended for I/O operations)
Console.WriteLine("1. Loading JSON asynchronously...");
var employeesAsync = await DataBlock.Connector.LoadJsonAsync(jsonPath);
Console.WriteLine($"   Loaded {employeesAsync.RowCount} rows with {employeesAsync.Schema.Count} columns\n");

// 2. Display schema information
Console.WriteLine("2. Schema Information:");
foreach (var colName in employeesAsync.Schema.GetColumnNames())
{
    var column = employeesAsync.GetColumn(colName);
    Console.WriteLine($"   - {colName}: {column.DataType.GetClrType().Name}");
}
Console.WriteLine();

// 3. Load JSON synchronously (alternative for simpler scenarios)
Console.WriteLine("3. Loading JSON synchronously...");
var employeesSync = DataBlock.Connector.LoadJson(jsonPath);
Console.WriteLine($"   Loaded {employeesSync.RowCount} rows\n");

// 4. Display sample data using row cursor
Console.WriteLine("4. First 10 employees:");
Console.WriteLine("   " + new string('-', 80));
Console.WriteLine($"   {"Id",-5} {"Name",-20} {"Department",-15} {"Salary",-12} {"Active",-8}");
Console.WriteLine("   " + new string('-', 80));

var cursor = employeesAsync.GetRowCursor("Id", "Name", "Department", "Salary", "IsActive");
int rowCount = 0;
while (cursor.MoveNext() && rowCount < 10)
{
    var id = cursor.GetValue("Id");
    var name = cursor.GetValue("Name");
    var dept = cursor.GetValue("Department");
    var salary = cursor.GetValue("Salary");
    var active = cursor.GetValue("IsActive");
    Console.WriteLine($"   {id,-5} {name,-20} {dept,-15} {salary,-12:C0} {active,-8}");
    rowCount++;
}
Console.WriteLine("   " + new string('-', 80));
Console.WriteLine($"   ... and {employeesAsync.RowCount - 10} more rows\n");

// 5. Basic filtering example
Console.WriteLine("5. Filtering: Engineering department employees...");
var engineers = employeesAsync.Where("Department", "Engineering");
Console.WriteLine($"   Found {engineers.RowCount} engineers\n");

// 6. Basic sorting example
Console.WriteLine("6. Sorting: Top 5 highest salaries...");
var topEarners = employeesAsync
    .Sort(SortDirection.Descending, "Salary")
    .Head(5);

Console.WriteLine("   " + new string('-', 50));
Console.WriteLine($"   {"Name",-25} {"Salary",-15}");
Console.WriteLine("   " + new string('-', 50));

var topCursor = topEarners.GetRowCursor("Name", "Salary");
while (topCursor.MoveNext())
{
    var name = topCursor.GetValue("Name");
    var salary = topCursor.GetValue("Salary");
    Console.WriteLine($"   {name,-25} {salary,-15:C0}");
}
Console.WriteLine("   " + new string('-', 50));

Console.WriteLine("\n=== Sample Complete ===");
