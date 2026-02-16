using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.S3Connector Basic Load Sample ===\n");

// 1. Configure the S3 connector for a public bucket (anonymous access)
Console.WriteLine("1. Creating S3 configuration for NOAA climate data...");

var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/1763.csv"  // Historical climate data (small file, ~1KB)
    // No credentials needed for public buckets
};

Console.WriteLine($"   Bucket: {config.BucketName}");
Console.WriteLine($"   Region: {config.Region}");
Console.WriteLine($"   Object: {config.ObjectKey}\n");

// 2. Create the connector and load data
Console.WriteLine("2. Loading data from S3...");
var connector = new S3DataConnector(config);

try
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var data = await connector.GetDataAsync();
    stopwatch.Stop();

    Console.WriteLine($"   Loaded {data.RowCount:N0} rows with {data.Schema.Count} columns");
    Console.WriteLine($"   Download and parse time: {stopwatch.ElapsedMilliseconds:N0}ms\n");

    // 3. Display schema information
    Console.WriteLine("3. Schema Information:");
    foreach (var colName in data.Schema.GetColumnNames())
    {
        var column = data.GetColumn(colName);
        Console.WriteLine($"   - {colName}: {column.DataType.GetClrType().Name}");
    }
    Console.WriteLine();

    // 4. Display sample data
    Console.WriteLine("4. First 10 climate observations:");
    Console.WriteLine("   " + new string('-', 80));
    Console.WriteLine($"   {"Station ID",-15} {"Date",-10} {"Element",-10} {"Value",-10} {"Source",-10}");
    Console.WriteLine("   " + new string('-', 80));

    var columnNames = data.Schema.GetColumnNames().ToArray();
    var cursor = data.GetRowCursor(columnNames);
    int rowCount = 0;

    while (cursor.MoveNext() && rowCount < 10)
    {
        // Get values based on available columns
        var col0 = columnNames.Length > 0 ? cursor.GetValue(columnNames[0])?.ToString() ?? "" : "";
        var col1 = columnNames.Length > 1 ? cursor.GetValue(columnNames[1])?.ToString() ?? "" : "";
        var col2 = columnNames.Length > 2 ? cursor.GetValue(columnNames[2])?.ToString() ?? "" : "";
        var col3 = columnNames.Length > 3 ? cursor.GetValue(columnNames[3])?.ToString() ?? "" : "";
        var col4 = columnNames.Length > 4 ? cursor.GetValue(columnNames[4])?.ToString() ?? "" : "";

        Console.WriteLine($"   {col0,-15} {col1,-10} {col2,-10} {col3,-10} {col4,-10}");
        rowCount++;
    }
    Console.WriteLine("   " + new string('-', 80));
    if (data.RowCount > 10)
        Console.WriteLine($"   ... and {data.RowCount - 10:N0} more rows\n");
    else
        Console.WriteLine();

    // 5. Basic statistics
    Console.WriteLine("5. Data Summary:");
    Console.WriteLine($"   Total observations: {data.RowCount:N0}");
    Console.WriteLine($"   Memory footprint: In-memory DataBlock");
}
finally
{
    // 6. Clean up
    connector.Dispose();
    Console.WriteLine("\n6. Connector disposed.");
}

Console.WriteLine("\n=== Sample Complete ===");
