# Datafication.ExcelConnector

[![NuGet](https://img.shields.io/nuget/v/Datafication.ExcelConnector.svg)](https://www.nuget.org/packages/Datafication.ExcelConnector)

A high-performance Excel file connector for .NET that provides seamless integration between Excel workbooks (XLSX, XLS) and the Datafication.Core DataBlock API.

## Description

Datafication.ExcelConnector is a specialized connector library that bridges Excel files and the Datafication.Core ecosystem. Built on the robust ExcelDataReader and ClosedXML libraries, it provides comprehensive Excel file support with flexible sheet selection, column filtering, row limiting, header customization, and both in-memory and streaming batch operations. The connector handles complex Excel workbooks with multiple sheets while maintaining high performance and ease of use.

### Key Features

- **Excel Format Support**: Read and write Excel files in both XLSX and XLS formats
- **Multiple Source Types**: Load Excel from local files, relative paths, or remote URLs (HTTP/HTTPS)
- **Flexible Sheet Selection**: Select sheets by name or index, defaults to first sheet
- **Advanced Configuration**: Header row offset, row skipping, column filtering, and row limiting
- **Pandas-Style API**: Configuration options inspired by pandas.read_excel() for familiarity
- **Streaming Support**: Efficient batch loading for large Excel files with `GetStorageDataAsync`
- **Shorthand API**: Simple one-line methods for common Excel loading scenarios
- **Excel Export**: Convert DataBlocks to professional XLSX files with `ExcelSink`
- **Auto-Sized Columns**: Exported Excel files have intelligently sized columns
- **Type Preservation**: Maintains proper data types for numbers, dates, booleans, and strings
- **Error Handling**: Global error handler configuration for graceful exception management
- **Validation**: Built-in configuration validation ensures correct setup before processing
- **Cross-Platform**: Works on Windows, Linux, and macOS

## Table of Contents

- [Description](#description)
  - [Key Features](#key-features)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
  - [Loading Excel Files (Shorthand)](#loading-excel-files-shorthand)
  - [Loading Excel with Configuration](#loading-excel-with-configuration)
  - [Selecting Specific Sheets](#selecting-specific-sheets)
  - [Loading from Remote URLs](#loading-from-remote-urls)
  - [Column Filtering with UseColumns](#column-filtering-with-usecolumns)
  - [Row Limiting with NRows](#row-limiting-with-nrows)
  - [Skipping Rows and Custom Headers](#skipping-rows-and-custom-headers)
  - [Excel Files without Headers](#excel-files-without-headers)
  - [Streaming Large Excel Files to Storage](#streaming-large-excel-files-to-storage)
  - [Writing DataBlocks to Excel](#writing-datablocks-to-excel)
  - [Error Handling](#error-handling)
  - [Working with Excel Data](#working-with-excel-data)
- [Configuration Reference](#configuration-reference)
  - [ExcelConnectorConfiguration](#excelconnectorconfiguration)
- [API Reference](#api-reference)
  - [Core Classes](#core-classes)
  - [Extension Methods](#extension-methods)
- [Common Patterns](#common-patterns)
  - [ETL Pipeline with Excel](#etl-pipeline-with-excel)
  - [Excel to VelocityDataBlock](#excel-to-velocitydatablock)
  - [Multi-Sheet Analysis](#multi-sheet-analysis)
  - [Excel Report Generation](#excel-report-generation)
- [Performance Tips](#performance-tips)
- [License](#license)

## Installation

> **Note**: Datafication.ExcelConnector is currently in pre-release. The packages are now available on nuget.org.

```bash
dotnet add package Datafication.ExcelConnector
```

**Running the Samples:**

```bash
cd samples/ExcelBasicLoad
dotnet run
```

**Dependencies:**
- ExcelDataReader (MIT License) - for reading Excel files
- ClosedXML (MIT License) - for writing Excel files

## Usage Examples

### Loading Excel Files (Shorthand)

The simplest way to load an Excel file is using the shorthand extension methods:

```csharp
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ExcelConnector;

// Load Excel from local file (async) - uses first sheet
var employees = await DataBlock.Connector.LoadExcelAsync("data/employees.xlsx");

Console.WriteLine($"Loaded {employees.RowCount} employees from Excel");

// Synchronous version
var departments = DataBlock.Connector.LoadExcel("data/departments.xlsx");
```

### Loading Excel with Configuration

For more control over Excel parsing, use the full configuration:

```csharp
using Datafication.Connectors.ExcelConnector;

// Create configuration with custom settings
var configuration = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/employees.xlsx"),
    HasHeader = true,
    SheetName = "Employees"
};

// Create connector and load data
var connector = new ExcelDataConnector(configuration);
var data = await connector.GetDataAsync();

Console.WriteLine($"Loaded {data.RowCount} rows with {data.Schema.Count} columns");
```

### Selecting Specific Sheets

Load data from specific sheets by name or index:

```csharp
// Load by sheet name
var configuration = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/sales_report.xlsx"),
    SheetName = "Q1 Sales",  // Select sheet by name
    HasHeader = true
};

var connector = new ExcelDataConnector(configuration);
var q1Data = await connector.GetDataAsync();

// Load by sheet index (0-based)
var config2 = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/sales_report.xlsx"),
    SheetIndex = 1,  // Select second sheet
    HasHeader = true
};

var connector2 = new ExcelDataConnector(config2);
var q2Data = await connector2.GetDataAsync();

// Note: If both SheetName and SheetIndex are set, SheetName takes precedence
// If neither is set, the first sheet is loaded
```

### Loading from Remote URLs

Load Excel files directly from HTTP/HTTPS URLs:

```csharp
// Load from remote URL (async)
var remoteData = await DataBlock.Connector.LoadExcelAsync(
    "https://example.com/data/sales.xlsx"
);

// Or with full configuration
var configuration = new ExcelConnectorConfiguration
{
    Source = new Uri("https://example.com/reports/monthly.xlsx"),
    HasHeader = true,
    SheetName = "Summary"
};

var connector = new ExcelDataConnector(configuration);
var webData = await connector.GetDataAsync();

Console.WriteLine($"Downloaded and loaded {webData.RowCount} rows");
```

### Column Filtering with UseColumns

Load only specific columns from an Excel file:

```csharp
// Load only CustomerID and CompanyName columns
var configuration = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/customers.xlsx"),
    HasHeader = true,
    UseColumns = "CustomerID, CompanyName"  // Comma-separated column names
};

var connector = new ExcelDataConnector(configuration);
var data = await connector.GetDataAsync();

Console.WriteLine($"Loaded {data.Schema.Count} columns");  // Should be 2
Assert.IsTrue(data.HasColumn("CustomerID"));
Assert.IsTrue(data.HasColumn("CompanyName"));

// UseColumns is case-insensitive and trims whitespace
var config2 = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/products.xlsx"),
    HasHeader = true,
    UseColumns = "productid,productname,unitprice"  // Can use lowercase
};
```

### Row Limiting with NRows

Limit the number of rows loaded from an Excel file:

```csharp
// Load only first 100 rows (after header)
var configuration = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/large_dataset.xlsx"),
    HasHeader = true,
    NRows = 100  // Limit to 100 data rows
};

var connector = new ExcelDataConnector(configuration);
var sample = await connector.GetDataAsync();

Assert.AreEqual(100, sample.RowCount);

// Useful for quick data preview or testing
var previewConfig = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/sales.xlsx"),
    HasHeader = true,
    NRows = 5
};

var preview = await DataBlock.Connector.LoadExcelAsync(previewConfig);
Console.WriteLine("First 5 rows preview:");
Console.WriteLine(await preview.TextTableAsync());
```

### Skipping Rows and Custom Headers

Handle Excel files with blank rows or non-standard header placement:

```csharp
// Skip first 2 rows, then treat row 3 as header
var configuration = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/report.xlsx"),
    HasHeader = true,
    SkipRows = 2,      // Skip first 2 rows
    HeaderRow = 0      // Header is the first row after skipping
};

var connector = new ExcelDataConnector(configuration);
var data = await connector.GetDataAsync();

// Advanced: Skip rows and offset header
var config2 = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/complex_report.xlsx"),
    HasHeader = true,
    SkipRows = 3,      // Skip first 3 rows
    HeaderRow = 1      // Header is 1 row after that (row 5 in the file)
};

// SkipRows + HeaderRow gives fine-grained control over where to find headers
```

### Excel Files without Headers

Load Excel files that don't have a header row:

```csharp
var configuration = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/raw_data.xlsx"),
    HasHeader = false  // No header row
};

var connector = new ExcelDataConnector(configuration);
var data = await connector.GetDataAsync();

// Columns will be named using Excel conventions: Column1, Column2, Column3, etc.
foreach (var columnName in data.Schema.GetColumnNames())
{
    Console.WriteLine($"Column: {columnName}");
}
```

### Streaming Large Excel Files to Storage

For large Excel files, stream data directly to VelocityDataBlock in batches:

```csharp
using Datafication.Storage.Velocity;

// Create VelocityDataBlock for efficient large-scale storage
var velocityBlock = new VelocityDataBlock("data/large_dataset.dfc");

// Configure Excel source
var configuration = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/1_million_rows.xlsx"),
    HasHeader = true,
    SheetName = "Data"
};

// Stream Excel data in batches of 10,000 rows
var connector = new ExcelDataConnector(configuration);
await connector.GetStorageDataAsync(velocityBlock, batchSize: 10000);

Console.WriteLine($"Streamed {velocityBlock.RowCount} rows to storage");
await velocityBlock.FlushAsync();
```

### Writing DataBlocks to Excel

Convert DataBlocks to professional Excel files:

```csharp
using Datafication.Sinks.Connectors.ExcelConnector;

// Create or load a DataBlock
var data = new DataBlock();
data.AddColumn(new DataColumn("Name", typeof(string)));
data.AddColumn(new DataColumn("Age", typeof(int)));
data.AddColumn(new DataColumn("Salary", typeof(decimal)));
data.AddColumn(new DataColumn("HireDate", typeof(DateTime)));

data.AddRow(new object[] { "Alice", 30, 75000m, new DateTime(2020, 1, 15) });
data.AddRow(new object[] { "Bob", 25, 65000m, new DateTime(2021, 3, 10) });
data.AddRow(new object[] { "Carol", 35, 85000m, new DateTime(2019, 7, 20) });

// Convert to Excel file (as byte array) - async
var excelBytes = await data.ExcelSinkAsync();

// Write to file
await File.WriteAllBytesAsync("output/employees.xlsx", excelBytes);

// Synchronous version
var excelOutput = data.ExcelSink();
File.WriteAllBytes("output/employees_sync.xlsx", excelOutput);

// The exported Excel file includes:
// - Header row with column names
// - Auto-sized columns based on content width
// - Proper data types (numbers, dates, booleans, strings)
// - Professional formatting
```

### Error Handling

Configure global error handling for Excel operations:

```csharp
var configuration = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/employees.xlsx"),
    HasHeader = true,
    ErrorHandler = (exception) =>
    {
        Console.WriteLine($"Excel Error: {exception.Message}");
        // Log to file, send alert, etc.
    }
};

var connector = new ExcelDataConnector(configuration);

try
{
    var data = await connector.GetDataAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load Excel: {ex.Message}");
}
```

### Working with Excel Data

Once loaded, use the full DataBlock API for data manipulation:

```csharp
// Load Excel file
var sales = await DataBlock.Connector.LoadExcelAsync("data/sales_data.xlsx");

// Filter, transform, and analyze
var result = sales
    .Where("Region", "West")
    .Where("Revenue", 10000m, ComparisonOperator.GreaterThan)
    .Compute("Profit", "Revenue - Cost")
    .Compute("Margin", "Profit / Revenue")
    .Select("ProductName", "Revenue", "Profit", "Margin")
    .Sort(SortDirection.Descending, "Profit")
    .Head(10);

Console.WriteLine("Top 10 profitable products in West region:");
Console.WriteLine(await result.TextTableAsync());

// Export results back to Excel
var resultExcel = await result.ExcelSinkAsync();
await File.WriteAllBytesAsync("output/top_products.xlsx", resultExcel);
```

## Configuration Reference

### ExcelConnectorConfiguration

Configuration class for Excel data sources with pandas-inspired options.

**Properties:**

- **`Source`** (Uri, required): Location of the Excel data source
  - File path: `new Uri("file:///C:/data/file.xlsx")`
  - HTTP/HTTPS URL: `new Uri("https://example.com/data.xlsx")`
  - Relative path: `new Uri("data/file.xlsx", UriKind.Relative)`

- **`SheetName`** (string?, optional): Name of the sheet to load
  - Default: `null` (loads first sheet)
  - Example: `"Employees"`, `"Q1 Sales"`, `"Summary"`
  - Takes precedence over `SheetIndex` if both are set

- **`SheetIndex`** (int?, optional): Zero-based index of the sheet to load
  - Default: `null` (loads first sheet)
  - Example: `0` (first sheet), `1` (second sheet)
  - Ignored if `SheetName` is also specified

- **`HasHeader`** (bool, default: true): Whether the first row contains column headers
  - `true`: First row (after skipping) is treated as column names
  - `false`: Columns are named `Column1`, `Column2`, etc.

- **`HeaderRow`** (int, default: 0): Offset for the header row after skipping
  - `0`: First row after `SkipRows` is the header
  - `1`: Skip one additional row before reading header
  - Works in combination with `SkipRows`

- **`SkipRows`** (int, default: 0): Number of rows to skip before reading any data
  - `0`: Start reading from first row
  - `2`: Skip first 2 rows
  - Applied before `HeaderRow` offset

- **`UseColumns`** (string?, optional): Comma-separated list of column names to include
  - Default: `null` (load all columns)
  - Example: `"CustomerID, CompanyName, ContactName"`
  - Case-insensitive matching
  - Whitespace is trimmed

- **`NRows`** (int?, optional): Maximum number of data rows to read
  - Default: `null` (read all rows)
  - Example: `100` (read only first 100 data rows)
  - Counted after header row

- **`Id`** (string, auto-generated): Unique identifier for the configuration
  - Automatically generated as GUID if not specified

- **`ErrorHandler`** (Action<Exception>?, optional): Global exception handler
  - Provides centralized error handling for Excel operations

**Example:**

```csharp
var config = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/sales.xlsx"),
    SheetName = "Q1 Sales",
    HasHeader = true,
    SkipRows = 2,
    UseColumns = "ProductID, ProductName, Revenue",
    NRows = 1000,
    Id = "q1-sales-connector",
    ErrorHandler = ex => Console.WriteLine($"Error: {ex.Message}")
};
```

## API Reference

For complete API documentation, see the [Datafication.Connectors.ExcelConnector API Reference](https://datafication.co/help/api/reference/Datafication.Connectors.ExcelConnector.html).

### Core Classes

**ExcelDataConnector**
- **Constructor**
  - `ExcelDataConnector(ExcelConnectorConfiguration configuration)` - Creates connector with validation
- **Methods**
  - `Task<DataBlock> GetDataAsync()` - Loads Excel file into memory as DataBlock
  - `Task<IStorageDataBlock> GetStorageDataAsync(IStorageDataBlock target, int batchSize = 10000)` - Streams Excel data in batches
  - `string GetConnectorId()` - Returns unique connector identifier
- **Properties**
  - `ExcelConnectorConfiguration Configuration` - Current configuration

**ExcelConnectorConfiguration**
- **Properties**
  - `Uri Source` - Excel source location
  - `string? SheetName` - Sheet name to load
  - `int? SheetIndex` - Sheet index to load (0-based)
  - `bool HasHeader` - Header row flag (default: true)
  - `int HeaderRow` - Header row offset (default: 0)
  - `int SkipRows` - Rows to skip (default: 0)
  - `string? UseColumns` - Column filter (comma-separated)
  - `int? NRows` - Row limit
  - `string Id` - Unique identifier
  - `Action<Exception>? ErrorHandler` - Error handler

**ExcelSink**
- Implements `IDataSink<byte[]>`
- **Methods**
  - `Task<byte[]> Transform(DataBlock dataBlock)` - Converts DataBlock to XLSX file (byte array)
- **Features**
  - Auto-sized columns based on content width
  - Proper type handling (numbers, dates, booleans, strings)
  - Professional formatting

**ExcelConnectorValidator**
- Validates `ExcelConnectorConfiguration` instances
- **Methods**
  - `ValidationResult Validate(IDataConnectorConfiguration configuration)` - Validates configuration

### Extension Methods

**ExcelConnectorExtensions** (namespace: `Datafication.Extensions.Connectors.ExcelConnector`)

```csharp
// Async shorthand methods
Task<DataBlock> LoadExcelAsync(this ConnectorExtensions ext, string source)
Task<DataBlock> LoadExcelAsync(this ConnectorExtensions ext, ExcelConnectorConfiguration config)

// Synchronous shorthand methods
DataBlock LoadExcel(this ConnectorExtensions ext, string source)
DataBlock LoadExcel(this ConnectorExtensions ext, ExcelConnectorConfiguration config)
```

**ExcelSinkExtension** (namespace: `Datafication.Sinks.Connectors.ExcelConnector`)

```csharp
// Convert DataBlock to Excel
Task<byte[]> ExcelSinkAsync(this DataBlock dataBlock)
byte[] ExcelSink(this DataBlock dataBlock)
```

## Common Patterns

### ETL Pipeline with Excel

```csharp
// Extract: Load Excel
var rawData = await DataBlock.Connector.LoadExcelAsync("input/sales_data.xlsx");

// Transform: Clean and process
var transformed = rawData
    .DropNulls(DropNullMode.Any)
    .Where("Status", "Cancelled", ComparisonOperator.NotEquals)
    .Compute("NetRevenue", "Revenue - Discount")
    .Compute("ProfitMargin", "NetRevenue / Revenue")
    .Select("OrderId", "ProductName", "NetRevenue", "ProfitMargin");

// Load: Export to Excel
var outputExcel = await transformed.ExcelSinkAsync();
await File.WriteAllBytesAsync("output/processed_sales.xlsx", outputExcel);

Console.WriteLine($"Processed {transformed.RowCount} orders");
```

### Excel to VelocityDataBlock

```csharp
using Datafication.Storage.Velocity;

// Load Excel configuration
var excelConfig = new ExcelConnectorConfiguration
{
    Source = new Uri("file:///data/large_sales.xlsx"),
    HasHeader = true,
    SheetName = "Sales Data"
};

// Create VelocityDataBlock with primary key
var velocityBlock = VelocityDataBlock.CreateEnterprise(
    "data/sales.dfc",
    primaryKeyColumn: "OrderId"
);

// Stream Excel data directly to VelocityDataBlock
var connector = new ExcelDataConnector(excelConfig);
await connector.GetStorageDataAsync(velocityBlock, batchSize: 50000);
await velocityBlock.FlushAsync();

Console.WriteLine($"Loaded {velocityBlock.RowCount} rows into VelocityDataBlock");

// Now query efficiently
var recentSales = velocityBlock
    .Where("OrderDate", DateTime.Now.AddDays(-30), ComparisonOperator.GreaterThan)
    .GroupByAggregate("Region", "Revenue", AggregationType.Sum, "total_revenue")
    .Execute();
```

### Multi-Sheet Analysis

```csharp
// Load data from multiple sheets in the same workbook
var filePath = "data/annual_report.xlsx";

// Load Q1 data
var q1Config = new ExcelConnectorConfiguration
{
    Source = new Uri(filePath),
    SheetName = "Q1",
    HasHeader = true
};
var q1Data = await DataBlock.Connector.LoadExcelAsync(q1Config);

// Load Q2 data
var q2Config = new ExcelConnectorConfiguration
{
    Source = new Uri(filePath),
    SheetName = "Q2",
    HasHeader = true
};
var q2Data = await DataBlock.Connector.LoadExcelAsync(q2Config);

// Combine and analyze
var combined = q1Data.Clone();
combined.AppendRowsBatch(q2Data);

var summary = combined
    .GroupBy("Region")
    .Aggregate(
        new[] { "Revenue" },
        new Dictionary<string, AggregationType>
        {
            { "Revenue", AggregationType.Sum }
        }
    );

Console.WriteLine("Half-Year Summary:");
Console.WriteLine(await summary.TextTableAsync());

// Export combined analysis
var reportExcel = await summary.ExcelSinkAsync();
await File.WriteAllBytesAsync("output/h1_summary.xlsx", reportExcel);
```

### Excel Report Generation

```csharp
// Load source data
var employees = await DataBlock.Connector.LoadExcelAsync("data/employees.xlsx");

// Generate department salary report
var salaryReport = employees
    .GroupBy("Department")
    .Aggregate(
        new[] { "Salary" },
        new Dictionary<string, AggregationType>
        {
            { "Salary", AggregationType.Mean }
        }
    )
    .Sort(SortDirection.Descending, "mean_Salary");

// Generate high performers report
var highPerformers = employees
    .Where("PerformanceRating", 4, ComparisonOperator.GreaterThanOrEqual)
    .Sort(SortDirection.Descending, "Salary")
    .Select("Name", "Department", "Salary", "PerformanceRating");

// Export both reports to Excel
var salaryReportExcel = await salaryReport.ExcelSinkAsync();
await File.WriteAllBytesAsync("reports/salary_by_department.xlsx", salaryReportExcel);

var performersExcel = await highPerformers.ExcelSinkAsync();
await File.WriteAllBytesAsync("reports/high_performers.xlsx", performersExcel);

Console.WriteLine("Reports generated successfully");
```

## Performance Tips

1. **Use Streaming for Large Files**: For Excel files with hundreds of thousands of rows, use `GetStorageDataAsync` to stream data directly to VelocityDataBlock
   ```csharp
   await connector.GetStorageDataAsync(velocityBlock, batchSize: 100000);
   ```

2. **Filter Columns Early**: Use `UseColumns` to load only needed columns, reducing memory usage and improving speed
   ```csharp
   UseColumns = "OrderID, CustomerID, Revenue"  // Load only 3 columns
   ```

3. **Limit Rows for Testing**: Use `NRows` when developing and testing to work with smaller datasets
   ```csharp
   NRows = 100  // Quick preview of first 100 rows
   ```

4. **Sheet Selection**: When working with multi-sheet workbooks, use `SheetIndex` or `SheetName` to avoid loading unnecessary sheets
   ```csharp
   SheetIndex = 0  // Faster than searching by name if you know the position
   ```

5. **Adjust Batch Size**: Tune the batch size based on available memory and row complexity
   - Small rows (few columns): Use larger batch sizes (50,000 - 100,000)
   - Wide rows (many columns): Use smaller batch sizes (10,000 - 25,000)

6. **Remote File Caching**: When loading from URLs repeatedly, download once and cache locally:
   ```csharp
   if (!File.Exists("cache/data.xlsx"))
   {
       var webData = await DataBlock.Connector.LoadExcelAsync("https://example.com/data.xlsx");
       var bytes = await webData.ExcelSinkAsync();
       await File.WriteAllBytesAsync("cache/data.xlsx", bytes);
   }
   var data = await DataBlock.Connector.LoadExcelAsync("cache/data.xlsx");
   ```

7. **Skip Empty Rows**: The connector handles empty rows gracefully. Use `SkipRows` if you know there are blank rows at the beginning

8. **Type Detection**: Excel files maintain type information (unlike CSV), so data types are preserved automatically for better performance

9. **Dispose DataBlocks**: For large Excel processing pipelines, dispose intermediate DataBlocks to free memory:
   ```csharp
   using (var rawData = await DataBlock.Connector.LoadExcelAsync("large_file.xlsx"))
   {
       var processed = rawData.Where(...).Select(...);
       // rawData automatically disposed here
   }
   ```

10. **Excel Export Optimization**: When exporting large DataBlocks to Excel, be aware that Excel has a row limit (1,048,576 rows in XLSX). Consider splitting large datasets or using CSV for extremely large exports.

## License

This library is licensed under the **Datafication SDK License Agreement**. See the [LICENSE](./LICENSE) file for details.

**Summary:**
- **Free Use**: Organizations with fewer than 5 developers AND annual revenue under $500,000 USD may use the SDK without a commercial license
- **Commercial License Required**: Organizations with 5+ developers OR annual revenue exceeding $500,000 USD must obtain a commercial license
- **Open Source Exemption**: Open source projects meeting specific criteria may be exempt from developer count limits

For commercial licensing inquiries, contact [support@datafication.co](mailto:support@datafication.co).

**Third-Party Libraries:**
- ExcelDataReader - MIT License
- ClosedXML - MIT License

---

**Datafication.ExcelConnector** - Seamlessly connect Excel workbooks to the Datafication ecosystem.

For more examples and documentation, visit our [samples directory](../../samples/).
