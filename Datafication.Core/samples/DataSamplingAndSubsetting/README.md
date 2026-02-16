# DataSamplingAndSubsetting Sample

This sample demonstrates data sampling and subsetting operations in Datafication.Core.

## Overview

The DataSamplingAndSubsetting sample shows how to:
- Get the first N rows using `Head()`
- Get the last N rows using `Tail()`
- Get random samples using `Sample()`
- Copy specific row ranges using `CopyRowRange()`
- Use seeds for reproducible sampling

## Key Features Demonstrated

### Head Operation

Get the first N rows from a DataBlock:
```csharp
var first5 = employees.Head(5);
```

### Tail Operation

Get the last N rows from a DataBlock:
```csharp
var last3 = employees.Tail(3);
```

### Sample Operation

#### Random Sampling
Get a random sample of rows:
```csharp
var randomSample = employees.Sample(5);
```

#### Reproducible Sampling
Use a seed for reproducible results:
```csharp
var sample1 = employees.Sample(5, seed: 42);
var sample2 = employees.Sample(5, seed: 42);
// sample1 and sample2 will have the same rows
```

### CopyRowRange Operation

Efficiently copy a range of rows:
```csharp
var middleRows = employees.CopyRowRange(startRow: 5, rowCount: 10);
```

### Practical Use Cases

#### Preview Data
```csharp
var preview = employees.Head(3);
```

#### Get Latest Records
```csharp
var latest = employees.Tail(5).Sort(SortDirection.Descending, "EmployeeId");
```

#### Random Sampling for Testing
```csharp
var testSample = employees.Sample(10, seed: 123);
```

#### Extract Specific Range
```csharp
var range = employees.CopyRowRange(startRow: 10, rowCount: 5);
```

#### Chain with Other Operations
```csharp
var topEarners = employees
    .Sort(SortDirection.Descending, "Salary")
    .Head(5);
```

## How to Run

```bash
cd DataSamplingAndSubsetting
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates:
- Head and Tail operations
- Random sampling with and without seeds
- Row range copying
- Practical use cases combining sampling with other operations

## Related Samples

- **BasicOperations** - Learn basic DataBlock operations
- **FilteringAndSorting** - Learn how to filter and sort data
- **ETLPipeline** - See sampling in a complete pipeline

