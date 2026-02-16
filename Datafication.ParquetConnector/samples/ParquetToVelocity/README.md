# ParquetToVelocity Sample

Demonstrates streaming Parquet data to VelocityDataBlock for high-performance storage and querying.

## Overview

This sample shows how to:
- Configure the Parquet connector for streaming
- Create a VelocityDataBlock with high-throughput options
- Stream Parquet data using `GetStorageDataAsync()`
- Configure batch sizes for memory efficiency
- Query the VelocityDataBlock with SIMD acceleration
- Perform aggregations on stored data

## Why Stream to VelocityDataBlock?

| Feature | Benefit |
|---------|---------|
| Memory efficiency | Processes data in batches, not all at once |
| Disk-backed storage | Handles datasets larger than RAM |
| SIMD acceleration | 10-30x faster queries |
| Persistent storage | Data survives application restarts |
| Columnar format | Combines Parquet's format with Velocity's speed |

## Key Features Demonstrated

### Configure Parquet Connector

```csharp
var config = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath),
    Id = "sales-connector"
};

var connector = new ParquetDataConnector(config);
```

### Create VelocityDataBlock

```csharp
var velocityOptions = VelocityOptions.CreateHighThroughput();
using var velocityBlock = new VelocityDataBlock(dfcPath, velocityOptions);
```

### Stream with Batch Processing

```csharp
const int batchSize = 10000;
await connector.GetStorageDataAsync(velocityBlock, batchSize);
await velocityBlock.FlushAsync();
```

### Query with SIMD Acceleration

```csharp
var europeSales = velocityBlock
    .Where("Region", "Europe")
    .Execute();
```

### Aggregation

```csharp
var regionStats = velocityBlock
    .GroupByAggregate("Region", "TotalRevenue", AggregationType.Sum, "SumRevenue")
    .Execute();
```

## Batch Size Recommendations

| Data Characteristics | Recommended Batch Size |
|---------------------|----------------------|
| Narrow rows (<10 columns) | 50,000 - 100,000 rows |
| Medium rows (10-50 columns) | 10,000 - 25,000 rows |
| Wide rows (50+ columns) | 5,000 - 10,000 rows |
| Limited memory | 1,000 - 5,000 rows |

## How to Run

```bash
cd ParquetToVelocity
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.ParquetConnector to VelocityDataBlock Sample ===

1. Source Parquet file information...
   File: SalesRecords50k.parquet
   Size: 2,234,567 bytes

2. Configuring Parquet connector...
   Connector ID: sales-parquet-connector

3. Creating VelocityDataBlock...
   Storage path: /tmp/parquet_velocity_sample/sales.dfc
   Options: High Throughput mode

4. Streaming Parquet to VelocityDataBlock...
   Batch size: 10,000 rows
   Total rows loaded: 50,000
   Load time: 250 ms
   Throughput: 200,000 rows/sec

5. Storage statistics...
   Total rows: 50,000
   Active rows: 50,000
   Deleted rows: 0
   Storage files: 1
   Estimated size: 3,456,789 bytes

6. Querying VelocityDataBlock (SIMD-accelerated)...
   Europe region sales: 9,972 orders

7. Aggregation: Total revenue by region...
   Region                    Total Revenue
   ---------------------------------------------
   Europe                    $123,456,789
   Asia                      $98,765,432
   ...

8. Sample data (first 5 rows):
   [Data table]

9. Batch size recommendations:
   [Recommendations table]

=== Summary ===
   [Summary with benefits]

=== Sample Complete ===
```

## Performance Considerations

- **Batch size**: Larger batches = faster loading, more memory usage
- **VelocityOptions**: Use `CreateHighThroughput()` for bulk loading
- **Flush**: Always call `FlushAsync()` after streaming completes
- **Dispose**: Use `using` to properly dispose VelocityDataBlock

## Related Samples

- **ParquetBasicLoad** - Loading into memory (smaller datasets)
- **ParquetConfiguration** - Full connector configuration options
- **ParquetRowGroups** - Understanding Parquet structure
