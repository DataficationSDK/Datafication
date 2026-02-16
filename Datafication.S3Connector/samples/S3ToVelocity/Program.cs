using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;
using Datafication.Storage.Velocity;

Console.WriteLine("=== Datafication.S3Connector to VelocityDataBlock Sample ===\n");
Console.WriteLine("This sample demonstrates streaming S3 data to disk-backed storage.\n");

// Setup paths
var dataPath = Path.Combine(Path.GetTempPath(), "s3_velocity_samples");
if (Directory.Exists(dataPath))
    Directory.Delete(dataPath, recursive: true);
Directory.CreateDirectory(dataPath);

try
{
    // 1. Basic S3 to Velocity streaming
    Console.WriteLine("1. Basic S3 to VelocityDataBlock Streaming");
    Console.WriteLine("   " + new string('-', 60));

    var s3Config = new S3ConnectorConfiguration
    {
        Region = "us-east-1",
        BucketName = "noaa-ghcn-pds",
        ObjectKey = "csv/by_year/1763.csv"
    };

    var basicPath = Path.Combine(dataPath, "countries.dfc");

    using (var velocity = new VelocityDataBlock(basicPath))
    {
        var connector = new S3DataConnector(s3Config);
        try
        {
            Console.WriteLine("   Downloading and streaming from S3...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await connector.GetStorageDataAsync(velocity, batchSize: 10000);
            await velocity.FlushAsync();

            stopwatch.Stop();

            Console.WriteLine($"   Rows loaded: {velocity.RowCount:N0}");
            Console.WriteLine($"   Time: {stopwatch.ElapsedMilliseconds:N0}ms");
            Console.WriteLine($"   Output file: {basicPath}\n");

            // Display sample data
            Console.WriteLine("   Sample data:");
            var cursor = velocity.GetRowCursor(velocity.Schema.GetColumnNames().ToArray());
            int count = 0;
            while (cursor.MoveNext() && count < 3)
            {
                var values = velocity.Schema.GetColumnNames()
                    .Select(c => cursor.GetValue(c)?.ToString() ?? "")
                    .ToArray();
                Console.WriteLine($"   {string.Join(" | ", values)}");
                count++;
            }
        }
        finally
        {
            connector.Dispose();
        }
    }
    Console.WriteLine();

    // 2. VelocityOptions for optimized storage
    Console.WriteLine("2. VelocityOptions for Optimized Storage");
    Console.WriteLine("   " + new string('-', 60));

    var optimizedPath = Path.Combine(dataPath, "optimized.dfc");
    var velocityOptions = new VelocityOptions
    {
        DefaultCompression = VelocityCompressionType.LZ4,
        EnableAutoCompression = true,
        AutoCompactionEnabled = false  // Disable during load, compact after
    };

    Console.WriteLine("   Options configured:");
    Console.WriteLine($"     Compression: {velocityOptions.DefaultCompression}");
    Console.WriteLine($"     AutoCompression: {velocityOptions.EnableAutoCompression}");
    Console.WriteLine($"     AutoCompaction: {velocityOptions.AutoCompactionEnabled}\n");

    using (var velocity = new VelocityDataBlock(optimizedPath, velocityOptions))
    {
        var connector = new S3DataConnector(s3Config);
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await connector.GetStorageDataAsync(velocity, batchSize: 5000);
            await velocity.FlushAsync();

            stopwatch.Stop();

            Console.WriteLine($"   Loaded {velocity.RowCount:N0} rows with compression");
            Console.WriteLine($"   Time: {stopwatch.ElapsedMilliseconds:N0}ms\n");
        }
        finally
        {
            connector.Dispose();
        }
    }

    // 3. Batch Size Comparison
    Console.WriteLine("3. Batch Size Comparison");
    Console.WriteLine("   " + new string('-', 60));

    var batchSizes = new[] { 1000, 5000, 10000 };

    foreach (var batchSize in batchSizes)
    {
        var batchPath = Path.Combine(dataPath, $"batch_{batchSize}.dfc");

        using (var velocity = new VelocityDataBlock(batchPath))
        {
            var connector = new S3DataConnector(s3Config);
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                await connector.GetStorageDataAsync(velocity, batchSize: batchSize);
                await velocity.FlushAsync();

                stopwatch.Stop();

                Console.WriteLine($"   Batch size {batchSize,6}: {stopwatch.ElapsedMilliseconds,5}ms ({velocity.RowCount:N0} rows)");
            }
            finally
            {
                connector.Dispose();
            }
        }

        // Cleanup intermediate file
        if (File.Exists(batchPath))
            File.Delete(batchPath);
    }
    Console.WriteLine();

    // 4. Loading larger dataset
    Console.WriteLine("4. Loading Larger Dataset (Climate Data 1764)");
    Console.WriteLine("   " + new string('-', 60));

    var largeConfig = new S3ConnectorConfiguration
    {
        Region = "us-east-1",
        BucketName = "noaa-ghcn-pds",
        ObjectKey = "csv/by_year/1764.csv"  // Another historical CSV file
    };

    var largePath = Path.Combine(dataPath, "climate_1764.dfc");

    using (var velocity = new VelocityDataBlock(largePath, new VelocityOptions
    {
        DefaultCompression = VelocityCompressionType.LZ4
    }))
    {
        var connector = new S3DataConnector(largeConfig);
        try
        {
            Console.WriteLine("   Streaming CSV file from S3...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await connector.GetStorageDataAsync(velocity, batchSize: 50000);
            await velocity.FlushAsync();

            stopwatch.Stop();

            var fileInfo = new FileInfo(largePath);

            Console.WriteLine($"   Rows loaded: {velocity.RowCount:N0}");
            Console.WriteLine($"   Time: {stopwatch.ElapsedMilliseconds:N0}ms");
            Console.WriteLine($"   File size: {fileInfo.Length / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"   Throughput: {velocity.RowCount / (stopwatch.ElapsedMilliseconds / 1000.0):N0} rows/sec\n");

            // Query the stored data
            Console.WriteLine("   Querying stored data...");
            var sample = velocity.Head(5);
            Console.WriteLine($"   First 5 rows retrieved from disk storage");
        }
        finally
        {
            connector.Dispose();
        }
    }

    // 5. Reopening existing VelocityDataBlock
    Console.WriteLine("5. Reopening Existing VelocityDataBlock");
    Console.WriteLine("   " + new string('-', 60));

    using (var reopened = await VelocityDataBlock.OpenAsync(basicPath))
    {
        Console.WriteLine($"   Reopened: {basicPath}");
        Console.WriteLine($"   Rows: {reopened.RowCount:N0}");
        Console.WriteLine($"   Columns: {string.Join(", ", reopened.Schema.GetColumnNames())}");
        Console.WriteLine("   Data is immediately available for querying\n");
    }

    // 6. Best Practices Summary
    Console.WriteLine("6. Best Practices Summary");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   - Use LZ4 compression for best speed/size balance");
    Console.WriteLine("   - Disable auto-compaction during bulk load");
    Console.WriteLine("   - Compact manually after all data is loaded");
    Console.WriteLine("   - Choose batch size based on available memory");
    Console.WriteLine("   - Always call FlushAsync() before closing");
    Console.WriteLine("   - Use OpenAsync() to reopen existing files\n");

    // 7. Code Example Summary
    Console.WriteLine("7. Code Pattern Summary");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   ```csharp");
    Console.WriteLine("   var options = new VelocityOptions");
    Console.WriteLine("   {");
    Console.WriteLine("       DefaultCompression = VelocityCompressionType.LZ4,");
    Console.WriteLine("       AutoCompactionEnabled = false");
    Console.WriteLine("   };");
    Console.WriteLine();
    Console.WriteLine("   using var velocity = new VelocityDataBlock(\"data.dfc\", options);");
    Console.WriteLine("   var connector = new S3DataConnector(config);");
    Console.WriteLine();
    Console.WriteLine("   await connector.GetStorageDataAsync(velocity, batchSize: 50000);");
    Console.WriteLine("   await velocity.FlushAsync();");
    Console.WriteLine("   connector.Dispose();");
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
