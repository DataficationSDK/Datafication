# S3OoklaParquet Sample

Demonstrates loading Hive-style partitioned Parquet data from the Ookla Open Data public S3 bucket.

## Overview

This sample shows how to:
- Load Parquet files from a public S3 bucket without credentials
- Work with Hive-style partitioning (type/year/quarter)
- Stream large Parquet files to VelocityDataBlock
- Display schema and sample data from network performance data

## Key Features Demonstrated

### Loading Hive-Partitioned Parquet

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-west-2",
    BucketName = "ookla-open-data",
    ObjectKey = "parquet/performance/type=mobile/year=2024/quarter=4/2024-10-01_performance_mobile_tiles.parquet"
};

using var connector = new S3DataConnector(config);
var data = await connector.GetDataAsync();
```

### Streaming to VelocityDataBlock

```csharp
using var velocity = new VelocityDataBlock("ookla_mobile.dfc", new VelocityOptions
{
    DefaultCompression = VelocityCompressionType.LZ4,
    AutoCompactionEnabled = false
});

using var connector = new S3DataConnector(config);
await connector.GetStorageDataAsync(velocity, batchSize: 50000);
```

**Note:** Some Parquet files with sparse nullable columns may require in-memory loading:

```csharp
// For files with nullable column issues, use in-memory loading:
var data = await connector.GetDataAsync();
await velocity.AppendDataBlockAsync(data);
```

### Loading Multiple Quarters

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-west-2",
    BucketName = "ookla-open-data",
    ObjectKey = "parquet/performance/type=mobile/year=2024/",  // All quarters
    AllowMultipleSegments = true
};
```

## Ookla Open Data Structure

| Partition | Values | Description |
|-----------|--------|-------------|
| `type=` | `mobile`, `fixed` | Network type |
| `year=` | `2019`-`2024` | Data year |
| `quarter=` | `1`, `2`, `3`, `4` | Calendar quarter |

### Data Columns

| Column | Description |
|--------|-------------|
| `avg_d_kbps` | Average download speed |
| `avg_u_kbps` | Average upload speed |
| `avg_lat_ms` | Average latency |
| `tests` | Number of speed tests |
| `devices` | Unique devices |
| `quadkey` | Location tile identifier |

## How to Run

```bash
cd S3OoklaParquet
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.S3Connector Ookla Parquet Sample ===

1. Ookla Open Data
   [Dataset description and structure]

2. Loading Single Parquet File
   Loading: parquet/performance/type=mobile/year=2024/quarter=4/...
   Loaded 1,200,000+ rows in ~15-30 seconds

   Schema:
     - quadkey: String
     - avg_d_kbps: Int64
     - avg_u_kbps: Int64
     ...

3. Streaming Parquet to VelocityDataBlock
   Note: Streaming requires consistent nullable column handling.
   Some Parquet files with sparse nullable columns may require
   in-memory loading (GetDataAsync) instead of streaming.

   Attempting streaming to disk-backed storage...
   Streaming encountered nullable column issue.
   This Parquet file has sparse nullable columns (e.g., avg_lat_down_ms).
   Recommendation: Use GetDataAsync() for in-memory loading instead.

=== Sample Complete ===
```

## Benefits of Hive-Style Partitioning

1. **Efficient querying** - Load only the partitions you need
2. **Clear organization** - type/year/quarter hierarchy
3. **Parallel processing** - Each partition is independent
4. **Incremental updates** - New quarters added without rewriting

## Related Samples

- **S3ParquetLoad** - Basic Parquet loading
- **S3BlockchainParquet** - Date-partitioned Parquet
- **S3ToVelocity** - Streaming to disk storage
- **S3MultiSegment** - Loading multiple files
