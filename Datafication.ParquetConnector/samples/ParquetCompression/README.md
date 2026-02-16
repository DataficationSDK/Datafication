# ParquetCompression Sample

Demonstrates compression options when exporting DataBlocks to Parquet format.

## Overview

This sample shows how to:
- Export with different compression methods (Snappy, Gzip, None)
- Compare file sizes and export times
- Choose the right compression for your use case
- Verify compressed exports can be re-loaded

## Compression Methods

| Method | Description |
|--------|-------------|
| **Snappy** | Default. Fast compression with good ratio. Best for general use. |
| **Gzip** | Higher compression ratio but slower. Best for archival or network transfer. |
| **None** | No compression. Fastest export but largest file size. |

## Key Features Demonstrated

### Export with Snappy (Default)

```csharp
var bytes = await dataBlock.ParquetSinkAsync(CompressionMethod.Snappy);
// or simply (Snappy is default):
var bytes = await dataBlock.ParquetSinkAsync();
```

### Export with Gzip

```csharp
var bytes = await dataBlock.ParquetSinkAsync(CompressionMethod.Gzip);
```

### Export with No Compression

```csharp
var bytes = await dataBlock.ParquetSinkAsync(CompressionMethod.None);
```

### Sync API with Compression

```csharp
var snappyBytes = dataBlock.ParquetSink(CompressionMethod.Snappy);
var gzipBytes = dataBlock.ParquetSink(CompressionMethod.Gzip);
```

## Typical Compression Results

| Method | Relative Size | Export Speed | Decompress Speed |
|--------|---------------|--------------|------------------|
| None | 1.00x (baseline) | Fastest | Fastest |
| Snappy | ~0.30-0.50x | Fast | Very Fast |
| Gzip | ~0.20-0.40x | Slower | Fast |

*Actual results vary based on data characteristics.*

## How to Run

```bash
cd ParquetCompression
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.ParquetConnector Compression Sample ===

1. Loading source data...
   Loaded 50,000 rows with 14 columns

2. Exporting with Snappy compression (default)...
   Size: 1,234,567 bytes
   Time: 150 ms

3. Exporting with Gzip compression...
   Size: 987,654 bytes
   Time: 450 ms

4. Exporting with no compression...
   Size: 2,345,678 bytes
   Time: 80 ms

5. Compression Comparison:
   Method       Size (bytes)       Time (ms)    Ratio        Notes
   ---------------------------------------------------------------------------
   None         2,345,678          80           1.00x        Fastest export
   Snappy       1,234,567          150          0.53x        Best balance
   Gzip         987,654            450          0.42x        Smallest size
   ---------------------------------------------------------------------------

   Space saved with Snappy: 47.4%
   Space saved with Gzip: 57.9%

6. Saving compressed files...
   [File paths]

7. Verifying all exports can be re-loaded...
   Snappy    : 50,000 rows loaded - OK
   Gzip      : 50,000 rows loaded - OK
   None      : 50,000 rows loaded - OK

8. Compression Recommendations:
   [Use case recommendations table]

=== Sample Complete ===
```

## Compression Recommendations

| Use Case | Recommended Method | Reason |
|----------|-------------------|--------|
| General purpose | Snappy | Best balance of speed and size |
| Maximum compression | Gzip | Smallest file size |
| Fastest export | None | No compression overhead |
| Network transfer | Gzip | Smaller = faster transfer |
| Local SSD storage | Snappy | Fast decompression |
| Archival | Gzip | Best compression ratio |
| Real-time processing | None or Snappy | Low latency |

## Related Samples

- **ParquetExport** - Basic export functionality
- **ParquetBasicLoad** - Loading compressed Parquet files
- **ParquetToVelocity** - Streaming large datasets
