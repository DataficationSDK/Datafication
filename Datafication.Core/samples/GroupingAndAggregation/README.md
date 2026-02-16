# GroupingAndAggregation Sample

This sample demonstrates how to group data and perform aggregations in Datafication.Core.

## Overview

The GroupingAndAggregation sample shows how to:
- Group data by columns using `GroupBy()`
- Perform aggregate operations (Min, Max, Sum, Mean, Count, StandardDeviation, Variance)
- Calculate percentiles
- Aggregate on all numeric columns
- Combine grouping with aggregation operations

## Key Features Demonstrated

### GroupBy Operations

#### Basic Grouping
```csharp
var groupedByDepartment = employees.GroupBy("Department");
```

#### Accessing Group Information
```csharp
var groupInfo = groupedByDepartment.Info(); // Returns DataBlock with group keys and counts
var groupCount = groupedByDepartment.Count;
var group = groupedByDepartment.GetGroup(0);
var groupKey = groupedByDepartment.GetGroupKey(0);
```

### Aggregate Functions

#### Single Column Aggregations
- `Min(columnName)` - Minimum value
- `Max(columnName)` - Maximum value
- `Sum(columnName)` - Sum of values
- `Mean(columnName)` - Average value
- `StandardDeviation(columnName)` - Standard deviation
- `Variance(columnName)` - Variance

```csharp
var minSalary = employees.Min("Salary");
var maxSalary = employees.Max("Salary");
var avgSalary = employees.Mean("Salary");
```

#### Aggregate All Numeric Columns
When no column is specified, aggregates are computed for all numeric columns:
```csharp
var allAverages = employees.Mean(); // Computes mean for all numeric columns
```

#### Percentile Calculations
```csharp
var medianSalary = employees.Percentile(0.5, "Salary");  // 50th percentile (median)
var p95Salary = employees.Percentile(0.95, "Salary");    // 95th percentile
```

#### Size
Get row count for all columns:
```csharp
var sizes = employees.Size();
```

### GroupBy with Aggregation

#### GroupByAggregate
Single aggregation per group:
```csharp
var departmentStats = employees.GroupByAggregate(
    "Department",
    "Salary",
    AggregationType.Mean,
    "AvgSalary"
);
```

#### Multiple Aggregations
Use `Aggregate()` method on grouped data:
```csharp
var departmentAggregations = employees.GroupBy("Department").Aggregate(
    new[] { "Count", "Salary" },
    new Dictionary<string, AggregationType>
    {
        { "Count", AggregationType.Count },
        { "Salary", AggregationType.Mean }
    }
);
```

### AggregationType Enum
- `Min` - Minimum value
- `Max` - Maximum value
- `Sum` - Sum of values
- `Mean` - Average value
- `Count` - Count of non-null values
- `StandardDeviation` - Standard deviation
- `Variance` - Variance

## How to Run

```bash
cd GroupingAndAggregation
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates:
- Grouping employees by department
- Various aggregate operations on salary data
- Percentile calculations
- Combined grouping and aggregation operations

## Related Samples

- **FilteringAndSorting** - Learn how to filter and sort data before grouping
- **MergingDataBlocks** - Learn how to merge data from multiple sources
- **ETLPipeline** - See grouping and aggregation in a complete pipeline

