# FilteringAndSorting Sample

This sample demonstrates how to filter and sort data in Datafication.Core using various comparison operators and sorting methods.

## Overview

The FilteringAndSorting sample shows how to:
- Filter data using `Where()` with equality
- Use comparison operators (GreaterThan, LessThan, etc.)
- Apply string operations (Contains, StartsWith, EndsWith)
- Use `WhereIn()` for multiple value matching
- Use `WhereNot()` for exclusion
- Sort data in ascending and descending order
- Chain filtering and sorting operations

## Key Features Demonstrated

### Filtering Operations

#### Where with Equality
```csharp
var engineeringEmployees = employees.Where("Department", "Engineering");
```

#### Comparison Operators
- `Equals` - Default, tests for equality
- `NotEquals` - Tests for inequality
- `GreaterThan` - Column value is greater than specified value
- `GreaterThanOrEqual` - Column value is greater than or equal
- `LessThan` - Column value is less than specified value
- `LessThanOrEqual` - Column value is less than or equal

```csharp
var highEarners = employees.Where("Salary", 80000m, ComparisonOperator.GreaterThan);
```

#### String Operations
- `Contains` - Column value contains the specified substring
- `StartsWith` - Column value starts with the specified string
- `EndsWith` - Column value ends with the specified string

```csharp
var namesStartingWithA = employees.Where("Name", "A", ComparisonOperator.StartsWith);
```

#### WhereIn
Filter rows where column value is in a collection:
```csharp
var selectedDepartments = employees.WhereIn("Department", new[] { "Engineering", "Sales" });
```

#### WhereNot
Filter rows where column does not match value:
```csharp
var nonMarketingEmployees = employees.WhereNot("Department", "Marketing");
```

### Sorting Operations

#### Sort
Sort data by a column in ascending or descending order:
```csharp
var sortedAsc = employees.Sort(SortDirection.Ascending, "Salary");
var sortedDesc = employees.Sort(SortDirection.Descending, "Salary");
```

### Method Chaining
Chain multiple operations together:
```csharp
var result = employees
    .Where("Department", "Engineering")
    .Where("Salary", 90000m, ComparisonOperator.GreaterThanOrEqual)
    .Sort(SortDirection.Descending, "Salary");
```

## How to Run

```bash
cd FilteringAndSorting
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates various filtering and sorting scenarios with output showing:
- Filtered results based on different criteria
- Sorted results in ascending and descending order
- Chained operations combining filters and sorts

## Related Samples

- **BasicOperations** - Learn basic DataBlock operations
- **GroupingAndAggregation** - Learn how to group and aggregate data
- **ETLPipeline** - See filtering and sorting in a complete pipeline

