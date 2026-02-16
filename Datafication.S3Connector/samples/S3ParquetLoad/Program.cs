using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;
using Datafication.Storage.Velocity;

Console.WriteLine("=== Datafication.S3Connector Parquet Load Sample ===\n");
Console.WriteLine("This sample demonstrates loading Parquet files from S3.\n");

// 1. Understanding Parquet Support
Console.WriteLine("1. Parquet File Support");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   The S3 connector automatically detects Parquet files by extension:");
Console.WriteLine("   - .parquet files are parsed using the Parquet connector");
Console.WriteLine("   - Schema is preserved from the Parquet metadata");
Console.WriteLine("   - Columnar storage is efficiently read");
Console.WriteLine();

// 2. Parquet Configuration (No special options needed)
Console.WriteLine("2. Loading Parquet from S3");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   ```csharp");
Console.WriteLine("   var config = new S3ConnectorConfiguration");
Console.WriteLine("   {");
Console.WriteLine("       Region = \"us-east-1\",");
Console.WriteLine("       BucketName = \"my-data-bucket\",");
Console.WriteLine("       ObjectKey = \"data/sales.parquet\"  // Extension triggers Parquet parsing");
Console.WriteLine("   };");
Console.WriteLine();
Console.WriteLine("   using var connector = new S3DataConnector(config);");
Console.WriteLine("   var data = await connector.GetDataAsync();");
Console.WriteLine();
Console.WriteLine("   // Parquet schema is automatically preserved");
Console.WriteLine("   foreach (var col in data.Schema.GetColumnNames())");
Console.WriteLine("   {");
Console.WriteLine("       Console.WriteLine($\"{col}: {data.GetColumn(col).DataType}\");");
Console.WriteLine("   }");
Console.WriteLine("   ```\n");

// 3. Supported File Types
Console.WriteLine("3. Supported File Types (Auto-detected)");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine($"   {"Extension",-15} {"Format",-20} {"Notes",-30}");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine($"   {".csv",-15} {"CSV",-20} {"Supports CsvSeparator, CsvHeaderRow",-30}");
Console.WriteLine($"   {".json",-15} {"JSON",-20} {"Array of objects or NDJSON",-30}");
Console.WriteLine($"   {".parquet",-15} {"Apache Parquet",-20} {"Columnar, schema preserved",-30}");
Console.WriteLine($"   {".xlsx, .xls",-15} {"Excel",-20} {"Supports ExcelSheetIndex",-30}");
Console.WriteLine();

// 4. Parquet Benefits
Console.WriteLine("4. Why Use Parquet?");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   - Columnar storage: Efficient for analytical queries");
Console.WriteLine("   - Compression: Built-in compression (Snappy, GZIP, LZ4)");
Console.WriteLine("   - Schema: Data types are preserved (no inference needed)");
Console.WriteLine("   - Performance: Faster reads for large datasets");
Console.WriteLine("   - Partitioning: Works well with partitioned data lakes");
Console.WriteLine();

// 5. Multi-Segment Parquet Loading
Console.WriteLine("5. Loading Multiple Parquet Files");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   ```csharp");
Console.WriteLine("   var config = new S3ConnectorConfiguration");
Console.WriteLine("   {");
Console.WriteLine("       Region = \"us-east-1\",");
Console.WriteLine("       BucketName = \"data-lake\",");
Console.WriteLine("       ObjectKey = \"warehouse/sales/year=2024/\",  // Parquet partitions");
Console.WriteLine("       AllowMultipleSegments = true");
Console.WriteLine("   };");
Console.WriteLine();
Console.WriteLine("   using var velocity = new VelocityDataBlock(\"sales-2024.dfc\");");
Console.WriteLine("   using var connector = new S3DataConnector(config);");
Console.WriteLine();
Console.WriteLine("   // All .parquet files in prefix are loaded");
Console.WriteLine("   await connector.GetStorageDataAsync(velocity, batchSize: 100000);");
Console.WriteLine("   ```\n");

// 6. Demonstrate CSV loading (since public Parquet buckets require auth)
Console.WriteLine("6. Live Demo: Loading CSV from Public Bucket");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   Note: Most public Parquet datasets require AWS credentials.");
Console.WriteLine("   Demonstrating with CSV to show the pattern works.\n");

var csvConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/1763.csv"
};

var csvConnector = new S3DataConnector(csvConfig);
try
{
    var data = await csvConnector.GetDataAsync();

    Console.WriteLine($"   File loaded successfully");
    Console.WriteLine($"   Rows: {data.RowCount}");
    Console.WriteLine($"   Columns: {data.Schema.Count}");
    Console.WriteLine();

    // Display schema
    Console.WriteLine("   Schema:");
    foreach (var col in data.Schema.GetColumnNames())
    {
        Console.WriteLine($"     - {col}: {data.GetColumn(col).DataType.GetClrType().Name}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}
finally
{
    csvConnector.Dispose();
}

Console.WriteLine();

// 7. Public Parquet Sources (require credentials)
Console.WriteLine("7. Public Parquet Data Sources");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   These buckets have Parquet data but require AWS credentials:\n");

var parquetSources = new[]
{
    ("nyc-tlc", "trip data/", "NYC Taxi trip records"),
    ("amazon-reviews-pds", "parquet/", "Amazon product reviews"),
    ("gdelt-open-data", "v2/", "Global events database"),
    ("athena-examples", "elb/plaintext/", "ELB access logs (Parquet)")
};

Console.WriteLine($"   {"Bucket",-20} {"Path",-20} {"Description",-30}");
Console.WriteLine("   " + new string('-', 70));
foreach (var (bucket, path, desc) in parquetSources)
{
    Console.WriteLine($"   {bucket,-20} {path,-20} {desc,-30}");
}

Console.WriteLine();

// 8. Full Example Pattern
Console.WriteLine("8. Complete Parquet Loading Pattern");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   ```csharp");
Console.WriteLine("   // Configuration");
Console.WriteLine("   var config = new S3ConnectorConfiguration");
Console.WriteLine("   {");
Console.WriteLine("       Region = \"us-east-1\",");
Console.WriteLine("       BucketName = \"my-bucket\",");
Console.WriteLine("       ObjectKey = \"data/report.parquet\",");
Console.WriteLine("       AccessKeyId = Environment.GetEnvironmentVariable(\"AWS_ACCESS_KEY_ID\"),");
Console.WriteLine("       SecretAccessKey = Environment.GetEnvironmentVariable(\"AWS_SECRET_ACCESS_KEY\")");
Console.WriteLine("   };");
Console.WriteLine();
Console.WriteLine("   // Load into memory");
Console.WriteLine("   using var connector = new S3DataConnector(config);");
Console.WriteLine("   var data = await connector.GetDataAsync();");
Console.WriteLine();
Console.WriteLine("   // Or stream to disk for large files");
Console.WriteLine("   using var velocity = new VelocityDataBlock(\"output.dfc\");");
Console.WriteLine("   await connector.GetStorageDataAsync(velocity, batchSize: 50000);");
Console.WriteLine("   ```\n");

// 9. Format detection note
Console.WriteLine("9. Format Detection");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   The connector detects format by file extension:");
Console.WriteLine("   - sales.parquet  -> Parquet connector");
Console.WriteLine("   - sales.csv      -> CSV connector");
Console.WriteLine("   - sales.json     -> JSON connector");
Console.WriteLine("   - sales.xlsx     -> Excel connector");
Console.WriteLine();
Console.WriteLine("   No special configuration needed - just use the correct extension.");

Console.WriteLine("\n=== Sample Complete ===");
