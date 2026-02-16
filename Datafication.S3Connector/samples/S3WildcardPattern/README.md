# S3WildcardPattern Sample

Demonstrates loading multiple files using prefix patterns from the NOAA Global Historical Climatology Network (GHCN) dataset.

## Overview

This sample shows how to:
- Load multiple files using S3 prefix patterns
- Process multi-year datasets sequentially
- Track loading progress across files
- Use AllowMultipleSegments for automatic multi-file loading

## Key Features Demonstrated

### Single File Load

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/2020.csv"
};

using var connector = new S3DataConnector(config);
var data = await connector.GetDataAsync();
```

### Prefix Pattern Loading

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/",  // All files in folder
    AllowMultipleSegments = true,
    ErrorHandler = (ex) => Console.WriteLine($"Segment error: {ex.Message}")
};

using var velocity = new VelocityDataBlock("all_climate.dfc");
using var connector = new S3DataConnector(config);
await connector.GetStorageDataAsync(velocity, batchSize: 100000);
```

### Sequential Multi-Year Loading with Progress

```csharp
// Load early years (small files) - modern years have ~35-40M rows each!
var years = new[] { "1763", "1764", "1765", "1766", "1767", "1768", "1769", "1770" };
long totalRows = 0;

foreach (var year in years)
{
    var config = new S3ConnectorConfiguration
    {
        Region = "us-east-1",
        BucketName = "noaa-ghcn-pds",
        ObjectKey = $"csv/by_year/{year}.csv"
    };

    using var connector = new S3DataConnector(config);
    var data = await connector.GetDataAsync();
    totalRows += data.RowCount;
    Console.WriteLine($"Loaded {year}: {data.RowCount:N0} rows");
}
```

## Prefix Pattern Matching

The S3 connector detects prefixes by:

| Pattern | Matches |
|---------|---------|
| `csv/by_year/` | All files in folder |
| `csv/by_year/20` | 2000.csv - 2099.csv |
| `csv/by_year/202` | 2020.csv - 2029.csv |
| `csv/by_year/2024` | 2024.csv only |

## NOAA GHCN Dataset

| File Pattern | Description | Size |
|--------------|-------------|------|
| `csv/by_year/YYYY.csv` | Daily observations | ~30MB/year |
| `ghcnd-stations.txt` | Weather station metadata | ~12MB |
| `ghcnd-countries.txt` | Country codes | ~5KB |
| `ghcnd-inventory.txt` | Station inventory | ~35MB |

## How to Run

```bash
cd S3WildcardPattern
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.S3Connector Wildcard Pattern Sample ===

1. NOAA GHCN Dataset
   [Dataset description]

2. Single File Load (Baseline)
   Loading: csv/by_year/1763.csv
   (1763 is the earliest year - very few weather stations existed)
   Rows: ~1,000
   Time: ~1-2 seconds

3. Understanding Prefix Patterns
   [Pattern detection rules]

4. Loading Multiple Years with Prefix Pattern
   [Multi-segment configuration]

5. Practical Example: Loading Early Years (1763-1770)
   Loading the earliest years of climate data.
   Early years have few observations (limited weather stations).
   Modern years (2020+) have ~35-40 million rows each!

   Loading 1763.csv... ~1,000 rows (1200ms)
   Loading 1764.csv... ~1,000 rows (800ms)
   Loading 1765.csv... ~1,000 rows (750ms)
   ...
   Loading 1770.csv... ~1,500 rows (900ms)

   Total rows loaded: ~10,000
   Total time: ~8-10 seconds
   Throughput: ~1,000 rows/sec

=== Sample Complete ===
```

## Error Handling for Multi-Segment

```csharp
var failedSegments = new List<string>();

var config = new S3ConnectorConfiguration
{
    ObjectKey = "csv/by_year/",
    AllowMultipleSegments = true,
    ErrorHandler = (ex) =>
    {
        failedSegments.Add(ex.Message);
        // Processing continues with next segment
    }
};

await connector.GetStorageDataAsync(velocity, batchSize: 50000);

if (failedSegments.Count > 0)
    Console.WriteLine($"{failedSegments.Count} segments failed");
```

## Performance Tips

- Use larger batch sizes (100k+) for multi-GB datasets
- Disable auto-compaction during bulk loading
- LZ4 compression balances speed and size
- Consider loading files individually for progress tracking
- Use ErrorHandler to continue on transient failures

## Related Samples

- **S3MultiSegment** - Basic multi-segment loading
- **S3PublicDataset** - Other NOAA data loading
- **S3ToVelocity** - VelocityDataBlock patterns
- **S3ErrorHandling** - Error handling strategies
