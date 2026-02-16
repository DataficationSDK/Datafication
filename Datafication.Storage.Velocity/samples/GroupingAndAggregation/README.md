# GroupingAndAggregation Sample

This sample demonstrates grouping and aggregation operations in Datafication.Storage.Velocity for analytics and summarization.

## Overview

The GroupingAndAggregation sample shows how to:
- Group data by a column
- Apply aggregation functions (Sum, Mean, Min, Max, Count)
- Perform multiple aggregations in a single query
- Combine filtering with grouping
- Calculate dataset-wide statistics

## Key Features Demonstrated

### GroupBy Operations
- `GroupBy(column)` - Group rows by column value
- Access groups with `GetGroupKey()` and `GetGroup()`

### Single Aggregation
- `GroupByAggregate(groupColumn, aggregateColumn, AggregationType, resultName)`
- Creates summarized results with group key and aggregate value

### Multiple Aggregations
- `GroupByAggregate(groupColumn, Dictionary<string, AggregationType>)`
- Apply different aggregations to different columns
- Result columns named with pattern: `{aggregationType}_{columnName}`

### Aggregation Types
- `Sum` - Total of values
- `Mean` - Average value
- `Min` - Minimum value
- `Max` - Maximum value
- `Count` - Number of non-null values
- `StandardDeviation` - Standard deviation
- `Variance` - Statistical variance

### Dataset Statistics
- `Min()`, `Max()`, `Mean()` - Column-level statistics
- `StandardDeviation()`, `Variance()` - Spread measures

## How to Run

```bash
cd GroupingAndAggregation
dotnet restore
dotnet run
```

## Expected Output

The sample will output:
1. GroupBy results with group counts
2. Sum aggregation by Region
3. Mean aggregation by Category
4. Multiple aggregations (Sum, Count)
5. Min and Max by Category
6. Dataset-wide statistics
7. Filtered grouping (Electronics only)
8. Sorted aggregation results

## Related Samples

- **QueryOperations** - Learn about filtering and sorting
- **WindowFunctions** - Learn about rolling aggregations
- **ComputedColumns** - Learn about calculated fields
