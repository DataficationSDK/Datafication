# S3ToVelocity Sample

Demonstrates streaming S3 data to VelocityDataBlock for disk-backed storage with bounded memory usage.

## Overview

This sample shows how to:
- Stream S3 data directly to VelocityDataBlock
- Configure VelocityOptions for optimal performance
- Compare different batch sizes
- Handle large datasets efficiently
- Reopen existing VelocityDataBlock files

## Key Features Demonstrated

### Basic S3 to VelocityDataBlock Streaming

```csharp
var s3Config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "ghcnd-stations.txt"
};

using var velocity = new VelocityDataBlock("stations.dfc");
using var connector = new S3DataConnector(s3Config);

await connector.GetStorageDataAsync(velocity, batchSize: 50000);
await velocity.FlushAsync();

Console.WriteLine($"Loaded {velocity.RowCount:N0} rows to disk");
```

### Optimized VelocityOptions

```csharp
var options = new VelocityOptions
{
    DefaultCompression = VelocityCompressionType.LZ4,  // Best speed/size balance
    EnableAutoCompression = true,
    AutoCompactionEnabled = false,  // Disable during bulk load
    AutoFlushEnabled = true
};

using var velocity = new VelocityDataBlock("data.dfc", options);
```

### Batch Size Selection

```csharp
// Low memory - smaller batches
await connector.GetStorageDataAsync(velocity, batchSize: 10000);

// Standard memory - medium batches
await connector.GetStorageDataAsync(velocity, batchSize: 50000);

// High memory - larger batches for better throughput
await connector.GetStorageDataAsync(velocity, batchSize: 100000);
```

### Reopening Existing Files

```csharp
// Open an existing VelocityDataBlock file
using var velocity = await VelocityDataBlock.OpenAsync("existing.dfc");
Console.WriteLine($"Rows: {velocity.RowCount}");

// Data is immediately available for querying
var filtered = velocity.Where("Department", "Engineering");
```

## VelocityOptions Reference

| Option | Default | Description |
|--------|---------|-------------|
| `DefaultCompression` | None | Compression type (None, LZ4, Snappy) |
| `EnableAutoCompression` | false | Auto-compress data chunks |
| `AutoCompactionEnabled` | true | Auto-compact storage files |
| `AutoFlushEnabled` | true | Auto-flush pending writes |
| `PrimaryKeyColumn` | null | Column for primary key operations |

## Batch Size Recommendations

| Environment | Batch Size | Memory Usage | Use Case |
|-------------|------------|--------------|----------|
| Low memory (< 4GB) | 10,000 | ~50-100 MB | Constrained environments |
| Standard (8-16GB) | 50,000 | ~250-500 MB | General purpose |
| High memory (> 16GB) | 100,000+ | ~500+ MB | Maximum throughput |

## How to Run

```bash
cd S3ToVelocity
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.S3Connector to VelocityDataBlock Sample ===

1. Basic S3 to VelocityDataBlock Streaming
   Downloading and streaming from S3...
   Rows loaded: 218
   Time: ~500ms
   Sample data shown...

2. VelocityOptions for Optimized Storage
   Options configured:
     Compression: LZ4
     AutoCompression: True
     AutoCompaction: False
   Loaded 218 rows with compression

3. Batch Size Comparison
   Batch size  1000:   XXXms (218 rows)
   Batch size  5000:   XXXms (218 rows)
   Batch size 10000:   XXXms (218 rows)

4. Loading Larger Dataset (Station Inventory)
   Streaming larger file from S3...
   Rows loaded: 750,000+
   Time: ~5-10 seconds
   Throughput: ~100,000 rows/sec

5. Reopening Existing VelocityDataBlock
   Reopened: countries.dfc
   Rows: 218
   Data is immediately available

6. Best Practices Summary
   [Best practices list]

=== Sample Complete ===
```

## Best Practices

1. **Use LZ4 compression** - Best balance of speed and compression ratio
2. **Disable auto-compaction during load** - Compact manually after all data is loaded
3. **Choose appropriate batch size** - Based on available memory
4. **Always call FlushAsync()** - Ensure all data is written before closing
5. **Use OpenAsync() for existing files** - Efficiently reopen without reloading

## Performance Tips

### For Large Datasets
```csharp
var options = new VelocityOptions
{
    DefaultCompression = VelocityCompressionType.LZ4,
    AutoCompactionEnabled = false  // Compact after all data loaded
};

using var velocity = new VelocityDataBlock("large.dfc", options);
await connector.GetStorageDataAsync(velocity, batchSize: 100000);
await velocity.FlushAsync();

// Optional: Compact after loading
// await velocity.CompactAsync();
```

### For Streaming Updates
```csharp
var options = new VelocityOptions
{
    AutoFlushEnabled = true,
    AutoCompactionEnabled = true
};
```

## Related Samples

- **S3BasicLoad** - Load S3 data into memory
- **S3MultiSegment** - Load multiple files with prefixes
- **S3ErrorHandling** - Error handling patterns
- **BasicOperations** (Velocity) - VelocityDataBlock basics
