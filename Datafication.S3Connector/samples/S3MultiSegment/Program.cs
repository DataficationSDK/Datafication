using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;
using Datafication.Storage.Velocity;

Console.WriteLine("=== Datafication.S3Connector Multi-Segment Sample ===\n");
Console.WriteLine("This sample demonstrates loading multiple files using prefix patterns.\n");

// Output file for VelocityDataBlock
var outputPath = Path.Combine(Path.GetTempPath(), "s3-multisegment-demo.dfc");

try
{
    // 1. Understanding Prefix Patterns
    Console.WriteLine("1. Understanding Prefix Patterns");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   Prefix patterns allow loading multiple files at once:");
    Console.WriteLine();
    Console.WriteLine("   Pattern                        | Matches");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   data/file.csv                  | Single file only");
    Console.WriteLine("   data/partitioned/              | All files in folder");
    Console.WriteLine("   data/logs-2024-*               | Wildcard matching");
    Console.WriteLine("   data/partitioned               | Folder (no extension)");
    Console.WriteLine();

    // 2. Single File vs Multi-Segment Mode
    Console.WriteLine("2. Single File Mode (Default)");
    Console.WriteLine("   " + new string('-', 60));

    var singleConfig = new S3ConnectorConfiguration
    {
        Region = "us-east-1",
        BucketName = "noaa-ghcn-pds",
        ObjectKey = "csv/by_year/1763.csv"  // Single file
        // AllowMultipleSegments = false (default)
    };

    var singleConnector = new S3DataConnector(singleConfig);
    try
    {
        var data = await singleConnector.GetDataAsync();
        Console.WriteLine($"   Single file loaded: {data.RowCount} rows\n");
    }
    finally
    {
        singleConnector.Dispose();
    }

    // 3. Why Multi-Segment Mode?
    Console.WriteLine("3. Why Multi-Segment Mode?");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   - Loads multiple files matching a prefix pattern");
    Console.WriteLine("   - Streams data to disk (VelocityDataBlock)");
    Console.WriteLine("   - Bounded memory usage regardless of total data size");
    Console.WriteLine("   - Required for partitioned datasets (year/month folders)");
    Console.WriteLine();

    // 4. Demonstration: Attempting prefix without multi-segment
    Console.WriteLine("4. Error: Prefix Pattern Without AllowMultipleSegments");
    Console.WriteLine("   " + new string('-', 60));

    var prefixWithoutMultiConfig = new S3ConnectorConfiguration
    {
        Region = "us-east-1",
        BucketName = "noaa-ghcn-pds",
        ObjectKey = "csv/",  // Prefix pattern
        AllowMultipleSegments = false  // Will cause error
    };

    var prefixConnector = new S3DataConnector(prefixWithoutMultiConfig);
    try
    {
        var data = await prefixConnector.GetDataAsync();
    }
    catch (NotSupportedException)
    {
        Console.WriteLine($"   Expected error: Prefix pattern detected");
        Console.WriteLine($"   The connector requires AllowMultipleSegments = true");
        Console.WriteLine($"   for prefix patterns to prevent memory issues.\n");
    }
    finally
    {
        prefixConnector.Dispose();
    }

    // 5. Multi-Segment Configuration Example
    Console.WriteLine("5. Multi-Segment Configuration");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   ```csharp");
    Console.WriteLine("   var config = new S3ConnectorConfiguration");
    Console.WriteLine("   {");
    Console.WriteLine("       Region = \"us-east-1\",");
    Console.WriteLine("       BucketName = \"my-data-lake\",");
    Console.WriteLine("       ObjectKey = \"data/year=2024/\",  // Prefix pattern");
    Console.WriteLine("       AllowMultipleSegments = true     // Required!");
    Console.WriteLine("   };");
    Console.WriteLine();
    Console.WriteLine("   // Create disk-backed storage");
    Console.WriteLine("   var velocity = new VelocityDataBlock(\"output.dfc\");");
    Console.WriteLine();
    Console.WriteLine("   // Stream all matching files to disk");
    Console.WriteLine("   await connector.GetStorageDataAsync(velocity, batchSize: 50000);");
    Console.WriteLine("   ```\n");

    // 6. Batch Size Considerations
    Console.WriteLine("6. Batch Size Recommendations");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine($"   {"Environment",-25} {"Batch Size",-15} {"Memory Est.",-15}");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine($"   {"Low memory (< 4GB)",-25} {"10,000",-15} {"~50-100 MB",-15}");
    Console.WriteLine($"   {"Standard (8-16GB)",-25} {"50,000",-15} {"~250-500 MB",-15}");
    Console.WriteLine($"   {"High memory (> 16GB)",-25} {"100,000",-15} {"~500 MB - 1 GB",-15}");
    Console.WriteLine();

    // 7. Practical Example: Demonstrating multi-segment with single file
    // (Using single file since public datasets don't have convenient prefixes)
    Console.WriteLine("7. Practical Example: Multi-Segment with VelocityDataBlock");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   Note: Using single file mode for demo (public datasets");
    Console.WriteLine("   don't have convenient multi-file prefixes for testing)\n");

    var demoConfig = new S3ConnectorConfiguration
    {
        Region = "us-east-1",
        BucketName = "noaa-ghcn-pds",
        ObjectKey = "csv/by_year/1763.csv",
        AllowMultipleSegments = false  // Single file for demo
    };

    // Create VelocityDataBlock for disk-backed storage
    var velocityOptions = new VelocityOptions
    {
        DefaultCompression = VelocityCompressionType.LZ4,
        AutoCompactionEnabled = false
    };

    using (var velocity = new VelocityDataBlock(outputPath, velocityOptions))
    {
        var demoConnector = new S3DataConnector(demoConfig);
        try
        {
            Console.WriteLine("   Loading to VelocityDataBlock...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await demoConnector.GetStorageDataAsync(velocity, batchSize: 10000);
            await velocity.FlushAsync();

            stopwatch.Stop();

            Console.WriteLine($"   Loaded {velocity.RowCount:N0} rows to disk");
            Console.WriteLine($"   Time: {stopwatch.ElapsedMilliseconds:N0}ms");
            Console.WriteLine($"   Output: {outputPath}\n");
        }
        finally
        {
            demoConnector.Dispose();
        }

        // Query the stored data
        Console.WriteLine("   Querying stored data:");
        var sample = velocity.Head(5);
        var cursor = sample.GetRowCursor(sample.Schema.GetColumnNames().ToArray());
        int count = 0;
        while (cursor.MoveNext() && count < 5)
        {
            var values = sample.Schema.GetColumnNames()
                .Select(c => cursor.GetValue(c)?.ToString() ?? "")
                .ToArray();
            Console.WriteLine($"   {string.Join(" | ", values)}");
            count++;
        }
    }

    Console.WriteLine();

    // 8. Error Handling in Multi-Segment Mode
    Console.WriteLine("8. Error Handling in Multi-Segment Mode");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   Use ErrorHandler to continue processing on failures:");
    Console.WriteLine();
    Console.WriteLine("   ```csharp");
    Console.WriteLine("   var config = new S3ConnectorConfiguration");
    Console.WriteLine("   {");
    Console.WriteLine("       ObjectKey = \"data/segments/\",");
    Console.WriteLine("       AllowMultipleSegments = true,");
    Console.WriteLine("       ErrorHandler = (Exception ex) =>");
    Console.WriteLine("       {");
    Console.WriteLine("           Console.WriteLine($\"Segment error: {ex.Message}\");");
    Console.WriteLine("           // Processing continues with next segment");
    Console.WriteLine("       }");
    Console.WriteLine("   };");
    Console.WriteLine("   ```\n");

    // 9. Constraints
    Console.WriteLine("9. Multi-Segment Constraints");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   - All files must be the same format (all CSV, all Parquet, etc.)");
    Console.WriteLine("   - GetDataAsync() is NOT supported (use GetStorageDataAsync)");
    Console.WriteLine("   - Segments are processed sequentially (not in parallel)");
    Console.WriteLine("   - Empty prefixes throw InvalidOperationException");
    Console.WriteLine();
}
finally
{
    // Cleanup
    if (File.Exists(outputPath))
    {
        File.Delete(outputPath);
        Console.WriteLine($"   Cleaned up: {outputPath}");
    }
}

Console.WriteLine("\n=== Sample Complete ===");
