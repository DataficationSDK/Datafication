# ExpressionsAndComputedColumns Sample

This sample demonstrates the expression engine and computed columns in Datafication.Core.

## Overview

The ExpressionsAndComputedColumns sample shows how to:
- Create computed columns using expressions
- Use arithmetic, comparison, and logical operators
- Validate expressions before using them
- Use computed columns in data pipelines
- Build complex calculations with parentheses

## Key Features Demonstrated

### Computed Columns

#### Basic Computed Column
Add a new column calculated from existing columns:
```csharp
var withRevenue = sales.Compute("Revenue", "Price * Quantity");
```

#### Complex Expressions
Chain multiple computed columns:
```csharp
var withProfit = sales
    .Compute("Revenue", "Price * Quantity")
    .Compute("TotalCost", "Cost * Quantity")
    .Compute("Profit", "Revenue - TotalCost")
    .Compute("ProfitMargin", "Profit / Revenue");
```

### Expression Validation

Validate expressions before using them:
```csharp
if (sales.ValidateExpression("Price * 0.15", out string error))
{
    var withDiscount = sales.Compute("Discount", "Price * 0.15");
}
else
{
    Console.WriteLine($"Invalid expression: {error}");
}
```

### Using Computed Columns in Pipelines

Computed columns can be used in filtering, sorting, and other operations:
```csharp
var analysis = sales
    .Compute("Profit", "Revenue - TotalCost")
    .Where("Profit", 500m, ComparisonOperator.GreaterThan)
    .Select("ProductName", "Profit")
    .Sort(SortDirection.Descending, "Profit");
```

### Expression Operators

#### Arithmetic Operators
- `+` - Addition
- `-` - Subtraction
- `*` - Multiplication
- `/` - Division
- `%` - Modulo

#### Comparison Operators
- `==` - Equality
- `!=` - Inequality
- `<` - Less than
- `<=` - Less than or equal
- `>` - Greater than
- `>=` - Greater than or equal

#### Logical Operators
- `&&` - Logical AND
- `||` - Logical OR
- `!` - Logical NOT

#### Parentheses
Use parentheses for grouping operations:
```csharp
var result = sales.Compute("ComplexCalc", "(Price - Cost) * Quantity");
```

### Math Functions

Built-in math functions for calculations:
```csharp
var result = data
    .Compute("distance", "SQRT(x * x + y * y)")
    .Compute("absError", "ABS(predicted - actual)")
    .Compute("roundedPrice", "ROUND(price, 2)");
```

Supported: `ABS`, `FLOOR`, `CEIL`, `ROUND`, `SQRT`, `POWER`, `EXP`, `LOG`, `LOG10`

### Date Functions

Extract date components:
```csharp
var result = orders
    .Compute("year", "YEAR(order_date)")
    .Compute("month", "MONTH(order_date)")
    .Compute("quarter", "QUARTER(order_date)")
    .Compute("dayOfWeek", "DAYOFWEEK(order_date)");
```

Date arithmetic:
```csharp
var result = orders
    .Compute("dueDate", "DATEADD('day', 30, order_date)")
    .Compute("processingDays", "DATEDIFF('day', order_date, ship_date)");
```

Supported extraction: `YEAR`, `MONTH`, `DAY`, `HOUR`, `MINUTE`, `SECOND`, `DAYOFWEEK`, `DAYOFYEAR`, `QUARTER`
Supported arithmetic: `DATEADD`, `DATEDIFF`, `NOW`, `TODAY`
SQL-compatible units: `year/yy`, `quarter/qq`, `month/mm`, `day/dd`, `hour/hh`, `minute/mi`, `second/ss`

### String Functions

Manipulate string data:
```csharp
var result = customers
    .Compute("upperName", "UPPER(name)")
    .Compute("cleanName", "TRIM(name)")
    .Compute("nameLength", "LEN(name)")
    .Compute("domain", "LOWER(RIGHT(email, LEN(email) - CHARINDEX('@', email)))");
```

String extraction and manipulation:
```csharp
var result = data
    .Compute("firstName", "LEFT(fullName, CHARINDEX(' ', fullName) - 1)")
    .Compute("cleanPhone", "REPLACE(phone, '-', '')")
    .Compute("fullName", "CONCAT(firstName, ' ', lastName)")
    .Compute("paddedCode", "LPAD(productCode, 8, '0')");
```

Supported: `UPPER`, `LOWER`, `TRIM`, `LTRIM`, `RTRIM`, `LEN`, `LENGTH`, `SUBSTRING`, `LEFT`, `RIGHT`, `REPLACE`, `CHARINDEX`, `CONCAT`, `LPAD`, `RPAD`, `REVERSE`

### CASE WHEN Conditionals

```csharp
var result = data.Compute("tier",
    "CASE WHEN revenue > 1000000 THEN 1 WHEN revenue > 500000 THEN 2 ELSE 3 END");
```

### Expression Features

- Column references by name
- Arithmetic operations
- Comparison operations
- Logical operations
- Parentheses for grouping
- Math functions (ABS, SQRT, ROUND, etc.)
- Date functions (YEAR, MONTH, DATEADD, etc.)
- String functions (UPPER, LOWER, SUBSTRING, etc.)
- CASE WHEN conditionals
- Support for numeric, boolean, DateTime, and string types

## How to Run

```bash
cd ExpressionsAndComputedColumns
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates:
- Basic and complex computed columns
- Expression validation
- Using computed columns in pipelines
- Various expression operators
- Logical operations

## Related Samples

- **BasicOperations** - Learn basic DataBlock operations
- **FilteringAndSorting** - Learn how to filter and sort data
- **ETLPipeline** - See computed columns in a complete pipeline

