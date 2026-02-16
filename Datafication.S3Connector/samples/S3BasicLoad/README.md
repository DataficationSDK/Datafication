# S3BasicLoad Sample

Demonstrates the simplest pattern for loading data from AWS S3 using anonymous access to a public bucket.

## Overview

This sample shows how to:
- Configure S3ConnectorConfiguration for public bucket access
- Create an S3DataConnector instance
- Load a single file from S3 into memory using `GetDataAsync()`
- Inspect schema and data
- Properly dispose the connector

## Key Features Demonstrated

### Anonymous Access to Public Buckets

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/1763.csv"  // Historical climate data
    // No credentials needed for public buckets
};

var connector = new S3DataConnector(config);
var data = await connector.GetDataAsync();
```

### Schema Inspection

```csharp
foreach (var colName in data.Schema.GetColumnNames())
{
    var column = data.GetColumn(colName);
    Console.WriteLine($"{colName}: {column.DataType.GetClrType().Name}");
}
```

### Row Cursor Iteration

```csharp
var cursor = data.GetRowCursor("Column1", "Column2", "Column3");
while (cursor.MoveNext())
{
    var value1 = cursor.GetValue("Column1");
    var value2 = cursor.GetValue("Column2");
}
```

### Proper Resource Disposal

```csharp
try
{
    var data = await connector.GetDataAsync();
    // Use data...
}
finally
{
    connector.Dispose();
}
```

## How to Run

```bash
cd S3BasicLoad
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.S3Connector Basic Load Sample ===

1. Creating S3 configuration for NOAA climate data...
   Bucket: noaa-ghcn-pds
   Region: us-east-1
   Object: csv/by_year/1763.csv

2. Loading data from S3...
   Loaded rows with columns
   Download and parse time: ~1-2 seconds

3. Schema Information:
   - Column names and types displayed

4. First 10 climate observations:
   [Table showing climate observation data]

5. Data Summary:
   Total observations: displayed
   Memory footprint: In-memory DataBlock

6. Connector disposed.

=== Sample Complete ===
```

## Public Dataset Used

This sample uses the **NOAA Global Historical Climatology Network Daily (GHCN-D)** dataset:

- **Bucket**: `noaa-ghcn-pds`
- **Region**: `us-east-1`
- **File**: `csv/by_year/1763.csv` (historical climate data, small file)
- **Description**: Daily climate observations from the GHCN-D network
- **Documentation**: [NOAA GHCN-D on AWS](https://registry.opendata.aws/noaa-ghcn/)

## Related Samples

- **S3PublicDataset** - More examples with public datasets
- **S3Configuration** - Full configuration options
- **S3ErrorHandling** - Error handling patterns
- **S3ToVelocity** - Streaming large files to disk storage
