using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;
using Datafication.Storage.Velocity;

Console.WriteLine("=== Datafication.S3Connector Ookla Parquet Sample ===\n");
Console.WriteLine("This sample demonstrates loading Hive-style partitioned Parquet data");
Console.WriteLine("from the Ookla Open Data public S3 bucket.\n");

// Output paths
var dataPath = Path.Combine(Path.GetTempPath(), "s3_ookla_samples");
if (Directory.Exists(dataPath))
    Directory.Delete(dataPath, recursive: true);
Directory.CreateDirectory(dataPath);

try
{
    // 1. About Ookla Open Data
    Console.WriteLine("1. Ookla Open Data");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   Speedtest by Ookla provides global network performance data.");
    Console.WriteLine("   Data is stored in Hive-style partitioned Parquet format:");
    Console.WriteLine();
    Console.WriteLine("   Bucket: ookla-open-data");
    Console.WriteLine("   Region: us-west-2");
    Console.WriteLine("   Path:   parquet/performance/type=<type>/year=<year>/quarter=<q>/");
    Console.WriteLine();
    Console.WriteLine("   Partition structure:");
    Console.WriteLine("     type=mobile | type=fixed");
    Console.WriteLine("     year=2019 | year=2020 | ... | year=2024");
    Console.WriteLine("     quarter=1 | quarter=2 | quarter=3 | quarter=4");
    Console.WriteLine();

    // 2. Single Parquet File Load
    Console.WriteLine("2. Loading Single Parquet File");
    Console.WriteLine("   " + new string('-', 60));

    // Load Q4 2024 mobile data (most recent available)
    var singleConfig = new S3ConnectorConfiguration
    {
        Region = "us-west-2",
        BucketName = "ookla-open-data",
        ObjectKey = "parquet/performance/type=mobile/year=2024/quarter=4/2024-10-01_performance_mobile_tiles.parquet"
        // No credentials - public bucket
    };

    Console.WriteLine($"   Loading: {singleConfig.ObjectKey}");
    Console.WriteLine("   (This file is ~100MB, loading may take a moment...)\n");

    var connector = new S3DataConnector(singleConfig);
    try
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var data = await connector.GetDataAsync();
        stopwatch.Stop();

        Console.WriteLine($"   Loaded {data.RowCount:N0} rows in {stopwatch.ElapsedMilliseconds:N0}ms\n");

        // Display schema
        Console.WriteLine("   Schema:");
        foreach (var col in data.Schema.GetColumnNames())
        {
            var column = data.GetColumn(col);
            Console.WriteLine($"     - {col}: {column.DataType.GetClrType().Name}");
        }
        Console.WriteLine();

        // Display sample data
        Console.WriteLine("   Sample data (first 5 rows):");
        Console.WriteLine("   " + new string('-', 80));

        var columnNames = data.Schema.GetColumnNames().Take(5).ToArray();
        Console.WriteLine($"   {string.Join(" | ", columnNames.Select(c => c.PadRight(15).Substring(0, 15)))}");
        Console.WriteLine("   " + new string('-', 80));

        var cursor = data.GetRowCursor(columnNames);
        int rowCount = 0;
        while (cursor.MoveNext() && rowCount < 5)
        {
            var values = columnNames
                .Select(c => (cursor.GetValue(c)?.ToString() ?? "").PadRight(15).Substring(0, 15))
                .ToArray();
            Console.WriteLine($"   {string.Join(" | ", values)}");
            rowCount++;
        }
        Console.WriteLine();
    }
    finally
    {
        connector.Dispose();
    }

    // 3. Stream to VelocityDataBlock
    Console.WriteLine("3. Streaming Parquet to VelocityDataBlock");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   Note: Streaming requires consistent nullable column handling.");
    Console.WriteLine("   Some Parquet files with sparse nullable columns may require");
    Console.WriteLine("   in-memory loading (GetDataAsync) instead of streaming.\n");

    var velocityPath = Path.Combine(dataPath, "ookla_mobile.dfc");
    var velocityOptions = new VelocityOptions
    {
        DefaultCompression = VelocityCompressionType.LZ4,
        AutoCompactionEnabled = false
    };

    using (var velocity = new VelocityDataBlock(velocityPath, velocityOptions))
    {
        var streamConnector = new S3DataConnector(singleConfig);
        try
        {
            Console.WriteLine("   Attempting streaming to disk-backed storage...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await streamConnector.GetStorageDataAsync(velocity, batchSize: 50000);
            await velocity.FlushAsync();

            stopwatch.Stop();

            var fileInfo = new FileInfo(velocityPath);
            Console.WriteLine($"   Rows: {velocity.RowCount:N0}");
            Console.WriteLine($"   Time: {stopwatch.ElapsedMilliseconds:N0}ms");
            Console.WriteLine($"   File size: {fileInfo.Length / 1024.0 / 1024.0:F2} MB (LZ4 compressed)");
            Console.WriteLine($"   Throughput: {velocity.RowCount / (stopwatch.ElapsedMilliseconds / 1000.0):N0} rows/sec\n");
        }
        catch (Exception ex) when (ex.Message.Contains("null") || ex.InnerException?.Message.Contains("null") == true)
        {
            Console.WriteLine($"   Streaming encountered nullable column issue.");
            Console.WriteLine($"   This Parquet file has sparse nullable columns (e.g., avg_lat_down_ms).");
            Console.WriteLine($"   Recommendation: Use GetDataAsync() for in-memory loading instead.\n");
            Console.WriteLine("   ```csharp");
            Console.WriteLine("   // For Parquet files with nullable columns, use in-memory loading:");
            Console.WriteLine("   var data = await connector.GetDataAsync();");
            Console.WriteLine("   // Then save to VelocityDataBlock if needed:");
            Console.WriteLine("   await velocity.AppendDataBlockAsync(data);");
            Console.WriteLine("   ```\n");
        }
        finally
        {
            streamConnector.Dispose();
        }
    }

    // 4. Multi-Segment Loading (Multiple Quarters)
    Console.WriteLine("4. Multi-Segment Parquet Loading");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   For loading multiple partitions, use a prefix pattern:");
    Console.WriteLine();
    Console.WriteLine("   ```csharp");
    Console.WriteLine("   var config = new S3ConnectorConfiguration");
    Console.WriteLine("   {");
    Console.WriteLine("       Region = \"us-west-2\",");
    Console.WriteLine("       BucketName = \"ookla-open-data\",");
    Console.WriteLine("       ObjectKey = \"parquet/performance/type=mobile/year=2024/\",  // All quarters");
    Console.WriteLine("       AllowMultipleSegments = true");
    Console.WriteLine("   };");
    Console.WriteLine();
    Console.WriteLine("   using var velocity = new VelocityDataBlock(\"ookla_2024.dfc\");");
    Console.WriteLine("   using var connector = new S3DataConnector(config);");
    Console.WriteLine("   await connector.GetStorageDataAsync(velocity, batchSize: 100000);");
    Console.WriteLine("   ```\n");

    // 5. Hive Partition Benefits
    Console.WriteLine("5. Benefits of Hive-Style Partitioning");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   - Efficient querying: Load only the partitions you need");
    Console.WriteLine("   - Clear data organization: type/year/quarter hierarchy");
    Console.WriteLine("   - Parallel processing: Each partition is independent");
    Console.WriteLine("   - Incremental updates: New quarters added without rewriting");
    Console.WriteLine();

    // 6. Available Partitions Reference
    Console.WriteLine("6. Ookla Open Data Partitions");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine($"   {"Type",-10} {"Years",-20} {"Quarters",-20}");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine($"   {"mobile",-10} {"2019-2024",-20} {"1, 2, 3, 4",-20}");
    Console.WriteLine($"   {"fixed",-10} {"2019-2024",-20} {"1, 2, 3, 4",-20}");
    Console.WriteLine();
    Console.WriteLine("   Data includes: avg_d_kbps, avg_u_kbps, avg_lat_ms, tests, devices, quadkey");
    Console.WriteLine();

    // 7. Code Pattern Summary
    Console.WriteLine("7. Complete Pattern");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   ```csharp");
    Console.WriteLine("   // Single partition file");
    Console.WriteLine("   var config = new S3ConnectorConfiguration");
    Console.WriteLine("   {");
    Console.WriteLine("       Region = \"us-west-2\",");
    Console.WriteLine("       BucketName = \"ookla-open-data\",");
    Console.WriteLine("       ObjectKey = \"parquet/performance/type=fixed/year=2024/quarter=3/\" +");
    Console.WriteLine("                   \"2024-07-01_performance_fixed_tiles.parquet\"");
    Console.WriteLine("   };");
    Console.WriteLine();
    Console.WriteLine("   using var connector = new S3DataConnector(config);");
    Console.WriteLine("   var data = await connector.GetDataAsync();");
    Console.WriteLine("   Console.WriteLine($\"Loaded {data.RowCount} network performance records\");");
    Console.WriteLine("   ```\n");
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
