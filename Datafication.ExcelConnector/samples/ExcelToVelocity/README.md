# ExcelToVelocity Sample

This sample demonstrates how to stream Excel data to VelocityDataBlock for high-performance storage and querying.

## Features Demonstrated

1. **GetStorageDataAsync Streaming** - Stream Excel data directly to VelocityDataBlock
2. **Batch Processing** - Control batch sizes for memory efficiency
3. **Query Operations** - Filter and analyze data stored in Velocity format
4. **Multi-Sheet Conversion** - Convert multiple Excel sheets to Velocity files
5. **Storage Statistics** - Monitor file sizes and row counts

## Why Use Velocity?

VelocityDataBlock provides:
- **High-throughput streaming** - Efficient batch processing for large datasets
- **Columnar storage** - Fast analytical queries
- **Compression** - Reduced file sizes
- **O(1) updates/deletes** - Enterprise CRUD operations
- **SIMD-optimized queries** - Fast filtering and aggregation

## Running the Sample

```bash
cd samples/ExcelToVelocity
dotnet run
```

## Output Files

| File | Source | Description |
|------|--------|-------------|
| web_events.dfc | WebEvents | 2,000 web analytics events |
| orders.dfc | Orders | 500 orders |
| customers.dfc | Customers | 200 customers |
| products.dfc | Products | 50 products |
| orderitems.dfc | OrderItems | 1,497 line items |

## Key Code Snippets

### Stream Excel to Velocity
```csharp
// Create connector
var connector = new ExcelDataConnector(config);

// Create VelocityDataBlock with default options (auto-compaction disabled)
var options = VelocityOptions.CreateDefault();
options.AutoCompactionEnabled = false;

using (var velocityBlock = new VelocityDataBlock(path, options))
{
    // Stream with batch processing
    await connector.GetStorageDataAsync(velocityBlock, batchSize: 500);
    await velocityBlock.FlushAsync();
}
```

### Query Velocity Data
```csharp
using var queryBlock = new VelocityDataBlock(path);

// Filter and execute query
var pageViews = queryBlock.Where("EventType", "page_view").Execute();
Console.WriteLine($"Page views: {pageViews.RowCount}");
```

### Custom Batch Sizes
```csharp
// Smaller batches for memory-constrained environments
await connector.GetStorageDataAsync(velocityBlock, batchSize: 100);

// Larger batches for throughput
await connector.GetStorageDataAsync(velocityBlock, batchSize: 1000);
```

## Velocity Options

| Option | Use Case |
|--------|----------|
| `VelocityOptions.CreateDefault()` | General purpose (recommended for streaming) |
| `VelocityOptions.CreateHighThroughput()` | Large data ingestion with compaction |
| `VelocityOptions.CreateUpdateOptimized()` | Frequent updates/deletes |

**Note:** When streaming data from connectors, disable auto-compaction for best results:
```csharp
var options = VelocityOptions.CreateDefault();
options.AutoCompactionEnabled = false;
```

## Data Flow

```
Excel File (.xlsx)
    |
    v
ExcelDataConnector (reads with configuration)
    |
    v
GetStorageDataAsync (streams in batches)
    |
    v
VelocityDataBlock (columnar storage)
    |
    v
DFC Files (.dfc) - High-performance storage
    |
    v
Query Operations (Where, Sort, Aggregate, etc.)
```

## Best Practices

1. **Choose appropriate batch sizes**
   - Default (10,000) works well for most cases
   - Smaller batches (100-500) for memory-constrained environments
   - Larger batches (5,000-10,000) for maximum throughput

2. **Always call FlushAsync()** after streaming to ensure all data is persisted

3. **Use appropriate VelocityOptions**
   - High-throughput for data ingestion
   - Update-optimized for OLTP-style workloads

4. **Dispose VelocityDataBlock** when done to release file handles
