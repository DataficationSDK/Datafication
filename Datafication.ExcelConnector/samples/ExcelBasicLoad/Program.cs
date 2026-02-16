using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ExcelConnector;

Console.WriteLine("=== Datafication.ExcelConnector Basic Load Sample ===\n");

var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data");
var excelPath = Path.Combine(dataPath, "demo_excel_connector_data.xlsx");

// ============================================================================
// 1. ASYNC LOADING - Recommended for most scenarios
// ============================================================================
Console.WriteLine("1. Async Loading (LoadExcelAsync)");
Console.WriteLine(new string('-', 50));

var customers = await DataBlock.Connector.LoadExcelAsync(excelPath);

Console.WriteLine($"   Loaded sheet: First sheet (default)");
Console.WriteLine($"   Total rows: {customers.RowCount}");
Console.WriteLine($"   Total columns: {customers.Schema.Count}");
Console.WriteLine();

// ============================================================================
// 2. SYNC LOADING - For synchronous code paths
// ============================================================================
Console.WriteLine("2. Sync Loading (LoadExcel)");
Console.WriteLine(new string('-', 50));

var customersSync = DataBlock.Connector.LoadExcel(excelPath);

Console.WriteLine($"   Loaded rows: {customersSync.RowCount}");
Console.WriteLine($"   Loaded columns: {customersSync.Schema.Count}");
Console.WriteLine();

// ============================================================================
// 3. SCHEMA INSPECTION - Examine column names and types
// ============================================================================
Console.WriteLine("3. Schema Inspection");
Console.WriteLine(new string('-', 50));

Console.WriteLine("   Columns:");
foreach (var columnName in customers.Schema.GetColumnNames())
{
    var column = customers.GetColumn(columnName);
    Console.WriteLine($"     - {column?.Name} ({column?.DataType.GetClrType().Name})");
}
Console.WriteLine();

// ============================================================================
// 4. ROW CURSOR - Iterate through data row by row
// ============================================================================
Console.WriteLine("4. Row Cursor (First 5 Rows)");
Console.WriteLine(new string('-', 50));

var cursor = customers.GetRowCursor();
int rowIndex = 0;
while (cursor.MoveNext() && rowIndex < 5)
{
    // Access values by column name using GetValue
    Console.WriteLine($"   Row {rowIndex + 1}: CustomerID={cursor.GetValue("CustomerID")}, FirstName={cursor.GetValue("FirstName")}, LastName={cursor.GetValue("LastName")}");
    rowIndex++;
}
Console.WriteLine();

// ============================================================================
// 5. WHERE - Filter data
// ============================================================================
Console.WriteLine("5. Where (Filter by State = 'California')");
Console.WriteLine(new string('-', 50));

var californiaCustomers = customers.Where("State", "California");

Console.WriteLine($"   California customers: {californiaCustomers.RowCount}");
Console.WriteLine();

// ============================================================================
// 6. SORT - Order data
// ============================================================================
Console.WriteLine("6. Sort (Order by LastName Ascending)");
Console.WriteLine(new string('-', 50));

var sortedCustomers = customers.Sort(SortDirection.Ascending, "LastName");

Console.WriteLine("   First 5 customers by LastName:");
var sortedCursor = sortedCustomers.GetRowCursor();
rowIndex = 0;
while (sortedCursor.MoveNext() && rowIndex < 5)
{
    Console.WriteLine($"     {rowIndex + 1}. {sortedCursor.GetValue("LastName")}, {sortedCursor.GetValue("FirstName")}");
    rowIndex++;
}
Console.WriteLine();

// ============================================================================
// 7. HEAD - Get first N rows
// ============================================================================
Console.WriteLine("7. Head (First 10 Rows)");
Console.WriteLine(new string('-', 50));

var first10 = customers.Head(10);

Console.WriteLine($"   Head(10) returned: {first10.RowCount} rows");
Console.WriteLine();

// ============================================================================
// 8. COMBINED OPERATIONS - Chain operations together
// ============================================================================
Console.WriteLine("8. Combined Operations (Where + Sort + Head)");
Console.WriteLine(new string('-', 50));

var result = customers
    .Where("Segment", "Enterprise")
    .Sort(SortDirection.Ascending, "LastName")
    .Head(5);

Console.WriteLine($"   Top 5 Enterprise customers (sorted by LastName):");
var resultCursor = result.GetRowCursor();
rowIndex = 0;
while (resultCursor.MoveNext())
{
    Console.WriteLine($"     {rowIndex + 1}. {resultCursor.GetValue("FirstName")} {resultCursor.GetValue("LastName")} - {resultCursor.GetValue("Email")}");
    rowIndex++;
}

Console.WriteLine("\n=== Sample Complete ===");
