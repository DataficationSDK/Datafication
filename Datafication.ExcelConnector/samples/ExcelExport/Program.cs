using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ExcelConnector;
using Datafication.Connectors.ExcelConnector;
using Datafication.Sinks.Connectors.ExcelConnector;

Console.WriteLine("=== Datafication.ExcelConnector Export Sample ===\n");

var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data");
var excelPath = Path.Combine(dataPath, "demo_excel_connector_data.xlsx");
var outputPath = Path.Combine(AppContext.BaseDirectory, "output");

// Create output directory and clean up any existing .xlsx files
if (Directory.Exists(outputPath))
{
    foreach (var file in Directory.GetFiles(outputPath, "*.xlsx"))
    {
        File.Delete(file);
    }
}
Directory.CreateDirectory(outputPath);

// ============================================================================
// 1. SIMPLE EXCEL EXPORT - Export a DataBlock to Excel (Async)
// ============================================================================
Console.WriteLine("1. Simple Export (ExcelSinkAsync)");
Console.WriteLine(new string('-', 50));

// Load data
var productsConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",
    HasHeader = true
};
var products = await DataBlock.Connector.LoadExcelAsync(productsConfig);

// Export to Excel
var productBytes = await products.ExcelSinkAsync();
var productOutputPath = Path.Combine(outputPath, "products_export.xlsx");
await File.WriteAllBytesAsync(productOutputPath, productBytes);

Console.WriteLine($"   Source: Products sheet ({products.RowCount} rows)");
Console.WriteLine($"   Output: {productOutputPath}");
Console.WriteLine($"   Size: {productBytes.Length:N0} bytes");
Console.WriteLine();

// ============================================================================
// 2. SYNC EXPORT - Export using synchronous method
// ============================================================================
Console.WriteLine("2. Sync Export (ExcelSink)");
Console.WriteLine(new string('-', 50));

var customersConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Customers",
    HasHeader = true
};
var customers = DataBlock.Connector.LoadExcel(customersConfig);

var customerBytes = customers.ExcelSink();
var customerOutputPath = Path.Combine(outputPath, "customers_export.xlsx");
File.WriteAllBytes(customerOutputPath, customerBytes);

Console.WriteLine($"   Source: Customers sheet ({customers.RowCount} rows)");
Console.WriteLine($"   Output: {customerOutputPath}");
Console.WriteLine($"   Size: {customerBytes.Length:N0} bytes");
Console.WriteLine();

// ============================================================================
// 3. TRANSFORM THEN EXPORT - Apply transformations before exporting
// ============================================================================
Console.WriteLine("3. Transform Then Export");
Console.WriteLine(new string('-', 50));

// Filter to get only Enterprise segment customers
var enterpriseCustomers = customers
    .Where("Segment", "Enterprise")
    .Select("CustomerID", "FirstName", "LastName", "Email", "State");

var enterpriseBytes = await enterpriseCustomers.ExcelSinkAsync();
var enterpriseOutputPath = Path.Combine(outputPath, "enterprise_customers.xlsx");
await File.WriteAllBytesAsync(enterpriseOutputPath, enterpriseBytes);

Console.WriteLine($"   Transform: Where(Segment=Enterprise) + Select(5 columns)");
Console.WriteLine($"   Result: {enterpriseCustomers.RowCount} rows");
Console.WriteLine($"   Output: {enterpriseOutputPath}");
Console.WriteLine();

// ============================================================================
// 4. AGGREGATED DATA EXPORT - Export summary statistics
// ============================================================================
Console.WriteLine("4. Aggregated Data Export");
Console.WriteLine(new string('-', 50));

// Count customers by segment
var segmentCounts = customers
    .GroupByAggregate("Segment", "CustomerID", AggregationType.Count, "CustomerCount");

var segmentBytes = await segmentCounts.ExcelSinkAsync();
var segmentOutputPath = Path.Combine(outputPath, "customers_by_segment.xlsx");
await File.WriteAllBytesAsync(segmentOutputPath, segmentBytes);

Console.WriteLine($"   Aggregation: GroupBy Segment, Count CustomerID");
Console.WriteLine($"   Result: {segmentCounts.RowCount} segment groups");
Console.WriteLine($"   Output: {segmentOutputPath}");

// Preview the data
Console.WriteLine("   Data preview:");
var cursor = segmentCounts.GetRowCursor();
while (cursor.MoveNext())
{
    Console.WriteLine($"     {cursor.GetValue("Segment")}: {cursor.GetValue("CustomerCount")} customers");
}
Console.WriteLine();

// ============================================================================
// 5. SORTED AND FILTERED EXPORT
// ============================================================================
Console.WriteLine("5. Sorted and Filtered Export");
Console.WriteLine(new string('-', 50));

// Get top products by category
var premiumProducts = products
    .Where("Category", "Electronics")
    .Sort(SortDirection.Descending, "ListPrice")
    .Head(10)
    .Select("ProductID", "ProductName", "Brand", "UnitCost", "ListPrice");

var premiumBytes = await premiumProducts.ExcelSinkAsync();
var premiumOutputPath = Path.Combine(outputPath, "premium_electronics.xlsx");
await File.WriteAllBytesAsync(premiumOutputPath, premiumBytes);

Console.WriteLine($"   Transform: Electronics + Sort by ListPrice desc + Top 10");
Console.WriteLine($"   Result: {premiumProducts.RowCount} products");
Console.WriteLine($"   Output: {premiumOutputPath}");
Console.WriteLine();

// ============================================================================
// 6. ROUNDTRIP VERIFICATION - Load, export, reload and verify
// ============================================================================
Console.WriteLine("6. Roundtrip Verification");
Console.WriteLine(new string('-', 50));

// Take a subset of products
var originalData = products.Head(20);
Console.WriteLine($"   Original data: {originalData.RowCount} rows, {originalData.Schema.Count} columns");

// Export to Excel
var roundtripBytes = await originalData.ExcelSinkAsync();
var roundtripPath = Path.Combine(outputPath, "roundtrip_test.xlsx");
await File.WriteAllBytesAsync(roundtripPath, roundtripBytes);

// Reload the exported file
var reloadedData = await DataBlock.Connector.LoadExcelAsync(roundtripPath);
Console.WriteLine($"   Reloaded data: {reloadedData.RowCount} rows, {reloadedData.Schema.Count} columns");

// Verify row count matches
Console.WriteLine($"   Verification: Row count {(originalData.RowCount == reloadedData.RowCount ? "MATCH" : "MISMATCH")}");
Console.WriteLine($"   Verification: Column count {(originalData.Schema.Count == reloadedData.Schema.Count ? "MATCH" : "MISMATCH")}");
Console.WriteLine();

// ============================================================================
// 7. MULTIPLE EXPORTS - Create multiple report files
// ============================================================================
Console.WriteLine("7. Multiple Exports (Report Suite)");
Console.WriteLine(new string('-', 50));

// Load orders
var ordersConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Orders",
    HasHeader = true
};
var orders = await DataBlock.Connector.LoadExcelAsync(ordersConfig);

var reports = new Dictionary<string, DataBlock>
{
    { "all_orders.xlsx", orders },
    { "shipped_orders.xlsx", orders.Where("Status", "Shipped") },
    { "pending_orders.xlsx", orders.Where("Status", "Pending") },
    { "cancelled_orders.xlsx", orders.Where("Status", "Cancelled") }
};

foreach (var (filename, data) in reports)
{
    var bytes = await data.ExcelSinkAsync();
    var path = Path.Combine(outputPath, filename);
    await File.WriteAllBytesAsync(path, bytes);
    Console.WriteLine($"   {filename}: {data.RowCount} rows");
}
Console.WriteLine();

// ============================================================================
// 8. OUTPUT SUMMARY
// ============================================================================
Console.WriteLine("8. Output Summary");
Console.WriteLine(new string('-', 50));

var outputFiles = Directory.GetFiles(outputPath, "*.xlsx");
long totalSize = 0;

Console.WriteLine($"   Output directory: {outputPath}");
Console.WriteLine($"   Files created: {outputFiles.Length}");
Console.WriteLine();
Console.WriteLine("   File listing:");
foreach (var file in outputFiles.OrderBy(f => f))
{
    var info = new FileInfo(file);
    totalSize += info.Length;
    Console.WriteLine($"     {info.Name,-35} {info.Length,10:N0} bytes");
}
Console.WriteLine(new string('-', 50));
Console.WriteLine($"     {"TOTAL",-35} {totalSize,10:N0} bytes");

Console.WriteLine("\n=== Sample Complete ===");
