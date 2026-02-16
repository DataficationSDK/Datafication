# ParquetExport Sample

Demonstrates exporting DataBlocks to Parquet format using the Datafication.ParquetConnector library.

## Overview

This sample shows how to:
- Export DataBlocks to Parquet bytes using `ParquetSinkAsync()`
- Use synchronous export with `ParquetSink()`
- Track skipped columns during export
- Save Parquet data to files
- Perform round-trip verification (export and re-import)
- Handle unsupported column types

## Key Features Demonstrated

### Basic Async Export

```csharp
var parquetBytes = await dataBlock.ParquetSinkAsync();
await File.WriteAllBytesAsync("output.parquet", parquetBytes);
```

### Synchronous Export

```csharp
var parquetBytes = dataBlock.ParquetSink();
File.WriteAllBytes("output.parquet", parquetBytes);
```

### Export with Skipped Columns Tracking

```csharp
var (data, skippedColumns) = await dataBlock.ParquetSinkWithSkippedColumnsAsync();

if (skippedColumns.Count > 0)
{
    Console.WriteLine($"Skipped: {string.Join(", ", skippedColumns)}");
}
```

### Round-Trip Verification

```csharp
// Export
var bytes = await dataBlock.ParquetSinkAsync();
await File.WriteAllBytesAsync("temp.parquet", bytes);

// Re-import
var reloaded = await DataBlock.Connector.LoadParquetAsync(
    new Uri("file://" + "temp.parquet"));

// Verify
Console.WriteLine($"Original: {dataBlock.RowCount}, Reloaded: {reloaded.RowCount}");
```

## Supported Types

| C# Type | Parquet Type |
|---------|--------------|
| int / Int32 | INT32 |
| long / Int64 | INT64 |
| float | FLOAT |
| double | DOUBLE |
| decimal | DECIMAL |
| bool | BOOLEAN |
| string | BYTE_ARRAY (UTF8) |
| DateTime | INT64 (TIMESTAMP) |
| byte[] | BYTE_ARRAY |

**Note:** Nested DataBlock columns are automatically skipped during export.

## How to Run

```bash
cd ParquetExport
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.ParquetConnector Export Sample ===

1. Creating sample DataBlock with various types...
   Created DataBlock with 5 rows and 6 columns
   Columns: Id, Name, Salary, StartDate, IsActive, Score

2. Exporting to Parquet (async)...
   Exported 1,234 bytes

3. Exporting to Parquet (sync)...
   Exported 1,234 bytes

4. Export with skipped columns tracking...
   Exported 567 bytes
   Skipped columns: Details
   Note: Nested DataBlock columns are not supported in Parquet format

5. Saving to file...
   Saved to: /tmp/ParquetExportSample/employees_export.parquet
   File size: 1,234 bytes

6. Round-trip verification (export then re-import)...
   Original rows: 5
   Reloaded rows: 5
   [Data comparison table showing all matches]

7. Exporting larger dataset...
   Rows exported: 50,000
   File size: 2,345,678 bytes

8. Supported Parquet Types:
   [Type reference table]

=== Sample Complete ===
```

## Use Cases

| Use Case | Approach |
|----------|----------|
| Data backup | Export to Parquet for archival |
| Data exchange | Share data in standard format |
| ETL pipeline | Transform then export results |
| Compression | Use Parquet's built-in compression |
| Schema preservation | Types are maintained in Parquet |

## Related Samples

- **ParquetCompression** - Export with different compression options
- **ParquetBasicLoad** - Loading Parquet files
- **ParquetRowGroups** - Remove RowGroup before export
