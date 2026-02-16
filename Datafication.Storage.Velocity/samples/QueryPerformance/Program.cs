using Datafication.Core.Data;
using Datafication.Storage.Velocity;
using System.Diagnostics;
using System.IO.Compression;

Console.WriteLine("=== Datafication.Storage.Velocity Query Performance Sample ===\n");

await RunBenchmarkAsync(args);

async Task RunBenchmarkAsync(string[] args)
{
    // Parse scale factor from command line (default: sf1)
    var scaleFactor = args.Length > 0 ? args[0].ToLowerInvariant() : "sf1";

    var validScaleFactors = new Dictionary<string, (string FileName, string Url, string Description)>
    {
        ["sf005"] = ("test_SF005.dfc", "https://datafication.co/assets/sf005/test_SF005.dfc.gz", "~5K rows"),
        ["sf1"] = ("test_SF1.dfc", "https://datafication.co/assets/sf1/test_SF1.dfc.gz", "~1M rows"),
        ["sf20"] = ("test_SF20.dfc", "https://datafication.co/assets/sf20/test_SF20.dfc.gz", "~20M rows"),
        ["sf50"] = ("test_SF50.dfc", "https://datafication.co/assets/sf50/test_SF50.dfc.gz", "~50M rows"),
        ["sf100"] = ("test_SF100.dfc", "https://datafication.co/assets/sf100/test_SF100.dfc.gz", "~100M rows")
    };

    if (!validScaleFactors.TryGetValue(scaleFactor, out var config))
    {
        Console.WriteLine($"Invalid scale factor: {scaleFactor}");
        Console.WriteLine("Valid options: sf005, sf1, sf20, sf50, sf100");
        Console.WriteLine("\nUsage: dotnet run [scale_factor]");
        Console.WriteLine("  sf005  - Small dataset (~5K rows)");
        Console.WriteLine("  sf1    - Medium dataset (~1M rows) [default]");
        Console.WriteLine("  sf20   - Large dataset (~20M rows)");
        Console.WriteLine("  sf50   - Very large dataset (~50M rows)");
        Console.WriteLine("  sf100  - Massive dataset (~100M rows)");
        return;
    }

    Console.WriteLine($"Scale Factor: {scaleFactor.ToUpperInvariant()} ({config.Description})");

    // Ensure data directory exists
    var dataPath = Path.Combine(AppContext.BaseDirectory, "data");
    Directory.CreateDirectory(dataPath);

    var dfcFilePath = Path.Combine(dataPath, config.FileName);

    // Download the dataset if it doesn't exist
    if (!File.Exists(dfcFilePath))
    {
        var gzFilePath = dfcFilePath + ".gz";

        Console.WriteLine($"\nDownloading dataset from {config.Url}...");

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(30);

        try
        {
            // Download the compressed file
            {
                var response = await httpClient.GetAsync(config.Url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength;

                await using var contentStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(gzFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalRead += bytesRead;

                    if (totalBytes.HasValue)
                    {
                        var progress = (double)totalRead / totalBytes.Value * 100;
                        Console.Write($"\rDownloading: {progress:F1}% ({totalRead:N0} / {totalBytes.Value:N0} bytes)");
                    }
                }
            }

            Console.WriteLine("\nDownload complete.");

            // Decompress the gzip file
            Console.WriteLine("Decompressing...");

            {
                await using var gzFileStream = new FileStream(gzFilePath, FileMode.Open, FileAccess.Read);
                await using var gzipStream = new GZipStream(gzFileStream, CompressionMode.Decompress);
                await using var outputStream = new FileStream(dfcFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                await gzipStream.CopyToAsync(outputStream);
            }

            Console.WriteLine($"Decompression complete: {dfcFilePath}");

            // Delete the compressed file
            File.Delete(gzFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError downloading dataset: {ex.Message}");
            Console.WriteLine("Please check your internet connection and try again.");

            // Clean up partial files
            if (File.Exists(gzFilePath)) File.Delete(gzFilePath);
            if (File.Exists(dfcFilePath)) File.Delete(dfcFilePath);

            return;
        }
    }
    else
    {
        Console.WriteLine($"Using cached dataset: {dfcFilePath}");
    }

    // Load the dataset
    Console.WriteLine("\nLoading dataset...");
    var loadSw = Stopwatch.StartNew();

    using var velocityDataBlock = await VelocityDataBlock.OpenAsync(dfcFilePath);

    loadSw.Stop();
    Console.WriteLine($"Dataset loaded in {loadSw.ElapsedMilliseconds:N0} ms");
    Console.WriteLine($"Total rows: {velocityDataBlock.RowCount:N0}");

    // Define the complex query
    Func<dynamic> BuildQuery = () =>
        velocityDataBlock
            .Select("Total Profit", "Total Cost", "Country")
            .Where("Total Profit", 4_000_000.0, ComparisonOperator.GreaterThan)
            .Where("Country", "United States", ComparisonOperator.Equals)
            .Sort(SortDirection.Descending, "Total Profit");

    Console.WriteLine("\n--- Query Definition ---");
    Console.WriteLine("  SELECT \"Total Profit\", \"Total Cost\", \"Country\"");
    Console.WriteLine("  WHERE \"Total Profit\" > 4,000,000");
    Console.WriteLine("  AND \"Country\" = 'United States'");
    Console.WriteLine("  ORDER BY \"Total Profit\" DESC");

    // Warm-up run
    Console.WriteLine("\n--- Warm-up ---");
    var warmupQuery = BuildQuery();
    var warmupResult = warmupQuery.Execute();
    Console.WriteLine($"Warm-up complete. Result rows: {warmupResult.RowCount:N0}");

    // Force garbage collection before benchmarking
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    // Additional warm-up to stabilize JIT
    _ = BuildQuery().Execute();

    // Benchmark configuration
    int iterations = 10;
    var perRunElapsedMs = new List<double>(iterations);
    var sw = new Stopwatch();
    long totalElapsedMs = 0;
    long totalRows = velocityDataBlock.RowCount;

    Console.WriteLine($"\n--- Benchmark ({iterations} iterations) ---");

    for (int i = 0; i < iterations; i++)
    {
        sw.Restart();
        var _ = BuildQuery().Execute();
        sw.Stop();

        double elapsedMs = Math.Max(0.001, sw.Elapsed.TotalMilliseconds);
        perRunElapsedMs.Add(elapsedMs);
        totalElapsedMs += sw.ElapsedMilliseconds;

        Console.WriteLine($"  Run {i + 1,2}: {elapsedMs,8:F2} ms ({totalRows / elapsedMs:N0} rows/ms)");
    }

    // Calculate statistics
    double avgMs = perRunElapsedMs.Average();
    double minMs = perRunElapsedMs.Min();
    double maxMs = perRunElapsedMs.Max();

    var sortedTimes = perRunElapsedMs.OrderBy(x => x).ToList();
    double medianMs = iterations % 2 == 0
        ? (sortedTimes[iterations / 2 - 1] + sortedTimes[iterations / 2]) / 2
        : sortedTimes[iterations / 2];

    double variance = perRunElapsedMs.Select(x => Math.Pow(x - avgMs, 2)).Sum() / iterations;
    double stdDevMs = Math.Sqrt(variance);

    double avgRowsPerMs = perRunElapsedMs
        .Select(ms => totalRows / ms)
        .Average();

    double overallRowsPerMs = totalElapsedMs > 0
        ? (iterations * (double)totalRows) / totalElapsedMs
        : 0;

    // Output results
    Console.WriteLine("\n--- Results ---");
    Console.WriteLine($"  Dataset size:      {totalRows:N0} rows");
    Console.WriteLine($"  Iterations:        {iterations}");
    Console.WriteLine();
    Console.WriteLine("  Timing Statistics:");
    Console.WriteLine($"    Average:         {avgMs:N2} ms (±{stdDevMs:N2} std dev)");
    Console.WriteLine($"    Median:          {medianMs:N2} ms");
    Console.WriteLine($"    Min:             {minMs:N2} ms");
    Console.WriteLine($"    Max:             {maxMs:N2} ms");
    Console.WriteLine();
    Console.WriteLine("  Throughput:");
    Console.WriteLine($"    Per-run average: {avgRowsPerMs:N0} rows/ms");
    Console.WriteLine($"    Overall:         {overallRowsPerMs:N0} rows/ms");

    Console.WriteLine("\n=== Benchmark Complete ===");
}
