using Datafication.Core.Data;

Console.WriteLine("=== Pivot Operations Sample ===\n");

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

Console.WriteLine("Original Sales Data (Long Format):");
Console.WriteLine($"Rows: {salesData.RowCount}, Columns: {salesData.ColumnCount}");
PrintDataBlock(salesData);

// Example 1: Basic pivot - Region becomes columns
Console.WriteLine("\n--- Example 1: Basic Pivot (Region -> Columns) ---");
Console.WriteLine("Pivot by Category, spreading Region values as columns with Sum of Sales:\n");

var pivotedByRegion = salesData.Pivot(
    indexColumn: "Category",
    pivotColumn: "Region",
    valueColumn: "Sales",
    aggregationType: AggregationType.Sum
);

PrintDataBlock(pivotedByRegion);

// Example 2: Pivot with different aggregation types
Console.WriteLine("\n--- Example 2: Pivot with Mean Aggregation ---");
Console.WriteLine("Average sales by Category across Regions:\n");

var pivotedMean = salesData.Pivot(
    indexColumn: "Category",
    pivotColumn: "Region",
    valueColumn: "Sales",
    aggregationType: AggregationType.Mean
);

PrintDataBlock(pivotedMean);

// Example 3: Multiple index columns
Console.WriteLine("\n--- Example 3: Multiple Index Columns ---");
Console.WriteLine("Pivot with Year and Category as index, Quarter as pivot column:\n");

var pivotedMultiIndex = salesData.Pivot(
    indexColumns: new[] { "Year", "Category" },
    pivotColumn: "Quarter",
    valueColumn: "Sales",
    aggregationType: AggregationType.Sum
);

PrintDataBlock(pivotedMultiIndex);

// Example 4: Count aggregation
Console.WriteLine("\n--- Example 4: Count Aggregation ---");
Console.WriteLine("Count of records by Category and Region:\n");

var pivotedCount = salesData.Pivot(
    indexColumn: "Category",
    pivotColumn: "Region",
    valueColumn: "Sales",
    aggregationType: AggregationType.Count
);

PrintDataBlock(pivotedCount);

// Example 5: Pivot Units instead of Sales
Console.WriteLine("\n--- Example 5: Pivot Different Value Column (Units) ---");
Console.WriteLine("Sum of Units sold by Category across Regions:\n");

var pivotedUnits = salesData.Pivot(
    indexColumn: "Category",
    pivotColumn: "Region",
    valueColumn: "Units",
    aggregationType: AggregationType.Sum
);

PrintDataBlock(pivotedUnits);

// Example 6: Chaining with other operations
Console.WriteLine("\n--- Example 6: Chaining Pivot with Where and Sort ---");
Console.WriteLine("Filter to 2024, pivot by Quarter, and sort by Q1 Sales descending:\n");

var chainedResult = salesData
    .Where("Year", 2024)
    .Pivot("Category", "Quarter", "Sales", AggregationType.Sum)
    .Sort(SortDirection.Descending, "Q1_Sales");

PrintDataBlock(chainedResult);

// Example 7: Min/Max aggregation
Console.WriteLine("\n--- Example 7: Min and Max Aggregations ---");
Console.WriteLine("Minimum sales by Category across Regions:\n");

var pivotedMin = salesData.Pivot(
    indexColumn: "Category",
    pivotColumn: "Region",
    valueColumn: "Sales",
    aggregationType: AggregationType.Min
);

PrintDataBlock(pivotedMin);

Console.WriteLine("Maximum sales by Category across Regions:\n");

var pivotedMax = salesData.Pivot(
    indexColumn: "Category",
    pivotColumn: "Region",
    valueColumn: "Sales",
    aggregationType: AggregationType.Max
);

PrintDataBlock(pivotedMax);

Console.WriteLine("\n=== Sample Complete ===");

// Helper method to print DataBlock contents
static void PrintDataBlock(DataBlock dataBlock)
{
    // Print header
    var columnNames = dataBlock.GetColumnNames().ToArray();
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
