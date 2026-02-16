# Datafication.Core

[![NuGet](https://img.shields.io/nuget/v/Datafication.Core.svg)](https://www.nuget.org/packages/Datafication.Core)

An in-memory data processing library for .NET that provides a powerful DataFrame-like API for data manipulation, transformation, and analysis.

## Description

Datafication.Core is a modern .NET library designed for efficient in-memory data operations. At its heart is the `DataBlock` class, which provides a rich API for working with tabular data similar to pandas DataFrames or R data.frames. The library excels at data transformation pipelines, ETL operations, and analytical workloads where performance and ease of use are paramount.

### Key Features

- **High Performance**: Optimized for speed with SIMD-accelerated operations and minimal memory allocations
- **Fluent API**: Chainable methods for building elegant data transformation pipelines
- **Type Safe**: Strongly-typed columns with full .NET type system support
- **Expression Engine**: Computed columns using powerful expression syntax
- **Rich Operations**: Comprehensive set of filtering, grouping, aggregation, and transformation operations
- **Window Functions**: Advanced analytics including moving averages, cumulative totals, ranking, and lag/lead operations
- **Multiple Data Sources**: Built-in connectors for CSV, Excel, JSON, Parquet, databases, and more
- **Zero Dependencies**: Core library has no external dependencies (connectors are separate packages)

## Table of Contents

- [Description](#description)
  - [Key Features](#key-features)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
  - [Creating and Populating DataBlocks](#creating-and-populating-datablocks)
  - [Column Operations](#column-operations)
  - [Row Operations](#row-operations)
  - [Filtering Data](#filtering-data)
  - [Sorting Data](#sorting-data)
  - [Grouping and Aggregation](#grouping-and-aggregation)
  - [Pivoting Data](#pivoting-data)
  - [Window Functions](#window-functions)
  - [Merging DataBlocks](#merging-datablocks)
  - [Data Sampling and Subsetting](#data-sampling-and-subsetting)
  - [Data Transformation](#data-transformation)
  - [Removing Duplicates](#removing-duplicates)
  - [Computed Columns with Expressions](#computed-columns-with-expressions)
  - [Schema and Information](#schema-and-information)
  - [Cloning and Copying](#cloning-and-copying)
  - [Method Chaining for Data Pipelines](#method-chaining-for-data-pipelines)
  - [Disposal and Resource Management](#disposal-and-resource-management)
  - [Loading Data from External Sources](#loading-data-from-external-sources)
- [Performance Tips](#performance-tips)
- [Common Patterns](#common-patterns)
  - [ETL Pipeline](#etl-pipeline)
  - [Data Analysis](#data-analysis)
  - [Data Quality](#data-quality)
- [API Reference](#api-reference)
  - [Core Classes](#core-classes)
  - [Enumerations](#enumerations)
- [License](#license)

## Installation

> **Note**: Datafication.Core is currently in pre-release. The packages are now available on nuget.org.

```bash
dotnet add package Datafication.Core
```

**Running the Samples:**

```bash
cd samples/BasicOperations
dotnet run
```

## Usage Examples

### Creating and Populating DataBlocks

```csharp
using Datafication.Core.Data;

// Create a new DataBlock
var employees = new DataBlock();

// Add columns with type information
employees.AddColumn(new DataColumn("EmployeeId", typeof(int)));
employees.AddColumn(new DataColumn("Name", typeof(string)));
employees.AddColumn(new DataColumn("Department", typeof(string)));
employees.AddColumn(new DataColumn("Salary", typeof(decimal)));
employees.AddColumn(new DataColumn("HireDate", typeof(DateTime)));

// Add rows
employees.AddRow(new object[] { 1, "Alice Johnson", "Engineering", 95000m, new DateTime(2020, 3, 15) });
employees.AddRow(new object[] { 2, "Bob Smith", "Marketing", 72000m, new DateTime(2021, 6, 1) });
employees.AddRow(new object[] { 3, "Carol White", "Engineering", 88000m, new DateTime(2019, 11, 20) });
employees.AddRow(new object[] { 4, "David Brown", "Sales", 65000m, new DateTime(2022, 1, 10) });
employees.AddRow(new object[] { 5, "Eve Davis", "Engineering", 102000m, new DateTime(2018, 5, 3) });

Console.WriteLine($"Total employees: {employees.RowCount}");
```

### Column Operations

```csharp
// Check if column exists
if (employees.HasColumn("Salary"))
{
    Console.WriteLine("Salary column exists");
}

// Get a column by name
var nameColumn = employees.GetColumn("Name");
Console.WriteLine($"Column: {nameColumn.Name}, Type: {nameColumn.DataType.GetClrType().Name}");

// Access column using indexer
var salaryColumn = employees["Salary"];

// Select specific columns (projection)
var nameAndSalary = employees.Select("Name", "Salary");

// Remove columns
var withoutHireDate = employees.Clone();
withoutHireDate.RemoveColumn("HireDate");
```

### Row Operations

```csharp
// Get row count
Console.WriteLine($"Total rows: {employees.RowCount}");

// Access individual cell values using indexer
var firstEmployeeName = employees[0, "Name"];
Console.WriteLine($"First employee: {firstEmployeeName}");

// Update a cell value
employees[0, "Salary"] = 98000m;

// Insert a row at specific position
employees.InsertRow(2, new object[] { 6, "Frank Miller", "HR", 70000m, new DateTime(2021, 9, 15) });

// Remove a row by index
employees.RemoveRow(5);

// Update an entire row
employees.UpdateRow(0, new object[] { 1, "Alice Johnson-Smith", "Engineering", 98000m, new DateTime(2020, 3, 15) });

// Iterate over rows
var cursor = employees.GetRowCursor("Name", "Department", "Salary");
while (cursor.MoveNext())
{
    var name = cursor.GetValue("Name");
    var dept = cursor.GetValue("Department");
    var salary = cursor.GetValue("Salary");
    Console.WriteLine($"{name} - {dept}: ${salary}");
}
```

### Filtering Data

```csharp
// Filter using Where with equality
var engineeringEmployees = employees.Where("Department", "Engineering");

// Filter with comparison operators
var highEarners = employees.Where("Salary", 80000m, ComparisonOperator.GreaterThan);

// Filter using multiple conditions
var seniorEngineers = employees
    .Where("Department", "Engineering")
    .Where("Salary", 90000m, ComparisonOperator.GreaterThanOrEqual);

// Filter using string operations
var namesStartingWithA = employees.Where("Name", "A", ComparisonOperator.StartsWith);
var namesContainingSmith = employees.Where("Name", "Smith", ComparisonOperator.Contains);

// Filter using WhereIn (multiple values)
var selectedDepartments = employees.WhereIn("Department", new[] { "Engineering", "Sales" });

// Filter using WhereNot (exclusion)
var nonMarketingEmployees = employees.WhereNot("Department", "Marketing");

// Available ComparisonOperators:
// Equals, NotEquals, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual,
// Contains, StartsWith, EndsWith
```

### Sorting Data

```csharp
// Sort in ascending order
var sortedBySalaryAsc = employees.Sort(SortDirection.Ascending, "Salary");

// Sort in descending order
var sortedBySalaryDesc = employees.Sort(SortDirection.Descending, "Salary");

// Chain sorting with filtering
var topEngineersBySalary = employees
    .Where("Department", "Engineering")
    .Sort(SortDirection.Descending, "Salary");
```

### Grouping and Aggregation

```csharp
// Group by a column
var groupedByDepartment = employees.GroupBy("Department");

// Access group information
var groupInfo = groupedByDepartment.Info();
Console.WriteLine($"Number of groups: {groupInfo.RowCount}");

// Aggregate operations on individual columns
var minSalary = employees.Min("Salary");
var maxSalary = employees.Max("Salary");
var avgSalary = employees.Mean("Salary");
var totalSalary = employees.Sum("Salary");
var salaryStdDev = employees.StandardDeviation("Salary");
var salaryVariance = employees.Variance("Salary");

// Calculate percentiles
var medianSalary = employees.Percentile(0.5, "Salary");  // 50th percentile
var p95Salary = employees.Percentile(0.95, "Salary");    // 95th percentile

// Get row count for all columns
var sizes = employees.Size();

// Aggregate on all numeric columns (when no column specified)
var allAverages = employees.Mean();  // Computes mean for all numeric columns

// Group by and aggregate (single column)
var avgSalaryByDept = employees.GroupByAggregate("Department", "Salary", AggregationType.Mean);

// Group by with multiple aggregations
var departmentStats = employees.GroupByAggregate("Department",
    new Dictionary<string, AggregationType>
    {
        { "Salary", AggregationType.Mean },
        { "EmployeeId", AggregationType.Count }
    });
```

### Pivoting Data

Pivot transforms data from long (normalized) format to wide format by spreading values from one column into multiple columns.

```csharp
// Sample sales data in long format:
// Category | Region | Sales
// Electronics | East | 15000
// Electronics | West | 12000
// Clothing | East | 8000
// Clothing | West | 7000

// Pivot from long to wide format
// Result: Category | East_Sales | West_Sales
var pivoted = salesData.Pivot(
    indexColumn: "Category",
    pivotColumn: "Region",
    valueColumn: "Sales",
    aggregationType: AggregationType.Sum
);

// Multiple index columns
var pivoted = data.Pivot(
    indexColumns: new[] { "Year", "Category" },
    pivotColumn: "Region",
    valueColumn: "Revenue",
    aggregationType: AggregationType.Mean
);

// Chain with other operations
var result = salesData
    .Where("Year", 2024)
    .Pivot("Product", "Quarter", "Revenue", AggregationType.Sum)
    .Sort(SortDirection.Descending, "Q1_Revenue");

// Available aggregation types for Pivot:
// - Sum: Sum of values (default)
// - Mean: Average of values
// - Min: Minimum value
// - Max: Maximum value
// - Count: Count of values
// - StandardDeviation: Standard deviation
// - Variance: Variance
```

### Window Functions

Window functions enable calculations across sets of rows that are related to the current row, preserving all row details while computing aggregate or analytical values.

```csharp
// Load time series data
var stockData = new DataBlock();
stockData.AddColumn(new DataColumn("Date", typeof(DateTime)));
stockData.AddColumn(new DataColumn("Symbol", typeof(string)));
stockData.AddColumn(new DataColumn("Price", typeof(double)));
stockData.AddColumn(new DataColumn("Volume", typeof(long)));

// Sample data
stockData.AddRow(new object[] { new DateTime(2025, 1, 1), "AAPL", 100.0, 1000000 });
stockData.AddRow(new object[] { new DateTime(2025, 1, 2), "AAPL", 105.0, 1200000 });
stockData.AddRow(new object[] { new DateTime(2025, 1, 3), "AAPL", 110.0, 900000 });
stockData.AddRow(new object[] { new DateTime(2025, 1, 4), "AAPL", 115.0, 1100000 });
stockData.AddRow(new object[] { new DateTime(2025, 1, 5), "AAPL", 120.0, 1300000 });

// Moving average over 3-day window
var withMA = stockData.Window(
    columnName: "Price",
    functionType: WindowFunctionType.MovingAverage,
    windowSize: 3,
    resultColumnName: "MA_3"
);

// Exponential moving average (EMA)
var withEMA = stockData.Window(
    "Price",
    WindowFunctionType.ExponentialMovingAverage,
    12,
    "EMA_12"
);

// Moving standard deviation for volatility
var withVolatility = stockData.Window(
    "Price",
    WindowFunctionType.MovingStandardDeviation,
    20,
    "Volatility_20"
);

// Cumulative sum (running total)
var withCumSum = stockData.Window(
    "Volume",
    WindowFunctionType.CumulativeSum,
    null,
    "TotalVolume"
);

// Lag/Lead for comparisons
var withLag = stockData.Window(
    "Price",
    WindowFunctionType.Lag,
    1,
    "PreviousPrice"
);

// Calculate price change
var withChange = withLag.Compute("Change", "Price - PreviousPrice");

// Ranking
var withRank = stockData.Window(
    "Price",
    WindowFunctionType.Rank,
    null,
    "PriceRank"
);

// Moving median
var withMedian = stockData.Window(
    "Price",
    WindowFunctionType.MovingMedian,
    5,
    "Median_5"
);

// Moving percentile (e.g., 75th percentile)
var withPercentile = stockData.Window(
    "Price",
    WindowFunctionType.MovingPercentile,
    10,
    "P75",
    percentile: 0.75
);

// Partitioned windows - calculate per symbol
var multiStock = new DataBlock(); // Add data for multiple symbols
// ... add rows ...
var perSymbol = multiStock.Window(
    "Price",
    WindowFunctionType.MovingAverage,
    50,
    "MA_50",
    partitionBy: new[] { "Symbol" }
);

// Available window functions:
// - MovingAverage, MovingSum, MovingMin, MovingMax, MovingCount
// - MovingStandardDeviation, MovingVariance
// - ExponentialMovingAverage
// - MovingMedian, MovingPercentile
// - CumulativeSum, CumulativeAverage, CumulativeMin, CumulativeMax
// - Lag, Lead
// - RowNumber, Rank, DenseRank
// - FirstValue, LastValue, NthValue
```

For more advanced window function examples, see the [WindowFunctions sample](samples/WindowFunctions/README.md).

### Merging DataBlocks

```csharp
// Create another DataBlock with department information
var departments = new DataBlock();
departments.AddColumn(new DataColumn("Department", typeof(string)));
departments.AddColumn(new DataColumn("Location", typeof(string)));
departments.AddColumn(new DataColumn("Budget", typeof(decimal)));

departments.AddRow(new object[] { "Engineering", "Building A", 500000m });
departments.AddRow(new object[] { "Marketing", "Building B", 300000m });
departments.AddRow(new object[] { "Sales", "Building C", 250000m });

// Inner join (only matching rows)
var innerJoined = employees.Merge(departments, "Department", MergeMode.Inner);

// Left join (all rows from left DataBlock)
var leftJoined = employees.Merge(departments, "Department", MergeMode.Left);

// Right join (all rows from right DataBlock)
var rightJoined = employees.Merge(departments, "Department", MergeMode.Right);

// Full outer join (all rows from both)
var fullJoined = employees.Merge(departments, "Department", MergeMode.Full);

// Merge with different key column names
var merged = employees.Merge(departments, "Department", "DeptName", MergeMode.Inner);

// Available MergeMode values: Left, Right, Full, Inner
```

### Data Sampling and Subsetting

```csharp
// Get first N rows
var first5 = employees.Head(5);

// Get last N rows
var last3 = employees.Tail(3);

// Get random sample of rows
var randomSample = employees.Sample(10);

// Get random sample with seed for reproducibility
var reproducibleSample = employees.Sample(10, seed: 42);

// Efficiently copy a range of rows
var middleRows = employees.CopyRowRange(startRow: 5, rowCount: 10);
```

### Data Transformation

```csharp
// Transpose rows and columns
var transposed = employees.Transpose();

// Transpose using specific column as headers
var transposedWithHeaders = employees.Transpose("Name");

// Melt (unpivot) from wide to long format
var melted = employees.Melt(
    fixedColumns: new[] { "EmployeeId", "Name" },
    meltedColumnName: "Attribute",
    meltedValueName: "Value"
);

// Drop rows with null values
var noNulls = employees.DropNulls(DropNullMode.Any);  // Drop if any column is null
var allNulls = employees.DropNulls(DropNullMode.All);  // Drop only if all columns are null

// Fill null values using various strategies
var forwardFilled = employees.FillNulls(FillMethod.ForwardFill, "Salary");
var backwardFilled = employees.FillNulls(FillMethod.BackwardFill, "Department");
var constantFilled = employees.FillNulls(FillMethod.ConstantValue, 0.0, "Bonus");
var meanFilled = employees.FillNulls(FillMethod.Mean, "Salary");
var medianFilled = employees.FillNulls(FillMethod.Median, "Age");
var modeFilled = employees.FillNulls(FillMethod.Mode, "Department");
var interpolated = sensors.FillNulls(FillMethod.LinearInterpolation, "Temperature");

// Chain multiple fill operations
var cleaned = rawData
    .FillNulls(FillMethod.ForwardFill, "sensor1")
    .FillNulls(FillMethod.Mean, "sensor2")
    .FillNulls(FillMethod.ConstantValue, (object)"Unknown", "location");
```

### Removing Duplicates

```csharp
// Drop duplicate rows based on all columns
// Keep first occurrence (default)
var uniqueRows = employees.DropDuplicates();

// Keep last occurrence of each duplicate
var lastOccurrences = employees.DropDuplicates(KeepDuplicateMode.Last);

// Remove all duplicates (keep only unique rows)
var onlyUnique = employees.DropDuplicates(KeepDuplicateMode.None);

// Drop duplicates based on specific columns
var uniqueNames = employees.DropDuplicates(KeepDuplicateMode.First, "Name");
var uniqueDeptNames = employees.DropDuplicates(KeepDuplicateMode.First, "Department", "Name");

// Real-world example: Clean up duplicate customer records
var uniqueCustomers = customers
    .DropDuplicates(KeepDuplicateMode.Last, "Email")  // Keep latest record per email
    .Where("Status", "Active")
    .Sort(SortDirection.Ascending, "LastName");

// Chain with other data quality operations
var cleanedEmployees = rawEmployees
    .DropNulls(DropNullMode.Any, "EmployeeId", "Name")  // Remove rows with missing required fields
    .DropDuplicates(KeepDuplicateMode.First, "EmployeeId")  // Remove duplicate IDs
    .Where("Status", "Active")
    .Sort(SortDirection.Ascending, "Name");

// Available KeepDuplicateMode values:
// - First: Keep the first occurrence of each duplicate set (default)
// - Last: Keep the last occurrence of each duplicate set
// - None: Remove all duplicates, keep only unique rows
```

### Computed Columns with Expressions

```csharp
// Add computed column using expression
var withBonus = employees.Compute("Bonus", "Salary * 0.10");

// Use computed columns in pipelines
var analysis = employees
    .Compute("Bonus", "Salary * 0.10")
    .Compute("TotalComp", "Salary + Bonus")
    .Where("TotalComp", 100000m, ComparisonOperator.GreaterThan)
    .Select("Name", "Department", "Salary", "Bonus", "TotalComp");

// Validate expression before using
if (employees.ValidateExpression("Salary * 0.15", out string error))
{
    var withHigherBonus = employees.Compute("Bonus", "Salary * 0.15");
}
else
{
    Console.WriteLine($"Invalid expression: {error}");
}

// Expressions support arithmetic, comparison, and logical operators:
// - Arithmetic: +, -, *, /, %
// - Comparison: ==, !=, <, <=, >, >= (returns 1 for true, 0 for false)
// - Logical: && (AND), || (OR), ! (NOT)
// - Parentheses for grouping
// - Math functions: ABS, ROUND, FLOOR, CEIL, SQRT, POWER, EXP, LOG, LOG10
// - Date extraction: YEAR, MONTH, DAY, HOUR, MINUTE, SECOND, DAYOFWEEK, DAYOFYEAR, QUARTER
// - Date arithmetic: DATEADD, DATEDIFF, NOW, TODAY
// - Date parsing/formatting: DATEPARSE, DATEFORMAT
// - String functions: UPPER, LOWER, TRIM, LTRIM, RTRIM, LEN, SUBSTRING, LEFT, RIGHT
// - String search/replace: REPLACE, CHARINDEX, CONCAT, LPAD, RPAD, REVERSE
// - Column references by name (use brackets for names with spaces)

// Logical operator examples
var withFlags = employees
    .Compute("IsHighPaidEngineer", "Department == 'Engineering' && Salary > 90000")
    .Compute("NeedsReview", "Salary < 65000 || Department == 'Sales'")
    .Compute("IsNotMarketing", "!(Department == 'Marketing')");

// Complex logical expressions
var qualified = employees
    .Compute("IsQualified", "(Salary > 90000 && Department == 'Engineering') || Department == 'Executive'");

// Date function examples
var withDateInfo = employees
    .Compute("HireYear", "YEAR(HireDate)")
    .Compute("HireMonth", "MONTH(HireDate)")
    .Compute("HireQuarter", "QUARTER(HireDate)")
    .Compute("TenureYears", "DATEDIFF('year', HireDate, NOW())");

// Date arithmetic
var withDueDate = orders
    .Compute("DueDate", "DATEADD('day', 30, OrderDate)")
    .Compute("ProcessingDays", "DATEDIFF('day', OrderDate, ShipDate)");

// SQL-compatible interval units: year/yy, quarter/qq, month/mm, day/dd, hour/hh, minute/mi, second/ss

// String function examples
var withStrings = customers
    .Compute("UpperName", "UPPER(Name)")
    .Compute("CleanEmail", "TRIM(LOWER(Email))")
    .Compute("NameLength", "LEN(Name)")
    .Compute("Domain", "LOWER(RIGHT(Email, LEN(Email) - CHARINDEX('@', Email)))");

// String manipulation
var formatted = data
    .Compute("FullName", "CONCAT(FirstName, ' ', LastName)")
    .Compute("CleanPhone", "REPLACE(Phone, '-', '')")
    .Compute("PaddedCode", "LPAD(ProductCode, 8, '0')");
```

### Schema and Information

```csharp
// Get schema information
var schema = employees.Schema;
Console.WriteLine($"Number of columns: {schema.Count}");

// Get all column names
var columnNames = schema.GetColumnNames();
foreach (var colName in columnNames)
{
    Console.WriteLine($"Column: {colName}");
}

// Check if column exists in schema
if (schema.ColumnExists("Salary"))
{
    Console.WriteLine("Salary column exists in schema");
}

// Get detailed information about the DataBlock
var info = employees.Info();
// Info returns a DataBlock with columns:
// - Column: column name
// - Label: column label
// - Type: data type
// - Non-Null Count: count of non-null values
// - Null Count: count of null values
```

### Cloning and Copying

```csharp
// Create a deep clone of the DataBlock
var employeesCopy = employees.Clone();

// Modifications to the clone don't affect the original
employeesCopy.AddRow(new object[] { 10, "New Employee", "IT", 75000m, DateTime.Now });
Console.WriteLine($"Original: {employees.RowCount}, Clone: {employeesCopy.RowCount}");

// Efficiently append rows from another DataBlock
var moreEmployees = new DataBlock();
// ... add columns and rows to moreEmployees ...
employeesCopy.AppendRowsBatch(moreEmployees);
```

### Method Chaining for Data Pipelines

```csharp
// Build complex data transformation pipelines
var result = employees
    .Where("Department", "Engineering")
    .Compute("AnnualBonus", "Salary * 0.15")
    .Compute("TotalComp", "Salary + AnnualBonus")
    .Select("Name", "Salary", "AnnualBonus", "TotalComp")
    .Sort(SortDirection.Descending, "TotalComp")
    .Head(10);

// Analyze data with grouped aggregations
var departmentAnalysis = employees.GroupByAggregate(
    "Department",
    "Salary",
    AggregationType.Mean);

// Complex filtering with multiple conditions
var targetEmployees = employees
    .Where("Department", "Engineering")
    .Where("Salary", 90000m, ComparisonOperator.GreaterThanOrEqual)
    .Where("HireDate", new DateTime(2020, 1, 1), ComparisonOperator.LessThan)
    .Select("Name", "Salary", "HireDate");
```

### Disposal and Resource Management

```csharp
// DataBlock implements IDisposable for proper resource cleanup
using (var largeDataBlock = new DataBlock())
{
    // ... work with data ...
} // Automatically disposed

// Check if disposed
if (employees.IsDisposed)
{
    Console.WriteLine("DataBlock has been disposed");
}

// Manual disposal
employees.Dispose();
```

### Loading Data from External Sources

```csharp
// Note: Connector extensions require additional NuGet packages

// Load from CSV file
var csvData = await DataBlock.Connector.LoadCsvAsync("data/employees.csv");

// Load from Excel file
var excelData = await DataBlock.Connector.LoadExcelAsync("data/employees.xlsx");

// Load from JSON file
var jsonData = await DataBlock.Connector.LoadJsonAsync("data/employees.json");

// Load from Parquet file
var parquetData = await DataBlock.Connector.LoadParquetAsync("data/employees.parquet");

// Load from database
// Requires Datafication.Connectors.AdoConnector package
var config = new AdoConnectorConfiguration
{
    ConnectionString = "Server=localhost;Database=MyDb;...",
    Query = "SELECT * FROM Employees"
};
var connector = new AdoDataConnector(config);
var dbData = await connector.GetDataAsync();
```

## Performance Tips

1. **Use Select early**: Reduce the number of columns as early as possible in your pipeline
2. **Filter before sorting**: Apply Where clauses before Sort operations
3. **Batch operations**: Use `AppendRowsBatch` instead of multiple `AddRow` calls for bulk inserts
4. **Reuse DataBlocks**: Clone only when necessary; most operations return new instances
5. **Dispose large DataBlocks**: Use `using` statements or call `Dispose()` to free memory promptly
6. **Expression optimization**: The expression engine uses SIMD acceleration for numeric operations

## Common Patterns

### ETL Pipeline
```csharp
var processed = await DataBlock.Connector.LoadCsvAsync("raw_data.csv")
    .Select("id", "name", "value", "category")
    .Where("value", 0, ComparisonOperator.GreaterThan)
    .Compute("processed_value", "value * 1.1")
    .Sort(SortDirection.Descending, "processed_value")
    .Head(1000);
```

### Data Analysis
```csharp
var summary = data
    .GroupBy("category")
    .Aggregate(
        new[] { "revenue", "cost" },
        new Dictionary<string, AggregationType>
        {
            { "revenue", AggregationType.Sum },
            { "cost", AggregationType.Sum }
        }
    )
    .Compute("profit", "revenue - cost")
    .Compute("margin", "profit / revenue")
    .Sort(SortDirection.Descending, "profit");
```

### Data Quality
```csharp
var clean = rawData
    .DropNulls(DropNullMode.Any)
    .Where("status", "invalid", ComparisonOperator.NotEquals)
    .Select("essential_column1", "essential_column2", "essential_column3");

var qualityReport = rawData.Info();
```

## API Reference

For complete API documentation, see the [Datafication.Core API Reference](https://datafication.co/help/api/reference/Datafication.Core.html).

### Core Classes

- **DataBlock**: Main class for tabular data operations
- **DataColumn**: Represents a column with typed values
- **DataSchema**: Describes the structure of a DataBlock
- **DataBlockGroup**: Result of GroupBy operations
- **IDataRowCursor**: Interface for iterating over rows

### Enumerations

- **ComparisonOperator**: Equals, NotEquals, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Contains, StartsWith, EndsWith
- **SortDirection**: Ascending, Descending
- **MergeMode**: Left, Right, Full, Inner
- **DropNullMode**: Any, All
- **FillMethod**: ForwardFill, BackwardFill, ConstantValue, Mean, Median, Mode, LinearInterpolation
- **AggregationType**: Min, Max, Sum, Mean, Count, StandardDeviation, Variance
- **KeepDuplicateMode**: First, Last, None

## License

This library is licensed under the **Datafication SDK License Agreement**. See the [LICENSE](./LICENSE) file for details.

**Summary:**
- **Free Use**: Organizations with fewer than 5 developers AND annual revenue under $500,000 USD may use the SDK without a commercial license
- **Commercial License Required**: Organizations with 5+ developers OR annual revenue exceeding $500,000 USD must obtain a commercial license
- **Open Source Exemption**: Open source projects meeting specific criteria may be exempt from developer count limits

For commercial licensing inquiries, contact [support@datafication.co](mailto:support@datafication.co).

---

**Datafication.Core** - Transform your data with elegance and performance.

For more examples and documentation, visit our [samples directory](../../samples/).
