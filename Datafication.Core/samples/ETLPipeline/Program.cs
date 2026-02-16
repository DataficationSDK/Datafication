using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core ETL Pipeline Sample ===\n");

// Simulate loading data from external source (in real scenario, this would be from CSV, database, etc.)
var rawData = new DataBlock();
rawData.AddColumn(new DataColumn("ProductId", typeof(int)));
rawData.AddColumn(new DataColumn("ProductName", typeof(string)));
rawData.AddColumn(new DataColumn("Category", typeof(string)));
rawData.AddColumn(new DataColumn("Price", typeof(decimal?)));
rawData.AddColumn(new DataColumn("Quantity", typeof(int?)));
rawData.AddColumn(new DataColumn("Cost", typeof(decimal?)));
rawData.AddColumn(new DataColumn("Status", typeof(string)));

// Add raw data with some issues (nulls, invalid values, etc.)
rawData.AddRow(new object[] { 1, "Widget A", "Electronics", 10.50m, 100, 5.00m, "Active" });
rawData.AddRow(new object[] { 2, "Widget B", "Electronics", null, 50, 8.00m, "Active" }); // Missing price
rawData.AddRow(new object[] { 3, "Widget C", "Furniture", 20.00m, null, 12.00m, "Inactive" }); // Missing quantity
rawData.AddRow(new object[] { 4, "Widget D", "Electronics", 8.25m, 200, null, "Active" }); // Missing cost
rawData.AddRow(new object[] { 5, "Widget E", "Furniture", 25.50m, 30, 15.00m, "Active" });
rawData.AddRow(new object[] { 6, "Widget F", "Electronics", 15.75m, 75, 9.00m, "Active" });
rawData.AddRow(new object[] { 7, "Widget G", "Furniture", null, null, null, "Inactive" }); // Multiple nulls
rawData.AddRow(new object[] { 8, "Widget H", "Electronics", 12.00m, 150, 6.50m, "Active" });

Console.WriteLine("Step 1: Raw Data Loaded");
Console.WriteLine($"   Rows: {rawData.RowCount}, Columns: {rawData.Schema.Count}\n");

// ETL Pipeline: Extract, Transform, Load
var processed = rawData
    // Extract: Select relevant columns
    .Select("ProductId", "ProductName", "Category", "Price", "Quantity", "Cost", "Status")
    
    // Transform: Clean data
    .Where("Status", "Active")  // Filter active products only
    .DropNulls(DropNullMode.Any)  // Remove rows with missing critical data
    
    // Transform: Fill remaining nulls
    .FillNulls(FillMethod.Mean, "Price")
    .FillNulls(FillMethod.Mean, "Cost")
    
    // Transform: Compute new columns
    .Compute("Revenue", "Price * Quantity")
    .Compute("TotalCost", "Cost * Quantity")
    .Compute("Profit", "Revenue - TotalCost")
    .Compute("ProfitMargin", "Profit / Revenue")
    
    // Transform: Filter and sort
    .Where("Profit", 0m, ComparisonOperator.GreaterThan)  // Only profitable products
    .Sort(SortDirection.Descending, "ProfitMargin")
    
    // Load: Limit results
    .Head(10);

Console.WriteLine("Step 2: ETL Pipeline Complete");
Console.WriteLine($"   Processed rows: {processed.RowCount}\n");

Console.WriteLine("Final Processed Data:");
PrintDataBlock(processed);

// Show transformation summary
Console.WriteLine("\nTransformation Summary:");
Console.WriteLine($"   Original rows: {rawData.RowCount}");
Console.WriteLine($"   After filtering Active: {rawData.Where("Status", "Active").RowCount}");
Console.WriteLine($"   After dropping nulls: {rawData.Where("Status", "Active").DropNulls(DropNullMode.Any).RowCount}");
Console.WriteLine($"   Final processed rows: {processed.RowCount}");

// Show top products by profit margin
var topProducts = processed.Head(3);
Console.WriteLine("\nTop 3 Products by Profit Margin:");
PrintDataBlock(topProducts.Select("ProductName", "Category", "ProfitMargin", "Profit"));

Console.WriteLine("\n=== Sample Complete ===");

static void PrintDataBlock(DataBlock dataBlock)
{
    if (dataBlock.RowCount == 0)
    {
        Console.WriteLine("   (No rows)");
        return;
    }

    var columnNames = dataBlock.Schema.GetColumnNames().ToArray();
    var cursor = dataBlock.GetRowCursor(columnNames);
    
    // Print header
    Console.WriteLine($"   {string.Join(" | ", columnNames)}");
    Console.WriteLine($"   {new string('-', Math.Min(120, columnNames.Sum(c => c.Length) + (columnNames.Length - 1) * 3))}");
    
    // Print rows
    while (cursor.MoveNext())
    {
        var values = columnNames.Select(col => 
        {
            var val = cursor.GetValue(col);
            if (val is decimal d) return d.ToString("F2");
            if (val is double db) return db.ToString("P2");
            return val?.ToString() ?? "null";
        });
        Console.WriteLine($"   {string.Join(" | ", values)}");
    }
}

