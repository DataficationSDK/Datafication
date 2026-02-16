using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;
using Datafication.Storage.Velocity;

Console.WriteLine("=== Datafication.S3Connector Wildcard Pattern Sample ===\n");
Console.WriteLine("This sample demonstrates loading multiple files using prefix patterns");
Console.WriteLine("from the NOAA Global Historical Climatology Network (GHCN) dataset.\n");

// Output paths
var dataPath = Path.Combine(Path.GetTempPath(), "s3_wildcard_samples");
if (Directory.Exists(dataPath))
    Directory.Delete(dataPath, recursive: true);
Directory.CreateDirectory(dataPath);

try
{
    // 1. About NOAA GHCN Data
    Console.WriteLine("1. NOAA GHCN Dataset");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   The Global Historical Climatology Network provides daily climate");
    Console.WriteLine("   observations from land surface stations worldwide since 1763.");
    Console.WriteLine();
    Console.WriteLine("   Bucket: noaa-ghcn-pds");
    Console.WriteLine("   Region: us-east-1");
    Console.WriteLine("   Path:   csv/by_year/");
    Console.WriteLine();
    Console.WriteLine("   File naming: YYYY.csv (e.g., 2020.csv, 2021.csv, 2022.csv)");
    Console.WriteLine("   Years available: 1763 to present");
    Console.WriteLine();

    // 2. Single File Load (Baseline)
    Console.WriteLine("2. Single File Load (Baseline)");
    Console.WriteLine("   " + new string('-', 60));

    var singleConfig = new S3ConnectorConfiguration
    {
        Region = "us-east-1",
        BucketName = "noaa-ghcn-pds",
        ObjectKey = "csv/by_year/1763.csv"  // First year of data - small file
    };

    Console.WriteLine($"   Loading: {singleConfig.ObjectKey}");
    Console.WriteLine("   (1763 is the earliest year - very few weather stations existed)\n");

    var singleConnector = new S3DataConnector(singleConfig);
    try
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var data = await singleConnector.GetDataAsync();
        stopwatch.Stop();

        Console.WriteLine($"   Rows: {data.RowCount:N0}");
        Console.WriteLine($"   Time: {stopwatch.ElapsedMilliseconds:N0}ms\n");
    }
    finally
    {
        singleConnector.Dispose();
    }

    // 3. Understanding Prefix Patterns
    Console.WriteLine("3. Understanding Prefix Patterns");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   The S3 connector detects prefixes by:");
    Console.WriteLine("     - Path ends with '/'           -> csv/by_year/");
    Console.WriteLine("     - Path contains '*'            -> csv/by_year/202*");
    Console.WriteLine("     - Path has no file extension   -> csv/by_year");
    Console.WriteLine();
    Console.WriteLine("   Prefix patterns require AllowMultipleSegments = true");
    Console.WriteLine("   and must use GetStorageDataAsync() (not GetDataAsync)");
    Console.WriteLine();

    // 4. Multi-File Loading with Prefix
    Console.WriteLine("4. Loading Multiple Years with Prefix Pattern");
    Console.WriteLine("   " + new string('-', 60));

    var multiConfig = new S3ConnectorConfiguration
    {
        Region = "us-east-1",
        BucketName = "noaa-ghcn-pds",
        ObjectKey = "csv/by_year/",  // Prefix - matches all year files
        AllowMultipleSegments = true,
        ErrorHandler = (ex) =>
        {
            // Handle per-segment errors gracefully
            Console.WriteLine($"   Warning: {ex.Message}");
        }
    };

    Console.WriteLine($"   Prefix: {multiConfig.ObjectKey}");
    Console.WriteLine("   This will match: 1763.csv, 1764.csv, ... 2024.csv, 2025.csv");
    Console.WriteLine();

    // Note: Loading ALL years would take a very long time
    // Instead, demonstrate the pattern with early years (small files)
    Console.WriteLine("   Note: Loading all 260+ years would take hours.");
    Console.WriteLine("   Demonstrating with early years (1763-1770).\n");

    // 5. Practical Example: In-Memory Loading with Progress
    Console.WriteLine("5. Practical Example: Loading Early Years (1763-1770)");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   Loading the earliest years of climate data.");
    Console.WriteLine("   Early years have few observations (limited weather stations).");
    Console.WriteLine("   Modern years (2020+) have ~35-40 million rows each!\n");

    var earlyYears = new[] { "1763", "1764", "1765", "1766", "1767", "1768", "1769", "1770" };
    long totalRows = 0;
    var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
    string[]? schemaColumns = null;

    foreach (var year in earlyYears)
    {
        var yearConfig = new S3ConnectorConfiguration
        {
            Region = "us-east-1",
            BucketName = "noaa-ghcn-pds",
            ObjectKey = $"csv/by_year/{year}.csv"
        };

        var yearConnector = new S3DataConnector(yearConfig);
        try
        {
            Console.Write($"   Loading {year}.csv... ");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var data = await yearConnector.GetDataAsync();

            stopwatch.Stop();
            totalRows += data.RowCount;
            schemaColumns ??= data.Schema.GetColumnNames().ToArray();

            Console.WriteLine($"{data.RowCount:N0} rows ({stopwatch.ElapsedMilliseconds:N0}ms)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            yearConnector.Dispose();
        }
    }

    totalStopwatch.Stop();

    Console.WriteLine();
    Console.WriteLine($"   Total rows loaded: {totalRows:N0}");
    Console.WriteLine($"   Total time: {totalStopwatch.ElapsedMilliseconds:N0}ms");
    Console.WriteLine($"   Throughput: {totalRows / (totalStopwatch.ElapsedMilliseconds / 1000.0):N0} rows/sec\n");

    // Display schema
    if (schemaColumns != null)
    {
        Console.WriteLine("   Schema:");
        foreach (var col in schemaColumns)
        {
            Console.WriteLine($"     - {col}");
        }
    }
    Console.WriteLine();

    // 6. True Multi-Segment Pattern (Code Example)
    Console.WriteLine("6. True Multi-Segment with AllowMultipleSegments");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   For automatic multi-file loading:");
    Console.WriteLine();
    Console.WriteLine("   ```csharp");
    Console.WriteLine("   var config = new S3ConnectorConfiguration");
    Console.WriteLine("   {");
    Console.WriteLine("       Region = \"us-east-1\",");
    Console.WriteLine("       BucketName = \"noaa-ghcn-pds\",");
    Console.WriteLine("       ObjectKey = \"csv/by_year/\",  // All files in folder");
    Console.WriteLine("       AllowMultipleSegments = true,");
    Console.WriteLine("       ErrorHandler = (ex) => Console.WriteLine($\"Segment error: {ex.Message}\")");
    Console.WriteLine("   };");
    Console.WriteLine();
    Console.WriteLine("   using var velocity = new VelocityDataBlock(\"all_climate.dfc\");");
    Console.WriteLine("   using var connector = new S3DataConnector(config);");
    Console.WriteLine();
    Console.WriteLine("   // Loads ALL matching CSV files sequentially");
    Console.WriteLine("   await connector.GetStorageDataAsync(velocity, batchSize: 100000);");
    Console.WriteLine("   ```\n");

    // 7. Wildcard-Like Patterns
    Console.WriteLine("7. Wildcard-Like Patterns");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   S3 prefix patterns work like wildcards:");
    Console.WriteLine();
    Console.WriteLine($"   {"Pattern",-35} {"Matches",-30}");
    Console.WriteLine("   " + new string('-', 65));
    Console.WriteLine($"   {"csv/by_year/",-35} {"All files in folder",-30}");
    Console.WriteLine($"   {"csv/by_year/20",-35} {"2000.csv - 2099.csv",-30}");
    Console.WriteLine($"   {"csv/by_year/202",-35} {"2020.csv - 2029.csv",-30}");
    Console.WriteLine($"   {"csv/by_year/2024",-35} {"2024.csv only",-30}");
    Console.WriteLine();
    Console.WriteLine("   Note: The connector lists objects matching the prefix,");
    Console.WriteLine("   then processes each matching file sequentially.");
    Console.WriteLine();

    // 8. Error Handling for Multi-Segment
    Console.WriteLine("8. Error Handling for Multi-Segment Loading");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   When loading multiple files, errors can occur per-segment.");
    Console.WriteLine("   Use ErrorHandler to continue processing on failures:");
    Console.WriteLine();
    Console.WriteLine("   ```csharp");
    Console.WriteLine("   var failedSegments = new List<string>();");
    Console.WriteLine();
    Console.WriteLine("   var config = new S3ConnectorConfiguration");
    Console.WriteLine("   {");
    Console.WriteLine("       ObjectKey = \"csv/by_year/\",");
    Console.WriteLine("       AllowMultipleSegments = true,");
    Console.WriteLine("       ErrorHandler = (ex) =>");
    Console.WriteLine("       {");
    Console.WriteLine("           failedSegments.Add(ex.Message);");
    Console.WriteLine("           // Processing continues with next segment");
    Console.WriteLine("       }");
    Console.WriteLine("   };");
    Console.WriteLine();
    Console.WriteLine("   await connector.GetStorageDataAsync(velocity, batchSize: 50000);");
    Console.WriteLine();
    Console.WriteLine("   if (failedSegments.Count > 0)");
    Console.WriteLine("       Console.WriteLine($\"{failedSegments.Count} segments failed\");");
    Console.WriteLine("   ```\n");

    // 9. Performance Tips
    Console.WriteLine("9. Performance Tips for Multi-File Loading");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   - Use larger batch sizes (100k+) for multi-GB datasets");
    Console.WriteLine("   - Disable auto-compaction during bulk loading");
    Console.WriteLine("   - LZ4 compression balances speed and size");
    Console.WriteLine("   - Consider loading files individually for progress tracking");
    Console.WriteLine("   - Use ErrorHandler to continue on transient failures");
    Console.WriteLine();
}
finally
{
    // Cleanup
    if (Directory.Exists(dataPath))
    {
        Directory.Delete(dataPath, recursive: true);
        Console.WriteLine($"Cleaned up: {dataPath}");
    }
}

Console.WriteLine("\n=== Sample Complete ===");
