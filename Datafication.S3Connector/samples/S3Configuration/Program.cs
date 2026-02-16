using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.S3Connector Configuration Sample ===\n");
Console.WriteLine("This sample demonstrates all S3ConnectorConfiguration options.\n");

// 1. Minimal Configuration (Anonymous Access)
Console.WriteLine("1. Minimal Configuration (Public Bucket)");
Console.WriteLine("   " + new string('-', 60));

var minimalConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/1763.csv"
};

Console.WriteLine($"   Region:     {minimalConfig.Region}");
Console.WriteLine($"   Bucket:     {minimalConfig.BucketName}");
Console.WriteLine($"   ObjectKey:  {minimalConfig.ObjectKey}");
Console.WriteLine($"   Id:         {minimalConfig.Id} (auto-generated)\n");

var minimalConnector = new S3DataConnector(minimalConfig);
try
{
    var data = await minimalConnector.GetDataAsync();
    Console.WriteLine($"   Loaded {data.RowCount} rows successfully\n");
}
finally
{
    minimalConnector.Dispose();
}

// 2. Full Configuration Reference (Documented)
Console.WriteLine("2. Full Configuration Reference");
Console.WriteLine("   " + new string('-', 60));

// NOTE: This configuration is for documentation purposes
// It shows all available options but uses anonymous access
var fullConfig = new S3ConnectorConfiguration
{
    // Required: AWS Region
    Region = "us-east-1",

    // Required: S3 Bucket name
    BucketName = "noaa-ghcn-pds",

    // Required: Object key or prefix
    ObjectKey = "csv/by_year/1763.csv",

    // Optional: Credentials for private buckets (null = anonymous)
    // AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
    // SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
    // SessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"),  // For STS

    // Optional: S3-compatible service URL (null = standard AWS S3)
    // ServiceUrl = "https://s3.wasabisys.com",

    // Optional: Force path-style URLs (required for some S3-compatible services)
    ForcePathStyle = false,

    // Optional: Multi-segment mode for loading multiple files
    AllowMultipleSegments = false,

    // Optional: CSV-specific settings
    CsvSeparator = ',',     // Default: comma
    CsvHeaderRow = true,    // Default: true

    // Optional: Excel-specific settings
    ExcelHasHeader = true,  // Default: true
    ExcelSheetIndex = 0,    // Default: 0 (first sheet)

    // Optional: Error handler for graceful error management
    ErrorHandler = (Exception ex) =>
    {
        Console.WriteLine($"   [ErrorHandler] {ex.Message}");
    }
};

Console.WriteLine("   Configuration Properties:");
Console.WriteLine($"     Region:              {fullConfig.Region}");
Console.WriteLine($"     BucketName:          {fullConfig.BucketName}");
Console.WriteLine($"     ObjectKey:           {fullConfig.ObjectKey}");
Console.WriteLine($"     ForcePathStyle:      {fullConfig.ForcePathStyle}");
Console.WriteLine($"     AllowMultipleSegments: {fullConfig.AllowMultipleSegments}");
Console.WriteLine($"     CsvSeparator:        '{fullConfig.CsvSeparator}'");
Console.WriteLine($"     CsvHeaderRow:        {fullConfig.CsvHeaderRow}");
Console.WriteLine($"     ExcelHasHeader:      {fullConfig.ExcelHasHeader}");
Console.WriteLine($"     ExcelSheetIndex:     {fullConfig.ExcelSheetIndex}");
Console.WriteLine($"     ErrorHandler:        {(fullConfig.ErrorHandler != null ? "Set" : "Not set")}\n");

// 3. CSV with Custom Separator
Console.WriteLine("3. CSV Configuration Options");
Console.WriteLine("   " + new string('-', 60));

Console.WriteLine("   Common CSV separators:");
Console.WriteLine("     CsvSeparator = ','   // Comma (default)");
Console.WriteLine("     CsvSeparator = ';'   // Semicolon (European)");
Console.WriteLine("     CsvSeparator = '\\t'  // Tab-separated");
Console.WriteLine("     CsvSeparator = '|'   // Pipe-separated\n");

Console.WriteLine("   Header row options:");
Console.WriteLine("     CsvHeaderRow = true  // First row is header (default)");
Console.WriteLine("     CsvHeaderRow = false // No header, columns named Column0, Column1...\n");

// 4. S3-Compatible Services Configuration
Console.WriteLine("4. S3-Compatible Services Configuration");
Console.WriteLine("   " + new string('-', 60));

Console.WriteLine("   MinIO:");
Console.WriteLine("   ```csharp");
Console.WriteLine("   ServiceUrl = \"http://localhost:9000\",");
Console.WriteLine("   ForcePathStyle = true");
Console.WriteLine("   ```\n");

Console.WriteLine("   DigitalOcean Spaces:");
Console.WriteLine("   ```csharp");
Console.WriteLine("   ServiceUrl = \"https://nyc3.digitaloceanspaces.com\",");
Console.WriteLine("   ForcePathStyle = true");
Console.WriteLine("   ```\n");

Console.WriteLine("   Wasabi:");
Console.WriteLine("   ```csharp");
Console.WriteLine("   ServiceUrl = \"https://s3.wasabisys.com\",");
Console.WriteLine("   ForcePathStyle = true");
Console.WriteLine("   ```\n");

// 5. Credentials from Environment Variables
Console.WriteLine("5. Authentication Patterns");
Console.WriteLine("   " + new string('-', 60));

Console.WriteLine("   Environment variables (recommended):");
Console.WriteLine("   ```csharp");
Console.WriteLine("   AccessKeyId = Environment.GetEnvironmentVariable(\"AWS_ACCESS_KEY_ID\"),");
Console.WriteLine("   SecretAccessKey = Environment.GetEnvironmentVariable(\"AWS_SECRET_ACCESS_KEY\")");
Console.WriteLine("   ```\n");

Console.WriteLine("   STS temporary credentials:");
Console.WriteLine("   ```csharp");
Console.WriteLine("   AccessKeyId = \"ASIATEMP...\",");
Console.WriteLine("   SecretAccessKey = \"secret...\",");
Console.WriteLine("   SessionToken = \"FwoGZXIvYXdzE...\"  // From STS");
Console.WriteLine("   ```\n");

Console.WriteLine("   Anonymous access (public buckets):");
Console.WriteLine("   ```csharp");
Console.WriteLine("   // Simply omit AccessKeyId and SecretAccessKey");
Console.WriteLine("   // The connector will use anonymous credentials");
Console.WriteLine("   ```\n");

// 6. Multi-Segment Configuration
Console.WriteLine("6. Multi-Segment Mode Configuration");
Console.WriteLine("   " + new string('-', 60));

Console.WriteLine("   Load multiple files with prefix pattern:");
Console.WriteLine("   ```csharp");
Console.WriteLine("   var config = new S3ConnectorConfiguration");
Console.WriteLine("   {");
Console.WriteLine("       ObjectKey = \"data/partitioned/\",  // Folder prefix");
Console.WriteLine("       AllowMultipleSegments = true       // Required!");
Console.WriteLine("   };");
Console.WriteLine("   ");
Console.WriteLine("   // Must use GetStorageDataAsync for multi-segment");
Console.WriteLine("   var velocity = new VelocityDataBlock(\"output.dfc\");");
Console.WriteLine("   await connector.GetStorageDataAsync(velocity, batchSize: 50000);");
Console.WriteLine("   ```\n");

// 7. Test the full config
Console.WriteLine("7. Testing Full Configuration...");
Console.WriteLine("   " + new string('-', 60));

var fullConnector = new S3DataConnector(fullConfig);
try
{
    var data = await fullConnector.GetDataAsync();
    Console.WriteLine($"   Successfully loaded {data.RowCount} rows");
    Console.WriteLine($"   Connector ID: {fullConnector.GetConnectorId()}");
}
finally
{
    fullConnector.Dispose();
}

Console.WriteLine("\n=== Sample Complete ===");
