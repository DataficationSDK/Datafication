# ParquetRowGroups Sample

Demonstrates Parquet row group awareness in the Datafication.ParquetConnector library.

## Overview

This sample shows how to:
- Understand the auto-added RowGroup column
- Analyze row group distribution
- Filter data by row group
- Calculate statistics per row group
- Remove the RowGroup column when not needed

## What Are Row Groups?

Parquet files organize data into row groups, which are independent chunks of rows. The ParquetConnector automatically adds a `RowGroup` column (Int32) to track which row group each row came from. This enables:

- **Data partitioning analysis**: Understand how data is distributed
- **Parallel processing**: Process row groups independently
- **Memory optimization**: Work with specific row groups

## Key Features Demonstrated

### RowGroup Column Auto-Addition

```csharp
var data = await DataBlock.Connector.LoadParquetAsync(
    new Uri("file://" + parquetPath));

// RowGroup column is automatically added as first column
var hasRowGroup = data.Schema.GetColumnNames().Contains("RowGroup");
// hasRowGroup == true
```

### Analyzing Row Group Distribution

```csharp
var grouped = data.GroupBy("RowGroup");
var distribution = grouped.Info();
// Returns DataBlock with "Group" and "Count" columns
```

### Filtering by Row Group

```csharp
// Get data from specific row group
var group0 = data.Where("RowGroup", 0);
var group1 = data.Where("RowGroup", 1);
```

### Statistics Per Row Group

```csharp
// Total revenue per row group
var stats = data.GroupByAggregate(
    "RowGroup",
    "TotalRevenue",
    AggregationType.Sum,
    "TotalRevenue"
);
```

### Removing RowGroup Column

```csharp
// Remove RowGroup for clean output or export
data.RemoveColumn("RowGroup");
```

## How to Run

```bash
cd ParquetRowGroups
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.ParquetConnector Row Groups Sample ===

1. Loading Parquet file...
   Loaded 50000 rows with 15 columns

2. Understanding the RowGroup column:
   [Explanation of row groups]

3. Schema (note RowGroup as first column):
   RowGroup: Int32 <-- Auto-added
   Region: String
   Country: String
   ...

4. Row group distribution:
   RowGroup        Row Count
   ----------------------------------------
   0               10000
   1               10000
   ...
   Total row groups: 5

5. Filtering to specific row group (RowGroup = 0)...
   Rows in RowGroup 0: 10000
   [Sample data]

6. Statistics per row group:
   [Table with revenue and profit per group]

7. Removing RowGroup column for clean output...
   Original columns: 15
   After removal: 14

8. Sample data without RowGroup column:
   [Clean data table]

=== Sample Complete ===
```

## Use Cases for Row Groups

| Use Case | Approach |
|----------|----------|
| Parallel processing | Process each row group independently |
| Memory management | Load one row group at a time |
| Data analysis | Compare statistics across row groups |
| Data quality | Check for uneven distribution |
| Export | Remove RowGroup before exporting |

## Related Samples

- **ParquetBasicLoad** - Simple loading patterns
- **ParquetToVelocity** - Streaming with batch processing
- **ParquetExport** - Exporting without RowGroup column
