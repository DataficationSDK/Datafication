# Datafication.Storage.Velocity

[![NuGet](https://img.shields.io/nuget/v/Datafication.Storage.Velocity.svg)](https://www.nuget.org/packages/Datafication.Storage.Velocity)

A disk-backed data storage library for .NET that provides a `VelocityDataBlock` API for working with large-scale datasets using columnar storage with full CRUD capabilities.

## Description

Datafication.Storage.Velocity extends the Datafication.Core library with advanced storage capabilities using the proprietary DFC (Datafication Columnar Format). The `VelocityDataBlock` class provides features including O(1) updates and deletes via built-in row IDs, optional primary key indexing, automatic compaction, tiered storage management, and SIMD-accelerated query execution. Designed for datasets ranging from thousands to millions of rows, it combines the performance of columnar storage with the flexibility of full CRUD operations.

### Key Features

- **DFC Columnar Storage**: Segmented file format optimized for analytical queries and updates
- **Built-in Row IDs**: Every row has a unique VelocityRowId for O(1) addressing - no primary key required
- **Full CRUD Support**: O(1) row updates and deletes using built-in row IDs or optional primary keys
- **Optional Primary Key Indexing**: Add business-friendly key lookups when needed
- **Deferred Execution**: Query plan optimization combines operations for single-pass efficiency
- **SIMD Acceleration**: Vectorized operations for numeric computations, string pattern matching, and window functions
- **Window Functions**: SIMD-optimized analytics with 10-30x performance improvement over DataBlock
- **Automatic Compaction**: Background or on-demand compaction with configurable triggers
- **Large Dataset Support**: Efficiently handles millions of rows with minimal memory footprint
- **Fluent Query API**: Chainable methods compatible with DataBlock operations
- **LZ4 Compression**: Optional column compression for reduced storage footprint

## Table of Contents

- [Description](#description)
  - [Key Features](#key-features)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
  - [Creating and Opening VelocityDataBlocks](#creating-and-opening-velocitydatablocks)
  - [Saving DataBlocks to DFC Storage](#saving-datablocks-to-dfc-storage)
  - [CRUD Operations](#crud-operations)
  - [Primary Key Operations](#primary-key-operations)
  - [Working Without a Primary Key](#working-without-a-primary-key)
  - [Batch Operations](#batch-operations)
  - [Query Operations with Deferred Execution](#query-operations-with-deferred-execution)
  - [Filtering and Selection](#filtering-and-selection)
  - [Sorting and Limiting](#sorting-and-limiting)
  - [Grouping and Aggregation](#grouping-and-aggregation)
  - [Pivoting Data](#pivoting-data)
  - [Window Functions (SIMD-Accelerated)](#window-functions-simd-accelerated)
  - [Computed Columns with Expressions](#computed-columns-with-expressions)
  - [Data Transformation](#data-transformation)
  - [Storage Management](#storage-management)
  - [Performance Optimization](#performance-optimization)
  - [Query Plan Optimization](#query-plan-optimization)
- [Performance Tips](#performance-tips)
- [Known Issues](#known-issues)
  - [Windows Memory-Mapped File Locking](#windows-memory-mapped-file-locking)
- [Common Patterns](#common-patterns)
  - [High-Throughput Ingestion](#high-throughput-ingestion)
  - [Real-Time Analytics](#real-time-analytics)
  - [Data Lake Operations](#data-lake-operations)
- [API Reference](#api-reference)
  - [Core Classes](#core-classes)
  - [Enumerations](#enumerations)
- [License](#license)

## Installation

> **Note**: Datafication.Storage.Velocity is currently in pre-release. The packages are now available on nuget.org.

```bash
dotnet add package Datafication.Storage.Velocity
```

**Running the Samples:**

```bash
cd samples/BasicOperations
dotnet run
```

## Usage Examples

### Creating and Opening VelocityDataBlocks

```csharp
using Datafication.Storage.Velocity;
using Datafication.Core.Data;

// Create a new VelocityDataBlock with default options
var velocityBlock = new VelocityDataBlock("data/employees.dfc");

// Create with features optimized for updates
var enterpriseBlock = VelocityDataBlock.CreateEnterprise(
    "data/orders.dfc",
    primaryKeyColumn: "OrderId"
);

// Create with high-throughput optimization
var highThroughputBlock = VelocityDataBlock.CreateHighThroughput(
    "data/events.dfc",
    primaryKeyColumn: "EventId"
);

// Open an existing DFC file
var existingBlock = await VelocityDataBlock.OpenAsync("data/customers.dfc");

// Open with custom options
var options = new VelocityOptions
{
    PrimaryKeyColumn = "CustomerId",
    SegmentSizeRows = 100000,
    EnableCompression = true,
    AutoCompactionEnabled = true,
    MaxSegmentsBeforeCompaction = 10
};
var customBlock = await VelocityDataBlock.OpenAsync("data/customers.dfc", options);
```

### Saving DataBlocks to DFC Storage

```csharp
// Convert an in-memory DataBlock to VelocityDataBlock
var dataBlock = new DataBlock();
dataBlock.AddColumn(new DataColumn("ProductId", typeof(int)));
dataBlock.AddColumn(new DataColumn("ProductName", typeof(string)));
dataBlock.AddColumn(new DataColumn("Price", typeof(decimal)));
dataBlock.AddRow(new object[] { 1, "Widget", 29.99m });
dataBlock.AddRow(new object[] { 2, "Gadget", 49.99m });

// Save to DFC format with primary key
var velocityBlock = await VelocityDataBlock.SaveAsync(
    "data/products.dfc",
    dataBlock,
    new VelocityOptions { PrimaryKeyColumn = "ProductId" }
);

Console.WriteLine($"Saved {velocityBlock.RowCount} rows to DFC storage");
```

### CRUD Operations

```csharp
// Add new rows
velocityBlock.AddRow(new object[] { 3, "Doohickey", 19.99m });

// Append batch of rows (more efficient for multiple inserts)
var batch = new DataBlock();
batch.AddColumn(new DataColumn("ProductId", typeof(int)));
batch.AddColumn(new DataColumn("ProductName", typeof(string)));
batch.AddColumn(new DataColumn("Price", typeof(decimal)));
batch.AddRow(new object[] { 4, "Thingamajig", 39.99m });
batch.AddRow(new object[] { 5, "Whatchamacallit", 24.99m });
await velocityBlock.AppendBatchAsync(batch);

// Update row by index
velocityBlock.UpdateRow(0, new object[] { 1, "Widget Pro", 34.99m });

// Delete row by index
velocityBlock.RemoveRow(2);

// Query current state
Console.WriteLine($"Total rows: {velocityBlock.TotalRowCount}");
Console.WriteLine($"Active rows: {velocityBlock.ActiveRowCount}");
```

### Primary Key Operations

```csharp
// Update row by primary key (O(1) performance)
await velocityBlock.UpdateRowAsync("ProductId-123", new object[] { 123, "Updated Product", 99.99m });

// Delete row by primary key
await velocityBlock.DeleteRowAsync("ProductId-456");

// Delete multiple rows by primary keys (batch operation)
await velocityBlock.DeleteRowsAsync(new[] { "ProductId-789", "ProductId-101", "ProductId-202" });

// Update multiple rows by primary keys
var updates = new Dictionary<string, object[]>
{
    ["ProductId-123"] = new object[] { 123, "Product A", 29.99m },
    ["ProductId-456"] = new object[] { 456, "Product B", 49.99m }
};
await velocityBlock.UpdateRowsAsync(updates);

// Find row ID by primary key
var rowId = await velocityBlock.FindRowIdAsync("ProductId-123");
if (rowId.HasValue)
{
    // Use row ID for subsequent operations (even faster than primary key)
    await velocityBlock.UpdateRowAsync(rowId.Value, new object[] { 123, "Product A v2", 34.99m });

    // Check if row is deleted
    bool isDeleted = velocityBlock.IsRowDeleted(rowId.Value);

    // Get value by row ID and column
    var productName = velocityBlock.GetValue(rowId.Value, "ProductName");
    var price = velocityBlock.GetValue(rowId.Value, 2); // by column index
}
```

### Working Without a Primary Key

Primary keys are optional in VelocityDataBlock. Even without a primary key configured, every row has a built-in `VelocityRowId` that provides O(1) addressing for updates and deletes. This may satisfy requirements when you don't have a natural primary key.

```csharp
// Create VelocityDataBlock without a primary key
var velocityBlock = new VelocityDataBlock("data/events.dfc");
// No primary key specified - that's okay!

// Option 1: Obtain VelocityRowId during initial read/query
using (var cursor = velocityBlock.GetRowCursor())
{
    var rowIdsToUpdate = new List<VelocityRowId>();
    var rowIndex = 0;

    while (cursor.MoveNext())
    {
        // Store row index of rows you want to update later
        if ((string)cursor["Status"] == "Pending")
        {
            // Convert row index to VelocityRowId for later use
            // Note: This is for illustration - normally you'd process immediately
            // VelocityRowId = segment index + local row index (built into DFC)
            rowIdsToUpdate.Add(VelocityRowId.FromInt64(rowIndex));
        }
        rowIndex++;
    }
}

// Option 2: Use row index directly (internally converted to VelocityRowId)
// Update by row index
velocityBlock.UpdateRow(5, new object[] { "Updated", DateTime.Now });

// Delete by row index
velocityBlock.RemoveRow(10);

// Option 3: Query to get row indices, then perform updates
var targetRows = velocityBlock
    .Where("Status", "Pending")
    .Execute();

// Process results and update original dataset
// (In practice, you'd need to track row indices through a custom workflow)

// VelocityRowId properties
var rowId = VelocityRowId.FromInt64(12345);
Console.WriteLine($"Row ID Value: {rowId.Value}");
Console.WriteLine($"Is Valid: {rowId.IsValid}");
Console.WriteLine($"Is Null: {rowId.IsNull}");

// VelocityRowId is a 64-bit struct (segment index + local row index)
long rowIdValue = rowId; // Implicit conversion to long
VelocityRowId restoredId = rowIdValue; // Implicit conversion from long

// When to use Primary Keys vs VelocityRowId:
// - Primary Key: When you have natural business keys (OrderId, CustomerId, etc.)
//   * Human-readable lookups
//   * FindRowIdAsync() for flexible access patterns
// - VelocityRowId only: When you don't have natural keys
//   * Still get O(1) updates/deletes by row position
//   * Slightly less overhead (no primary key index)
//   * Good for log files, event streams, time-series data
```

### Batch Operations

```csharp
// Update multiple rows by index (optimized batch operation)
var indexUpdates = new Dictionary<int, object[]>
{
    [0] = new object[] { 1, "First Product", 19.99m },
    [1] = new object[] { 2, "Second Product", 29.99m },
    [2] = new object[] { 3, "Third Product", 39.99m }
};
await velocityBlock.UpdateRowsByIndexAsync(indexUpdates);

// Batch append with flush
await velocityBlock.AppendBatchAsync(largeBatch);
await velocityBlock.FlushAsync(); // Ensure all changes are persisted
```

### Query Operations with Deferred Execution

```csharp
// Build query plan (no execution yet)
var query = velocityBlock
    .Where("Category", "Electronics")
    .Where("Price", 100.0, ComparisonOperator.LessThan)
    .Select("ProductName", "Price", "StockQuantity")
    .Sort(SortDirection.Ascending, "Price")
    .Head(10);

// Execute query plan (optimized single-pass execution)
var result = query.Execute();

// Query plan is cleared after execution
// Build new query for next operation
var query2 = velocityBlock
    .Where("Category", "Books")
    .GroupByAggregate("Author", "Price", AggregationType.Mean, "avg_price")
    .Sort(SortDirection.Descending, "avg_price")
    .Execute();
```

### Filtering and Selection

```csharp
// Simple Where filtering
var result = velocityBlock
    .Where("Department", "Engineering")
    .Execute();

// Multiple Where conditions (optimized to single pass)
var result = velocityBlock
    .Where("Department", "Engineering")
    .Where("Salary", 80000, ComparisonOperator.GreaterThan)
    .Where("YearsExperience", 5, ComparisonOperator.GreaterThanOrEqual)
    .Execute();

// String pattern matching with SIMD acceleration
var result = velocityBlock
    .WhereContains("Email", "@example.com")
    .Execute();

var result = velocityBlock
    .WhereStartsWith("ProductName", "Pro")
    .Execute();

var result = velocityBlock
    .WhereEndsWith("FileName", ".pdf")
    .Execute();

// Complex filtering with predicates
var result = velocityBlock
    .Filter(row =>
        (int)row["Age"] >= 25 &&
        (int)row["Age"] <= 65 &&
        (string)row["Status"] == "Active"
    )
    .Execute();

// Select specific columns (columnar read optimization)
var result = velocityBlock
    .Select("Name", "Email", "Department")
    .Execute();

// Combined Where + Select (single-pass optimization)
var result = velocityBlock
    .Where("IsActive", true)
    .Select("EmployeeId", "Name", "Salary")
    .Execute();
```

### Sorting and Limiting

```csharp
// Sort by column
var result = velocityBlock
    .Sort(SortDirection.Descending, "Salary")
    .Execute();

// Get first N rows (optimized early termination)
var result = velocityBlock
    .Head(100)
    .Execute();

// Get last N rows
var result = velocityBlock
    .Tail(50)
    .Execute();

// Random sampling with seed
var result = velocityBlock
    .Sample(1000, seed: 42)
    .Execute();

// Combined filtering, sorting, and limiting
var topEngineers = velocityBlock
    .Where("Department", "Engineering")
    .Sort(SortDirection.Descending, "Salary")
    .Head(10)
    .Execute();
```

### Grouping and Aggregation

```csharp
// Group by single column
var result = velocityBlock
    .GroupBy("Department")
    .Execute(); // Returns DataBlockGroup

// Group by with single aggregation
var result = velocityBlock
    .GroupByAggregate("Department", "Salary", AggregationType.Mean, "avg_salary")
    .Execute(); // Returns DataBlock with Department and avg_salary columns

// Group by with multiple aggregations
var aggregations = new Dictionary<string, AggregationType>
{
    ["Salary"] = AggregationType.Mean,
    ["EmployeeId"] = AggregationType.Count,
    ["Bonus"] = AggregationType.Sum
};
var result = velocityBlock
    .GroupByAggregate("Department", aggregations)
    .Execute();

// Aggregate entire dataset
var result = velocityBlock
    .Mean("Salary", "Bonus")
    .Execute();

var result = velocityBlock
    .Sum("Revenue", "Profit")
    .Execute();

// Statistical aggregations
var stats = velocityBlock
    .Select("Age")
    .Execute();
var minAge = stats.Min();
var maxAge = stats.Max();
var avgAge = stats.Mean();
var stdDevAge = stats.StandardDeviation();
```

### Pivoting Data

Pivot transforms data from long format to wide format. Like GroupByAggregate, Pivot requires `Execute()` because the output schema depends on the data.

```csharp
// Pivot from long to wide format (deferred execution)
var result = velocityBlock
    .Where("Year", 2024)
    .Pivot("Category", "Region", "Sales", AggregationType.Sum)
    .Execute();

// Multiple index columns
var result = velocityBlock
    .Pivot(
        indexColumns: new[] { "Year", "Product" },
        pivotColumn: "Quarter",
        valueColumn: "Revenue"
    )
    .Sort(SortDirection.Descending, "Q1_Revenue")
    .Execute();

// Different aggregation types
var avgPivot = velocityBlock
    .Pivot("Category", "Region", "Sales", AggregationType.Mean)
    .Execute();

// Chain Where -> Pivot -> Sort -> Head
var topCategories = velocityBlock
    .Where("Year", 2024)
    .Pivot("Category", "Region", "Sales", AggregationType.Sum)
    .Sort(SortDirection.Descending, "East_Sales")
    .Head(5)
    .Execute();
```

**Note:** Pivot output columns are determined by unique values in the pivot column, which requires scanning the data.

### Window Functions (SIMD-Accelerated)

VelocityDataBlock provides SIMD-accelerated window functions for high-performance time series and analytical operations. These vectorized implementations are 10-30x faster than DataBlock equivalents.

```csharp
// Moving average with SIMD acceleration (100M+ values/sec)
var result = velocityBlock
    .Window("Price", WindowFunctionType.MovingAverage, 20, "MA_20")
    .Execute();

// Exponential moving average for trend analysis
var result = velocityBlock
    .Window("Price", WindowFunctionType.ExponentialMovingAverage, 12, "EMA_12")
    .Execute();

// Moving standard deviation for volatility (Welford's algorithm)
var result = velocityBlock
    .Window("Price", WindowFunctionType.MovingStandardDeviation, 20, "StdDev_20")
    .Execute();

// Cumulative sum (SIMD prefix sum - 130M+ values/sec)
var result = velocityBlock
    .Window("Volume", WindowFunctionType.CumulativeSum, null, "TotalVolume")
    .Execute();

// Lag/Lead for comparisons
var result = velocityBlock
    .Window("Price", WindowFunctionType.Lag, 1, "PreviousPrice", defaultValue: 0.0)
    .Execute();

// Chain multiple window functions (all SIMD-optimized)
var result = velocityBlock
    .Window("Close", WindowFunctionType.MovingAverage, 20, "SMA_20")
    .Window("Close", WindowFunctionType.ExponentialMovingAverage, 12, "EMA_12")
    .Window("Close", WindowFunctionType.MovingStandardDeviation, 20, "StdDev_20")
    .Window("Volume", WindowFunctionType.CumulativeSum, null, "CumulativeVolume")
    .Execute(); // Entire chain optimized in query plan

// Partitioned windows - calculate per category with SIMD
var result = velocityBlock
    .Window("Sales", WindowFunctionType.MovingAverage, 7, "MA_7",
        partitionByColumns: new[] { "Region" })
    .Execute();

// SIMD-Accelerated Functions (VelocityDataBlock):
// - MovingAverage, MovingSum, MovingMin, MovingMax (10-15x faster)
// - MovingStandardDeviation, MovingVariance (Welford's algorithm, 16x faster)
// - ExponentialMovingAverage (7x faster)
// - CumulativeSum, CumulativeAverage, CumulativeMin, CumulativeMax (16x faster)
// - Lag, Lead, RowNumber, Rank, DenseRank
// - FirstValue, LastValue, NthValue
//
// DataBlock-Only Functions (not vectorized):
// - MovingMedian, MovingPercentile (sorting-based)
//
// Performance: ~50-150M values/sec on modern CPUs with AVX-512/AVX2
```

For complete window function documentation, see [Window Functions Guide](../../agent-docs/docs/window-functions-guide.md).

### Computed Columns with Expressions

```csharp
// Add computed column using expression
var result = velocityBlock
    .Compute("ProfitMargin", "Profit / Revenue")
    .Execute();

// Multiple computed columns
var result = velocityBlock
    .Compute("FullName", "FirstName + ' ' + LastName")
    .Compute("AnnualSalary", "MonthlySalary * 12")
    .Compute("TaxRate", "Salary * 0.3")
    .Execute();

// Filter by computed column
var result = velocityBlock
    .Compute("ProfitMargin", "Profit / Revenue")
    .Where("ProfitMargin", 0.2, ComparisonOperator.GreaterThan)
    .Execute();

// Expressions support arithmetic, comparison, and logical operators:
// - Arithmetic: +, -, *, /, %
// - Comparison: ==, !=, <, <=, >, >= (returns 1 for true, 0 for false)
// - Logical: && (AND), || (OR), ! (NOT)
// - Math functions: ABS, ROUND, FLOOR, CEIL, SQRT, POWER, EXP, LOG, LOG10
// - Date extraction: YEAR, MONTH, DAY, HOUR, MINUTE, SECOND, DAYOFWEEK, DAYOFYEAR, QUARTER
// - Date arithmetic: DATEADD, DATEDIFF, NOW, TODAY
// - Date parsing/formatting: DATEPARSE, DATEFORMAT
// - String functions: UPPER, LOWER, TRIM, LTRIM, RTRIM, LEN, SUBSTRING, LEFT, RIGHT
// - String search/replace: REPLACE, CHARINDEX, CONCAT, LPAD, RPAD, REVERSE
// - CASE WHEN conditionals for complex logic

// Logical operator examples
var result = velocityBlock
    .Compute("IsHighValue", "Price > 100 && Quantity > 50")
    .Compute("NeedsReview", "Price < 10 || Quantity > 1000")
    .Compute("IsNotDiscounted", "!(DiscountPercent > 0)")
    .Execute();

// Complex logical expressions with parentheses
var result = velocityBlock
    .Compute("VipOrder", "(Price > 100 && Quantity > 50) || CustomerTier == 5")
    .Where("VipOrder", 1, ComparisonOperator.Equals) // Filter for VIP orders
    .Execute();

// Validate expression before execution
string error;
if (velocityBlock.ValidateExpression("Price > 100 && Quantity > 50", out error))
{
    Console.WriteLine("Expression is valid");
}
else
{
    Console.WriteLine($"Expression error: {error}");
}

// Performance Note: Logical operators use standard evaluator (10-50M values/sec)
// Arithmetic and simple comparisons use SIMD vectorization (100-150M values/sec)

// Date function examples (SIMD-optimized extraction: 80-120M values/sec)
var result = velocityBlock
    .Compute("OrderYear", "YEAR(OrderDate)")
    .Compute("OrderMonth", "MONTH(OrderDate)")
    .Compute("OrderQuarter", "QUARTER(OrderDate)")
    .Compute("DaysSinceOrder", "DATEDIFF('day', OrderDate, NOW())")
    .Execute();

// Date arithmetic
var result = velocityBlock
    .Compute("DueDate", "DATEADD('day', 30, OrderDate)")
    .Compute("ProcessingDays", "DATEDIFF('day', OrderDate, ShipDate)")
    .Where("ProcessingDays", 7, ComparisonOperator.GreaterThan)
    .Execute();

// SQL-compatible interval units: year/yy, quarter/qq, month/mm, day/dd, hour/hh, minute/mi, second/ss

// String function examples (loop-unrolled batch processing)
var result = velocityBlock
    .Compute("UpperName", "UPPER(CustomerName)")
    .Compute("CleanEmail", "TRIM(LOWER(Email))")
    .Compute("NameLength", "LEN(CustomerName)")
    .Compute("Domain", "LOWER(RIGHT(Email, LEN(Email) - CHARINDEX('@', Email)))")
    .Execute();

// String manipulation
var result = velocityBlock
    .Compute("FullName", "CONCAT(FirstName, ' ', LastName)")
    .Compute("CleanPhone", "REPLACE(Phone, '-', '')")
    .Compute("PaddedCode", "LPAD(ProductCode, 8, '0')")
    .Execute();
```

### Data Transformation

```csharp
// Melt (unpivot) from wide to long format
var result = velocityBlock
    .Melt(
        fixedColumns: new[] { "ProductId", "ProductName" },
        meltedColumnName: "Metric",
        meltedValueName: "Value"
    )
    .Execute();

// Transpose rows and columns
var result = velocityBlock
    .Transpose(headerColumnName: "MetricName")
    .Execute();

// Drop rows with null values
var result = velocityBlock
    .DropNulls("Email", "PhoneNumber") // Drop rows where either column is null
    .Execute();

var result = velocityBlock
    .DropNulls(DropNullMode.Any) // Drop rows with any null value
    .Execute();

var result = velocityBlock
    .DropNulls(DropNullMode.All) // Drop rows where all values are null
    .Execute();

// Drop duplicate rows based on all columns
var uniqueRows = velocityBlock
    .DropDuplicates()  // Keep first occurrence (default)
    .Execute();

var lastOccurrences = velocityBlock
    .DropDuplicates(KeepDuplicateMode.Last)  // Keep last occurrence
    .Execute();

var onlyUnique = velocityBlock
    .DropDuplicates(KeepDuplicateMode.None)  // Remove all duplicates
    .Execute();

// Drop duplicates based on specific columns (deferred execution)
var uniqueEmails = velocityBlock
    .DropDuplicates(KeepDuplicateMode.Last, "Email")  // Keep latest record per email
    .Execute();

var uniqueUsernames = velocityBlock
    .DropDuplicates(KeepDuplicateMode.First, "UserId", "AccountType")
    .Execute();

// Chain with other operations for data quality pipeline
var cleanedData = velocityBlock
    .Where("Status", "Active", ComparisonOperator.Equals)
    .DropNulls("Email", "UserId")  // Remove rows with null required fields
    .DropDuplicates(KeepDuplicateMode.Last, "Email")  // Keep latest per email
    .Select("UserId", "Email", "Name", "LastLoginDate")
    .Sort(SortDirection.Descending, "LastLoginDate")
    .Execute();

// Fill null values using various strategies (lazy evaluation)
var result = velocityBlock
    .Where("Country", "USA", ComparisonOperator.Equals)
    .Select("Date", "Temperature", "Humidity")
    .FillNulls(FillMethod.ForwardFill, "Temperature") // Forward fill temperature
    .FillNulls(FillMethod.Mean, "Humidity")           // Fill humidity with mean
    .Execute();

// Different fill methods
var forwardFilled = velocityBlock
    .FillNulls(FillMethod.ForwardFill, "Sensor1")
    .Execute();

var backwardFilled = velocityBlock
    .FillNulls(FillMethod.BackwardFill, "Sensor2")
    .Execute();

var constantFilled = velocityBlock
    .FillNulls(FillMethod.ConstantValue, 0.0, "ErrorCount")
    .Execute();

var meanFilled = velocityBlock
    .FillNulls(FillMethod.Mean, "Price")
    .Execute();

var interpolated = velocityBlock
    .FillNulls(FillMethod.LinearInterpolation, "Temperature")
    .Execute();

// Chain multiple fill operations for optimal query performance
var cleaned = velocityBlock
    .Where("Status", "Active", ComparisonOperator.Equals)
    .Select("Date", "Value1", "Value2", "Category")
    .FillNulls(FillMethod.ForwardFill, "Value1")
    .FillNulls(FillMethod.Median, "Value2")
    .FillNulls(FillMethod.ConstantValue, (object)"Unknown", "Category")
    .Sort(SortDirection.Ascending, "Date")
    .Execute();

// Merge (join) with another DataBlock
var otherBlock = new DataBlock();
// ... populate otherBlock ...

var result = velocityBlock
    .Merge(otherBlock, "EmployeeId", "EmpId", MergeMode.Inner)
    .Execute();
```

### Storage Management

```csharp
// Get storage statistics
var stats = await velocityBlock.GetStorageStatsAsync();
Console.WriteLine($"Total Rows: {stats.TotalRows}");
Console.WriteLine($"Active Rows: {stats.ActiveRows}");
Console.WriteLine($"Deleted Rows: {stats.DeletedRows}");
Console.WriteLine($"Deleted Percentage: {stats.DeletedPercentage:F2}%");
Console.WriteLine($"Storage Files: {stats.StorageFiles}");
Console.WriteLine($"Size: {stats.EstimatedSizeBytes / 1024 / 1024:F2} MB");
Console.WriteLine($"Can Compact: {stats.CanCompact}");

// Manual compaction
if (stats.CanCompact)
{
    await velocityBlock.CompactAsync();
    Console.WriteLine("Compaction completed");
}

// Compaction with specific strategy
await velocityBlock.CompactAsync(VelocityCompactionStrategy.Aggressive);

// Configure automatic compaction
velocityBlock.ConfigureAutoCompaction(
    enabled: true,
    trigger: VelocityCompactionTrigger.SegmentCount,
    threshold: 10 // Compact when 10+ segments exist
);

// Enable background compaction
velocityBlock.EnableBackgroundCompaction(enabled: true);

// Flush pending changes to disk
await velocityBlock.FlushAsync();

// Get primary key index statistics
var (indexedKeys, indexBuilt, segments) = velocityBlock.GetPrimaryKeyIndexStats();
Console.WriteLine($"Indexed Keys: {indexedKeys}, Index Built: {indexBuilt}, Segments: {segments}");
```

### Performance Optimization

```csharp
// Create with optimized settings for your workload
var options = new VelocityOptions
{
    // Primary key for fast lookups
    PrimaryKeyColumn = "UserId",

    // Larger segments for better compression and fewer files
    SegmentSizeRows = 1000000,

    // Enable LZ4 compression
    EnableCompression = true,

    // Auto-compaction settings
    AutoCompactionEnabled = true,
    AutoCompactionTrigger = VelocityCompactionTrigger.DeletedPercentage,
    DeletedPercentageThreshold = 20.0, // Compact when 20% deleted

    // Background compaction
    EnableBackgroundCompaction = true
};

var optimizedBlock = new VelocityDataBlock("data/large_dataset.dfc", options);
```

### Query Plan Optimization

```csharp
// VelocityDataBlock automatically optimizes query plans
// Example: Reorders operations for maximum efficiency

// User writes:
var query = velocityBlock
    .Select("Name", "Salary", "Department")
    .Where("Department", "Engineering")
    .Where("Salary", 80000, ComparisonOperator.GreaterThan)
    .Sort(SortDirection.Descending, "Salary");

// VelocityDataBlock optimizes to:
// 1. Apply Where filters first (reduces dataset size)
// 2. Then Select (only read needed columns)
// 3. Finally Sort (on smaller result set)

var result = query.Execute(); // Executes optimized plan

// Clear query plan without executing
query.ClearQueryPlan();

// Inspect query plan (for debugging)
var plan = query.GetQueryPlan(); // Internal method
```

## Performance Tips

1. **Primary Keys are Optional**: Every row has a built-in `VelocityRowId` for O(1) operations. Add a primary key only when you need business-friendly lookups
   ```csharp
   // Without primary key - use built-in VelocityRowId (slightly faster, less overhead)
   var options = new VelocityOptions(); // No PrimaryKeyColumn specified

   // With primary key - for human-readable lookups (OrderId, CustomerId, etc.)
   var options = new VelocityOptions { PrimaryKeyColumn = "CustomerId" };
   ```

2. **Batch Operations**: Use batch methods for multiple updates/deletes instead of individual operations
   ```csharp
   await velocityBlock.UpdateRowsAsync(updates); // Better than multiple UpdateRowAsync calls
   ```

3. **Deferred Execution**: Build complete query chains before calling `Execute()` to enable single-pass optimization
   ```csharp
   var result = velocityBlock.Where(...).Select(...).Sort(...).Execute(); // Single optimized pass
   ```

4. **Select Early**: Use `Select()` to reduce columns early in the query chain
   ```csharp
   velocityBlock.Select("Id", "Name").Where(...) // Reads fewer columns
   ```

5. **Head for Early Termination**: Use `Head()` when you only need first N rows for massive performance gains
   ```csharp
   velocityBlock.Where(...).Head(100).Execute() // Stops after finding 100 matches
   ```

6. **Compact Regularly**: Use auto-compaction or manual compaction to maintain query performance
   ```csharp
   velocityBlock.ConfigureAutoCompaction(enabled: true);
   ```

7. **Segment Size Tuning**: Larger segments (100K-1M rows) provide better compression and fewer files
   ```csharp
   new VelocityOptions { SegmentSizeRows = 1000000 }
   ```

8. **Compression for Storage**: Enable LZ4 compression for large datasets to reduce disk footprint
   ```csharp
   new VelocityOptions { EnableCompression = true }
   ```

9. **Numeric Filters First**: VelocityDataBlock automatically reorders Where operations, but you can help by placing numeric filters before string filters

10. **Dispose Properly**: Always dispose VelocityDataBlock when done to release file handles
    ```csharp
    using var velocityBlock = new VelocityDataBlock("data/file.dfc");
    // ... use velocityBlock ...
    // Automatically disposed here
    ```

## Known Issues

### Windows Memory-Mapped File Locking

On Windows, file replacement operations (e.g. `File.Move`) can fail if any code path still holds an open handle to the underlying DFC files (including memory-mapped views). VelocityDataBlock aggressively disposes readers/writers before compaction and other file operations; users typically need to ensure they **dispose `VelocityDataBlock` and any public cursors/streams they obtain** (e.g. `using var cursor = velocityBlock.GetRowCursor()`).

**Symptoms:**
- `IOException: The process cannot access the file because it is being used by another process`
- `UnauthorizedAccessException` when deleting or moving DFC files
- Intermittent failures during compaction operations

**Mitigations Built Into VelocityDataBlock:**
- Readers/writers are explicitly disposed before compaction and file operations
- Vectorized analytics helpers dispose internal resources deterministically
- Proper disposal ordering to minimize lock retention

**Recommended Workarounds:**

1. **Use try/catch with retry logic** when performing file operations after disposal:
   ```csharp
   async Task SafeFileOperationAsync(string filePath, Func<Task> operation, int maxRetries = 3)
   {
       for (int attempt = 0; attempt < maxRetries; attempt++)
       {
           try
           {
               await operation();
               return;
           }
           catch (IOException) when (attempt < maxRetries - 1)
           {
               await Task.Delay(100 * (attempt + 1)); // Exponential backoff
           }
       }
       throw new IOException($"Failed to access file after {maxRetries} attempts: {filePath}");
   }
   ```

2. **Add a small delay** before file operations after disposal:
   ```csharp
   velocityBlock.Dispose();
   await Task.Delay(100); // Allow Windows to release file locks
   File.Delete(filePath);
   ```

**Note:** This issue does not affect Linux or macOS, which handle memory-mapped file cleanup more predictably.

## Common Patterns

### High-Throughput Ingestion

```csharp
// Optimized for high-speed data ingestion
var options = VelocityOptions.CreateHighThroughput();
options.PrimaryKeyColumn = "EventId";

using var eventStore = new VelocityDataBlock("data/events.dfc", options);

// Batch append for maximum throughput
var batchSize = 10000;
var batch = new DataBlock();
// ... add columns to batch ...

foreach (var event in eventStream)
{
    batch.AddRow(event.ToArray());

    if (batch.RowCount >= batchSize)
    {
        await eventStore.AppendBatchAsync(batch);
        batch = new DataBlock(); // Reset for next batch
        // ... re-add columns ...
    }
}

// Flush remaining
if (batch.RowCount > 0)
{
    await eventStore.AppendBatchAsync(batch);
}

await eventStore.FlushAsync();
```

### Real-Time Analytics

```csharp
// Efficient real-time aggregations on large datasets
using var salesData = await VelocityDataBlock.OpenAsync("data/sales.dfc");

// Get today's sales summary (deferred execution optimizes this)
var todaySummary = salesData
    .Where("SaleDate", DateTime.Today, ComparisonOperator.GreaterThanOrEqual)
    .GroupByAggregate("ProductCategory", new Dictionary<string, AggregationType>
    {
        ["SaleAmount"] = AggregationType.Sum,
        ["OrderId"] = AggregationType.Count
    })
    .Sort(SortDirection.Descending, "sum_SaleAmount")
    .Execute();

// Display results
foreach (var row in todaySummary.GetRowCursor())
{
    Console.WriteLine($"{row["ProductCategory"]}: {row["sum_SaleAmount"]:C} ({row["count_OrderId"]} orders)");
}
```

### Data Lake Operations

```csharp
// Data lake with automatic management
var options = VelocityOptions.CreateEnterprise();
options.PrimaryKeyColumn = "RecordId";
options.AutoCompactionEnabled = true;
options.DeletedPercentageThreshold = 15.0;
options.EnableBackgroundCompaction = true;

using var dataLake = new VelocityDataBlock("data/customer_data.dfc", options);

// Incremental updates
var updates = LoadIncrementalUpdates();
await dataLake.UpdateRowsAsync(updates);

// Delete old records
var oldRecordIds = await GetExpiredRecordIds();
await dataLake.DeleteRowsAsync(oldRecordIds);

// Query latest data
var activeCustomers = dataLake
    .Where("Status", "Active")
    .Where("LastActivityDate", DateTime.Now.AddDays(-30), ComparisonOperator.GreaterThan)
    .Select("CustomerId", "Name", "Email", "Tier")
    .Execute();

// Automatic compaction runs in background to optimize storage
var stats = await dataLake.GetStorageStatsAsync();
Console.WriteLine($"Storage efficiency: {100 - stats.DeletedPercentage:F1}%");
```

## API Reference

For complete API documentation, see the [Datafication.Storage.Velocity API Reference](https://datafication.co/help/api/reference/Datafication.Storage.Velocity.html).

### Core Classes

**VelocityDataBlock**
- **Constructors**
  - `VelocityDataBlock(string filePath, VelocityOptions? options = null)`
- **Static Factory Methods**
  - `CreateEnterprise(string filePath, string? primaryKeyColumn = null)` - Optimized for updates
  - `CreateHighThroughput(string filePath, string? primaryKeyColumn = null)` - Optimized for ingestion
  - `OpenAsync(string pathOrId, VelocityOptions? options = null)` - Open existing DFC file
  - `SaveAsync(string pathOrId, IDataBlock source, VelocityOptions? options = null)` - Save DataBlock to DFC
- **Properties**
  - `int RowCount` - Active (non-deleted) row count
  - `uint TotalRowCount` - Total rows including deleted
  - `uint ActiveRowCount` - Active rows (same as RowCount)
  - `DataSchema Schema` - Column schema
  - `bool SupportsBatchAppend` - Always true
- **CRUD Operations**
  - `AddRow(params object[] values)` - Add single row
  - `UpdateRow(int rowIndex, object[] values)` - Update by index
  - `RemoveRow(int rowIndex)` - Delete by index
  - `AddColumn(DataColumn column)` - Add column to schema
  - `DeleteRowAsync(VelocityRowId rowId)` - Delete by row ID (O(1))
  - `DeleteRowAsync(string primaryKey)` - Delete by primary key
  - `DeleteRowsAsync(IEnumerable<string> primaryKeys)` - Batch delete by primary keys
  - `UpdateRowAsync(VelocityRowId rowId, object[] newValues)` - Update by row ID (O(1))
  - `UpdateRowAsync(string primaryKey, object[] newValues)` - Update by primary key
  - `UpdateRowsAsync(Dictionary<string, object[]> updates)` - Batch update by primary keys
  - `UpdateRowsByIndexAsync(Dictionary<int, object[]> updates)` - Batch update by index
  - `AppendBatchAsync(DataBlock batch)` - Append multiple rows efficiently
  - `AppendAsync(IDataBlock additionalData)` - Append additional data
- **Query Operations** (Fluent API with deferred execution)
  - `Select(params string[] columnNames)` - Project columns
  - `Where(string columnName, object value, ComparisonOperator op = Equals)` - Filter rows
  - `WhereContains(string columnName, string pattern)` - String pattern matching (SIMD optimized)
  - `WhereStartsWith(string columnName, string pattern)` - String prefix matching
  - `WhereEndsWith(string columnName, string pattern)` - String suffix matching
  - `Filter(Func<Dictionary<string, object>, bool> predicate, params string[] columnNames)` - Predicate filtering
  - `GroupBy(string columnName)` - Group by column
  - `GroupByAggregate(string groupByColumn, string aggregateColumn, AggregationType, string? resultColumnName)` - Group and aggregate
  - `GroupByAggregate(string groupByColumn, Dictionary<string, AggregationType> aggregations, Dictionary<string, string>? resultColumnNames)` - Multiple aggregations
  - `Sort(SortDirection direction, string columnName)` - Sort by column
  - `Head(int rowCount)` - First N rows
  - `Tail(int rowCount)` - Last N rows
  - `Sample(int rowCount, int? seed = null)` - Random sampling
  - `Compute(string columnName, string expression)` - Add computed column
  - `Merge(DataBlock other, string keyColumn, string keyColumnOther, MergeMode mergeMode)` - Join DataBlocks
  - `Melt(IEnumerable<string> fixedColumns, string meltedColumnName, string meltedValueName)` - Unpivot
  - `Pivot(string indexColumn, string pivotColumn, string valueColumn, AggregationType aggregationType = Sum)` - Pivot from long to wide format
  - `Pivot(IEnumerable<string> indexColumns, string pivotColumn, string valueColumn, AggregationType aggregationType = Sum, string columnNameFormat = "{pivot}_{value}")` - Multi-index pivot
  - `Transpose(string? headerColumnName = null)` - Transpose
  - `DropNulls(params string[] columnNames)` - Drop rows with nulls
  - `DropNulls(DropNullMode dropMode)` - Drop rows with nulls (Any/All mode)
  - `DropDuplicates(KeepDuplicateMode keep = First)` - Remove duplicate rows based on all columns
  - `DropDuplicates(KeepDuplicateMode keep, params string[] columns)` - Remove duplicates based on specific columns
  - `FillNulls(FillMethod method, params string[] columnNames)` - Fill null values with various strategies (lazy)
  - `FillNulls(FillMethod method, object constantValue, params string[] columnNames)` - Fill nulls with constant value (lazy)
  - `Min(params string[] fields)` - Minimum aggregation
  - `Max(params string[] fields)` - Maximum aggregation
  - `Mean(params string[] fields)` - Mean aggregation
  - `Sum(params string[] fields)` - Sum aggregation
  - `StandardDeviation(params string[] fields)` - Standard deviation
  - `Variance(params string[] fields)` - Variance
  - `Count(params string[] fields)` - Count non-null values
- **Query Execution**
  - `Execute()` - Execute query plan and return DataBlock
  - `ClearQueryPlan()` - Clear query operations without executing
- **Storage Operations**
  - `CompactAsync()` - Manual compaction
  - `CompactAsync(VelocityCompactionStrategy strategy)` - Compaction with strategy
  - `FlushAsync()` - Flush pending changes to disk
  - `GetStorageStatsAsync()` - Get storage statistics
- **Configuration**
  - `ConfigureAutoCompaction(bool enabled, VelocityCompactionTrigger trigger, int threshold)` - Configure auto-compaction
  - `EnableBackgroundCompaction(bool enabled = true)` - Enable/disable background compaction
- **Lookup Operations**
  - `FindRowIdAsync(string primaryKey)` - Find row ID by primary key
  - `IsRowDeleted(VelocityRowId rowId)` - Check if row is deleted
  - `GetValue(VelocityRowId rowId, int columnIndex)` - Get value by row ID and column index
  - `GetValue(VelocityRowId rowId, string columnName)` - Get value by row ID and column name
  - `GetPrimaryKeyIndexStats()` - Get primary key index statistics
  - `GetValue(int rowIndex, int columnIndex)` - Get value by row and column index
  - `GetRowCursor()` - Get row cursor for iteration
  - `GetRowCursor(params string[] columnNames)` - Get filtered row cursor
  - `HasColumn(string columnName)` - Check if column exists
  - `GetColumn(string columnName)` - Get column by name
  - `Clone()` - Create in-memory clone as DataBlock
  - `Info()` - Get schema information as DataBlock
- **Validation**
  - `ValidateExpression(string expression, out string error)` - Validate expression syntax
- **Resource Management**
  - `Dispose()` - Release resources

**VelocityOptions**
- **Properties**
  - `string? PrimaryKeyColumn` - Primary key column name
  - `uint SegmentSizeRows` - Rows per segment (default: 100,000)
  - `bool EnableCompression` - Enable LZ4 compression
  - `bool AutoCompactionEnabled` - Enable automatic compaction
  - `CompactionTrigger AutoCompactionTrigger` - Compaction trigger type
  - `int MaxSegmentsBeforeCompaction` - Max segments before auto-compaction
  - `double DeletedPercentageThreshold` - Deleted % threshold for compaction
  - `bool EnableBackgroundCompaction` - Enable background compaction
- **Static Factory Methods**
  - `CreateDefault()` - Default balanced options
  - `CreateUpdateOptimized()` - Optimized for frequent updates
  - `CreateHighThroughput()` - Optimized for high-speed ingestion

**StorageStats**
- **Properties**
  - `uint TotalRows` - Total rows including deleted
  - `uint ActiveRows` - Active (non-deleted) rows
  - `uint DeletedRows` - Number of deleted rows
  - `double DeletedPercentage` - Percentage of deleted rows
  - `int StorageFiles` - Number of storage files
  - `long EstimatedSizeBytes` - Estimated total size in bytes
  - `bool CanCompact` - Whether compaction would be beneficial

**VelocityRowId**
- Opaque identifier for O(1) row access
- Obtained from `FindRowIdAsync()` method

### Enumerations

**VelocityCompactionStrategy** (same as CompactionStrategy)
- `Quick` - Fast compaction, less optimization
- `Standard` - Balanced compaction
- `Aggressive` - Maximum optimization, longer duration

**VelocityCompactionTrigger** (same as CompactionTrigger)
- `SegmentCount` - Trigger on segment count
- `DeletedPercentage` - Trigger on deleted row percentage
- `Manual` - Manual compaction only

**ComparisonOperator** (from Datafication.Core)
- `Equals` - Equality comparison
- `NotEquals` - Inequality comparison
- `GreaterThan` - Greater than
- `GreaterThanOrEqual` - Greater than or equal
- `LessThan` - Less than
- `LessThanOrEqual` - Less than or equal
- `Contains` - String contains (SIMD optimized)
- `StartsWith` - String starts with (SIMD optimized)
- `EndsWith` - String ends with (SIMD optimized)

**SortDirection** (from Datafication.Core)
- `Ascending` - Sort ascending
- `Descending` - Sort descending

**MergeMode** (from Datafication.Core)
- `Left` - Left outer join
- `Right` - Right outer join
- `Full` - Full outer join
- `Inner` - Inner join

**DropNullMode** (from Datafication.Core)
- `Any` - Drop rows with any null value
- `All` - Drop rows where all values are null

**KeepDuplicateMode** (from Datafication.Core)
- `First` - Keep the first occurrence of each duplicate set
- `Last` - Keep the last occurrence of each duplicate set
- `None` - Remove all duplicates, keep only unique rows

**FillMethod** (from Datafication.Core)
- `ForwardFill` - Propagate last valid observation forward
- `BackwardFill` - Use next valid observation to fill gap
- `ConstantValue` - Fill with a specified constant value
- `Mean` - Fill with column mean (numeric only)
- `Median` - Fill with column median (numeric only)
- `Mode` - Fill with most frequent value
- `LinearInterpolation` - Linear interpolation between values (numeric only)

**AggregationType** (from Datafication.Core)
- `Count` - Count non-null values
- `Sum` - Sum of values
- `Mean` - Average value
- `Min` - Minimum value
- `Max` - Maximum value
- `StandardDeviation` - Standard deviation
- `Variance` - Variance

## License

This library is licensed under the **Datafication SDK License Agreement**. See the [LICENSE](./LICENSE) file for details.

**Summary:**
- **Free Use**: Organizations with fewer than 5 developers AND annual revenue under $500,000 USD may use the SDK without a commercial license
- **Commercial License Required**: Organizations with 5+ developers OR annual revenue exceeding $500,000 USD must obtain a commercial license
- **Open Source Exemption**: Open source projects meeting specific criteria may be exempt from developer count limits

For commercial licensing inquiries, contact [support@datafication.co](mailto:support@datafication.co).
