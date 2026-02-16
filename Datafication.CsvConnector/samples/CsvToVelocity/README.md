# CsvToVelocity Sample

Demonstrates streaming large CSV files to VelocityDataBlock using `GetStorageDataAsync()` for high-performance, disk-backed storage.

## Overview

This sample shows how to:
- Stream CSV data to VelocityDataBlock in batches
- Configure batch sizes for optimal memory usage
- Query VelocityDataBlock with SIMD acceleration
- Perform aggregations on large datasets
- View storage statistics

## Key Features Demonstrated

### Streaming CSV to VelocityDataBlock

```csharp
using Datafication.Storage.Velocity;
using Datafication.Connectors.CsvConnector;

// Configure CSV connector
var csvConfig = new CsvConnectorConfiguration
{
    Source = new Uri("file:///path/to/large.csv"),
    HeaderRow = true
};

var csvConnector = new CsvDataConnector(csvConfig);

// Create VelocityDataBlock
var velocityBlock = new VelocityDataBlock("data.dfc");

// Stream data in batches
await csvConnector.GetStorageDataAsync(velocityBlock, batchSize: 10000);
await velocityBlock.FlushAsync();

Console.WriteLine($"Loaded {velocityBlock.RowCount} rows");
```

### Batch Size Configuration

```csharp
// Small batch for memory-constrained environments
await csvConnector.GetStorageDataAsync(velocityBlock, batchSize: 1000);

// Large batch for high-throughput scenarios
await csvConnector.GetStorageDataAsync(velocityBlock, batchSize: 50000);
```

### Querying with SIMD Acceleration

```csharp
var results = velocityBlock
    .Where("Region", "West")
    .Sort(SortDirection.Descending, "Amount")
    .Head(100)
    .Execute();
```

### Storage Statistics

```csharp
var stats = await velocityBlock.GetStorageStatsAsync();
Console.WriteLine($"Total rows: {stats.TotalRows}");
Console.WriteLine($"Storage size: {stats.EstimatedSizeBytes} bytes");
```

## Batch Size Recommendations

| Data Characteristics | Recommended Batch Size |
|---------------------|----------------------|
| Narrow rows (<10 columns) | 50,000 - 100,000 |
| Medium rows (10-50 columns) | 10,000 - 25,000 |
| Wide rows (50+ columns) | 5,000 - 10,000 |
| Limited memory | 1,000 - 5,000 |

## How to Run

```bash
cd CsvToVelocity
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.CsvConnector to VelocityDataBlock Sample ===

1. Source CSV file information...
   File: sales_large.csv
   Size: 500,000 bytes
   Rows: 10,000

2. Configuring CSV connector...
   Connector ID: sales-data-connector

3. Creating VelocityDataBlock...
   Storage path: /tmp/csv_velocity_sample/sales.dfc

4. Streaming CSV to VelocityDataBlock...
   Batch size: 1,000 rows
   Total rows loaded: 10,000
   Load time: 250 ms
   Throughput: 40,000 rows/sec

5. Storage statistics...
   Total rows: 10,000
   Active rows: 10,000
   Storage files: 1
   Estimated size: 450,000 bytes

6. Querying VelocityDataBlock (SIMD-accelerated)...
   West region sales: 2,000 orders

7. Aggregation: Sales by region...
   [Region statistics table]

=== Sample Complete ===
```

## Benefits of Streaming to VelocityDataBlock

| Benefit | Description |
|---------|-------------|
| **Memory Efficient** | Processes data in batches, not all at once |
| **Disk-Backed** | Handles datasets larger than available RAM |
| **SIMD Accelerated** | 10-30x faster queries than in-memory |
| **Persistent** | Data survives application restarts |
| **Compressible** | Automatic LZ4 compression support |

## When to Use This Pattern

- CSV files with millions of rows
- Limited available memory
- Need for persistent storage
- Repeated queries on the same data
- High-performance analytics requirements

## Data File

**sales_large.csv** - Contains 10,000 sales records with:
- OrderId, Date, Product, Quantity, UnitPrice, Region, Customer

## Related Samples

- **CsvBasicLoad** - In-memory loading for smaller files
- **CsvWriteBack** - Exporting data back to CSV
- **CsvFullConfiguration** - All CSV configuration options
