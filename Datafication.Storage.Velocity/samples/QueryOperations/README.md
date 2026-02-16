# QueryOperations Sample

This sample demonstrates query operations in Datafication.Storage.Velocity, including filtering, sorting, limiting, and the deferred execution query plan.

## Overview

The QueryOperations sample shows how to:
- Filter data with Where and comparison operators
- Use SIMD-accelerated string matching (Contains, StartsWith, EndsWith)
- Project specific columns with Select
- Sort data in ascending or descending order
- Limit results with Head, Tail, and Sample
- Chain multiple operations with deferred execution

## Key Features Demonstrated

### Filtering
- `Where(column, value)` - Equality filter
- `Where(column, value, ComparisonOperator)` - Comparison filter
- Multiple Where clauses combined in single pass

### String Pattern Matching (SIMD-Accelerated)
- `WhereContains()` - Find rows containing substring
- `WhereStartsWith()` - Find rows starting with prefix
- `WhereEndsWith()` - Find rows ending with suffix

### Column Projection
- `Select(columns...)` - Return only specified columns

### Sorting
- `Sort(SortDirection.Ascending, column)` - Sort ascending
- `Sort(SortDirection.Descending, column)` - Sort descending

### Limiting
- `Head(n)` - First n rows (with early termination)
- `Tail(n)` - Last n rows
- `Sample(n, seed)` - Random sample with optional seed

### Deferred Execution
- Build query chain without execution
- `Execute()` - Run optimized query plan
- Single-pass optimization for multiple operations

## How to Run

```bash
cd QueryOperations
dotnet restore
dotnet run
```

## Expected Output

The sample will output:
1. Simple Where filter results
2. Multiple Where conditions
3. WhereContains string matching
4. WhereStartsWith string matching
5. WhereEndsWith string matching
6. Select column projection
7. Ascending sort results
8. Descending sort results
9. Head (top N) results
10. Tail (bottom N) results
11. Random sample results
12. Combined query results
13. Complex query chain results

## Related Samples

- **GroupingAndAggregation** - Learn about grouping and aggregate functions
- **ComputedColumns** - Learn about expression-based computed columns
- **WindowFunctions** - Learn about SIMD-accelerated analytics
