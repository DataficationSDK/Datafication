# AdoStreamingLargeData Sample

Demonstrates streaming large database result sets to VelocityDataBlock for memory-efficient processing.

## Overview

This sample shows how to:
- Stream database results in batches using `GetStorageDataAsync()`
- Configure batch sizes for optimal performance
- Use VelocityDataBlock for disk-backed, high-performance storage
- Query streamed data with SIMD-accelerated operations
- Handle datasets larger than available RAM

## Key Features Demonstrated

### Configure ADO Connector for Large Queries

```csharp
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandText = "SELECT * FROM Transactions",
    CommandTimeout = 300  // 5 minutes for large queries
};

var connector = new AdoDataConnector(configuration);
```

### Create VelocityDataBlock

```csharp
var velocityOptions = VelocityOptions.CreateHighThroughput();
using var velocityBlock = new VelocityDataBlock(dfcPath, velocityOptions);
```

### Stream Database to VelocityDataBlock

```csharp
const int batchSize = 1000;
await connector.GetStorageDataAsync(velocityBlock, batchSize);
await velocityBlock.FlushAsync();

Console.WriteLine($"Loaded {velocityBlock.RowCount} rows");
```

### Query VelocityDataBlock

```csharp
var highValue = velocityBlock
    .Where("Amount", 500.0, ComparisonOperator.GreaterThan)
    .Execute();
```

### Aggregate VelocityDataBlock

```csharp
var categoryStats = velocityBlock
    .GroupByAggregate("Category", "Amount", AggregationType.Sum, "TotalAmount")
    .Execute();
```

## How to Run

```bash
cd AdoStreamingLargeData
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.AdoConnector Streaming Large Data Sample ===

1. Creating SQLite database with 10,000 transactions...
   Database created in 450 ms

2. Configuring ADO connector...
   Connector ID: transactions-connector
   Command timeout: 300 seconds

3. Creating VelocityDataBlock...
   Storage path: /tmp/ado_velocity_sample/transactions.dfc
   Mode: High Throughput

4. Streaming database to VelocityDataBlock...
   Batch size: 1,000 rows
   Total rows loaded: 10,000
   Stream time: 120 ms
   Throughput: 83,333 rows/sec

5. Storage statistics...
   Total rows: 10,000
   Active rows: 10,000
   Deleted rows: 0
   Storage files: 1
   Estimated size: 524,288 bytes

6. Querying VelocityDataBlock...
   High-value transactions (>$500): 4,892
   Query time: 5 ms

7. Aggregation: Transactions by category...
   ---------------------------------------------
   Category             Total Amount
   ---------------------------------------------
   Electronics          $1,025,432.15
   Clothing             $985,221.50
   ...

=== Sample Complete ===
```

## Batch Size Recommendations

| Data Characteristics | Recommended Batch Size |
|---------------------|------------------------|
| Narrow rows (<10 columns) | 50,000 - 100,000 rows |
| Medium rows (10-50 columns) | 10,000 - 25,000 rows |
| Wide rows (50+ columns) | 5,000 - 10,000 rows |
| Limited memory | 1,000 - 5,000 rows |

## Benefits of Streaming to VelocityDataBlock

1. **Memory Efficient**: Processes data in batches, never loading entire dataset into memory
2. **Disk-Backed**: Handles datasets larger than available RAM
3. **SIMD-Accelerated**: Queries run 10-30x faster than traditional approaches
4. **Persistent**: Data survives application restarts
5. **Compressed**: Built-in LZ4 compression reduces storage footprint

## Database Schema

**Transactions**
| Column | Type | Description |
|--------|------|-------------|
| TransactionId | INTEGER | Primary key |
| TransactionDate | TEXT | Transaction date |
| CustomerId | TEXT | Customer identifier |
| Amount | REAL | Transaction amount |
| Category | TEXT | Product category |
| Status | TEXT | Transaction status |

## When to Use Streaming

Use `GetStorageDataAsync()` instead of `GetDataAsync()` when:
- Result set exceeds available memory
- Processing millions of rows
- Building data pipelines with persistent storage
- Need high-performance querying on large datasets

## Related Samples

- **AdoBasicQuery** - Simple in-memory query execution
- **AdoDataManipulation** - DataBlock operations on results
- **AdoFullConfiguration** - All configuration options
