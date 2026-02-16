using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core Expressions and Computed Columns Sample ===\n");

// Create sample sales data
var sales = new DataBlock();
sales.AddColumn(new DataColumn("ProductId", typeof(int)));
sales.AddColumn(new DataColumn("ProductName", typeof(string)));
sales.AddColumn(new DataColumn("Price", typeof(decimal)));
sales.AddColumn(new DataColumn("Quantity", typeof(int)));
sales.AddColumn(new DataColumn("Cost", typeof(decimal)));

sales.AddRow(new object[] { 1, "Widget A", 10.50m, 100, 5.00m });
sales.AddRow(new object[] { 2, "Widget B", 15.75m, 50, 8.00m });
sales.AddRow(new object[] { 3, "Widget C", 20.00m, 75, 12.00m });
sales.AddRow(new object[] { 4, "Widget D", 8.25m, 200, 4.00m });
sales.AddRow(new object[] { 5, "Widget E", 25.50m, 30, 15.00m });

Console.WriteLine("Original DataBlock:");
PrintDataBlock(sales);

// 1. Basic computed column with arithmetic
var withRevenue = sales.Compute("Revenue", "Price * Quantity");
Console.WriteLine("\n1. Compute('Revenue', 'Price * Quantity'):");
PrintDataBlock(withRevenue.Select("ProductName", "Price", "Quantity", "Revenue"));

// 2. Complex expressions with multiple operations
var withProfit = sales
    .Compute("Revenue", "Price * Quantity")
    .Compute("TotalCost", "Cost * Quantity")
    .Compute("Profit", "Revenue - TotalCost")
    .Compute("ProfitMargin", "Profit / Revenue");
Console.WriteLine("\n2. Complex expressions - Revenue, TotalCost, Profit, ProfitMargin:");
PrintDataBlock(withProfit.Select("ProductName", "Revenue", "TotalCost", "Profit", "ProfitMargin"));

// 3. Expression validation
Console.WriteLine("\n3. Expression Validation:");
if (sales.ValidateExpression("Price * 0.15", out string error))
{
    Console.WriteLine("   Expression 'Price * 0.15' is valid");
    var withDiscount = sales.Compute("Discount", "Price * 0.15");
    PrintDataBlock(withDiscount.Select("ProductName", "Price", "Discount"));
}
else
{
    Console.WriteLine($"   Invalid expression: {error}");
}

if (sales.ValidateExpression("InvalidColumn * 2", out string error2))
{
    Console.WriteLine("   Expression is valid");
}
else
{
    Console.WriteLine($"   Invalid expression: {error2}");
}

// 4. Using computed columns in pipelines
var analysis = sales
    .Compute("Revenue", "Price * Quantity")
    .Compute("TotalCost", "Cost * Quantity")
    .Compute("Profit", "Revenue - TotalCost")
    .Where("Profit", 500m, ComparisonOperator.GreaterThan)
    .Select("ProductName", "Price", "Quantity", "Revenue", "TotalCost", "Profit")
    .Sort(SortDirection.Descending, "Profit");
Console.WriteLine("\n4. Using computed columns in pipelines:");
Console.WriteLine("   Compute -> Where -> Select -> Sort:");
PrintDataBlock(analysis);

// 5. Expression operators demonstration
var withCalculations = sales
    .Compute("DoublePrice", "Price * 2")
    .Compute("HalfPrice", "Price / 2")
    .Compute("PricePlusTen", "Price + 10")
    .Compute("PriceMinusFive", "Price - 5")
    .Compute("PriceModThree", "Price % 3")
    .Compute("IsExpensive", "Price > 15")
    .Compute("IsCheap", "Price < 10")
    .Compute("PriceEqualsTen", "Price == 10.50")
    .Compute("PriceNotEqualsTen", "Price != 10.50");
Console.WriteLine("\n5. Expression operators demonstration:");
PrintDataBlock(withCalculations.Select("ProductName", "Price", "DoublePrice", "HalfPrice", "PricePlusTen", "PriceMinusFive", "PriceModThree", "IsExpensive", "IsCheap"));

// 6. Logical operators
var withLogic = sales
    .Compute("HighPriceHighQuantity", "Price > 15 && Quantity > 50")
    .Compute("LowPriceOrHighQuantity", "Price < 10 || Quantity > 100")
    .Compute("NotExpensive", "!(Price > 20)");
Console.WriteLine("\n6. Logical operators (&&, ||, !):");
PrintDataBlock(withLogic.Select("ProductName", "Price", "Quantity", "HighPriceHighQuantity", "LowPriceOrHighQuantity", "NotExpensive"));

// 7. Parentheses for grouping
var withGrouping = sales
    .Compute("ComplexCalc", "(Price - Cost) * Quantity")
    .Compute("GroupedCalc", "Price * (Quantity + 10)");
Console.WriteLine("\n7. Parentheses for grouping:");
PrintDataBlock(withGrouping.Select("ProductName", "Price", "Cost", "Quantity", "ComplexCalc", "GroupedCalc"));

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
            if (val is bool b) return b.ToString();
            return val?.ToString() ?? "null";
        });
        Console.WriteLine($"   {string.Join(" | ", values)}");
    }
}

