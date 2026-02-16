using Datafication.Storage.Velocity;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Storage.Velocity Grouping & Aggregation Sample ===\n");

await RunSampleAsync();

async Task RunSampleAsync()
{
    var dataPath = Path.Combine(Path.GetTempPath(), "velocity_grouping_sample");

    // Clean up any stale files from previous runs
    if (Directory.Exists(dataPath))
        Directory.Delete(dataPath, recursive: true);
    Directory.CreateDirectory(dataPath);

    var filePath = Path.Combine(dataPath, "sales.dfc");

    using var velocityBlock = new VelocityDataBlock(filePath);

    velocityBlock.AddColumn(new DataColumn("OrderId", typeof(int)));
    velocityBlock.AddColumn(new DataColumn("Region", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Category", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Amount", typeof(decimal)));
    velocityBlock.AddColumn(new DataColumn("Quantity", typeof(int)));

    // Add sample sales data
    velocityBlock.AddRow(new object[] { 1, "North", "Electronics", 1200.00m, 2 });
    velocityBlock.AddRow(new object[] { 2, "South", "Clothing", 350.00m, 5 });
    velocityBlock.AddRow(new object[] { 3, "North", "Electronics", 899.99m, 1 });
    velocityBlock.AddRow(new object[] { 4, "East", "Furniture", 2500.00m, 1 });
    velocityBlock.AddRow(new object[] { 5, "South", "Electronics", 450.00m, 3 });
    velocityBlock.AddRow(new object[] { 6, "North", "Clothing", 175.50m, 4 });
    velocityBlock.AddRow(new object[] { 7, "West", "Furniture", 1800.00m, 2 });
    velocityBlock.AddRow(new object[] { 8, "East", "Electronics", 650.00m, 2 });
    velocityBlock.AddRow(new object[] { 9, "West", "Clothing", 280.00m, 3 });
    velocityBlock.AddRow(new object[] { 10, "North", "Furniture", 3200.00m, 1 });
    velocityBlock.AddRow(new object[] { 11, "South", "Furniture", 1500.00m, 1 });
    velocityBlock.AddRow(new object[] { 12, "East", "Clothing", 420.00m, 6 });
    await velocityBlock.FlushAsync();

    Console.WriteLine("Sample sales data (12 orders):\n");

    // 1. GroupBy single column with count
    Console.WriteLine("1. GroupBy Region (with order count):");
    var byRegion = velocityBlock
        .GroupByAggregate("Region", "OrderId", AggregationType.Count, "OrderCount")
        .Execute();
    for (int i = 0; i < byRegion.RowCount; i++)
    {
        var region = byRegion[i, "Region"];
        var count = byRegion[i, "OrderCount"];
        Console.WriteLine($"   {region}: {count} orders");
    }

    // 2. GroupByAggregate with single aggregation
    Console.WriteLine("\n2. GroupByAggregate - Sum of Amount by Region:");
    var sumByRegion = velocityBlock
        .GroupByAggregate("Region", "Amount", AggregationType.Sum, "TotalSales")
        .Execute();
    PrintDataBlock(sumByRegion, "Region", "TotalSales");

    // 3. GroupByAggregate with Mean
    Console.WriteLine("\n3. Average Amount by Category:");
    var avgByCategory = velocityBlock
        .GroupByAggregate("Category", "Amount", AggregationType.Mean, "AvgAmount")
        .Execute();
    PrintDataBlock(avgByCategory, "Category", "AvgAmount");

    // 4. Multiple aggregations
    Console.WriteLine("\n4. Multiple aggregations by Region:");
    var aggregations = new Dictionary<string, AggregationType>
    {
        ["Amount"] = AggregationType.Sum,
        ["Quantity"] = AggregationType.Sum,
        ["OrderId"] = AggregationType.Count
    };
    var multiAgg = velocityBlock
        .GroupByAggregate("Region", aggregations)
        .Execute();
    PrintDataBlock(multiAgg, "Region", "sum_Amount", "sum_Quantity", "count_OrderId");

    // 5. Min and Max aggregations
    Console.WriteLine("\n5. Min and Max Amount by Category:");
    var minMax = new Dictionary<string, AggregationType>
    {
        ["Amount"] = AggregationType.Min,
    };
    var minByCategory = velocityBlock
        .GroupByAggregate("Category", minMax)
        .Execute();

    var maxAgg = new Dictionary<string, AggregationType>
    {
        ["Amount"] = AggregationType.Max,
    };
    var maxByCategory = velocityBlock
        .GroupByAggregate("Category", maxAgg)
        .Execute();

    Console.WriteLine("   Category         Min Amount       Max Amount");
    Console.WriteLine("   ---------------  ---------------  ---------------");
    for (int i = 0; i < minByCategory.RowCount; i++)
    {
        var category = minByCategory[i, "Category"];
        var minAmt = minByCategory[i, "min_Amount"];
        var maxAmt = maxByCategory[i, "max_Amount"];
        Console.WriteLine($"   {category,-15}  {minAmt,15:C}  {maxAmt,15:C}");
    }

    // 6. Dataset-wide aggregations
    Console.WriteLine("\n6. Dataset-wide statistics:");
    var amountData = velocityBlock.Select("Amount").Execute();
    Console.WriteLine($"   Total orders: {velocityBlock.RowCount}");
    Console.WriteLine($"   Min Amount: {amountData.Min()[0, "Amount"]:C}");
    Console.WriteLine($"   Max Amount: {amountData.Max()[0, "Amount"]:C}");
    Console.WriteLine($"   Mean Amount: {amountData.Mean()[0, "Amount"]:C}");
    Console.WriteLine($"   Std Deviation: {amountData.StandardDeviation()[0, "Amount"]:C}");

    // 7. Filtered grouping
    Console.WriteLine("\n7. Filtered grouping - Electronics sales by Region:");
    var electronicsbyRegion = velocityBlock
        .Where("Category", "Electronics")
        .GroupByAggregate("Region", "Amount", AggregationType.Sum, "ElectronicsSales")
        .Execute();
    PrintDataBlock(electronicsbyRegion, "Region", "ElectronicsSales");

    // 8. Sorted aggregation results
    Console.WriteLine("\n8. Top regions by total sales:");
    var topRegions = velocityBlock
        .GroupByAggregate("Region", "Amount", AggregationType.Sum, "TotalSales")
        .Sort(SortDirection.Descending, "TotalSales")
        .Execute();
    PrintDataBlock(topRegions, "Region", "TotalSales");

    // Dispose the VelocityDataBlock before cleanup
    velocityBlock.Dispose();

    // Cleanup
    Directory.Delete(dataPath, recursive: true);

    Console.WriteLine("\n=== Sample Complete ===");
    Console.WriteLine("\nAggregation Types:");
    Console.WriteLine("  - Count, Sum, Mean, Min, Max");
    Console.WriteLine("  - StandardDeviation, Variance");
}

static void PrintDataBlock(DataBlock data, params string[] columns)
{
    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{col,-18} ");
    }
    Console.WriteLine();

    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{new string('-', 18)} ");
    }
    Console.WriteLine();

    for (int i = 0; i < data.RowCount; i++)
    {
        Console.Write("   ");
        foreach (var col in columns)
        {
            var value = data[i, col];
            string formatted = value switch
            {
                decimal d => $"{d:C}",
                double d => $"{d:F2}",
                _ => value?.ToString() ?? "null"
            };
            Console.Write($"{formatted,-18} ");
        }
        Console.WriteLine();
    }
}
