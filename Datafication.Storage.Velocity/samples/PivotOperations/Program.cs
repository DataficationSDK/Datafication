using Datafication.Core.Data;
using Datafication.Storage.Velocity;

Console.WriteLine("=== VelocityDataBlock Pivot Operations Sample ===\n");

// Create sample sales data in long format
var salesData = new DataBlock();
salesData.AddColumn(new DataColumn("Category", typeof(string)));
salesData.AddColumn(new DataColumn("Region", typeof(string)));
salesData.AddColumn(new DataColumn("Quarter", typeof(string)));
salesData.AddColumn(new DataColumn("Year", typeof(int)));
salesData.AddColumn(new DataColumn("Sales", typeof(double)));
salesData.AddColumn(new DataColumn("Units", typeof(int)));

// Add sample data
salesData.AddRow(new object[] { "Electronics", "East", "Q1", 2024, 15000.0, 150 });
salesData.AddRow(new object[] { "Electronics", "East", "Q2", 2024, 18000.0, 180 });
salesData.AddRow(new object[] { "Electronics", "West", "Q1", 2024, 12000.0, 120 });
salesData.AddRow(new object[] { "Electronics", "West", "Q2", 2024, 14000.0, 140 });
salesData.AddRow(new object[] { "Clothing", "East", "Q1", 2024, 8000.0, 400 });
salesData.AddRow(new object[] { "Clothing", "East", "Q2", 2024, 9500.0, 475 });
salesData.AddRow(new object[] { "Clothing", "West", "Q1", 2024, 7000.0, 350 });
salesData.AddRow(new object[] { "Clothing", "West", "Q2", 2024, 8500.0, 425 });
salesData.AddRow(new object[] { "Electronics", "East", "Q1", 2023, 12000.0, 120 });
salesData.AddRow(new object[] { "Electronics", "West", "Q1", 2023, 10000.0, 100 });
salesData.AddRow(new object[] { "Clothing", "East", "Q1", 2023, 6000.0, 300 });
salesData.AddRow(new object[] { "Clothing", "West", "Q1", 2023, 5500.0, 275 });

// Create temporary file path for Velocity storage
var tempPath = Path.Combine(Path.GetTempPath(), $"pivot_sample_{Guid.NewGuid()}.dfc");

try
{
    Console.WriteLine("Original Sales Data (Long Format):");
    Console.WriteLine($"Rows: {salesData.RowCount}, Columns: {salesData.Schema.Count}");
    PrintDataBlock(salesData);

    // Save to Velocity format
    Console.WriteLine($"\nSaving to Velocity format: {tempPath}");
    using var velocityBlock = await VelocityDataBlock.SaveAsync(tempPath, salesData);
    Console.WriteLine($"Saved {velocityBlock.RowCount} rows to DFC storage\n");

    // Example 1: Basic pivot with deferred execution
    Console.WriteLine("--- Example 1: Basic Pivot (Region -> Columns) with Execute() ---");
    Console.WriteLine("Pivot by Category, spreading Region values as columns with Sum of Sales:\n");

    var pivotedByRegion = velocityBlock
        .Pivot("Category", "Region", "Sales", AggregationType.Sum)
        .Execute();

    PrintDataBlock(pivotedByRegion);

    // Example 2: Chaining Where + Pivot + Execute
    Console.WriteLine("--- Example 2: Where -> Pivot -> Execute Chain ---");
    Console.WriteLine("Filter to 2024, then pivot by Quarter:\n");

    var chainedResult = velocityBlock
        .Where("Year", 2024)
        .Pivot("Category", "Quarter", "Sales", AggregationType.Sum)
        .Execute();

    PrintDataBlock(chainedResult);

    // Example 3: Multiple index columns with Velocity
    Console.WriteLine("--- Example 3: Multiple Index Columns ---");
    Console.WriteLine("Pivot with Year and Category as index, Region as pivot column:\n");

    var multiIndexResult = velocityBlock
        .Pivot(
            indexColumns: new[] { "Year", "Category" },
            pivotColumn: "Region",
            valueColumn: "Sales",
            aggregationType: AggregationType.Sum)
        .Execute();

    PrintDataBlock(multiIndexResult);

    // Example 4: Pivot + Sort + Head
    Console.WriteLine("--- Example 4: Pivot -> Sort -> Head Chain ---");
    Console.WriteLine("Pivot by Region, sort by East_Sales descending, take top 1:\n");

    var sortedPivot = velocityBlock
        .Where("Year", 2024)
        .Pivot("Category", "Region", "Sales", AggregationType.Sum)
        .Sort(SortDirection.Descending, "East_Sales")
        .Head(1)
        .Execute();

    PrintDataBlock(sortedPivot);

    // Example 5: Mean aggregation
    Console.WriteLine("--- Example 5: Mean Aggregation ---");
    Console.WriteLine("Average sales by Category across Quarters:\n");

    var meanResult = velocityBlock
        .Pivot("Category", "Quarter", "Sales", AggregationType.Mean)
        .Execute();

    PrintDataBlock(meanResult);

    Console.WriteLine("\n=== Sample Complete ===");
}
finally
{
    // Clean up temporary file
    if (File.Exists(tempPath))
    {
        try
        {
            File.Delete(tempPath);
            Console.WriteLine($"\nCleaned up temporary file: {tempPath}");
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}

// Helper method to print DataBlock contents
static void PrintDataBlock(DataBlock dataBlock)
{
    // Print header
    var columnNames = dataBlock.Schema.GetColumnNames().ToArray();
    Console.WriteLine(string.Join(" | ", columnNames.Select(c => c.PadRight(15))));
    Console.WriteLine(new string('-', columnNames.Length * 18));

    // Print rows
    var cursor = dataBlock.GetRowCursor(columnNames);
    while (cursor.MoveNext())
    {
        var values = new List<string>();
        foreach (var colName in columnNames)
        {
            var value = cursor.GetValue(colName);
            var displayValue = value?.ToString() ?? "null";
            if (value is double d)
            {
                displayValue = d.ToString("F2");
            }
            values.Add(displayValue.PadRight(15));
        }
        Console.WriteLine(string.Join(" | ", values));
    }
    Console.WriteLine();
}
