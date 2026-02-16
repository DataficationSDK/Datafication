using Datafication.Storage.Velocity;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Storage.Velocity Computed Columns Sample ===\n");

await RunSampleAsync();

async Task RunSampleAsync()
{
    var dataPath = Path.Combine(Path.GetTempPath(), "velocity_computed_sample");

    // Clean up any stale files from previous runs
    if (Directory.Exists(dataPath))
        Directory.Delete(dataPath, recursive: true);
    Directory.CreateDirectory(dataPath);

    var filePath = Path.Combine(dataPath, "orders.dfc");

    using var velocityBlock = new VelocityDataBlock(filePath);

    velocityBlock.AddColumn(new DataColumn("OrderId", typeof(int)));
    velocityBlock.AddColumn(new DataColumn("ProductName", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("UnitPrice", typeof(double)));
    velocityBlock.AddColumn(new DataColumn("Quantity", typeof(int)));
    velocityBlock.AddColumn(new DataColumn("DiscountPercent", typeof(double)));
    velocityBlock.AddColumn(new DataColumn("Cost", typeof(double)));

    // Add sample order data
    velocityBlock.AddRow(new object[] { 1, "Laptop", 999.99, 2, 10.0, 600.0 });
    velocityBlock.AddRow(new object[] { 2, "Mouse", 29.99, 5, 0.0, 10.0 });
    velocityBlock.AddRow(new object[] { 3, "Keyboard", 79.99, 3, 15.0, 30.0 });
    velocityBlock.AddRow(new object[] { 4, "Monitor", 399.99, 1, 5.0, 200.0 });
    velocityBlock.AddRow(new object[] { 5, "Headphones", 149.99, 4, 20.0, 50.0 });
    velocityBlock.AddRow(new object[] { 6, "USB Hub", 34.99, 10, 0.0, 15.0 });
    velocityBlock.AddRow(new object[] { 7, "Webcam", 89.99, 2, 0.0, 40.0 });
    velocityBlock.AddRow(new object[] { 8, "Speakers", 199.99, 1, 25.0, 80.0 });
    await velocityBlock.FlushAsync();

    Console.WriteLine("Order data (8 orders):\n");

    // 1. Basic arithmetic expression
    Console.WriteLine("1. Basic arithmetic - Subtotal (UnitPrice * Quantity):");
    var withSubtotal = velocityBlock
        .Compute("Subtotal", "UnitPrice * Quantity")
        .Execute();
    PrintDataBlock(withSubtotal, "ProductName", "UnitPrice", "Quantity", "Subtotal");

    // 2. Multiple computed columns
    Console.WriteLine("\n2. Multiple computed columns - Discount and Total:");
    var withTotals = velocityBlock
        .Compute("Subtotal", "UnitPrice * Quantity")
        .Compute("DiscountAmount", "(UnitPrice * Quantity) * (DiscountPercent / 100)")
        .Compute("Total", "(UnitPrice * Quantity) * (1 - DiscountPercent / 100)")
        .Execute();
    PrintDataBlock(withTotals, "ProductName", "Subtotal", "DiscountAmount", "Total");

    // 3. Profit calculation
    Console.WriteLine("\n3. Profit calculation (Revenue - Cost):");
    var withProfit = velocityBlock
        .Compute("Revenue", "UnitPrice * Quantity")
        .Compute("TotalCost", "Cost * Quantity")
        .Compute("Profit", "(UnitPrice * Quantity) - (Cost * Quantity)")
        .Compute("ProfitMargin", "((UnitPrice - Cost) / UnitPrice) * 100")
        .Execute();
    PrintDataBlock(withProfit, "ProductName", "Revenue", "TotalCost", "Profit", "ProfitMargin");

    // 4. Math functions
    Console.WriteLine("\n4. Math functions (ROUND, SQRT, ABS):");
    var withMath = velocityBlock
        .Compute("RoundedPrice", "ROUND(UnitPrice, 0)")
        .Compute("SqrtPrice", "SQRT(UnitPrice)")
        .Compute("AbsDiscount", "ABS(DiscountPercent - 15)")
        .Execute();
    PrintDataBlock(withMath, "ProductName", "UnitPrice", "RoundedPrice", "SqrtPrice", "AbsDiscount");

    // 5. Logical operators
    Console.WriteLine("\n5. Logical operators (boolean expressions):");
    var withLogic = velocityBlock
        .Compute("HasDiscount", "DiscountPercent > 0")
        .Compute("HighValue", "UnitPrice > 100 && Quantity >= 2")
        .Compute("NeedsReview", "DiscountPercent > 15 || UnitPrice > 500")
        .Execute();
    PrintDataBlockLogic(withLogic, "ProductName", "UnitPrice", "DiscountPercent", "HasDiscount", "HighValue", "NeedsReview");

    // 6. Complex logical expression
    Console.WriteLine("\n6. Complex logical expression (VIP orders):");
    var withVip = velocityBlock
        .Compute("Subtotal", "UnitPrice * Quantity")
        .Compute("IsVipOrder", "(UnitPrice * Quantity > 500 && DiscountPercent > 0) || Quantity > 5")
        .Execute();
    PrintDataBlockLogic(withVip, "ProductName", "Subtotal", "Quantity", "IsVipOrder");

    // 7. Filter then compute (filter on source columns, then add computed)
    Console.WriteLine("\n7. Filter then compute - High profit items (margin > 50%):");
    // Note: Filter on source columns first, then compute derived columns
    // ProfitMargin > 50% means (UnitPrice - Cost) / UnitPrice > 0.5
    // Which simplifies to: Cost < UnitPrice * 0.5 (cost is less than half the price)
    var highProfit = velocityBlock
        .Compute("ProfitMargin", "((UnitPrice - Cost) / UnitPrice) * 100")
        .Execute()
        .Where("ProfitMargin", 50.0, ComparisonOperator.GreaterThan);
    PrintDataBlock(highProfit, "ProductName", "UnitPrice", "Cost", "ProfitMargin");

    // 8. Expression validation
    Console.WriteLine("\n8. Expression validation:");
    string error;

    var validExpr = "UnitPrice * Quantity * (1 - DiscountPercent / 100)";
    if (velocityBlock.ValidateExpression(validExpr, out error))
    {
        Console.WriteLine($"   '{validExpr}'");
        Console.WriteLine($"   -> Valid expression");
    }

    var invalidExpr = "UnitPrice ** Quantity";
    if (!velocityBlock.ValidateExpression(invalidExpr, out error))
    {
        Console.WriteLine($"\n   '{invalidExpr}'");
        Console.WriteLine($"   -> Invalid: {error}");
    }

    // 9. Chained computation with window function
    Console.WriteLine("\n9. Compute with window function - Running profit:");
    var withRunning = velocityBlock
        .Compute("Profit", "(UnitPrice - Cost) * Quantity")
        .Window("Profit", WindowFunctionType.CumulativeSum, null, "RunningProfit")
        .Execute();
    PrintDataBlock(withRunning, "ProductName", "Profit", "RunningProfit");

    // Dispose the VelocityDataBlock before cleanup
    velocityBlock.Dispose();

    // Cleanup
    Directory.Delete(dataPath, recursive: true);

    Console.WriteLine("\n=== Sample Complete ===");
    Console.WriteLine("\nSupported Operators:");
    Console.WriteLine("  - Arithmetic: +, -, *, /, %");
    Console.WriteLine("  - Comparison: ==, !=, <, <=, >, >=");
    Console.WriteLine("  - Logical: && (AND), || (OR), ! (NOT)");
    Console.WriteLine("  - Functions: ABS, ROUND, FLOOR, CEIL, SQRT, POWER, EXP, LOG");
}

static void PrintDataBlock(DataBlock data, params string[] columns)
{
    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{col,-16} ");
    }
    Console.WriteLine();

    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{new string('-', 16)} ");
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
                double d when col.Contains("Margin") || col.Contains("Percent") => $"{d:F1}%",
                double d => $"{d:F2}",
                int n => $"{n}",
                _ => value?.ToString() ?? "null"
            };
            Console.Write($"{formatted,-16} ");
        }
        Console.WriteLine();
    }
}

static void PrintDataBlockLogic(DataBlock data, params string[] columns)
{
    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{col,-16} ");
    }
    Console.WriteLine();

    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{new string('-', 16)} ");
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
                double d when col.Contains("Has") || col.Contains("Is") || col.Contains("Needs") || col.Contains("High") =>
                    d == 1 ? "Yes" : "No",
                double d => $"{d:F2}",
                int n => $"{n}",
                _ => value?.ToString() ?? "null"
            };
            Console.Write($"{formatted,-16} ");
        }
        Console.WriteLine();
    }
}
