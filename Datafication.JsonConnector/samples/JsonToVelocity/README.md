# JsonToVelocity Sample

Demonstrates streaming large JSON datasets to VelocityDataBlock using the `GetStorageDataAsync` method for efficient batch processing.

## Overview

This sample shows how to:
- Stream JSON data directly to VelocityDataBlock using batch operations
- Configure batch sizes for optimal performance
- Compare memory vs. storage-based loading approaches
- Query and filter data stored in VelocityDataBlock
- Analyze compression ratios and storage efficiency

## Key Features Demonstrated

### Streaming JSON to VelocityDataBlock

```csharp
// Create VelocityDataBlock
using var velocityBlock = new VelocityDataBlock("data.dfc", VelocityOptions.CreateHighThroughput());

// Create connector and stream data
var connector = new JsonDataConnector(new JsonConnectorConfiguration
{
    Source = new Uri("file:///path/to/large_data.json")
});

await connector.GetStorageDataAsync(velocityBlock, batchSize: 1000);
await velocityBlock.FlushAsync();
```

### Batch Size Configuration

```csharp
// Smaller batches = lower memory, more I/O operations
await connector.GetStorageDataAsync(velocityBlock, batchSize: 100);

// Larger batches = higher memory, fewer I/O operations
await connector.GetStorageDataAsync(velocityBlock, batchSize: 10000);
```

### Querying VelocityDataBlock

```csharp
// Filter
var northSales = velocityBlock.Where("Region", "North").Execute();

// Sort
var topSales = velocityBlock
    .Sort(SortDirection.Descending, "Amount")
    .Head(10)
    .Execute();

// Iterate
var cursor = velocityBlock.GetRowCursor("Id", "Name", "Amount");
while (cursor.MoveNext())
{
    Console.WriteLine(cursor.GetValue("Name"));
}
```

## When to Use VelocityDataBlock

| Scenario | Use VelocityDataBlock |
|----------|----------------------|
| Large datasets (100K+ rows) | Yes |
| Frequent queries on same data | Yes |
| Data needs persistence | Yes |
| Small datasets (< 10K rows) | DataBlock is usually sufficient |
| One-time transformations | DataBlock may be faster |

## Batch Size Guidelines

| Batch Size | Memory Usage | Best For |
|------------|--------------|----------|
| 100-500 | Low | Memory-constrained environments |
| 1000-5000 | Medium | General-purpose (recommended) |
| 10000+ | High | Maximum throughput when memory allows |

## How to Run

```bash
cd JsonToVelocity
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.JsonConnector to VelocityDataBlock Sample ===

1. Source JSON file information...
   File: sales_large.json
   Size: 260,708 bytes

2. Baseline: Loading JSON to DataBlock (in-memory)...
   Loaded 1200 rows to memory in Xms

3. Configuring JSON connector...
   Connector ID: sales-json-connector

4. Creating VelocityDataBlock...
   Storage path: /tmp/velocity_json_samples/sales.dfc
   Options: High Throughput mode

5. Streaming JSON to VelocityDataBlock...
   (Using GetStorageDataAsync for batch processing)
   Batch size: 500 rows
   Total rows loaded: 1,200
   Load time: X ms

6. Storage statistics...
   Total rows: 1,200
   JSON file size: 260,708 bytes
   DFC file size:  XX,XXX bytes
   Compression: XX.X%

7. Querying VelocityDataBlock (SIMD-accelerated)...
   North region sales: XXX rows
   West region sales: XXX rows

...

=== Sample Complete ===
```

## Data Files

- **Input**: `data/sales_large.json` - 1200 sales transaction records
- **Output**: Temporary `.dfc` files created in system temp directory (cleaned up after sample)

## Additional Package

This sample requires the `Datafication.Storage.Velocity` package in addition to `Datafication.JsonConnector`.

## Related Samples

- **JsonBasicLoad** - Basic JSON loading to DataBlock
- **JsonETLPipeline** - ETL workflows without VelocityDataBlock
- **Datafication.Storage.Velocity/BasicOperations** - More VelocityDataBlock examples
