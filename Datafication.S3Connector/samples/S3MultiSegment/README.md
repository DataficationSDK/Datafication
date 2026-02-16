# S3MultiSegment Sample

Demonstrates loading multiple files from S3 using prefix patterns and streaming to disk-backed storage.

## Overview

This sample shows how to:
- Use prefix patterns to match multiple files
- Enable multi-segment mode with `AllowMultipleSegments = true`
- Stream data to VelocityDataBlock for bounded memory usage
- Configure batch sizes for different memory environments
- Handle errors during multi-segment processing

## Key Features Demonstrated

### Multi-Segment Configuration

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-data-lake",
    ObjectKey = "data/year=2024/",  // Prefix pattern
    AllowMultipleSegments = true     // Required for prefixes
};

var connector = new S3DataConnector(config);

// Create disk-backed storage
var velocity = new VelocityDataBlock("output.dfc");

// Stream all matching files to disk
await connector.GetStorageDataAsync(velocity, batchSize: 50000);
await velocity.FlushAsync();

Console.WriteLine($"Loaded {velocity.RowCount:N0} rows");
```

### Prefix Patterns

| Pattern | Interpretation | Example Matches |
|---------|---------------|-----------------|
| `data/file.csv` | Single file | `data/file.csv` only |
| `data/partitioned/` | All files in folder | Everything under `data/partitioned/` |
| `data/logs-2024-*` | Wildcard pattern | `logs-2024-01.csv`, `logs-2024-02.csv` |
| `data/partitioned` | Folder (no extension) | All files under `data/partitioned/` |

### Error Handling in Multi-Segment Mode

```csharp
var config = new S3ConnectorConfiguration
{
    ObjectKey = "data/segments/",
    AllowMultipleSegments = true,
    ErrorHandler = (Exception ex) =>
    {
        Console.WriteLine($"Segment error: {ex.Message}");
        // Processing continues with next segment
    }
};
```

### Batch Size Selection

```csharp
// Low memory environment (< 4GB RAM)
await connector.GetStorageDataAsync(velocity, batchSize: 10000);

// Standard environment (8-16GB RAM)
await connector.GetStorageDataAsync(velocity, batchSize: 50000);

// High memory environment (> 16GB RAM)
await connector.GetStorageDataAsync(velocity, batchSize: 100000);
```

## Batch Size Recommendations

| Environment | Batch Size | Memory Estimate |
|-------------|------------|-----------------|
| Low memory (< 4GB) | 10,000 | ~50-100 MB |
| Standard (8-16GB) | 50,000 | ~250-500 MB |
| High memory (> 16GB) | 100,000 | ~500 MB - 1 GB |

Memory footprint depends on row width (number and size of columns).

## Multi-Segment Constraints

1. **Same file type required** - All files must be the same format (CSV, Parquet, JSON, or Excel)
2. **GetDataAsync() not supported** - Multi-segment only works with `GetStorageDataAsync()`
3. **Sequential processing** - Segments are processed one at a time
4. **No empty prefixes** - Throws `InvalidOperationException` if no files match

## How to Run

```bash
cd S3MultiSegment
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.S3Connector Multi-Segment Sample ===

1. Understanding Prefix Patterns
   [Pattern examples table]

2. Single File Mode (Default)
   Single file loaded: 218 rows

3. Why Multi-Segment Mode?
   [Explanation of benefits]

4. Error: Prefix Pattern Without AllowMultipleSegments
   Expected error: Prefix pattern detected

5. Multi-Segment Configuration
   [Code example]

6. Batch Size Recommendations
   [Recommendations table]

7. Practical Example: Multi-Segment with VelocityDataBlock
   Loading to VelocityDataBlock...
   Loaded 218 rows to disk
   Time: ~500ms

8. Error Handling in Multi-Segment Mode
   [Error handling example]

9. Multi-Segment Constraints
   [Constraint list]

=== Sample Complete ===
```

## Real-World Use Cases

### Partitioned Data (Hive-style)
```csharp
// Load all January 2024 data
config.ObjectKey = "warehouse/events/year=2024/month=01/";
config.AllowMultipleSegments = true;
```

### Log Files by Date
```csharp
// Load all March 2024 logs
config.ObjectKey = "logs/app-2024-03-*";
config.AllowMultipleSegments = true;
```

### Multi-Region Data
```csharp
// Load all US region data
config.ObjectKey = "data/region=US/";
config.AllowMultipleSegments = true;
```

## Related Samples

- **S3BasicLoad** - Single file loading
- **S3ToVelocity** - Detailed VelocityDataBlock usage
- **S3ErrorHandling** - Error handling patterns
- **S3Configuration** - All configuration options
