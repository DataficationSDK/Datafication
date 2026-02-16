# S3ParquetLoad Sample

Demonstrates loading Apache Parquet files from S3 with automatic format detection.

## Overview

This sample shows how to:
- Load Parquet files from S3 (automatic format detection)
- Understand the supported file types
- Load multiple Parquet partitions
- Work with Parquet schema preservation

## Key Features Demonstrated

### Loading Single Parquet File

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data/sales.parquet"  // Extension triggers Parquet parsing
};

using var connector = new S3DataConnector(config);
var data = await connector.GetDataAsync();

// Schema is automatically preserved from Parquet metadata
foreach (var col in data.Schema.GetColumnNames())
{
    var column = data.GetColumn(col);
    Console.WriteLine($"{col}: {column.DataType.GetClrType().Name}");
}
```

### Loading Multiple Parquet Partitions

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "data-lake",
    ObjectKey = "warehouse/sales/year=2024/",  // Parquet partitions
    AllowMultipleSegments = true
};

using var velocity = new VelocityDataBlock("sales-2024.dfc");
using var connector = new S3DataConnector(config);

// All .parquet files in prefix are loaded
await connector.GetStorageDataAsync(velocity, batchSize: 100000);
```

### With Authentication

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "private-bucket",
    ObjectKey = "analytics/report.parquet",
    AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
    SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
};
```

## Supported File Types

| Extension | Format | Notes |
|-----------|--------|-------|
| `.csv` | CSV | Supports `CsvSeparator`, `CsvHeaderRow` |
| `.json` | JSON | Array of objects or NDJSON |
| `.parquet` | Apache Parquet | Columnar, schema preserved |
| `.xlsx`, `.xls` | Excel | Supports `ExcelSheetIndex`, `ExcelHasHeader` |

## Why Use Parquet?

1. **Columnar storage** - Efficient for analytical queries
2. **Built-in compression** - Snappy, GZIP, LZ4 support
3. **Schema preservation** - Data types are preserved
4. **Performance** - Faster reads for large datasets
5. **Partitioning** - Works well with data lake patterns

## Public Parquet Sources

These buckets contain Parquet data (require AWS credentials):

| Bucket | Path | Description |
|--------|------|-------------|
| `nyc-tlc` | `trip data/` | NYC Taxi trip records |
| `amazon-reviews-pds` | `parquet/` | Amazon product reviews |
| `gdelt-open-data` | `v2/` | Global events database |
| `athena-examples` | `elb/plaintext/` | ELB access logs |

## How to Run

```bash
cd S3ParquetLoad
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.S3Connector Parquet Load Sample ===

1. Parquet File Support
   [Automatic detection explanation]

2. Loading Parquet from S3
   [Code example]

3. Supported File Types (Auto-detected)
   .csv          CSV             Supports CsvSeparator, CsvHeaderRow
   .json         JSON            Array of objects or NDJSON
   .parquet      Apache Parquet  Columnar, schema preserved
   .xlsx, .xls   Excel           Supports ExcelSheetIndex

4. Why Use Parquet?
   [Benefits list]

5. Loading Multiple Parquet Files
   [Multi-segment code example]

6. Live Demo: Loading CSV from Public Bucket
   File loaded successfully
   Rows: 218
   Columns: 2

7. Public Parquet Data Sources
   [Table of sources requiring credentials]

8. Complete Parquet Loading Pattern
   [Full code example]

=== Sample Complete ===
```

## Best Practices

1. **Use Parquet for analytics** - Better performance than CSV for large datasets
2. **Partition your data** - Use Hive-style partitioning (year=2024/month=01/)
3. **Stream large files** - Use `GetStorageDataAsync()` with VelocityDataBlock
4. **Leverage compression** - Parquet files are typically pre-compressed

## Parquet vs CSV Performance

| Aspect | CSV | Parquet |
|--------|-----|---------|
| Read speed | Slower | Faster |
| File size | Larger | Smaller (compressed) |
| Schema | Inferred | Embedded |
| Column selection | Full scan | Efficient |
| Data types | String parsing | Native types |

## Related Samples

- **S3BasicLoad** - Basic S3 loading
- **S3MultiSegment** - Loading multiple files
- **S3ToVelocity** - Streaming to disk storage
- **S3Configuration** - All configuration options
