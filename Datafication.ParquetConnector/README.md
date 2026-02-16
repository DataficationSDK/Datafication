# Datafication.ParquetConnector

[![NuGet](https://img.shields.io/nuget/v/Datafication.ParquetConnector.svg)](https://www.nuget.org/packages/Datafication.ParquetConnector)

A high-performance Apache Parquet file connector for .NET that provides seamless integration between Parquet columnar data files and the Datafication.Core DataBlock API.

## Description

Datafication.ParquetConnector is a specialized connector library that bridges Apache Parquet files and the Datafication.Core ecosystem. Built on the robust Parquet.Net library, it provides efficient reading of Parquet columnar format files with support for row groups, local and remote files, streaming batch operations, and high-performance data loading. The connector excels at processing large-scale analytical datasets stored in the industry-standard Parquet format.

### Key Features

- **Apache Parquet Support**: Read and write Parquet files in the industry-standard columnar format
- **Row Group Awareness**: Automatically tracks and preserves Parquet row group information
- **High Performance**: Optimized for large datasets with millions of rows
- **Multiple Source Types**: Load Parquet from local files or remote URLs (HTTP/HTTPS)
- **Parquet Export (Sink)**: Transform DataBlock to Parquet format with configurable compression
- **Streaming Support**: Efficient batch loading for large Parquet files with `GetStorageDataAsync`
- **Shorthand API**: Simple one-line methods for common Parquet loading and export scenarios
- **Type Preservation**: Maintains native data types from Parquet schema
- **Columnar Efficiency**: Leverages Parquet's columnar storage for fast data access
- **Error Handling**: Global error handler configuration for graceful exception management
- **Validation**: Built-in configuration validation ensures correct setup before processing
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **Big Data Ready**: Designed for analytical workloads and data lake operations

## Table of Contents

- [Description](#description)
  - [Key Features](#key-features)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
  - [Loading Parquet Files (Shorthand)](#loading-parquet-files-shorthand)
  - [Loading Parquet with Configuration](#loading-parquet-with-configuration)
  - [Loading from Remote URLs](#loading-from-remote-urls)
  - [Working with Row Groups](#working-with-row-groups)
  - [Streaming Large Parquet Files to Storage](#streaming-large-parquet-files-to-storage)
  - [Error Handling](#error-handling)
  - [Working with Parquet Data](#working-with-parquet-data)
  - [High-Performance Data Loading](#high-performance-data-loading)
  - [Exporting DataBlock to Parquet](#exporting-datablock-to-parquet)
  - [Parquet Export with Compression Options](#parquet-export-with-compression-options)
- [Configuration Reference](#configuration-reference)
  - [ParquetConnectorConfiguration](#parquetconnectorconfiguration)
- [API Reference](#api-reference)
  - [Core Classes](#core-classes)
  - [Extension Methods](#extension-methods)
  - [Sink Classes](#sink-classes)
- [Common Patterns](#common-patterns)
  - [Data Lake ETL Pipeline](#data-lake-etl-pipeline)
  - [Parquet to VelocityDataBlock](#parquet-to-velocitydatablock)
  - [Big Data Analysis](#big-data-analysis)
  - [Parquet Data Exploration](#parquet-data-exploration)
  - [DataBlock to Parquet Export Pipeline](#datablock-to-parquet-export-pipeline)
- [Performance Tips](#performance-tips)
- [Understanding Parquet Format](#understanding-parquet-format)
- [Known Limitations](#known-limitations)
- [License](#license)

## Installation

> **Note**: Datafication.ParquetConnector is currently in pre-release. The packages are now available on nuget.org.

```bash
dotnet add package Datafication.ParquetConnector
```

**Running the Samples:**

```bash
cd samples/ParquetBasicLoad
dotnet run
```

**Dependencies:**
- Parquet.Net (MIT License) - for reading Apache Parquet files

## Usage Examples

### Loading Parquet Files (Shorthand)

The simplest way to load a Parquet file is using the shorthand extension methods:

```csharp
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ParquetConnector;

// Load Parquet from local file (async)
var flightData = await DataBlock.Connector.LoadParquetAsync(
    new Uri("file:///data/flights.parquet")
);

Console.WriteLine($"Loaded {flightData.RowCount} flight records");

// Synchronous version
var salesData = DataBlock.Connector.LoadParquet(
    new Uri("file:///data/sales.parquet")
);
```

### Loading Parquet with Configuration

For more control, use the full configuration:

```csharp
using Datafication.Connectors.ParquetConnector;

// Create configuration
var configuration = new ParquetConnectorConfiguration
{
    Source = new Uri("file:///data/analytics/user_events.parquet"),
    Id = "user-events-connector"
};

// Create connector and load data
var connector = new ParquetDataConnector(configuration);
var data = await connector.GetDataAsync();

Console.WriteLine($"Loaded {data.RowCount} rows with {data.Schema.Count} columns");

// The connector automatically adds a "RowGroup" column
// which indicates which Parquet row group each row came from
```

### Loading from Remote URLs

Load Parquet files directly from HTTP/HTTPS URLs:

```csharp
// Load from remote URL (async)
var remoteData = await DataBlock.Connector.LoadParquetAsync(
    new Uri("https://example.com/data/analytics.parquet")
);

// Or with full configuration
var configuration = new ParquetConnectorConfiguration
{
    Source = new Uri("https://data.example.com/public/datasets/sales_2024.parquet")
};

var connector = new ParquetDataConnector(configuration);
var webData = await connector.GetDataAsync();

Console.WriteLine($"Downloaded and loaded {webData.RowCount} rows");
```

### Working with Row Groups

Parquet files store data in row groups for efficient columnar access. The connector automatically tracks this:

```csharp
// Load Parquet file
var data = await DataBlock.Connector.LoadParquetAsync(
    new Uri("file:///data/large_dataset.parquet")
);

// The "RowGroup" column is automatically added
// It contains the row group number (1-based) for each row
var rowGroupInfo = data.GroupBy("RowGroup").Info();
Console.WriteLine("Row Groups:");
Console.WriteLine(await rowGroupInfo.TextTableAsync());

// You can remove the RowGroup column if not needed
data.RemoveColumn("RowGroup");

// Or use it for analysis
var rowGroupCounts = data
    .GroupBy("RowGroup")
    .Aggregate(
        new[] { "RowGroup" },
        new Dictionary<string, AggregationType>
        {
            { "RowGroup", AggregationType.Count }
        }
    );

Console.WriteLine("Records per row group:");
Console.WriteLine(await rowGroupCounts.TextTableAsync());
```

### Streaming Large Parquet Files to Storage

For large Parquet files, stream data directly to VelocityDataBlock in batches:

```csharp
using Datafication.Storage.Velocity;

// Create VelocityDataBlock for efficient large-scale storage
var velocityBlock = new VelocityDataBlock("data/large_dataset.dfc");

// Configure Parquet source
var configuration = new ParquetConnectorConfiguration
{
    Source = new Uri("file:///data/10_million_rows.parquet")
};

// Stream Parquet data in batches of 50,000 rows
var connector = new ParquetDataConnector(configuration);
await connector.GetStorageDataAsync(velocityBlock, batchSize: 50000);

Console.WriteLine($"Streamed {velocityBlock.RowCount} rows to storage");
await velocityBlock.FlushAsync();
```

### Error Handling

Configure global error handling for Parquet operations:

```csharp
var configuration = new ParquetConnectorConfiguration
{
    Source = new Uri("file:///data/analytics.parquet"),
    ErrorHandler = (exception) =>
    {
        Console.WriteLine($"Parquet Error: {exception.Message}");
        // Log to file, send alert, etc.
    }
};

var connector = new ParquetDataConnector(configuration);

try
{
    var data = await connector.GetDataAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load Parquet: {ex.Message}");
}
```

### Working with Parquet Data

Once loaded, use the full DataBlock API for data manipulation:

```csharp
// Load Parquet file
var flights = await DataBlock.Connector.LoadParquetAsync(
    new Uri("file:///data/flights_1m.parquet")
);

// Remove the auto-generated RowGroup column if not needed
flights.RemoveColumn("RowGroup");

// Filter, transform, and analyze
var delayedFlights = flights
    .Where("DEP_DELAY", 15, ComparisonOperator.GreaterThan)
    .Where("CANCELLED", 0, ComparisonOperator.Equals)
    .Compute("DELAY_CATEGORY", "DEP_DELAY > 60 ? 'Severe' : 'Moderate'")
    .Select("FL_DATE", "CARRIER", "ORIGIN", "DEST", "DEP_DELAY", "DELAY_CATEGORY")
    .Sort(SortDirection.Descending, "DEP_DELAY")
    .Head(100);

Console.WriteLine("Top 100 delayed flights:");
Console.WriteLine(await delayedFlights.TextTableAsync());

// Analyze delay patterns by carrier
var carrierDelays = flights
    .Where("DEP_DELAY", 0, ComparisonOperator.GreaterThan)
    .GroupBy("CARRIER")
    .Aggregate(
        new[] { "DEP_DELAY" },
        new Dictionary<string, AggregationType>
        {
            { "DEP_DELAY", AggregationType.Mean }
        }
    )
    .Sort(SortDirection.Descending, "mean_DEP_DELAY");

Console.WriteLine("Average delays by carrier:");
Console.WriteLine(await carrierDelays.TextTableAsync());
```

### High-Performance Data Loading

Parquet format is optimized for large datasets:

```csharp
using System.Diagnostics;

// Load 1M+ row Parquet file efficiently
var configuration = new ParquetConnectorConfiguration
{
    Source = new Uri("file:///data/events_1m.parquet")
};

var connector = new ParquetDataConnector(configuration);
var stopwatch = Stopwatch.StartNew();

var data = await connector.GetDataAsync();

stopwatch.Stop();
Console.WriteLine($"Loaded {data.RowCount:N0} rows in {stopwatch.ElapsedMilliseconds:N0} ms");
Console.WriteLine($"Performance: {(data.RowCount / (stopwatch.ElapsedMilliseconds / 1000.0)):N0} rows/sec");

// Benchmark multiple runs
int runs = 5;
long totalMs = 0;

for (int i = 0; i < runs; i++)
{
    stopwatch.Restart();
    var benchData = await connector.GetDataAsync();
    stopwatch.Stop();
    totalMs += stopwatch.ElapsedMilliseconds;
}

Console.WriteLine($"Average load time over {runs} runs: {totalMs / runs:N0} ms");
```

### Exporting DataBlock to Parquet

Transform any DataBlock into Parquet format using the sink extension methods:

```csharp
using Datafication.Core.Data;
using Datafication.Sinks.Connectors.ParquetConnector;

// Create or load a DataBlock
var dataBlock = new DataBlock();
dataBlock.AddColumn(new DataColumn("Id", typeof(int)));
dataBlock.AddColumn(new DataColumn("Name", typeof(string)));
dataBlock.AddColumn(new DataColumn("Score", typeof(double)));

dataBlock.AddRow(new object[] { 1, "Alice", 95.5 });
dataBlock.AddRow(new object[] { 2, "Bob", 87.3 });
dataBlock.AddRow(new object[] { 3, "Charlie", 92.1 });

// Export to Parquet (async)
var parquetBytes = await dataBlock.ParquetSinkAsync();

// Save to file
await File.WriteAllBytesAsync("output.parquet", parquetBytes);

// Or use synchronous version
var parquetBytesSync = dataBlock.ParquetSink();
```

### Parquet Export with Compression Options

Configure compression when exporting:

```csharp
using Parquet;

// Use Gzip compression (higher compression ratio)
var gzipBytes = await dataBlock.ParquetSinkAsync(CompressionMethod.Gzip);

// Use Snappy compression (default, faster)
var snappyBytes = await dataBlock.ParquetSinkAsync(CompressionMethod.Snappy);

// No compression (fastest, larger files)
var uncompressedBytes = await dataBlock.ParquetSinkAsync(CompressionMethod.None);

// Check for skipped columns (e.g., nested DataBlock columns)
var (bytes, skippedColumns) = await dataBlock.ParquetSinkWithSkippedColumnsAsync();
if (skippedColumns.Count > 0)
{
    Console.WriteLine($"Skipped columns: {string.Join(", ", skippedColumns)}");
}
```

## Configuration Reference

### ParquetConnectorConfiguration

Configuration class for Apache Parquet data sources.

**Properties:**

- **`Source`** (Uri, required): Location of the Parquet data source
  - File path: `new Uri("file:///C:/data/file.parquet")`
  - HTTP/HTTPS URL: `new Uri("https://example.com/data.parquet")`
  - Must have `.parquet` extension

- **`Id`** (string, auto-generated): Unique identifier for the configuration
  - Automatically generated as GUID if not specified

- **`ErrorHandler`** (Action<Exception>?, optional): Global exception handler
  - Provides centralized error handling for Parquet operations

**Example:**

```csharp
var config = new ParquetConnectorConfiguration
{
    Source = new Uri("file:///data/analytics.parquet"),
    Id = "analytics-connector",
    ErrorHandler = ex => Console.WriteLine($"Error: {ex.Message}")
};
```

## API Reference

For complete API documentation, see the [Datafication.Connectors.ParquetConnector API Reference](https://datafication.co/help/api/reference/Datafication.Connectors.ParquetConnector.html).

### Core Classes

**ParquetDataConnector**
- **Constructor**
  - `ParquetDataConnector(ParquetConnectorConfiguration configuration)` - Creates connector with validation
- **Methods**
  - `Task<DataBlock> GetDataAsync()` - Loads Parquet file into memory as DataBlock
  - `Task<IStorageDataBlock> GetStorageDataAsync(IStorageDataBlock target, int batchSize = 10000)` - Streams Parquet data in batches
  - `string GetConnectorId()` - Returns unique connector identifier
- **Properties**
  - `ParquetConnectorConfiguration Configuration` - Current configuration
- **Notes**
  - Automatically adds a `RowGroup` column (integer) to track Parquet row groups
  - Row groups are numbered starting from 1
  - Preserves native Parquet data types

**ParquetConnectorConfiguration**
- **Properties**
  - `Uri Source` - Parquet source location (must end with .parquet)
  - `string Id` - Unique identifier
  - `Action<Exception>? ErrorHandler` - Error handler

**ParquetConnectorValidator**
- Validates `ParquetConnectorConfiguration` instances
- **Methods**
  - `ValidationResult Validate(IDataConnectorConfiguration configuration)` - Validates configuration
- **Validation Rules**
  - Source must be provided
  - File URI must point to existing file
  - File must have `.parquet` extension
  - HTTP/HTTPS URLs are accepted
  - FTP and other schemes are rejected

### Extension Methods

**ParquetConnectorExtensions** (namespace: `Datafication.Extensions.Connectors.ParquetConnector`)

```csharp
// Async shorthand methods
Task<DataBlock> LoadParquetAsync(this ConnectorExtensions ext, Uri source)
Task<DataBlock> LoadParquetAsync(this ConnectorExtensions ext, ParquetConnectorConfiguration config)

// Synchronous shorthand methods
DataBlock LoadParquet(this ConnectorExtensions ext, Uri source)
DataBlock LoadParquet(this ConnectorExtensions ext, ParquetConnectorConfiguration config)
```

### Sink Classes

**ParquetSink** (namespace: `Datafication.Sinks.Connectors.ParquetConnector`)
- **Description**: Transforms a DataBlock into a Parquet-formatted byte array
- **Implements**: `IDataSink<byte[]>`
- **Properties**
  - `CompressionMethod Compression` - Compression method (default: Snappy)
  - `List<string> SkippedColumns` - Column names skipped during transform
- **Methods**
  - `Task<byte[]> Transform(DataBlock dataBlock)` - Transforms DataBlock to Parquet bytes
- **Notes**
  - Nested DataBlock columns are automatically skipped
  - Guid values are stored as strings
  - TimeSpan values are stored as milliseconds (Int64)
  - DateTimeOffset values are converted to UTC DateTime

**ParquetSinkExtension** (namespace: `Datafication.Sinks.Connectors.ParquetConnector`)

```csharp
// Async extension methods
Task<byte[]> ParquetSinkAsync(this DataBlock dataBlock, CompressionMethod compression = Snappy)
Task<(byte[] Data, List<string> SkippedColumns)> ParquetSinkWithSkippedColumnsAsync(this DataBlock dataBlock, CompressionMethod compression = Snappy)

// Synchronous extension methods
byte[] ParquetSink(this DataBlock dataBlock, CompressionMethod compression = Snappy)
byte[] ParquetSink(this DataBlock dataBlock, out List<string> skippedColumns, CompressionMethod compression = Snappy)
(byte[] Data, List<string> SkippedColumns) ParquetSinkWithSkippedColumns(this DataBlock dataBlock, CompressionMethod compression = Snappy)
```

## Common Patterns

### Data Lake ETL Pipeline

```csharp
// Extract: Load from Parquet data lake
var rawEvents = await DataBlock.Connector.LoadParquetAsync(
    new Uri("file:///datalake/raw/user_events_2024.parquet")
);

// Transform: Clean and enrich
var transformed = rawEvents
    .RemoveColumn("RowGroup")  // Remove row group tracking
    .DropNulls(DropNullMode.Any)
    .Where("event_type", "page_view")
    .Compute("hour_of_day", "HOUR(event_time)")
    .Compute("is_mobile", "device_type == 'mobile'")
    .Select("user_id", "event_time", "page_url", "hour_of_day", "is_mobile");

// Load: Write to VelocityDataBlock for efficient querying
var velocityBlock = VelocityDataBlock.CreateEnterprise(
    "data/processed_events.dfc",
    primaryKeyColumn: "user_id"
);

await velocityBlock.AppendBatchAsync(transformed);
await velocityBlock.FlushAsync();

Console.WriteLine($"Processed {transformed.RowCount:N0} events");
```

### Parquet to VelocityDataBlock

```csharp
using Datafication.Storage.Velocity;

// Load Parquet configuration
var parquetConfig = new ParquetConnectorConfiguration
{
    Source = new Uri("file:///data/large_analytics.parquet")
};

// Create VelocityDataBlock with primary key
var velocityBlock = VelocityDataBlock.CreateHighThroughput(
    "data/analytics.dfc",
    primaryKeyColumn: "event_id"
);

// Stream Parquet data directly to VelocityDataBlock
var connector = new ParquetDataConnector(parquetConfig);
await connector.GetStorageDataAsync(velocityBlock, batchSize: 100000);
await velocityBlock.FlushAsync();

Console.WriteLine($"Loaded {velocityBlock.RowCount:N0} rows into VelocityDataBlock");

// Now query efficiently with deferred execution
var topUsers = velocityBlock
    .Where("event_date", DateTime.Now.AddDays(-7), ComparisonOperator.GreaterThan)
    .GroupByAggregate("user_id", "event_id", AggregationType.Count, "event_count")
    .Sort(SortDirection.Descending, "event_count")
    .Head(100)
    .Execute();

Console.WriteLine("Top 100 active users:");
Console.WriteLine(await topUsers.TextTableAsync());
```

### Big Data Analysis

```csharp
// Load large Parquet file (millions of rows)
var flights = await DataBlock.Connector.LoadParquetAsync(
    new Uri("file:///data/flights_10m.parquet")
);

flights.RemoveColumn("RowGroup");

// Complex analytical query
var airportStats = flights
    .Where("CANCELLED", 0, ComparisonOperator.Equals)
    .GroupBy("ORIGIN")
    .Aggregate(
        new[] { "DEP_DELAY", "DISTANCE" },
        new Dictionary<string, AggregationType>
        {
            { "DEP_DELAY", AggregationType.Mean },
            { "DISTANCE", AggregationType.Mean }
        }
    )
    .Compute("delay_score", "mean_DEP_DELAY / mean_DISTANCE")
    .Sort(SortDirection.Descending, "delay_score")
    .Head(20);

Console.WriteLine("Top 20 airports by delay-to-distance ratio:");
Console.WriteLine(await airportStats.TextTableAsync());

// Time series analysis
var dailyStats = flights
    .GroupBy("FL_DATE")
    .Aggregate(
        new[] { "DEP_DELAY" },
        new Dictionary<string, AggregationType>
        {
            { "DEP_DELAY", AggregationType.Mean }
        }
    )
    .Sort(SortDirection.Ascending, "FL_DATE");

Console.WriteLine("Daily average delays:");
Console.WriteLine(await dailyStats.TextTableAsync());
```

### DataBlock to Parquet Export Pipeline

```csharp
using Datafication.Core.Data;
using Datafication.Sinks.Connectors.ParquetConnector;

// Load data from CSV
var rawData = await DataBlock.Connector.LoadCsvAsync(
    new Uri("file:///data/sales_report.csv")
);

// Transform data
var processedData = rawData
    .DropNulls(DropNullMode.Any)
    .Where("Amount", 0, ComparisonOperator.GreaterThan)
    .Compute("Quarter", "QUARTER(SaleDate)")
    .Select("CustomerId", "ProductId", "Amount", "Quarter", "SaleDate");

// Export to Parquet with Gzip compression
var (parquetBytes, skippedColumns) = await processedData.ParquetSinkWithSkippedColumnsAsync(
    CompressionMethod.Gzip
);

// Log any skipped columns
if (skippedColumns.Count > 0)
{
    Console.WriteLine($"Warning: Skipped columns: {string.Join(", ", skippedColumns)}");
}

// Save to data lake
var outputPath = $"datalake/processed/sales_{DateTime.Now:yyyyMMdd}.parquet";
await File.WriteAllBytesAsync(outputPath, parquetBytes);

Console.WriteLine($"Exported {processedData.RowCount:N0} rows to {outputPath}");
Console.WriteLine($"File size: {parquetBytes.Length / 1024.0:N2} KB");
```

### Parquet Data Exploration

```csharp
// Quick exploration of Parquet file structure
var data = await DataBlock.Connector.LoadParquetAsync(
    new Uri("file:///data/unknown_dataset.parquet")
);

// Inspect schema
var info = data.Info();
Console.WriteLine("Dataset Schema:");
Console.WriteLine(await info.TextTableAsync());

// Sample first 10 rows
var preview = data.Head(10);
Console.WriteLine("\nFirst 10 rows:");
Console.WriteLine(await preview.TextTableAsync());

// Get statistical summary for numeric columns
Console.WriteLine("\nStatistical Summary:");
Console.WriteLine($"Total rows: {data.RowCount:N0}");

var numericColumns = data.Schema.GetColumnNames()
    .Where(col => IsNumeric(data.GetColumn(col).DataType.GetClrType()))
    .ToList();

foreach (var col in numericColumns)
{
    Console.WriteLine($"\n{col}:");
    Console.WriteLine($"  Min: {data.Min(col)}");
    Console.WriteLine($"  Max: {data.Max(col)}");
    Console.WriteLine($"  Mean: {data.Mean(col):F2}");
    Console.WriteLine($"  StdDev: {data.StandardDeviation(col):F2}");
}

bool IsNumeric(Type type)
{
    return type == typeof(int) || type == typeof(long) ||
           type == typeof(double) || type == typeof(decimal) ||
           type == typeof(float) || type == typeof(short);
}
```

## Performance Tips

1. **Use Streaming for Very Large Files**: For Parquet files with tens of millions of rows, use `GetStorageDataAsync` to stream data directly to VelocityDataBlock
   ```csharp
   await connector.GetStorageDataAsync(velocityBlock, batchSize: 100000);
   ```

2. **Optimal Batch Size**: Parquet row groups are already optimized for reading. Tune batch size based on row group size and available memory
   - For typical Parquet files: Use batch sizes of 50,000 - 100,000 rows
   - For wide tables: Use smaller batch sizes (10,000 - 25,000 rows)

3. **Remove RowGroup Column**: If you don't need row group tracking, remove it early to reduce memory usage
   ```csharp
   data.RemoveColumn("RowGroup");
   ```

4. **Columnar Efficiency**: Parquet's columnar format makes it extremely efficient for analytical queries. Use `Select()` early to load only needed columns if possible

5. **Local Processing**: Download remote Parquet files once and process locally for repeated analysis:
   ```csharp
   if (!File.Exists("cache/data.parquet"))
   {
       var httpClient = new HttpClient();
       var bytes = await httpClient.GetByteArrayAsync("https://example.com/data.parquet");
       await File.WriteAllBytesAsync("cache/data.parquet", bytes);
   }
   var data = await DataBlock.Connector.LoadParquetAsync(new Uri("file:///cache/data.parquet"));
   ```

6. **Type Preservation**: Parquet maintains native data types, so no type conversion overhead (unlike CSV)

7. **Compression Awareness**: Parquet files are typically compressed (Snappy, Gzip, etc.). The connector handles decompression automatically - no configuration needed

8. **Row Group Optimization**: Parquet row groups are designed for parallel processing. Consider the row group structure when designing your processing pipeline

9. **Memory Management**: For large datasets, dispose DataBlocks promptly:
   ```csharp
   using (var rawData = await DataBlock.Connector.LoadParquetAsync(new Uri("file:///large.parquet")))
   {
       var processed = rawData.RemoveColumn("RowGroup").Where(...).Select(...);
       // rawData automatically disposed here
   }
   ```

10. **Benchmark Your Workload**: Parquet is optimized for analytical queries on large datasets. For small files (<10K rows), CSV might be simpler

## Understanding Parquet Format

Apache Parquet is a columnar storage format optimized for analytics:

**Key Characteristics:**
- **Columnar Storage**: Data organized by columns rather than rows, enabling efficient analytical queries
- **Compression**: Excellent compression ratios due to similar data types in each column
- **Row Groups**: Data divided into row groups (typically 100K-1M rows) for parallel processing
- **Schema Evolution**: Supports adding/removing columns over time
- **Type Safety**: Rich type system with precise data types

**When to Use Parquet:**
- Large analytical datasets (millions to billions of rows)
- Data warehouse and data lake scenarios
- When query patterns involve many rows but few columns
- When compression and storage efficiency matter
- Big data processing with Spark, Hadoop, or cloud data warehouses

**RowGroup Column:**
The connector automatically adds a `RowGroup` column (integer, 1-based) that indicates which Parquet row group each row originated from. This can be useful for:
- Understanding data organization
- Debugging row group boundaries
- Optimizing batch processing strategies

You can safely remove this column if not needed:
```csharp
data.RemoveColumn("RowGroup");
```

## Known Limitations

### Parquet Sink (Export) Limitations

The ParquetSink has the following known limitations when exporting DataBlock to Parquet format:

1. **Nested DataBlock Columns Are Skipped**
   - Columns containing nested `DataBlock` values (complex/hierarchical data) are automatically excluded from the output
   - Parquet supports nested structures, but the sink currently only handles flat tabular data
   - Skipped column names are available via the `SkippedColumns` property or the `out` parameter in extension methods
   - **Recommendation**: Flatten nested structures before exporting, or use the JSON sink for hierarchical data

2. **Type Conversions**
   - **Guid**: Stored as string (no native Parquet Guid type)
   - **TimeSpan**: Stored as Int64 (total milliseconds)
   - **DateTimeOffset**: Converted to UTC DateTime
   - **Char**: Stored as single-character string
   - **Decimal**: Uses Parquet's default decimal representation; very high precision values may lose precision

3. **Row Groups**
   - The sink creates a single row group for all data
   - For very large exports, consider batching the data and creating multiple files

### Parquet Connector (Import) Limitations

1. **Nested Parquet Structures**
   - Complex/nested Parquet schemas are not currently supported
   - Only flat columnar data is read

2. **RowGroup Column**
   - A `RowGroup` column (integer, 1-based) is automatically added to track source row groups
   - Remove with `data.RemoveColumn("RowGroup")` if not needed

## License

This library is licensed under the **Datafication SDK License Agreement**. See the [LICENSE](./LICENSE) file for details.

**Summary:**
- **Free Use**: Organizations with fewer than 5 developers AND annual revenue under $500,000 USD may use the SDK without a commercial license
- **Commercial License Required**: Organizations with 5+ developers OR annual revenue exceeding $500,000 USD must obtain a commercial license
- **Open Source Exemption**: Open source projects meeting specific criteria may be exempt from developer count limits

For commercial licensing inquiries, contact [support@datafication.co](mailto:support@datafication.co).

**Third-Party Libraries:**
- Parquet.Net - MIT License

---

**Datafication.ParquetConnector** - Seamlessly connect Apache Parquet files to the Datafication ecosystem.

For more examples and documentation, visit our [samples directory](../../samples/).
