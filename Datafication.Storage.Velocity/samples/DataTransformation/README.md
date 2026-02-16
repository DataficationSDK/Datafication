# DataTransformation Sample

This sample demonstrates data transformation operations in Datafication.Storage.Velocity for data cleaning, reshaping, and joining datasets.

## Overview

The DataTransformation sample shows how to:
- Remove rows with null values using DropNulls
- Remove duplicate rows using DropDuplicates
- Fill missing values with various strategies
- Reshape data with Melt (unpivot)
- Join datasets with Merge
- Build data quality pipelines

## Key Features Demonstrated

### DropNulls
- `DropNulls("column")` - Drop rows where specific column is null
- `DropNulls(DropNullMode.Any)` - Drop rows with any null value
- `DropNulls(DropNullMode.All)` - Drop rows where all values are null

### DropDuplicates
- `DropDuplicates(KeepDuplicateMode.First, columns)` - Keep first occurrence
- `DropDuplicates(KeepDuplicateMode.Last, columns)` - Keep last occurrence
- `DropDuplicates(KeepDuplicateMode.None)` - Remove all duplicates

### FillNulls
- `FillMethod.ForwardFill` - Propagate last valid value forward
- `FillMethod.BackwardFill` - Use next valid value
- `FillMethod.Mean` - Fill with column mean
- `FillMethod.Median` - Fill with column median
- `FillMethod.ConstantValue` - Fill with specific value
- `FillMethod.LinearInterpolation` - Linear interpolation

### Melt (Unpivot)
- Convert wide format to long format
- Specify fixed columns and melted column/value names

### Merge (Join)
- `MergeMode.Inner` - Only matching rows
- `MergeMode.Left` - All left rows, matching right
- `MergeMode.Right` - All right rows, matching left
- `MergeMode.Full` - All rows from both sides

### Data Quality Pipelines
- Chain multiple transformations in single query
- Combine Where, DropNulls, FillNulls, DropDuplicates

## How to Run

```bash
cd DataTransformation
dotnet restore
dotnet run
```

## Expected Output

The sample will output:
1. DropNulls - removing null rows
2. DropDuplicates - first vs last occurrence
3. FillNulls - forward fill and mean fill
4. Melt - wide to long transformation
5. Merge - inner and left join examples
6. Complete data quality pipeline

## Related Samples

- **QueryOperations** - Learn about filtering and selection
- **GroupingAndAggregation** - Learn about aggregation
- **StorageManagement** - Learn about compaction and storage
