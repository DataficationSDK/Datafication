using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ExcelConnector;
using Datafication.Connectors.ExcelConnector;

Console.WriteLine("=== Datafication.ExcelConnector Configuration Sample ===\n");

var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data");
var excelPath = Path.Combine(dataPath, "demo_excel_connector_data.xlsx");

// ============================================================================
// 1. USE COLUMNS - Select only specific columns
// ============================================================================
Console.WriteLine("1. UseColumns - Select Specific Columns");
Console.WriteLine(new string('-', 50));

var useColumnsConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Orders",
    HasHeader = true,
    UseColumns = "OrderID, CustomerID, Total"  // Only load these columns
};
var ordersSubset = await DataBlock.Connector.LoadExcelAsync(useColumnsConfig);

Console.WriteLine($"   Requested columns: OrderID, CustomerID, Total");
Console.WriteLine($"   Loaded columns: {string.Join(", ", ordersSubset.Schema.GetColumnNames())}");
Console.WriteLine($"   Total rows: {ordersSubset.RowCount}");
Console.WriteLine();

// ============================================================================
// 2. NROWS - Limit the number of rows loaded
// ============================================================================
Console.WriteLine("2. NRows - Limit Rows Loaded");
Console.WriteLine(new string('-', 50));

var nRowsConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "WebEvents",
    HasHeader = true,
    NRows = 100  // Only load first 100 rows
};
var limitedEvents = await DataBlock.Connector.LoadExcelAsync(nRowsConfig);

Console.WriteLine($"   Sheet has 2,000 rows");
Console.WriteLine($"   NRows setting: 100");
Console.WriteLine($"   Loaded rows: {limitedEvents.RowCount}");
Console.WriteLine();

// ============================================================================
// 3. COMBINED: UseColumns + NRows
// ============================================================================
Console.WriteLine("3. Combined: UseColumns + NRows");
Console.WriteLine(new string('-', 50));

var combinedConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "WebEvents",
    HasHeader = true,
    UseColumns = "EventID, CustomerID, EventType",
    NRows = 50
};
var combinedResult = await DataBlock.Connector.LoadExcelAsync(combinedConfig);

Console.WriteLine($"   Columns: {string.Join(", ", combinedResult.Schema.GetColumnNames())}");
Console.WriteLine($"   Rows: {combinedResult.RowCount}");

// Preview data
Console.WriteLine("   Preview (first 3 rows):");
var cursor = combinedResult.GetRowCursor();
int rowIndex = 0;
while (cursor.MoveNext() && rowIndex < 3)
{
    Console.WriteLine($"     EventID={cursor.GetValue("EventID")}, CustomerID={cursor.GetValue("CustomerID")}, EventType={cursor.GetValue("EventType")}");
    rowIndex++;
}
Console.WriteLine();

// ============================================================================
// 4. SKIP ROWS - Skip rows at the beginning of the sheet
// ============================================================================
Console.WriteLine("4. SkipRows - Skip Initial Rows");
Console.WriteLine(new string('-', 50));

// Load Products normally first
var normalConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",
    HasHeader = true
};
var normalProducts = await DataBlock.Connector.LoadExcelAsync(normalConfig);
Console.WriteLine($"   Normal load: {normalProducts.RowCount} rows");

// Skip first 5 data rows
var skipConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",
    HasHeader = true,
    SkipRows = 5  // Skip 5 rows before reading header
};
var skippedProducts = await DataBlock.Connector.LoadExcelAsync(skipConfig);
Console.WriteLine($"   With SkipRows=5: {skippedProducts.RowCount} rows");
Console.WriteLine();

// ============================================================================
// 5. HEADER ROW - Specify which row contains headers
// ============================================================================
Console.WriteLine("5. HeaderRow - Custom Header Row Position");
Console.WriteLine(new string('-', 50));

// HeaderRow specifies additional offset after SkipRows
var headerRowConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",
    HasHeader = true,
    SkipRows = 2,    // Skip first 2 rows
    HeaderRow = 0    // Then the next row is the header (row 3 in original)
};
var headerRowResult = await DataBlock.Connector.LoadExcelAsync(headerRowConfig);

Console.WriteLine($"   SkipRows=2, HeaderRow=0");
Console.WriteLine($"   Result: {headerRowResult.RowCount} rows");
Console.WriteLine($"   Columns: {string.Join(", ", headerRowResult.Schema.GetColumnNames())}");
Console.WriteLine();

// ============================================================================
// 6. HAS HEADER = FALSE - No header row (auto-generate column names)
// ============================================================================
Console.WriteLine("6. HasHeader = false (Auto-generated Column Names)");
Console.WriteLine(new string('-', 50));

var noHeaderConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",
    HasHeader = false,  // First row will be treated as data, not headers
    NRows = 5
};
var noHeaderResult = await DataBlock.Connector.LoadExcelAsync(noHeaderConfig);

Console.WriteLine($"   HasHeader = false");
Console.WriteLine($"   Auto-generated columns: {string.Join(", ", noHeaderResult.Schema.GetColumnNames())}");
Console.WriteLine($"   Total rows (includes what would be header): {noHeaderResult.RowCount}");

// Show first row which contains what would normally be the header
var noHeaderCursor = noHeaderResult.GetRowCursor();
if (noHeaderCursor.MoveNext())
{
    var firstValues = noHeaderResult.Schema.GetColumnNames().Select(c => noHeaderCursor.GetValue(c)?.ToString() ?? "null");
    Console.WriteLine($"   First row data: {string.Join(", ", firstValues)}");
}
Console.WriteLine();

// ============================================================================
// 7. EXCEL CONNECTOR VALIDATOR - Validate configurations before loading
// ============================================================================
Console.WriteLine("7. ExcelConnectorValidator - Validate Configurations");
Console.WriteLine(new string('-', 50));

var validator = new ExcelConnectorValidator();

// Valid configuration
var validConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Customers",
    HasHeader = true
};
var validResult = validator.Validate(validConfig);
Console.WriteLine($"   Valid config: IsValid = {validResult.IsValid}");

// Invalid configuration - null source
var invalidConfig1 = new ExcelConnectorConfiguration
{
    Source = null!,
    SheetName = "Customers"
};
var invalidResult1 = validator.Validate(invalidConfig1);
Console.WriteLine($"   Null source: IsValid = {invalidResult1.IsValid}");
if (!invalidResult1.IsValid)
{
    Console.WriteLine($"     Errors: {string.Join(", ", invalidResult1.Errors)}");
}

// Invalid configuration - file doesn't exist
var invalidConfig2 = new ExcelConnectorConfiguration
{
    Source = new Uri("/nonexistent/path/file.xlsx", UriKind.Relative),
    SheetName = "Customers"
};
var invalidResult2 = validator.Validate(invalidConfig2);
Console.WriteLine($"   Non-existent file: IsValid = {invalidResult2.IsValid}");
if (!invalidResult2.IsValid)
{
    Console.WriteLine($"     Errors: Source file does not exist...");
}
Console.WriteLine();

// ============================================================================
// 8. CONFIGURATION SUMMARY
// ============================================================================
Console.WriteLine("8. Configuration Options Summary");
Console.WriteLine(new string('-', 50));

Console.WriteLine(@"
   | Option       | Type    | Default | Description                            |
   |--------------|---------|---------|----------------------------------------|
   | Source       | Uri     | -       | File path or URL (required)            |
   | SheetName    | string? | null    | Sheet name to load                     |
   | SheetIndex   | int?    | null    | Sheet index (0-based)                  |
   | HasHeader    | bool    | true    | First row contains headers             |
   | HeaderRow    | int     | 0       | Offset after SkipRows for header row   |
   | SkipRows     | int     | 0       | Rows to skip before reading            |
   | UseColumns   | string? | null    | Comma-separated column names to load   |
   | NRows        | int?    | null    | Maximum rows to read                   |
   | ErrorHandler | Action? | null    | Error callback function                |
");

Console.WriteLine("\n=== Sample Complete ===");
