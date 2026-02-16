# S3Configuration Sample

Demonstrates all available configuration options for the S3 connector.

## Overview

This sample shows how to:
- Configure minimal settings for public bucket access
- Use all S3ConnectorConfiguration properties
- Configure CSV and Excel-specific options
- Set up S3-compatible services (MinIO, Wasabi, DigitalOcean)
- Use different authentication methods

## Key Features Demonstrated

### Minimal Configuration

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data/file.csv"
};
```

### Full Configuration

```csharp
var config = new S3ConnectorConfiguration
{
    // Core settings
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data/file.csv",

    // Authentication (omit for anonymous)
    AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
    SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
    SessionToken = null,  // For STS temporary credentials

    // S3-compatible services
    ServiceUrl = null,        // Custom endpoint URL
    ForcePathStyle = false,   // Path-style vs virtual-hosted

    // Multi-segment mode
    AllowMultipleSegments = false,

    // CSV options
    CsvSeparator = ',',
    CsvHeaderRow = true,

    // Excel options
    ExcelHasHeader = true,
    ExcelSheetIndex = 0,

    // Error handling
    ErrorHandler = (ex) => Console.WriteLine($"Error: {ex.Message}")
};
```

### S3-Compatible Services

```csharp
// MinIO (local development)
var minioConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data.csv",
    ServiceUrl = "http://localhost:9000",
    ForcePathStyle = true,
    AccessKeyId = "minioadmin",
    SecretAccessKey = "minioadmin"
};

// DigitalOcean Spaces
var spacesConfig = new S3ConnectorConfiguration
{
    Region = "nyc3",
    BucketName = "my-space",
    ObjectKey = "data.csv",
    ServiceUrl = "https://nyc3.digitaloceanspaces.com",
    ForcePathStyle = true,
    AccessKeyId = "YOUR_SPACES_KEY",
    SecretAccessKey = "YOUR_SPACES_SECRET"
};
```

### CSV Options

```csharp
// European CSV (semicolon separator)
var euroConfig = new S3ConnectorConfiguration
{
    Region = "eu-west-1",
    BucketName = "euro-data",
    ObjectKey = "sales.csv",
    CsvSeparator = ';',
    CsvHeaderRow = true
};

// Tab-separated file
var tsvConfig = new S3ConnectorConfiguration
{
    // ...
    CsvSeparator = '\t'
};
```

### Excel Options

```csharp
var excelConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "reports",
    ObjectKey = "quarterly.xlsx",
    ExcelHasHeader = true,
    ExcelSheetIndex = 2  // Third sheet (zero-based)
};
```

## Configuration Reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Region` | string | Required | AWS region (e.g., "us-east-1") |
| `BucketName` | string | Required | S3 bucket name |
| `ObjectKey` | string | Required | Object key or prefix pattern |
| `AccessKeyId` | string? | null | AWS access key (null = anonymous) |
| `SecretAccessKey` | string? | null | AWS secret key |
| `SessionToken` | string? | null | STS session token |
| `ServiceUrl` | string? | null | Custom S3 endpoint URL |
| `ForcePathStyle` | bool | false | Use path-style URLs |
| `AllowMultipleSegments` | bool | false | Enable multi-file loading |
| `CsvSeparator` | char? | ',' | CSV delimiter character |
| `CsvHeaderRow` | bool? | true | CSV has header row |
| `ExcelHasHeader` | bool? | true | Excel has header row |
| `ExcelSheetIndex` | int? | 0 | Excel sheet index (zero-based) |
| `ErrorHandler` | Action<Exception>? | null | Custom error handler |
| `Id` | string | auto | Unique connector identifier |

## How to Run

```bash
cd S3Configuration
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.S3Connector Configuration Sample ===

1. Minimal Configuration (Public Bucket)
   Region:     us-east-1
   Bucket:     noaa-ghcn-pds
   ObjectKey:  ghcnd-countries.txt
   Loaded 218 rows successfully

2. Full Configuration Reference
   [All configuration properties listed]

3. CSV Configuration Options
   [CSV separator and header examples]

4. S3-Compatible Services Configuration
   [MinIO, DigitalOcean, Wasabi examples]

5. Authentication Patterns
   [Environment variables, STS, anonymous examples]

6. Multi-Segment Mode Configuration
   [Prefix pattern examples]

7. Testing Full Configuration...
   Successfully loaded 218 rows

=== Sample Complete ===
```

## Related Samples

- **S3BasicLoad** - Simplest loading pattern
- **S3ErrorHandling** - Error handling patterns
- **S3MultiSegment** - Multi-file loading with prefixes
- **S3ToVelocity** - Streaming to disk storage
