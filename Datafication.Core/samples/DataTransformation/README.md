# DataTransformation Sample

This sample demonstrates data transformation operations in Datafication.Core, including transpose, melt, null handling, and data reshaping.

## Overview

The DataTransformation sample shows how to:
- Transpose rows and columns
- Melt (unpivot) data from wide to long format
- Drop rows with null values
- Fill null values using various strategies
- Chain multiple transformation operations

## Key Features Demonstrated

### Transpose Operations

#### Basic Transpose
Swap rows and columns:
```csharp
var transposed = employees.Transpose();
```

#### Transpose with Header Column
Use a specific column as headers:
```csharp
var transposedWithHeaders = employees.Transpose("Name");
```

### Melt (Unpivot) Operations

Convert from wide format to long format:
```csharp
var melted = employees.Melt(
    fixedColumns: new[] { "EmployeeId", "Name" },
    meltedColumnName: "Attribute",
    meltedValueName: "Value"
);
```

### DropNulls Operations

#### DropNullMode.Any
Drop rows where any column is null:
```csharp
var noNulls = employees.DropNulls(DropNullMode.Any);
```

#### DropNullMode.All
Drop rows only where all columns are null:
```csharp
var allNulls = employees.DropNulls(DropNullMode.All);
```

### FillNulls Operations

#### ForwardFill
Propagate last valid observation forward:
```csharp
var forwardFilled = employees.FillNulls(FillMethod.ForwardFill, "Salary");
```

#### BackwardFill
Use next valid observation to fill backwards:
```csharp
var backwardFilled = employees.FillNulls(FillMethod.BackwardFill, "Bonus");
```

#### ConstantValue
Fill with a constant value:
```csharp
var constantFilled = employees.FillNulls(FillMethod.ConstantValue, 0.0, "Bonus");
```

#### Mean
Fill with the mean of the column:
```csharp
var meanFilled = employees.FillNulls(FillMethod.Mean, "Salary");
```

#### Median
Fill with the median of the column:
```csharp
var medianFilled = employees.FillNulls(FillMethod.Median, "Salary");
```

#### Mode
Fill with the most frequent value:
```csharp
var modeFilled = employees.FillNulls(FillMethod.Mode, "Department");
```

#### LinearInterpolation
Fill using linear interpolation (for time series):
```csharp
var interpolated = sensors.FillNulls(FillMethod.LinearInterpolation, "Temperature");
```

### Chaining Operations

Chain multiple fill operations:
```csharp
var cleaned = employees
    .FillNulls(FillMethod.ForwardFill, "Salary")
    .FillNulls(FillMethod.Mean, "Bonus")
    .FillNulls(FillMethod.ConstantValue, (object)"Unknown", "Department");
```

### FillMethod Enum
- `ForwardFill` - Propagate last valid value forward
- `BackwardFill` - Use next valid value backward
- `ConstantValue` - Fill with specified constant
- `Mean` - Fill with column mean (numeric only)
- `Median` - Fill with column median (numeric only)
- `Mode` - Fill with most frequent value
- `LinearInterpolation` - Linear interpolation (numeric, time series)

### DropNullMode Enum
- `Any` - Drop if any column is null
- `All` - Drop only if all columns are null

## How to Run

```bash
cd DataTransformation
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates:
- Transpose operations with and without header columns
- Melt operation converting wide to long format
- DropNulls with different modes (Any/All)
- Various null filling strategies:
  - ForwardFill - propagate last valid value
  - BackwardFill - use next valid value
  - ConstantValue - fill with a constant
  - Mean - fill with column mean (works with nullable numeric types)
  - Median - fill with column median (works with nullable numeric types)
  - Mode - fill with most frequent value
  - LinearInterpolation - interpolate between values
- Chained transformation operations

## Related Samples

- **DataQuality** - Learn about data quality operations
- **RemovingDuplicates** - Learn how to remove duplicate rows
- **ETLPipeline** - See transformations in a complete pipeline

