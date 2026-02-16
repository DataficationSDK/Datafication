using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.S3Connector Public Dataset Sample ===\n");
Console.WriteLine("This sample demonstrates loading and analyzing public AWS datasets.\n");

// Example 1: NOAA Climate Data - Load a small sample
Console.WriteLine("1. Loading NOAA Climate Data (Historical 1763)...");
Console.WriteLine("   " + new string('-', 60));

var countryConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/1763.csv"  // Historical climate data (small file)
};

var countryConnector = new S3DataConnector(countryConfig);
try
{
    var climateData = await countryConnector.GetDataAsync();
    Console.WriteLine($"   Loaded {climateData.RowCount} climate observations\n");

    // Display sample data
    Console.WriteLine("   Sample Climate Observations:");
    var cursor = climateData.GetRowCursor(climateData.Schema.GetColumnNames().ToArray());
    int count = 0;
    while (cursor.MoveNext() && count < 10)
    {
        var values = climateData.Schema.GetColumnNames()
            .Select(c => cursor.GetValue(c)?.ToString() ?? "")
            .ToArray();
        Console.WriteLine($"   {string.Join(" | ", values)}");
        count++;
    }
    if (climateData.RowCount > 10)
    {
        Console.WriteLine($"   ... and {climateData.RowCount - 10} more observations");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}
finally
{
    countryConnector.Dispose();
}

Console.WriteLine();

// Example 2: NOAA Climate Data - Different Year
Console.WriteLine("2. Loading NOAA Climate Data (Year 1764)...");
Console.WriteLine("   " + new string('-', 60));

var inventoryConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/1764.csv"  // Another historical year
};

var inventoryConnector = new S3DataConnector(inventoryConfig);
try
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var inventory = await inventoryConnector.GetDataAsync();
    stopwatch.Stop();

    Console.WriteLine($"   Loaded {inventory.RowCount:N0} climate observations");
    Console.WriteLine($"   Load time: {stopwatch.ElapsedMilliseconds:N0}ms\n");

    // Display schema
    Console.WriteLine("   Schema:");
    foreach (var col in inventory.Schema.GetColumnNames())
    {
        Console.WriteLine($"     - {col}");
    }
    Console.WriteLine();

    // Sample data
    Console.WriteLine("   Sample observations:");
    var invCursor = inventory.GetRowCursor(inventory.Schema.GetColumnNames().ToArray());
    int invCount = 0;
    while (invCursor.MoveNext() && invCount < 5)
    {
        var values = inventory.Schema.GetColumnNames()
            .Select(c => invCursor.GetValue(c)?.ToString() ?? "")
            .Take(6)
            .ToArray();
        Console.WriteLine($"     {string.Join(" | ", values)}");
        invCount++;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}
finally
{
    inventoryConnector.Dispose();
}

Console.WriteLine();

// Example 3: Working with JSON data from COVID-19 Data Lake
Console.WriteLine("3. COVID-19 Data Lake (Demonstrating different bucket)...");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine("   Bucket: covid19-lake (us-east-2)");
Console.WriteLine("   Note: This bucket contains various COVID-19 datasets");
Console.WriteLine("   from multiple sources in CSV and JSON formats.\n");

// Display available public datasets
Console.WriteLine("4. Popular AWS Open Data Buckets (Anonymous Access):");
Console.WriteLine("   " + new string('-', 60));

var publicBuckets = new[]
{
    ("noaa-ghcn-pds", "us-east-1", "NOAA Climate Data (CSV)"),
    ("noaa-goes16", "us-east-1", "NOAA Weather Satellite (NetCDF)"),
    ("covid19-lake", "us-east-2", "COVID-19 Datasets (CSV/JSON)"),
    ("spacenet-dataset", "us-east-1", "SpaceNet Satellite Imagery"),
    ("amazon-reviews-pds", "us-east-1", "Amazon Product Reviews (Parquet)")
};

Console.WriteLine($"   {"Bucket",-25} {"Region",-12} {"Description",-35}");
Console.WriteLine("   " + new string('-', 75));
foreach (var (bucket, region, desc) in publicBuckets)
{
    Console.WriteLine($"   {bucket,-25} {region,-12} {desc,-35}");
}

Console.WriteLine("\n   To use any of these:");
Console.WriteLine("   ```csharp");
Console.WriteLine("   var config = new S3ConnectorConfiguration");
Console.WriteLine("   {");
Console.WriteLine("       Region = \"<region>\",");
Console.WriteLine("       BucketName = \"<bucket-name>\",");
Console.WriteLine("       ObjectKey = \"<path/to/file.csv>\"");
Console.WriteLine("   };");
Console.WriteLine("   ```");

Console.WriteLine("\n=== Sample Complete ===");
