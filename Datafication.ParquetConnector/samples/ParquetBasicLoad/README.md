# ParquetBasicLoad Sample

Demonstrates the simplest patterns for loading Parquet files using the Datafication.ParquetConnector library.

## Overview

This sample shows how to:
- Load Parquet files using the shorthand `LoadParquetAsync()` method
- Load Parquet files using the synchronous `LoadParquet()` method
- Inspect the schema and data types of loaded data
- Display data using row cursors
- Perform basic filtering and sorting operations

## Key Features Demonstrated

### Asynchronous Loading (Recommended)

```csharp
var data = await DataBlock.Connector.LoadParquetAsync(
    new Uri("file://" + parquetPath));
Console.WriteLine($"Loaded {data.RowCount} rows");
```

### Synchronous Loading

```csharp
var data = DataBlock.Connector.LoadParquet(
    new Uri("file://" + parquetPath));
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
var cursor = data.GetRowCursor("Region", "Country", "TotalRevenue");
while (cursor.MoveNext())
{
    var region = cursor.GetValue("Region");
    var country = cursor.GetValue("Country");
    var revenue = cursor.GetValue("TotalRevenue");
}
```

## How to Run

```bash
cd ParquetBasicLoad
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.ParquetConnector Basic Load Sample ===

1. Loading Parquet asynchronously...
   Loaded 50000 rows with 15 columns

2. Schema Information:
   - RowGroup: Int32
   - Region: String
   - Country: String
   - ItemType: String
   - SalesChannel: String
   - OrderPriority: String
   - OrderDate: DateTime
   - OrderId: Int64
   - ShipDate: DateTime
   - UnitsSold: Int32
   - UnitPrice: Double
   - UnitCost: Double
   - TotalRevenue: Double
   - TotalCost: Double
   - TotalProfit: Double

3. Loading Parquet synchronously...
   Loaded 50000 rows

4. First 10 sales records:
   [Table showing sales data]

5. Filtering: Sales in Europe...
   Found 9972 sales records in Europe

6. Sorting: Top 5 highest revenue transactions...
   [Table showing top earners]

=== Sample Complete ===
```

## Data File

This sample uses `data/SalesRecords50k.parquet` which contains 50,000 sales records with the following columns:
- Region (string)
- Country (string)
- ItemType (string)
- SalesChannel (string)
- OrderPriority (string)
- OrderDate (DateTime)
- OrderId (long)
- ShipDate (DateTime)
- UnitsSold (int)
- UnitPrice (double)
- UnitCost (double)
- TotalRevenue (double)
- TotalCost (double)
- TotalProfit (double)

## Related Samples

- **ParquetConfiguration** - Loading with full configuration options
- **ParquetErrorHandling** - Error handling patterns
- **ParquetExport** - Exporting DataBlocks to Parquet format
