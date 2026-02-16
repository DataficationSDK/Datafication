# Datafication.S3Connector

[![NuGet](https://img.shields.io/nuget/v/Datafication.S3Connector.svg)](https://www.nuget.org/packages/Datafication.S3Connector)

An S3 file connector for .NET that provides seamless integration between AWS S3 (and S3-compatible services) and the Datafication.Core DataBlock API.

## Description

Datafication.S3Connector is a specialized connector library that bridges Amazon S3 storage and the Datafication.Core ecosystem. It provides robust data loading from S3 buckets with automatic format detection, support for multiple authentication methods, and both single-file and multi-segment streaming modes. The connector handles various S3 scenarios including partitioned data, wildcard patterns, and S3-compatible services while maintaining bounded memory usage for large datasets.

### Key Features

- **Single File Mode**: Load individual S3 objects directly into memory as DataBlock
- **Multi-Segment Mode**: Stream multiple file segments to disk-backed storage with bounded memory
- **Automatic Format Detection**: Intelligently detects and parses CSV, JSON, Parquet, and Excel files
- **Multiple Authentication**: Supports AWS credentials, temporary credentials (STS), or anonymous access for public buckets
- **S3-Compatible Services**: Works with MinIO, DigitalOcean Spaces, Wasabi, and other S3-compatible storage
- **Prefix Patterns**: Match multiple files using folder paths or wildcard patterns
- **Memory-Safe Streaming**: Multi-segment streaming prevents memory issues with large datasets
- **Batch Size Control**: Configurable batch sizes to tune memory usage vs. I/O performance
- **Error Handling**: Global error handler configuration for graceful exception management in multi-segment mode
- **Validation**: Built-in configuration validation ensures correct setup before processing

## Table of Contents

- [Description](#description)
  - [Key Features](#key-features)
- [Installation](#installation)
- [Supported File Types](#supported-file-types)
- [Usage Examples](#usage-examples)
  - [Single File Mode](#single-file-mode)
  - [Anonymous Access (Public Buckets)](#anonymous-access-public-buckets)
  - [Multi-Segment Mode](#multi-segment-mode)
  - [Prefix Patterns](#prefix-patterns)
  - [Temporary Credentials (STS)](#temporary-credentials-sts)
  - [S3-Compatible Services](#s3-compatible-services)
  - [Error Handling](#error-handling)
  - [CSV and Excel Options](#csv-and-excel-options)
- [Configuration Reference](#configuration-reference)
  - [S3ConnectorConfiguration](#s3connectorconfiguration)
- [API Reference](#api-reference)
  - [Core Classes](#core-classes)
- [Common Patterns](#common-patterns)
  - [Load Partitioned Parquet Data](#load-partitioned-parquet-data)
  - [Process Log Files by Pattern](#process-log-files-by-pattern)
  - [Public Dataset Analysis](#public-dataset-analysis)
- [Performance Tips](#performance-tips)
- [Limitations](#limitations)
- [Troubleshooting](#troubleshooting)
- [License](#license)

## Installation

> **Note**: Datafication.S3Connector is currently in pre-release. The packages are now available on nuget.org.

```bash
dotnet add package Datafication.S3Connector
```

**Running the Samples:**

```bash
cd samples/S3BasicLoad
dotnet run
```

## Supported File Types

The S3 connector automatically detects and parses the following formats based on file extension:

- CSV (`.csv`) - With customizable separator and header row options
- JSON (`.json`) - Standard JSON format
- Parquet (`.parquet`) - Columnar storage format
- Excel (`.xlsx`, `.xls`) - With sheet selection and header row options

## Usage Examples

### Single File Mode

Load a single file from S3 into memory:

```csharp
using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;

var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data/sales-2024.csv",
    AccessKeyId = "YOUR_ACCESS_KEY",
    SecretAccessKey = "YOUR_SECRET_KEY"
};

var connector = new S3DataConnector(config);
DataBlock dataBlock = await connector.GetDataAsync();

Console.WriteLine($"Loaded {dataBlock.RowCount} rows");
```

### Anonymous Access (Public Buckets)

Access public S3 buckets without credentials:

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",  // Public dataset
    ObjectKey = "csv/by_year/2023.csv"
    // No credentials needed for public buckets
};

var connector = new S3DataConnector(config);
var dataBlock = await connector.GetDataAsync();
```

### Multi-Segment Mode

Stream multiple file segments from S3 to disk-backed storage, avoiding memory issues with large datasets:

```csharp
using Datafication.Connectors.S3Connector;
using Datafication.Storage.Velocity;

// Configure for multi-segment mode
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-data-lake",
    ObjectKey = "data/sales/year=2024/",  // Prefix to match all files in folder
    AllowMultipleSegments = true,         // Required for multi-segment
    AccessKeyId = "YOUR_ACCESS_KEY",
    SecretAccessKey = "YOUR_SECRET_KEY"
};

var connector = new S3DataConnector(config);

// Create disk-backed storage
var velocity = new VelocityDataBlock("sales-2024.dfc");

// Stream all matching segments to disk
await connector.GetStorageDataAsync(velocity, batchSize: 50000);

// Flush to ensure all data is written
await velocity.FlushAsync();

Console.WriteLine($"Loaded {velocity.RowCount} rows from multiple segments");
```

### Prefix Patterns

The connector automatically detects prefix patterns to match multiple files:

| ObjectKey Pattern | Interpretation | Example Matches |
|-------------------|---------------|-----------------|
| `data/file.csv` | Single file | `data/file.csv` only |
| `data/partitioned/` | All files in folder | All files under `data/partitioned/` |
| `data/logs-2024-*` | Wildcard pattern | `logs-2024-01.csv`, `logs-2024-02.csv`, etc. |
| `data/partitioned` | Folder (no extension) | All files under `data/partitioned/` |

### Temporary Credentials (STS)

Use AWS Security Token Service for temporary access:

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data/file.csv",
    AccessKeyId = "ASIATEMP...",
    SecretAccessKey = "secret...",
    SessionToken = "FwoGZXIvYXdzE..."  // STS session token
};
```

### S3-Compatible Services

Configure for MinIO, DigitalOcean Spaces, Wasabi, and other S3-compatible storage:

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data/file.csv",
    ServiceUrl = "https://nyc3.digitaloceanspaces.com",  // Custom endpoint
    ForcePathStyle = true,  // Required for most S3-compatible services
    AccessKeyId = "YOUR_ACCESS_KEY",
    SecretAccessKey = "YOUR_SECRET_KEY"
};
```

### Error Handling

Handle errors gracefully with a custom error handler:

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data/segments/",
    AllowMultipleSegments = true,
    ErrorHandler = (Exception ex) =>
    {
        Console.Error.WriteLine($"Error processing segment: {ex.Message}");
        // Log error, send notification, etc.
        // Connector will continue with next segment
    }
};

var connector = new S3DataConnector(config);
var velocity = new VelocityDataBlock("output.dfc");

// Errors are handled, processing continues
await connector.GetStorageDataAsync(velocity);
```

### CSV and Excel Options

Configure format-specific options for CSV and Excel files:

```csharp
// CSV with custom separator and no header
var csvConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data/european_data.csv",
    CsvSeparator = ';',      // Semicolon-delimited
    CsvHeaderRow = false,    // No header row
    AccessKeyId = "KEY",
    SecretAccessKey = "SECRET"
};

// Excel with specific sheet
var excelConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data/report.xlsx",
    ExcelSheetIndex = 2,     // Third sheet (zero-based)
    ExcelHasHeader = true,   // First row is header
    AccessKeyId = "KEY",
    SecretAccessKey = "SECRET"
};
```

## Configuration Reference

### S3ConnectorConfiguration

Configuration class for S3 data sources.

**Connection Properties:**

- **`Region`** (string, required): AWS region for the S3 bucket
  - Example: `"us-east-1"`, `"eu-west-1"`

- **`BucketName`** (string, required): Name of the S3 bucket
  - Must follow S3 bucket naming rules (3-63 characters, no underscores, no uppercase)

- **`ObjectKey`** (string, required): S3 object key or prefix pattern
  - Single file: `"data/file.csv"`
  - Prefix: `"data/partitioned/"` or `"data/logs-*"`

**Authentication Properties:**

- **`AccessKeyId`** (string?, optional): AWS access key ID
  - Leave empty for anonymous access to public buckets

- **`SecretAccessKey`** (string?, optional): AWS secret access key
  - Required if `AccessKeyId` is specified

- **`SessionToken`** (string?, optional): STS session token for temporary credentials

**S3-Compatible Service Properties:**

- **`ServiceUrl`** (string?, optional): Custom endpoint URL for S3-compatible services
  - Example: `"https://nyc3.digitaloceanspaces.com"`

- **`ForcePathStyle`** (bool, default: false): Use path-style addressing
  - Set to `true` for most S3-compatible services

**Multi-Segment Properties:**

- **`AllowMultipleSegments`** (bool, default: false): Enable prefix pattern matching
  - Must be `true` to use prefix patterns with `GetStorageDataAsync`

**Format-Specific Properties:**

- **`CsvSeparator`** (char?, optional): CSV field separator
  - Default: `,` (comma)
  - Common values: `;` (semicolon), `\t` (tab)

- **`CsvHeaderRow`** (bool, default: true): Whether CSV has a header row

- **`ExcelHasHeader`** (bool, default: true): Whether Excel sheet has a header row

- **`ExcelSheetIndex`** (int, default: 0): Zero-based worksheet index

**Other Properties:**

- **`Id`** (string, auto-generated): Unique identifier for the configuration

- **`ErrorHandler`** (Action<Exception>?, optional): Global exception handler
  - In multi-segment mode, allows processing to continue after errors

**Example:**

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-data-lake",
    ObjectKey = "data/sales/",
    AllowMultipleSegments = true,
    AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
    SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
    CsvSeparator = ',',
    CsvHeaderRow = true,
    ErrorHandler = ex => Console.WriteLine($"Error: {ex.Message}")
};
```

## API Reference

For complete API documentation, see the [Datafication.Connectors.S3Connector API Reference](https://datafication.co/help/api/reference/Datafication.Connectors.S3Connector.html).

### Core Classes

**S3DataConnector**
- Implements `IDataConnector`, `IDisposable`
- **Constructor**
  - `S3DataConnector(S3ConnectorConfiguration configuration)` - Creates connector with validation
- **Methods**
  - `Task<DataBlock> GetDataAsync()` - Loads single S3 object into memory as DataBlock
  - `Task<IStorageDataBlock> GetStorageDataAsync(IStorageDataBlock target, int batchSize = 10000)` - Streams S3 data in batches
  - `string GetConnectorId()` - Returns unique connector identifier
  - `void Dispose()` - Disposes the underlying S3 client
- **Properties**
  - `S3ConnectorConfiguration Configuration` - Current configuration

**S3ConnectorConfiguration**
- Implements `IDataConnectorConfiguration`
- See [Configuration Reference](#configuration-reference) for all properties

**S3ConnectorValidator**
- Validates `S3ConnectorConfiguration` instances
- **Methods**
  - `ValidationResult Validate(IDataConnectorConfiguration configuration)` - Validates configuration
- **Validation Rules**
  - Required fields: Id, Region, BucketName, ObjectKey
  - Authentication consistency (both keys or neither)
  - Service URL format validation
  - S3 bucket naming rules compliance

**S3FileTypeDetector**
- Static utility for file type detection
- **Methods**
  - `static SupportedFileType DetectFileType(string objectKey)` - Detects format from extension
  - `static bool IsSupported(string objectKey)` - Checks if format is supported
  - `static string GetSupportedTypesDescription()` - Returns human-readable format list

**S3DataProvider** (namespace: `Datafication.Factories.S3Connector`)
- Factory class implementing `IDataConnectorFactory`
- **Methods**
  - `IDataConnector CreateDataConnector(IDataConnectorConfiguration configuration)` - Creates validated connector

## Common Patterns

### Load Partitioned Parquet Data

```csharp
using Datafication.Connectors.S3Connector;
using Datafication.Storage.Velocity;

var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "analytics-data",
    ObjectKey = "warehouse/events/year=2024/month=01/",
    AllowMultipleSegments = true,
    AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
    SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
};

var connector = new S3DataConnector(config);
var velocity = new VelocityDataBlock("events-2024-01.dfc");

Console.WriteLine("Streaming partitioned Parquet data from S3...");
await connector.GetStorageDataAsync(velocity, batchSize: 100000);
await velocity.FlushAsync();

Console.WriteLine($"Loaded {velocity.RowCount:N0} events");
```

### Process Log Files by Pattern

```csharp
using Datafication.Connectors.S3Connector;
using Datafication.Storage.Velocity;

var config = new S3ConnectorConfiguration
{
    Region = "us-west-2",
    BucketName = "application-logs",
    ObjectKey = "logs/app-2024-03-*.csv",  // Wildcard pattern
    AllowMultipleSegments = true,
    AccessKeyId = "YOUR_KEY",
    SecretAccessKey = "YOUR_SECRET"
};

var connector = new S3DataConnector(config);
var velocity = new VelocityDataBlock("march-logs.dfc");

// Stream all matching log files
await connector.GetStorageDataAsync(velocity, batchSize: 25000);

// Query the consolidated logs
var errorLogs = velocity.Where("level == 'ERROR'");
Console.WriteLine($"Found {errorLogs.RowCount} errors in March logs");
```

### Public Dataset Analysis

```csharp
using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;

// NOAA climate data (public bucket)
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/2023.csv"
    // No credentials - public access
};

var connector = new S3DataConnector(config);
var climate = await connector.GetDataAsync();

var summary = climate
    .GroupBy("ELEMENT")
    .Aggregate("VALUE", "avg");

Console.WriteLine("Average climate values by element:");
summary.Info();
```

## Performance Tips

1. **Use the Same Region**: Place your compute resources in the same AWS region as your S3 bucket to minimize latency and data transfer costs.

2. **Choose the Right Mode**:
   - Single file mode (`GetDataAsync`) for datasets under 1GB that fit in memory
   - Multi-segment mode (`GetStorageDataAsync`) for large datasets or partitioned data

3. **Tune Batch Size**: Adjust batch size based on your memory constraints:
   - Low memory (< 4GB RAM): `batchSize: 10000`
   - Standard (8-16GB RAM): `batchSize: 50000`
   - High memory (> 16GB RAM): `batchSize: 100000`

4. **Enable VelocityDataBlock Compression**: Reduce disk I/O with LZ4 compression:
   ```csharp
   var options = VelocityOptions.CreateHighThroughput();
   options.DefaultCompression = VelocityCompressionType.LZ4;
   var velocity = new VelocityDataBlock("output.dfc", options);
   ```

5. **Disable Auto-Compaction During Load**: Compact manually after all segments are loaded:
   ```csharp
   var options = VelocityOptions.CreateHighThroughput();
   options.AutoCompactionEnabled = false;
   var velocity = new VelocityDataBlock("output.dfc", options);

   await connector.GetStorageDataAsync(velocity, batchSize: 50000);
   await velocity.CompactAsync();  // Compact once after loading
   ```

6. **Use Error Handlers in Production**: For multi-segment processing, configure an error handler to log issues while allowing processing to continue:
   ```csharp
   config.ErrorHandler = ex => logger.LogError(ex, "Segment processing failed");
   ```

7. **Prefer Parquet for Large Datasets**: Parquet files are columnar and compressed, resulting in faster transfers and lower S3 costs compared to CSV.

## Limitations

### Multi-Segment Restrictions

1. **GetDataAsync() Not Supported**: Multi-segment mode only works with `GetStorageDataAsync()` to prevent memory issues
2. **Same File Type Required**: All segments must be the same format (all CSV, all Parquet, etc.)
3. **Sequential Processing**: Segments are processed one at a time (parallel processing not yet supported)

### General Limitations

- **Maximum File Size**: Limited by available disk space for temporary files
- **Temporary Storage**: Downloaded files are temporarily stored locally during processing
- **Network Dependency**: Requires network access to S3 bucket

## Troubleshooting

### "ObjectKey appears to be a prefix pattern"

**Cause**: Using a prefix pattern with `GetDataAsync()` or `AllowMultipleSegments = false`

**Solution**:
```csharp
// Option 1: Enable multi-segment with GetStorageDataAsync
config.AllowMultipleSegments = true;
var velocity = new VelocityDataBlock("output.dfc");
await connector.GetStorageDataAsync(velocity);

// Option 2: Specify a single file
config.ObjectKey = "data/specific-file.csv";  // Include extension
```

### "Mixed file types found in prefix"

**Cause**: Prefix matches files of different types (e.g., both .csv and .parquet)

**Solution**: Use a more specific prefix or organize files by type:
```csharp
// Instead of: "data/"
config.ObjectKey = "data/csv/";  // Only CSV files
```

### "No objects found matching prefix"

**Cause**: Prefix doesn't match any objects in the bucket

**Solution**: Verify the prefix and bucket name:
```csharp
// Check your prefix carefully
config.ObjectKey = "correct/path/to/files/";
config.BucketName = "correct-bucket-name";
```

## License

This library is licensed under the **Datafication SDK License Agreement**. See the [LICENSE](./LICENSE) file for details.

**Summary:**
- **Free Use**: Organizations with fewer than 5 developers AND annual revenue under $500,000 USD may use the SDK without a commercial license
- **Commercial License Required**: Organizations with 5+ developers OR annual revenue exceeding $500,000 USD must obtain a commercial license
- **Open Source Exemption**: Open source projects meeting specific criteria may be exempt from developer count limits

For commercial licensing inquiries, contact [support@datafication.co](mailto:support@datafication.co).

---

**Datafication.S3Connector** - Seamlessly connect AWS S3 and S3-compatible storage to the Datafication ecosystem.

For more examples and documentation, visit our [samples directory](./samples/).
