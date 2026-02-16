# Datafication.CsvConnector

[![NuGet](https://img.shields.io/nuget/v/Datafication.CsvConnector.svg)](https://www.nuget.org/packages/Datafication.CsvConnector)

A high-performance CSV file connector for .NET that provides seamless integration between CSV data sources and the Datafication.Core DataBlock API.

## Description

Datafication.CsvConnector is a specialized connector library that bridges CSV files and the Datafication.Core ecosystem. It provides robust CSV parsing with automatic type detection, support for local and remote files, customizable delimiters, and both in-memory and streaming batch operations. The connector handles various CSV formats and edge cases while maintaining high performance and ease of use.

### Key Features

- **Multiple Source Types**: Load CSV from local files, relative paths, or remote URLs (HTTP/HTTPS)
- **Automatic Type Detection**: Intelligently infers column data types from CSV content
- **Flexible Parsing**: Customizable delimiters, header row handling, and quote character support
- **Streaming Support**: Efficient batch loading for large CSV files with `GetStorageDataAsync`
- **Shorthand API**: Simple one-line methods for common CSV loading scenarios
- **CSV Export**: Convert DataBlocks back to CSV format with `CsvStringSink`
- **Error Handling**: Global error handler configuration for graceful exception management
- **Validation**: Built-in configuration validation ensures correct setup before processing
- **Cross-Platform**: Works on Windows, Linux, and macOS

## Table of Contents

- [Description](#description)
  - [Key Features](#key-features)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
  - [Loading CSV Files (Shorthand)](#loading-csv-files-shorthand)
  - [Loading CSV with Configuration](#loading-csv-with-configuration)
  - [Loading from Remote URLs](#loading-from-remote-urls)
  - [Custom Delimiters and Separators](#custom-delimiters-and-separators)
  - [CSV without Header Row](#csv-without-header-row)
  - [Streaming Large CSV Files to Storage](#streaming-large-csv-files-to-storage)
  - [Writing DataBlocks to CSV](#writing-datablocks-to-csv)
  - [Error Handling](#error-handling)
  - [Working with CSV Data](#working-with-csv-data)
- [Configuration Reference](#configuration-reference)
  - [CsvConnectorConfiguration](#csvconnectorconfiguration)
- [API Reference](#api-reference)
  - [Core Classes](#core-classes)
  - [Extension Methods](#extension-methods)
- [Common Patterns](#common-patterns)
  - [ETL Pipeline with CSV](#etl-pipeline-with-csv)
  - [CSV to VelocityDataBlock](#csv-to-velocitydatablock)
  - [Data Analysis from CSV](#data-analysis-from-csv)
- [Performance Tips](#performance-tips)
- [License](#license)

## Installation

> **Note**: Datafication.CsvConnector is currently in pre-release. The packages are now available on nuget.org.

```bash
dotnet add package Datafication.CsvConnector
```

**Running the Samples:**

```bash
cd samples/CsvBasicLoad
dotnet run
```

## Usage Examples

### Loading CSV Files (Shorthand)

The simplest way to load a CSV file is using the shorthand extension methods:

```csharp
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.CsvConnector;

// Load CSV from local file (async)
var employees = await DataBlock.Connector.LoadCsvAsync("data/employees.csv");

Console.WriteLine($"Loaded {employees.RowCount} employees");

// Synchronous version
var departments = DataBlock.Connector.LoadCsv("data/departments.csv");
```

### Loading CSV with Configuration

For more control over CSV parsing, use the full configuration:

```csharp
using Datafication.Connectors.CsvConnector;

// Create configuration with custom settings
var configuration = new CsvConnectorConfiguration
{
    Source = new Uri("file:///data/employees.csv"),
    HeaderRow = true,
    Separator = ','
};

// Create connector and load data
var connector = new CsvDataConnector(configuration);
var data = await connector.GetDataAsync();

Console.WriteLine($"Loaded {data.RowCount} rows with {data.Schema.Count} columns");
```

### Loading from Remote URLs

Load CSV files directly from HTTP/HTTPS URLs:

```csharp
// Load from remote URL (async)
var remoteData = await DataBlock.Connector.LoadCsvAsync(
    "https://example.com/data/sales.csv"
);

// Or with full configuration
var configuration = new CsvConnectorConfiguration
{
    Source = new Uri("https://people.sc.fsu.edu/~jburkardt/data/csv/hw_25000.csv"),
    HeaderRow = true
};

var connector = new CsvDataConnector(configuration);
var webData = await connector.GetDataAsync();

Console.WriteLine($"Downloaded and loaded {webData.RowCount} rows");
```

### Custom Delimiters and Separators

Handle CSV files with different delimiters:

```csharp
// Semicolon-delimited CSV
var configuration = new CsvConnectorConfiguration
{
    Source = new Uri("file:///data/european_data.csv"),
    HeaderRow = true,
    Separator = ';'  // Use semicolon instead of comma
};

var connector = new CsvDataConnector(configuration);
var data = await connector.GetDataAsync();

// Tab-delimited file (TSV)
var tsvConfig = new CsvConnectorConfiguration
{
    Source = new Uri("file:///data/report.tsv"),
    HeaderRow = true,
    Separator = '\t'  // Tab separator
};

var tsvConnector = new CsvDataConnector(tsvConfig);
var tsvData = await tsvConnector.GetDataAsync();
```

### CSV without Header Row

Load CSV files that don't have a header row:

```csharp
var configuration = new CsvConnectorConfiguration
{
    Source = new Uri("file:///data/no_headers.csv"),
    HeaderRow = false  // No header row
};

var connector = new CsvDataConnector(configuration);
var data = await connector.GetDataAsync();

// Columns will be named: Column_1, Column_2, Column_3, etc.
foreach (var columnName in data.Schema.GetColumnNames())
{
    Console.WriteLine($"Column: {columnName}");
}
```

### Streaming Large CSV Files to Storage

For large CSV files, stream data directly to VelocityDataBlock in batches:

```csharp
using Datafication.Storage.Velocity;

// Create VelocityDataBlock for efficient large-scale storage
var velocityBlock = new VelocityDataBlock("data/large_dataset.dfc");

// Configure CSV source
var configuration = new CsvConnectorConfiguration
{
    Source = new Uri("file:///data/5_million_rows.csv"),
    HeaderRow = true,
    Separator = ','
};

// Stream CSV data in batches of 10,000 rows
var connector = new CsvDataConnector(configuration);
await connector.GetStorageDataAsync(velocityBlock, batchSize: 10000);

Console.WriteLine($"Streamed {velocityBlock.RowCount} rows to storage");
await velocityBlock.FlushAsync();
```

### Writing DataBlocks to CSV

Convert DataBlocks back to CSV format:

```csharp
using Datafication.Sinks.Connectors.CsvConnector;

// Create or load a DataBlock
var data = new DataBlock();
data.AddColumn(new DataColumn("Name", typeof(string)));
data.AddColumn(new DataColumn("Age", typeof(int)));
data.AddColumn(new DataColumn("Salary", typeof(decimal)));

data.AddRow(new object[] { "Alice", 30, 75000m });
data.AddRow(new object[] { "Bob", 25, 65000m });
data.AddRow(new object[] { "Carol", 35, 85000m });

// Convert to CSV string (async)
var csvString = await data.CsvStringSinkAsync();
Console.WriteLine(csvString);

// Synchronous version
var csvOutput = data.CsvStringSink();

// Write to file
await File.WriteAllTextAsync("output/employees.csv", csvString);
```

### Error Handling

Configure global error handling for CSV operations:

```csharp
var configuration = new CsvConnectorConfiguration
{
    Source = new Uri("file:///data/employees.csv"),
    HeaderRow = true,
    ErrorHandler = (exception) =>
    {
        Console.WriteLine($"CSV Error: {exception.Message}");
        // Log to file, send alert, etc.
    }
};

var connector = new CsvDataConnector(configuration);

try
{
    var data = await connector.GetDataAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load CSV: {ex.Message}");
}
```

### Working with CSV Data

Once loaded, use the full DataBlock API for data manipulation:

```csharp
// Load CSV file
var sales = await DataBlock.Connector.LoadCsvAsync("data/sales.csv");

// Filter, transform, and analyze
var result = sales
    .Where("Region", "West")
    .Where("Revenue", 10000m, ComparisonOperator.GreaterThan)
    .Compute("Profit", "Revenue - Cost")
    .Compute("Margin", "Profit / Revenue")
    .Select("ProductName", "Revenue", "Profit", "Margin")
    .Sort(SortDirection.Descending, "Profit")
    .Head(10);

Console.WriteLine($"Top 10 profitable products in West region:");
Console.WriteLine(await result.TextTableAsync());

// Export results back to CSV
var resultCsv = await result.CsvStringSinkAsync();
await File.WriteAllTextAsync("output/top_products.csv", resultCsv);
```

## Configuration Reference

### CsvConnectorConfiguration

Configuration class for CSV data sources.

**Properties:**

- **`Source`** (Uri, required): Location of the CSV data source
  - File path: `new Uri("file:///C:/data/file.csv")`
  - HTTP/HTTPS URL: `new Uri("https://example.com/data.csv")`
  - Relative path: `new Uri("data/file.csv", UriKind.Relative)`

- **`Separator`** (char?, optional): Field separator character
  - Default: `,` (comma)
  - Common values: `;` (semicolon), `\t` (tab)

- **`HeaderRow`** (bool, default: true): Whether the first row contains column headers
  - `true`: First row is treated as column names
  - `false`: Columns are named `Column_1`, `Column_2`, etc.

- **`Id`** (string, auto-generated): Unique identifier for the configuration
  - Automatically generated as GUID if not specified

- **`ErrorHandler`** (Action<Exception>?, optional): Global exception handler
  - Provides centralized error handling for CSV operations

**Example:**

```csharp
var config = new CsvConnectorConfiguration
{
    Source = new Uri("file:///data/employees.csv"),
    Separator = ',',
    HeaderRow = true,
    Id = "employees-connector",
    ErrorHandler = ex => Console.WriteLine($"Error: {ex.Message}")
};
```

## API Reference

For complete API documentation, see the [Datafication.Connectors.CsvConnector API Reference](https://datafication.co/help/api/reference/Datafication.Connectors.CsvConnector.html).

### Core Classes

**CsvDataConnector**
- **Constructor**
  - `CsvDataConnector(CsvConnectorConfiguration configuration)` - Creates connector with validation
- **Methods**
  - `Task<DataBlock> GetDataAsync()` - Loads entire CSV into memory as DataBlock
  - `Task<IStorageDataBlock> GetStorageDataAsync(IStorageDataBlock target, int batchSize = 10000)` - Streams CSV data in batches
  - `string GetConnectorId()` - Returns unique connector identifier
- **Properties**
  - `CsvConnectorConfiguration Configuration` - Current configuration

**CsvConnectorConfiguration**
- **Properties**
  - `Uri Source` - CSV source location
  - `char? Separator` - Field separator (default: comma)
  - `bool HeaderRow` - Header row flag (default: true)
  - `string Id` - Unique identifier
  - `Action<Exception>? ErrorHandler` - Error handler

**CsvStringSink**
- Implements `IDataSink<string>`
- **Methods**
  - `Task<string> Transform(DataBlock dataBlock)` - Converts DataBlock to CSV string

**CsvConnectorValidator**
- Validates `CsvConnectorConfiguration` instances
- **Methods**
  - `ValidationResult Validate(IDataConnectorConfiguration configuration)` - Validates configuration

### Extension Methods

**CsvConnectorExtensions** (namespace: `Datafication.Extensions.Connectors.CsvConnector`)

```csharp
// Async shorthand methods
Task<DataBlock> LoadCsvAsync(this ConnectorExtensions ext, string source)
Task<DataBlock> LoadCsvAsync(this ConnectorExtensions ext, CsvConnectorConfiguration config)

// Synchronous shorthand methods
DataBlock LoadCsv(this ConnectorExtensions ext, string source)
DataBlock LoadCsv(this ConnectorExtensions ext, CsvConnectorConfiguration config)
```

**CsvStringSinkExtension** (namespace: `Datafication.Sinks.Connectors.CsvConnector`)

```csharp
// Convert DataBlock to CSV
Task<string> CsvStringSinkAsync(this DataBlock dataBlock)
string CsvStringSink(this DataBlock dataBlock)
```

## Common Patterns

### ETL Pipeline with CSV

```csharp
// Extract: Load CSV
var rawData = await DataBlock.Connector.LoadCsvAsync("input/sales_data.csv");

// Transform: Clean and process
var transformed = rawData
    .DropNulls(DropNullMode.Any)
    .Where("Status", "Cancelled", ComparisonOperator.NotEquals)
    .Compute("NetRevenue", "Revenue - Discount")
    .Compute("ProfitMargin", "NetRevenue / Revenue")
    .Select("OrderId", "ProductName", "NetRevenue", "ProfitMargin");

// Load: Export to CSV
var outputCsv = await transformed.CsvStringSinkAsync();
await File.WriteAllTextAsync("output/processed_sales.csv", outputCsv);

Console.WriteLine($"Processed {transformed.RowCount} orders");
```

### CSV to VelocityDataBlock

```csharp
using Datafication.Storage.Velocity;

// Load CSV configuration
var csvConfig = new CsvConnectorConfiguration
{
    Source = new Uri("file:///data/large_sales.csv"),
    HeaderRow = true
};

// Create VelocityDataBlock with primary key
var velocityBlock = VelocityDataBlock.CreateEnterprise(
    "data/sales.dfc",
    primaryKeyColumn: "OrderId"
);

// Stream CSV data directly to VelocityDataBlock
var connector = new CsvDataConnector(csvConfig);
await connector.GetStorageDataAsync(velocityBlock, batchSize: 50000);
await velocityBlock.FlushAsync();

Console.WriteLine($"Loaded {velocityBlock.RowCount} rows into VelocityDataBlock");

// Now query efficiently
var recentSales = velocityBlock
    .Where("OrderDate", DateTime.Now.AddDays(-30), ComparisonOperator.GreaterThan)
    .GroupByAggregate("Region", "Revenue", AggregationType.Sum, "total_revenue")
    .Execute();
```

### Data Analysis from CSV

```csharp
// Load employee data
var employees = await DataBlock.Connector.LoadCsvAsync("data/employees.csv");

// Department salary analysis
var salaryStats = employees
    .GroupBy("Department")
    .Aggregate(
        new[] { "Salary" },
        new Dictionary<string, AggregationType>
        {
            { "Salary", AggregationType.Mean }
        }
    );

Console.WriteLine("Average Salary by Department:");
Console.WriteLine(await salaryStats.TextTableAsync());

// Export analysis results
var analysisCsv = await salaryStats.CsvStringSinkAsync();
await File.WriteAllTextAsync("output/salary_analysis.csv", analysisCsv);

// High earners report
var highEarners = employees
    .Where("Salary", 100000m, ComparisonOperator.GreaterThanOrEqual)
    .Sort(SortDirection.Descending, "Salary")
    .Select("Name", "Department", "Salary", "HireDate");

var reportCsv = await highEarners.CsvStringSinkAsync();
await File.WriteAllTextAsync("output/high_earners.csv", reportCsv);
```

## Performance Tips

1. **Use Streaming for Large Files**: For CSV files with millions of rows, use `GetStorageDataAsync` to stream data directly to VelocityDataBlock instead of loading everything into memory
   ```csharp
   await connector.GetStorageDataAsync(velocityBlock, batchSize: 100000);
   ```

2. **Adjust Batch Size**: Tune the batch size based on available memory and row width
   - Small rows (few columns): Use larger batch sizes (50,000 - 100,000)
   - Wide rows (many columns): Use smaller batch sizes (10,000 - 25,000)

3. **Automatic Type Detection**: The connector automatically detects column types from CSV content, which may add slight overhead. If you need maximum speed, consider loading as strings and converting types manually.

4. **Remote File Caching**: When loading from URLs repeatedly, consider downloading once and caching locally:
   ```csharp
   // Download once
   if (!File.Exists("cache/data.csv"))
   {
       var webData = await DataBlock.Connector.LoadCsvAsync("https://example.com/data.csv");
       await File.WriteAllTextAsync("cache/data.csv", await webData.CsvStringSinkAsync());
   }

   // Use cached version
   var data = await DataBlock.Connector.LoadCsvAsync("cache/data.csv");
   ```

5. **Skip Empty Rows**: The connector automatically skips rows where all values are null or whitespace, improving data quality and reducing memory usage.

6. **Dispose DataBlocks**: For large CSV processing pipelines, dispose intermediate DataBlocks to free memory:
   ```csharp
   using (var rawData = await DataBlock.Connector.LoadCsvAsync("large_file.csv"))
   {
       var processed = rawData.Where(...).Select(...);
       // rawData automatically disposed here
   }
   ```

7. **Header Row Processing**: Setting `HeaderRow = true` (default) is more efficient than manually extracting headers, as the connector optimizes for this common case.

## License

This library is licensed under the **Datafication SDK License Agreement**. See the [LICENSE](./LICENSE) file for details.

**Summary:**
- **Free Use**: Organizations with fewer than 5 developers AND annual revenue under $500,000 USD may use the SDK without a commercial license
- **Commercial License Required**: Organizations with 5+ developers OR annual revenue exceeding $500,000 USD must obtain a commercial license
- **Open Source Exemption**: Open source projects meeting specific criteria may be exempt from developer count limits

For commercial licensing inquiries, contact [support@datafication.co](mailto:support@datafication.co).

---

**Datafication.CsvConnector** - Seamlessly connect CSV data to the Datafication ecosystem.

For more examples and documentation, visit our [samples directory](../../samples/).
