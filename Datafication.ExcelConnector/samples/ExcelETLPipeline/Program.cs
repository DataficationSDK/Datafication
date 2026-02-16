using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ExcelConnector;
using Datafication.Connectors.ExcelConnector;
using Datafication.Sinks.Connectors.ExcelConnector;

Console.WriteLine("=== Datafication.ExcelConnector ETL Pipeline Sample ===\n");
Console.WriteLine("This sample demonstrates a complete Extract-Transform-Load pipeline");
Console.WriteLine("using Excel as both source and destination.\n");

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
// PHASE 1: EXTRACT - Load multiple sheets from Excel
// ============================================================================
Console.WriteLine("+----------------------------------------------------------+");
Console.WriteLine("|                    PHASE 1: EXTRACT                       |");
Console.WriteLine("+----------------------------------------------------------+\n");

Console.WriteLine("Loading data from multiple Excel sheets...\n");

// Load Customers
var customersConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Customers",
    HasHeader = true
};
var customers = await DataBlock.Connector.LoadExcelAsync(customersConfig);
Console.WriteLine($"   [OK] Customers: {customers.RowCount} rows");

// Load Products
var productsConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",
    HasHeader = true
};
var products = await DataBlock.Connector.LoadExcelAsync(productsConfig);
Console.WriteLine($"   [OK] Products: {products.RowCount} rows");

// Load Orders
var ordersConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Orders",
    HasHeader = true
};
var orders = await DataBlock.Connector.LoadExcelAsync(ordersConfig);
Console.WriteLine($"   [OK] Orders: {orders.RowCount} rows");

// Load Order Items
var orderItemsConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "OrderItems",
    HasHeader = true
};
var orderItems = await DataBlock.Connector.LoadExcelAsync(orderItemsConfig);
Console.WriteLine($"   [OK] OrderItems: {orderItems.RowCount} rows");

Console.WriteLine($"\n   Total records extracted: {customers.RowCount + products.RowCount + orders.RowCount + orderItems.RowCount}");

// ============================================================================
// PHASE 2: TRANSFORM - Data enrichment and analysis
// ============================================================================
Console.WriteLine("\n+----------------------------------------------------------+");
Console.WriteLine("|                   PHASE 2: TRANSFORM                      |");
Console.WriteLine("+----------------------------------------------------------+\n");

// ---------------------------------------------
// 2.1 Enrich Orders with Customer Information (JOIN)
// ---------------------------------------------
Console.WriteLine("2.1 Enriching Orders with Customer Data (Left Merge)...");

var enrichedOrders = orders.Merge(customers, "CustomerID", MergeMode.Left);
Console.WriteLine($"    Merged columns: {enrichedOrders.Schema.Count}");
Console.WriteLine($"    Result rows: {enrichedOrders.RowCount}");

// Preview enriched data
var enrichedCursor = enrichedOrders.GetRowCursor();
Console.WriteLine("    Sample enriched order:");
if (enrichedCursor.MoveNext())
{
    Console.WriteLine($"      OrderID: {enrichedCursor.GetValue("OrderID")}, Customer: {enrichedCursor.GetValue("FirstName")} {enrichedCursor.GetValue("LastName")}, Segment: {enrichedCursor.GetValue("Segment")}");
}
Console.WriteLine();

// ---------------------------------------------
// 2.2 Revenue by Customer Segment (GroupBy + Aggregate)
// ---------------------------------------------
Console.WriteLine("2.2 Calculating Revenue by Customer Segment...");

var revenueBySegment = enrichedOrders
    .GroupByAggregate("Segment", "Total", AggregationType.Sum, "TotalRevenue");

Console.WriteLine("    Revenue by Segment:");
var segmentCursor = revenueBySegment.GetRowCursor();
while (segmentCursor.MoveNext())
{
    Console.WriteLine($"      {segmentCursor.GetValue("Segment")}: ${segmentCursor.GetValue("TotalRevenue"):N2}");
}
Console.WriteLine();

// ---------------------------------------------
// 2.3 Order Count by Status (GroupBy + Count)
// ---------------------------------------------
Console.WriteLine("2.3 Analyzing Order Status Distribution...");

var ordersByStatus = orders
    .GroupByAggregate("Status", "OrderID", AggregationType.Count, "OrderCount");

Console.WriteLine("    Orders by Status:");
var statusCursor = ordersByStatus.GetRowCursor();
while (statusCursor.MoveNext())
{
    Console.WriteLine($"      {statusCursor.GetValue("Status")}: {statusCursor.GetValue("OrderCount")} orders");
}
Console.WriteLine();

// ---------------------------------------------
// 2.4 Revenue by State (Geographic Analysis)
// ---------------------------------------------
Console.WriteLine("2.4 Calculating Revenue by State...");

var revenueByState = enrichedOrders
    .GroupByAggregate("State", "Total", AggregationType.Sum, "StateRevenue");

// Sort by revenue (descending) and get top 5 states
var topStates = revenueByState.Sort(SortDirection.Descending, "StateRevenue").Head(5);

Console.WriteLine("    Top 5 States by Revenue:");
var stateCursor = topStates.GetRowCursor();
int rank = 1;
while (stateCursor.MoveNext())
{
    Console.WriteLine($"      {rank}. {stateCursor.GetValue("State")}: ${stateCursor.GetValue("StateRevenue"):N2}");
    rank++;
}
Console.WriteLine();

// ---------------------------------------------
// 2.5 Customer Segments Summary
// ---------------------------------------------
Console.WriteLine("2.5 Creating Customer Segment Summary...");

var segmentSummary = customers
    .GroupByAggregate("Segment", "CustomerID", AggregationType.Count, "CustomerCount");

Console.WriteLine("    Customers by Segment:");
var summCursor = segmentSummary.GetRowCursor();
while (summCursor.MoveNext())
{
    Console.WriteLine($"      {summCursor.GetValue("Segment")}: {summCursor.GetValue("CustomerCount")} customers");
}
Console.WriteLine();

// ---------------------------------------------
// 2.6 Product Category Analysis
// ---------------------------------------------
Console.WriteLine("2.6 Product Category Analysis...");

var categoryAnalysis = products
    .GroupByAggregate("Category", "ProductID", AggregationType.Count, "ProductCount");

Console.WriteLine("    Products by Category:");
var catCursor = categoryAnalysis.GetRowCursor();
while (catCursor.MoveNext())
{
    Console.WriteLine($"      {catCursor.GetValue("Category")}: {catCursor.GetValue("ProductCount")} products");
}
Console.WriteLine();

// ---------------------------------------------
// 2.7 Create Filtered Reports
// ---------------------------------------------
Console.WriteLine("2.7 Creating Filtered Reports...");

// Enterprise customers
var enterpriseCustomers = customers
    .Where("Segment", "Enterprise")
    .Select("CustomerID", "FirstName", "LastName", "Email", "State");
Console.WriteLine($"    Enterprise Customers: {enterpriseCustomers.RowCount}");

// Shipped orders
var shippedOrders = orders.Where("Status", "Shipped");
Console.WriteLine($"    Shipped Orders: {shippedOrders.RowCount}");

// Active customers only
var activeCustomers = customers.Where("IsActive", true);
Console.WriteLine($"    Active Customers: {activeCustomers.RowCount}");

// ============================================================================
// PHASE 3: LOAD - Export reports to Excel
// ============================================================================
Console.WriteLine("\n+----------------------------------------------------------+");
Console.WriteLine("|                     PHASE 3: LOAD                         |");
Console.WriteLine("+----------------------------------------------------------+\n");

Console.WriteLine("Exporting analysis results to Excel files...\n");

// ---------------------------------------------
// 3.1 Export Revenue by Segment Report
// ---------------------------------------------
var revenueReportPath = Path.Combine(outputPath, "revenue_by_segment.xlsx");
var revenueBytes = await revenueBySegment.ExcelSinkAsync();
await File.WriteAllBytesAsync(revenueReportPath, revenueBytes);
Console.WriteLine($"   [OK] Revenue by Segment: {revenueReportPath}");

// ---------------------------------------------
// 3.2 Export Order Status Report
// ---------------------------------------------
var statusReportPath = Path.Combine(outputPath, "orders_by_status.xlsx");
var statusBytes = await ordersByStatus.ExcelSinkAsync();
await File.WriteAllBytesAsync(statusReportPath, statusBytes);
Console.WriteLine($"   [OK] Orders by Status: {statusReportPath}");

// ---------------------------------------------
// 3.3 Export Top States Report
// ---------------------------------------------
var statesReportPath = Path.Combine(outputPath, "top_states_revenue.xlsx");
var statesBytes = await topStates.ExcelSinkAsync();
await File.WriteAllBytesAsync(statesReportPath, statesBytes);
Console.WriteLine($"   [OK] Top States by Revenue: {statesReportPath}");

// ---------------------------------------------
// 3.4 Export Customer Segment Summary
// ---------------------------------------------
var segmentReportPath = Path.Combine(outputPath, "customer_segments.xlsx");
var segmentBytes = await segmentSummary.ExcelSinkAsync();
await File.WriteAllBytesAsync(segmentReportPath, segmentBytes);
Console.WriteLine($"   [OK] Customer Segments: {segmentReportPath}");

// ---------------------------------------------
// 3.5 Export Enterprise Customers List
// ---------------------------------------------
var enterpriseReportPath = Path.Combine(outputPath, "enterprise_customers.xlsx");
var enterpriseBytes = await enterpriseCustomers.ExcelSinkAsync();
await File.WriteAllBytesAsync(enterpriseReportPath, enterpriseBytes);
Console.WriteLine($"   [OK] Enterprise Customers: {enterpriseReportPath}");

// ---------------------------------------------
// 3.6 Export Category Analysis
// ---------------------------------------------
var categoryReportPath = Path.Combine(outputPath, "product_categories.xlsx");
var categoryBytes = await categoryAnalysis.ExcelSinkAsync();
await File.WriteAllBytesAsync(categoryReportPath, categoryBytes);
Console.WriteLine($"   [OK] Product Categories: {categoryReportPath}");

// ---------------------------------------------
// 3.7 Export Enriched Orders (Full Detail)
// ---------------------------------------------
var enrichedOrdersPath = Path.Combine(outputPath, "enriched_orders.xlsx");
var enrichedOrdersBytes = await enrichedOrders.ExcelSinkAsync();
await File.WriteAllBytesAsync(enrichedOrdersPath, enrichedOrdersBytes);
Console.WriteLine($"   [OK] Enriched Orders: {enrichedOrdersPath}");

// ============================================================================
// SUMMARY
// ============================================================================
Console.WriteLine("\n+----------------------------------------------------------+");
Console.WriteLine("|                       SUMMARY                             |");
Console.WriteLine("+----------------------------------------------------------+\n");

var outputFiles = Directory.GetFiles(outputPath, "*.xlsx");
long totalSize = 0;

Console.WriteLine("   ETL Pipeline completed successfully!\n");
Console.WriteLine("   Reports generated:");
foreach (var file in outputFiles.OrderBy(f => f))
{
    var info = new FileInfo(file);
    totalSize += info.Length;
    Console.WriteLine($"     - {info.Name}");
}

Console.WriteLine($"\n   Total reports: {outputFiles.Length}");
Console.WriteLine($"   Total size: {totalSize:N0} bytes");

Console.WriteLine(@"
   Pipeline Flow:
   +-----------------------------------------------------------+
   |  EXTRACT                                                  |
   |  - Customers (200 rows)                                   |
   |  - Products (50 rows)                                     |
   |  - Orders (500 rows)                                      |
   |  - OrderItems (1,497 rows)                                |
   +--------------------------+--------------------------------+
                              |
                              v
   +-----------------------------------------------------------+
   |  TRANSFORM                                                |
   |  - Merge (Join orders with customers)                     |
   |  - GroupBy + Aggregate (Revenue analysis)                 |
   |  - Where (Filtering)                                      |
   |  - Select (Column selection)                              |
   |  - Sort + Head (Top N analysis)                           |
   +--------------------------+--------------------------------+
                              |
                              v
   +-----------------------------------------------------------+
   |  LOAD                                                     |
   |  - 7 Excel reports exported                               |
   |  - Ready for business consumption                         |
   +-----------------------------------------------------------+
");

Console.WriteLine("=== Sample Complete ===");
