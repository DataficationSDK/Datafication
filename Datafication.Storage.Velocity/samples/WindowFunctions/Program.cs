using Datafication.Storage.Velocity;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Storage.Velocity Window Functions Sample ===\n");
Console.WriteLine("SIMD-accelerated window functions for time series analytics\n");

await RunSampleAsync();

async Task RunSampleAsync()
{
    var dataPath = Path.Combine(Path.GetTempPath(), "velocity_window_sample");

    // Clean up any stale files from previous runs
    if (Directory.Exists(dataPath))
        Directory.Delete(dataPath, recursive: true);
    Directory.CreateDirectory(dataPath);

    var filePath = Path.Combine(dataPath, "stock_prices.dfc");

    using var velocityBlock = new VelocityDataBlock(filePath);

    velocityBlock.AddColumn(new DataColumn("Date", typeof(DateTime)));
    velocityBlock.AddColumn(new DataColumn("Symbol", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Price", typeof(double)));
    velocityBlock.AddColumn(new DataColumn("Volume", typeof(long)));

    // Add 15 days of stock data
    var baseDate = new DateTime(2025, 1, 1);
    for (int i = 0; i < 15; i++)
    {
        velocityBlock.AddRow(new object[]
        {
            baseDate.AddDays(i),
            "AAPL",
            150.0 + (i * 2.5) + (i % 3) * 1.5,
            1000000L + (i * 50000)
        });
    }
    await velocityBlock.FlushAsync();

    Console.WriteLine("Stock price data (15 days):\n");

    // 1. Moving Average
    Console.WriteLine("1. Moving Average (3-day window):");
    var withMA = velocityBlock
        .Window("Price", WindowFunctionType.MovingAverage, 3, "MA_3")
        .Execute();
    PrintWindow(withMA, "Date", "Price", "MA_3");

    // 2. Exponential Moving Average
    Console.WriteLine("\n2. Exponential Moving Average (5-day window):");
    var withEMA = velocityBlock
        .Window("Price", WindowFunctionType.ExponentialMovingAverage, 5, "EMA_5")
        .Execute();
    PrintWindow(withEMA, "Date", "Price", "EMA_5");

    // 3. Moving Standard Deviation (Volatility)
    Console.WriteLine("\n3. Moving Standard Deviation (5-day window):");
    var withStdDev = velocityBlock
        .Window("Price", WindowFunctionType.MovingStandardDeviation, 5, "Volatility")
        .Execute();
    PrintWindow(withStdDev, "Date", "Price", "Volatility");

    // 4. Cumulative Sum
    Console.WriteLine("\n4. Cumulative Sum of Volume:");
    var withCumSum = velocityBlock
        .Window("Volume", WindowFunctionType.CumulativeSum, null, "TotalVolume")
        .Execute();
    PrintWindowLong(withCumSum, "Date", "Volume", "TotalVolume");

    // 5. Lag - Previous Day Price
    Console.WriteLine("\n5. Lag Function (Previous Day Price):");
    var withLag = velocityBlock
        .Window("Price", WindowFunctionType.Lag, 1, "PrevPrice", defaultValue: 0.0)
        .Compute("Change", "Price - PrevPrice")
        .Execute();
    PrintWindow(withLag, "Date", "Price", "PrevPrice", "Change");

    // 6. Moving Min and Max
    Console.WriteLine("\n6. Moving Min and Max (5-day window):");
    var withMinMax = velocityBlock
        .Window("Price", WindowFunctionType.MovingMin, 5, "Low_5D")
        .Window("Price", WindowFunctionType.MovingMax, 5, "High_5D")
        .Execute();
    PrintWindow(withMinMax, "Date", "Price", "Low_5D", "High_5D");

    // 7. Chained Window Functions - Technical Analysis
    Console.WriteLine("\n7. Technical Analysis (Bollinger Bands):");
    var technicalAnalysis = velocityBlock
        .Window("Price", WindowFunctionType.MovingAverage, 5, "SMA_5")
        .Window("Price", WindowFunctionType.MovingStandardDeviation, 5, "StdDev_5")
        .DropNulls("StdDev_5") // Window functions return null for initial rows - drop them for Bollinger Bands
        .Compute("BB_Upper", "SMA_5 + (StdDev_5 * 2)")
        .Compute("BB_Lower", "SMA_5 - (StdDev_5 * 2)")
        .Execute();
    PrintWindow(technicalAnalysis, "Date", "Price", "SMA_5", "BB_Upper", "BB_Lower");

    // 8. Ranking Functions
    Console.WriteLine("\n8. Ranking by Price:");
    var withRanks = velocityBlock
        .Window("Price", WindowFunctionType.Rank, null, "Rank")
        .Window(null, WindowFunctionType.RowNumber, null, "RowNum")
        .Execute();
    PrintWindowRank(withRanks, "Date", "Price", "Rank", "RowNum");

    // 9. Partitioned Windows - Multi-Symbol
    Console.WriteLine("\n9. Partitioned Windows (Multiple Symbols):");
    var multiPath = Path.Combine(dataPath, "multi_stock.dfc");
    using var multiBlock = new VelocityDataBlock(multiPath);

    multiBlock.AddColumn(new DataColumn("Date", typeof(DateTime)));
    multiBlock.AddColumn(new DataColumn("Symbol", typeof(string)));
    multiBlock.AddColumn(new DataColumn("Price", typeof(double)));

    for (int i = 0; i < 5; i++)
    {
        multiBlock.AddRow(new object[] { baseDate.AddDays(i), "AAPL", 150.0 + i * 3.0 });
        multiBlock.AddRow(new object[] { baseDate.AddDays(i), "GOOGL", 280.0 + i * 2.0 });
    }
    await multiBlock.FlushAsync();

    var perSymbol = multiBlock
        .Window("Price", WindowFunctionType.MovingAverage, 3, "MA_3",
            partitionByColumns: new[] { "Symbol" })
        .Execute();
    PrintWindow(perSymbol, "Date", "Symbol", "Price", "MA_3");

    // 10. Sales Rolling Analysis
    Console.WriteLine("\n10. Sales Rolling Analysis (7-Day Sum):");
    var salesPath = Path.Combine(dataPath, "sales.dfc");
    using var salesBlock = new VelocityDataBlock(salesPath);

    salesBlock.AddColumn(new DataColumn("Date", typeof(DateTime)));
    salesBlock.AddColumn(new DataColumn("Amount", typeof(double)));

    for (int i = 0; i < 10; i++)
    {
        salesBlock.AddRow(new object[] { baseDate.AddDays(i), 1000.0 + (i * 100) });
    }
    await salesBlock.FlushAsync();

    var salesAnalysis = salesBlock
        .Window("Amount", WindowFunctionType.MovingSum, 7, "Rolling_7D")
        .Window("Amount", WindowFunctionType.CumulativeSum, null, "Cumulative")
        .Execute();
    PrintWindow(salesAnalysis, "Date", "Amount", "Rolling_7D", "Cumulative");

    // Dispose VelocityDataBlocks before cleanup
    velocityBlock.Dispose();
    multiBlock.Dispose();
    salesBlock.Dispose();

    // Cleanup
    Directory.Delete(dataPath, recursive: true);

    Console.WriteLine("\n=== Sample Complete ===");
    Console.WriteLine("\nSIMD-Accelerated Functions:");
    Console.WriteLine("  - MovingAverage, MovingSum, MovingMin, MovingMax");
    Console.WriteLine("  - MovingStandardDeviation, ExponentialMovingAverage");
    Console.WriteLine("  - CumulativeSum, Lag, Lead, Rank, RowNumber");
    Console.WriteLine("  - Performance: 50-150M values/sec with AVX2/AVX-512");
}

static void PrintWindow(DataBlock data, params string[] columns)
{
    int maxRows = columns.Length > 4 ? 6 : 8;

    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{col,-14} ");
    }
    Console.WriteLine();

    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{new string('-', 14)} ");
    }
    Console.WriteLine();

    for (int i = 0; i < Math.Min(maxRows, data.RowCount); i++)
    {
        Console.Write("   ");
        foreach (var col in columns)
        {
            var value = data[i, col];
            string formatted = FormatValue(value, col);
            Console.Write($"{formatted,-14} ");
        }
        Console.WriteLine();
    }

    if (data.RowCount > maxRows)
    {
        Console.WriteLine($"   ... ({data.RowCount - maxRows} more rows)");
    }
}

static void PrintWindowLong(DataBlock data, params string[] columns)
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

    for (int i = 0; i < Math.Min(6, data.RowCount); i++)
    {
        Console.Write("   ");
        foreach (var col in columns)
        {
            var value = data[i, col];
            string formatted = value switch
            {
                DateTime dt => dt.ToString("MM/dd/yyyy"),
                long l => $"{l:N0}",
                double d => $"{d:N0}",
                _ => value?.ToString() ?? "null"
            };
            Console.Write($"{formatted,-16} ");
        }
        Console.WriteLine();
    }

    if (data.RowCount > 6)
    {
        Console.WriteLine($"   ... ({data.RowCount - 6} more rows)");
    }
}

static void PrintWindowRank(DataBlock data, params string[] columns)
{
    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{col,-14} ");
    }
    Console.WriteLine();

    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{new string('-', 14)} ");
    }
    Console.WriteLine();

    for (int i = 0; i < Math.Min(8, data.RowCount); i++)
    {
        Console.Write("   ");
        foreach (var col in columns)
        {
            var value = data[i, col];
            string formatted = value switch
            {
                DateTime dt => dt.ToString("MM/dd/yyyy"),
                double d when col == "Price" => $"{d:F2}",
                double d => $"{d:F0}",
                _ => value?.ToString() ?? "null"
            };
            Console.Write($"{formatted,-14} ");
        }
        Console.WriteLine();
    }

    if (data.RowCount > 8)
    {
        Console.WriteLine($"   ... ({data.RowCount - 8} more rows)");
    }
}

static string FormatValue(object? value, string columnName)
{
    if (value == null) return "null";

    return value switch
    {
        DateTime dt => dt.ToString("MM/dd/yyyy"),
        double d when columnName.Contains("Pct") => $"{d:F2}%",
        double d => $"{d:F2}",
        long l => $"{l:N0}",
        _ => value.ToString() ?? "null"
    };
}
