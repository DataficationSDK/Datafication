using System.Diagnostics;
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.JsonConnector;
using Datafication.Sinks.Connectors.JsonConnector;

Console.WriteLine("=== Datafication.JsonConnector ETL Pipeline Sample ===\n");

var stopwatch = new Stopwatch();
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");

// ============================================================
// EXTRACT PHASE
// ============================================================
Console.WriteLine("=== EXTRACT PHASE ===\n");

// Extract 1: Load employees from local file
stopwatch.Restart();
Console.WriteLine("1. Extracting employees from local JSON...");
var employees = await DataBlock.Connector.LoadJsonAsync(Path.Combine(dataPath, "employees.json"));
stopwatch.Stop();
Console.WriteLine($"   Loaded {employees.RowCount} employees in {stopwatch.ElapsedMilliseconds}ms\n");

// Extract 2: Load products from local file
stopwatch.Restart();
Console.WriteLine("2. Extracting products from local JSON...");
var products = await DataBlock.Connector.LoadJsonAsync(Path.Combine(dataPath, "products.json"));
stopwatch.Stop();
Console.WriteLine($"   Loaded {products.RowCount} products in {stopwatch.ElapsedMilliseconds}ms\n");

// Extract 3: Load sales transactions
stopwatch.Restart();
Console.WriteLine("3. Extracting sales transactions...");
var sales = await DataBlock.Connector.LoadJsonAsync(Path.Combine(dataPath, "sales_large.json"));
stopwatch.Stop();
Console.WriteLine($"   Loaded {sales.RowCount} sales in {stopwatch.ElapsedMilliseconds}ms\n");

// ============================================================
// TRANSFORM PHASE
// ============================================================
Console.WriteLine("=== TRANSFORM PHASE ===\n");

// Transform 1: Filter active employees in Engineering
stopwatch.Restart();
Console.WriteLine("4. Filtering active engineering employees...");
var activeEngineers = employees
    .Where("Department", "Engineering")
    .Where("IsActive", true);
stopwatch.Stop();
Console.WriteLine($"   Found {activeEngineers.RowCount} active engineers in {stopwatch.ElapsedMilliseconds}ms\n");

// Transform 2: Compute total value for each sale using expression
stopwatch.Restart();
Console.WriteLine("5. Computing total value for sales...");
var salesWithTotal = sales.Compute("TotalValue", "Quantity * UnitPrice");
stopwatch.Stop();
Console.WriteLine($"   Added TotalValue column to {salesWithTotal.RowCount} rows in {stopwatch.ElapsedMilliseconds}ms\n");

// Transform 3: Group sales by region - Sum of TotalValue
stopwatch.Restart();
Console.WriteLine("6. Aggregating sales by region...");
var salesByRegion = salesWithTotal.GroupByAggregate("Region", "TotalValue", AggregationType.Sum, "TotalRevenue");
stopwatch.Stop();
Console.WriteLine($"   Created {salesByRegion.RowCount} region summaries in {stopwatch.ElapsedMilliseconds}ms");

// Display region summary
Console.WriteLine("   " + new string('-', 40));
Console.WriteLine($"   {"Region",-15} {"Total Revenue",-20}");
Console.WriteLine("   " + new string('-', 40));
var regionCursor = salesByRegion.GetRowCursor("Region", "TotalRevenue");
while (regionCursor.MoveNext())
{
    var region = regionCursor.GetValue("Region");
    var revenue = Convert.ToDouble(regionCursor.GetValue("TotalRevenue"));
    Console.WriteLine($"   {region,-15} {revenue,-20:C2}");
}
Console.WriteLine("   " + new string('-', 40));
Console.WriteLine();

// Transform 4: Get top 10 products by total sales
stopwatch.Restart();
Console.WriteLine("7. Finding top products by sales volume...");
var productSales = salesWithTotal.GroupByAggregate("ProductId", "TotalValue", AggregationType.Sum, "TotalSales");
var topProducts = productSales
    .Sort(SortDirection.Descending, "TotalSales")
    .Head(10);
stopwatch.Stop();
Console.WriteLine($"   Identified top 10 products in {stopwatch.ElapsedMilliseconds}ms\n");

// Transform 5: Sort employees by salary
stopwatch.Restart();
Console.WriteLine("8. Sorting employees by salary...");
var sortedEmployees = employees
    .Sort(SortDirection.Descending, "Salary")
    .Select("Name", "Department", "Salary");
stopwatch.Stop();
Console.WriteLine($"   Sorted {sortedEmployees.RowCount} employees in {stopwatch.ElapsedMilliseconds}ms\n");

// Transform 6: Aggregate employees by department - Count
stopwatch.Restart();
Console.WriteLine("9. Aggregating employee count by department...");
var deptHeadCount = employees.GroupByAggregate("Department", "Id", AggregationType.Count, "HeadCount");
stopwatch.Stop();
Console.WriteLine($"   Created {deptHeadCount.RowCount} department summaries in {stopwatch.ElapsedMilliseconds}ms");

// Display department stats
Console.WriteLine("   " + new string('-', 40));
Console.WriteLine($"   {"Department",-20} {"HeadCount",-15}");
Console.WriteLine("   " + new string('-', 40));
var deptCursor = deptHeadCount.GetRowCursor("Department", "HeadCount");
while (deptCursor.MoveNext())
{
    var dept = deptCursor.GetValue("Department");
    var count = deptCursor.GetValue("HeadCount");
    Console.WriteLine($"   {dept,-20} {count,-15}");
}
Console.WriteLine("   " + new string('-', 40));
Console.WriteLine();

// Transform 7: Get mean salary by department
stopwatch.Restart();
Console.WriteLine("10. Computing mean salary by department...");
var deptSalary = employees.GroupByAggregate("Department", "Salary", AggregationType.Mean, "AvgSalary");
stopwatch.Stop();
Console.WriteLine($"   Computed averages in {stopwatch.ElapsedMilliseconds}ms");

Console.WriteLine("   " + new string('-', 45));
Console.WriteLine($"   {"Department",-20} {"Avg Salary",-20}");
Console.WriteLine("   " + new string('-', 45));
var salCursor = deptSalary.GetRowCursor("Department", "AvgSalary");
while (salCursor.MoveNext())
{
    var dept = salCursor.GetValue("Department");
    var avg = Convert.ToDouble(salCursor.GetValue("AvgSalary"));
    Console.WriteLine($"   {dept,-20} {avg,-20:C0}");
}
Console.WriteLine("   " + new string('-', 45));
Console.WriteLine();

// ============================================================
// LOAD PHASE
// ============================================================
Console.WriteLine("=== LOAD PHASE ===\n");

// Load 1: Export region summary to JSON
stopwatch.Restart();
Console.WriteLine("11. Exporting region summary to JSON...");
var regionJson = await salesByRegion.JsonStringSinkAsync();
var regionOutputPath = Path.Combine(dataPath, "output_region_summary.json");
await File.WriteAllTextAsync(regionOutputPath, regionJson);
stopwatch.Stop();
Console.WriteLine($"    Saved to {Path.GetFileName(regionOutputPath)} ({regionJson.Length:N0} chars) in {stopwatch.ElapsedMilliseconds}ms\n");

// Load 2: Export top products to JSON
stopwatch.Restart();
Console.WriteLine("12. Exporting top products to JSON...");
var topProductsJson = await topProducts.JsonStringSinkAsync();
var productsOutputPath = Path.Combine(dataPath, "output_top_products.json");
await File.WriteAllTextAsync(productsOutputPath, topProductsJson);
stopwatch.Stop();
Console.WriteLine($"    Saved to {Path.GetFileName(productsOutputPath)} ({topProductsJson.Length:N0} chars) in {stopwatch.ElapsedMilliseconds}ms\n");

// Load 3: Export department head count to JSON
stopwatch.Restart();
Console.WriteLine("13. Exporting department statistics to JSON...");
var deptJson = await deptHeadCount.JsonStringSinkAsync();
var deptOutputPath = Path.Combine(dataPath, "output_department_stats.json");
await File.WriteAllTextAsync(deptOutputPath, deptJson);
stopwatch.Stop();
Console.WriteLine($"    Saved to {Path.GetFileName(deptOutputPath)} ({deptJson.Length:N0} chars) in {stopwatch.ElapsedMilliseconds}ms\n");

// ============================================================
// PIPELINE SUMMARY
// ============================================================
Console.WriteLine("=== PIPELINE SUMMARY ===\n");
Console.WriteLine($"   Sources processed:     3 (employees, products, sales)");
Console.WriteLine($"   Total records loaded:  {employees.RowCount + products.RowCount + sales.RowCount:N0}");
Console.WriteLine($"   Outputs generated:     3 (region_summary, top_products, department_stats)");
Console.WriteLine($"   Transformations:       7 (filter, compute, groupby, sort, select, aggregate)");

Console.WriteLine("\n=== Sample Complete ===");
