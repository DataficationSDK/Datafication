using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core Window Functions Sample ===\n");

// Create sample stock price data
var stockData = new DataBlock();
stockData.AddColumn(new DataColumn("Date", typeof(DateTime)));
stockData.AddColumn(new DataColumn("Symbol", typeof(string)));
stockData.AddColumn(new DataColumn("Price", typeof(double)));
stockData.AddColumn(new DataColumn("Volume", typeof(long)));

// Add 15 days of sample data for AAPL
for (int i = 0; i < 15; i++)
{
    stockData.AddRow(new object[]
    {
        new DateTime(2025, 1, 1).AddDays(i),
        "AAPL",
        100.0 + (i * 5.0) + (i % 3) * 2.0,  // Trending price with variation
        1000000 + (i * 50000)
    });
}

Console.WriteLine($"Created stock data with {stockData.RowCount} rows\n");

// 1. Moving Average
Console.WriteLine("1. Moving Average (3-day window):");
var withMA = stockData.Window("Price", WindowFunctionType.MovingAverage, 3, "MA_3");
PrintWindow(withMA, "Date", "Price", "MA_3");

// 2. Exponential Moving Average
Console.WriteLine("\n2. Exponential Moving Average (5-day window):");
var withEMA = stockData.Window("Price", WindowFunctionType.ExponentialMovingAverage, 5, "EMA_5");
PrintWindow(withEMA, "Date", "Price", "EMA_5");

// 3. Moving Standard Deviation (Volatility)
Console.WriteLine("\n3. Moving Standard Deviation (5-day window):");
var withStdDev = stockData.Window("Price", WindowFunctionType.MovingStandardDeviation, 5, "StdDev_5");
PrintWindow(withStdDev, "Date", "Price", "StdDev_5");

// 4. Cumulative Sum (Running Total)
Console.WriteLine("\n4. Cumulative Sum of Volume:");
var withCumSum = stockData.Window("Volume", WindowFunctionType.CumulativeSum, null, "TotalVolume");
PrintWindow(withCumSum, "Date", "Volume", "TotalVolume");

// 5. Lag (Previous Day Comparison)
Console.WriteLine("\n5. Lag Function (Previous Day Price):");
var withLag = stockData
    .Window("Price", WindowFunctionType.Lag, 1, "PrevPrice", defaultValue: 0.0)
    .Compute("Change", "Price - PrevPrice")
    .Compute("ChangePct", "(Price - PrevPrice) / PrevPrice * 100");
PrintWindow(withLag, "Date", "Price", "PrevPrice", "Change", "ChangePct");

// 6. Moving Min and Max (Support/Resistance)
Console.WriteLine("\n6. Moving Min and Max (5-day window):");
var withMinMax = stockData
    .Window("Price", WindowFunctionType.MovingMin, 5, "Low_5D")
    .Window("Price", WindowFunctionType.MovingMax, 5, "High_5D");
PrintWindow(withMinMax, "Date", "Price", "Low_5D", "High_5D");

// 7. Moving Median
Console.WriteLine("\n7. Moving Median (5-day window):");
var withMedian = stockData.Window("Price", WindowFunctionType.MovingMedian, 5, "Median_5");
PrintWindow(withMedian, "Date", "Price", "Median_5");

// 8. Chaining Multiple Window Functions (Technical Analysis)
Console.WriteLine("\n8. Technical Analysis (Multiple Indicators):");
var technicalAnalysis = stockData
    .Window("Price", WindowFunctionType.MovingAverage, 5, "SMA_5")
    .Window("Price", WindowFunctionType.MovingAverage, 10, "SMA_10")
    .Window("Price", WindowFunctionType.ExponentialMovingAverage, 5, "EMA_5")
    .Window("Price", WindowFunctionType.MovingStandardDeviation, 5, "StdDev_5")
    .Compute("BB_Upper", "SMA_5 + (StdDev_5 * 2)")
    .Compute("BB_Lower", "SMA_5 - (StdDev_5 * 2)");
Console.WriteLine("   Columns: Date, Price, SMA_5, SMA_10, EMA_5, BB_Upper, BB_Lower");
PrintWindow(technicalAnalysis, "Date", "Price", "SMA_5", "BB_Upper", "BB_Lower");

// 9. Ranking Functions
Console.WriteLine("\n9. Ranking by Price:");
var withRanks = stockData
    .Window("Price", WindowFunctionType.Rank, null, "Rank")
    .Window("Price", WindowFunctionType.DenseRank, null, "DenseRank")
    .Window(null, WindowFunctionType.RowNumber, null, "RowNum");
PrintWindow(withRanks, "Date", "Price", "Rank", "DenseRank", "RowNum");

// 10. Partitioned Windows (Multi-Symbol Analysis)
Console.WriteLine("\n10. Partitioned Windows (Multiple Symbols):");
var multiSymbolData = new DataBlock();
multiSymbolData.AddColumn(new DataColumn("Date", typeof(DateTime)));
multiSymbolData.AddColumn(new DataColumn("Symbol", typeof(string)));
multiSymbolData.AddColumn(new DataColumn("Price", typeof(double)));

// Add data for AAPL and GOOGL
for (int i = 0; i < 5; i++)
{
    multiSymbolData.AddRow(new object[] { new DateTime(2025, 1, 1).AddDays(i), "AAPL", 100.0 + i * 5.0 });
    multiSymbolData.AddRow(new object[] { new DateTime(2025, 1, 1).AddDays(i), "GOOGL", 200.0 + i * 3.0 });
}

var perSymbol = multiSymbolData.Window(
    "Price",
    WindowFunctionType.MovingAverage,
    3,
    "MA_3",
    partitionByColumns: new[] { "Symbol" }
);
PrintWindow(perSymbol, "Date", "Symbol", "Price", "MA_3");

// 11. Sales Analytics Example
Console.WriteLine("\n11. Sales Analytics (7-Day Rolling Total):");
var salesData = new DataBlock();
salesData.AddColumn(new DataColumn("Date", typeof(DateTime)));
salesData.AddColumn(new DataColumn("Amount", typeof(decimal)));

for (int i = 0; i < 10; i++)
{
    salesData.AddRow(new object[] { new DateTime(2025, 1, 1).AddDays(i), 1000m + (i * 100m) });
}

var salesAnalysis = salesData
    .Window("Amount", WindowFunctionType.MovingSum, 7, "Rolling_7D")
    .Window("Amount", WindowFunctionType.CumulativeSum, null, "CumulativeTotal");
PrintWindow(salesAnalysis, "Date", "Amount", "Rolling_7D", "CumulativeTotal");

Console.WriteLine("\n=== Sample Complete ===");
Console.WriteLine("\nKey Takeaways:");
Console.WriteLine("  - Window functions preserve all rows (unlike GROUP BY)");
Console.WriteLine("  - Partial windows are included (first rows use available data)");
Console.WriteLine("  - Use partitionBy to calculate per category");
Console.WriteLine("  - Chain multiple window functions for complex analysis");
Console.WriteLine("  - For datasets > 100K rows, use VelocityDataBlock for 10-30x performance");

static void PrintWindow(DataBlock data, params string[] columns)
{
    int maxRows = columns.Length > 4 ? 5 : 10;  // Show fewer rows if many columns

    // Print header
    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{col,-15} ");
    }
    Console.WriteLine();

    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{new string('-', 15)} ");
    }
    Console.WriteLine();

    // Print data rows
    for (int i = 0; i < Math.Min(maxRows, data.RowCount); i++)
    {
        Console.Write("   ");
        foreach (var col in columns)
        {
            var value = data[i, col];
            string formatted = FormatValue(value, col);
            Console.Write($"{formatted,-15} ");
        }
        Console.WriteLine();
    }

    if (data.RowCount > maxRows)
    {
        Console.WriteLine($"   ... ({data.RowCount - maxRows} more rows)");
    }
}

static string FormatValue(object? value, string columnName)
{
    if (value == null)
        return "null";

    return value switch
    {
        DateTime dt => dt.ToString("MM/dd/yyyy"),
        double d when columnName.Contains("Pct") => $"{d:F2}%",
        double d => $"{d:F2}",
        decimal dec => $"${dec:N0}",
        long l => $"{l:N0}",
        _ => value.ToString() ?? "null"
    };
}
