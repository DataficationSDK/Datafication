# Datafication.JsonConnector

[![NuGet](https://img.shields.io/nuget/v/Datafication.JsonConnector.svg)](https://www.nuget.org/packages/Datafication.JsonConnector)

A high-performance JSON file connector for .NET that provides seamless integration between JSON data sources and the Datafication.Core DataBlock API.

## Description

Datafication.JsonConnector is a specialized connector library that bridges JSON files, APIs, and the Datafication.Core ecosystem. It provides robust JSON parsing with automatic schema inference, support for local and remote files via HTTP/HTTPS, nested structure handling, and both in-memory and streaming batch operations. The connector handles JSON objects, arrays, and complex nested structures while maintaining high performance and ease of use.

### Key Features

- **Multiple Source Types**: Load JSON from local files, relative paths, or remote URLs (HTTP/HTTPS)
- **Automatic Schema Inference**: Intelligently infers column data types from JSON content
- **Nested Structure Support**: Handles complex nested JSON objects and arrays
- **Streaming Support**: Efficient batch loading for large JSON files with `GetStorageDataAsync`
- **JSON Export**: Convert DataBlocks back to JSON format with `JsonStringSink`
- **Async Operations**: Full async/await support for non-blocking I/O
- **Error Handling**: Global error handler configuration for graceful exception management
- **Validation**: Built-in configuration validation ensures correct setup before processing
- **Type Conversion**: Automatic conversion of JSON types to appropriate .NET types
- **Cross-Platform**: Works on Windows, Linux, and macOS

## Table of Contents

- [Description](#description)
  - [Key Features](#key-features)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
  - [Loading JSON Files (Shorthand)](#loading-json-files-shorthand)
  - [Loading JSON with Configuration](#loading-json-with-configuration)
  - [Loading from Remote URLs](#loading-from-remote-urls)
  - [Streaming Large JSON Files to Storage](#streaming-large-json-files-to-storage)
  - [Writing DataBlocks to JSON](#writing-datablocks-to-json)
  - [Error Handling](#error-handling)
  - [Working with JSON Data](#working-with-json-data)
- [Configuration Reference](#configuration-reference)
  - [JsonConnectorConfiguration](#jsonconnectorconfiguration)
- [API Reference](#api-reference)
  - [Core Classes](#core-classes)
  - [Extension Methods](#extension-methods)
- [Common Patterns](#common-patterns)
  - [Loading JSON API Data for Analysis](#loading-json-api-data-for-analysis)
  - [ETL Pipeline with JSON](#etl-pipeline-with-json)
  - [JSON to VelocityDataBlock](#json-to-velocitydatablock)
- [Performance Tips](#performance-tips)
- [License](#license)

## Installation

> **Note**: Datafication.JsonConnector is currently in pre-release. The packages are now available on nuget.org.

```bash
dotnet add package Datafication.JsonConnector
```

**Running the Samples:**

```bash
cd samples/JsonBasicLoad
dotnet run
```

## Usage Examples

### Loading JSON Files (Shorthand)

The simplest way to load a JSON file is using the shorthand extension methods:

```csharp
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.JsonConnector;

// Load JSON from local file (async)
var employees = await DataBlock.Connector.LoadJsonAsync("data/employees.json");

Console.WriteLine($"Loaded {employees.RowCount} employees");

// Synchronous version
var departments = DataBlock.Connector.LoadJson("data/departments.json");

// Load from remote URL
var apiData = await DataBlock.Connector.LoadJsonAsync("https://api.example.com/users");
```

### Loading JSON with Configuration

For more control over JSON loading, use the full configuration:

```csharp
using Datafication.Connectors.JsonConnector;

// Create configuration with custom settings
var configuration = new JsonConnectorConfiguration
{
    Id = "employees-json",
    Source = new Uri("file:///data/employees.json"),
    ErrorHandler = (ex) => Console.WriteLine($"Error: {ex.Message}")
};

// Create connector and load data
var connector = new JsonDataConnector(configuration);
var data = await connector.GetDataAsync();

Console.WriteLine($"Loaded {data.RowCount} rows with {data.Schema.Count} columns");
```

### Loading from Remote URLs

Load JSON data directly from HTTP/HTTPS URLs:

```csharp
// Configure remote URL source
var configuration = new JsonConnectorConfiguration
{
    Source = new Uri("https://api.example.com/users")
};

var connector = new JsonDataConnector(configuration);
var webData = await connector.GetDataAsync();

Console.WriteLine($"Downloaded and loaded {webData.RowCount} rows");
```

### Streaming Large JSON Files to Storage

For large JSON files, stream data directly to VelocityDataBlock in batches:

```csharp
using Datafication.Storage.Velocity;
using Datafication.Connectors.JsonConnector;

// Create VelocityDataBlock for efficient large-scale storage
using var velocityBlock = new VelocityDataBlock("data/large_dataset.dfc");

// Configure JSON source
var configuration = new JsonConnectorConfiguration
{
    Source = new Uri("file:///data/large_records.json")
};

// Stream JSON data in batches of 10,000 rows
var connector = new JsonDataConnector(configuration);
await connector.GetStorageDataAsync(velocityBlock, batchSize: 10000);

Console.WriteLine($"Streamed {velocityBlock.RowCount} rows to storage");
await velocityBlock.FlushAsync();
```

### Writing DataBlocks to JSON

Convert DataBlocks back to JSON format:

```csharp
using Datafication.Core.Data;
using Datafication.Sinks.Connectors.JsonConnector;

// Create or load a DataBlock
var data = new DataBlock();
data.AddColumn(new DataColumn("Name", typeof(string)));
data.AddColumn(new DataColumn("Age", typeof(int)));
data.AddColumn(new DataColumn("Salary", typeof(decimal)));

data.AddRow(new object[] { "Alice", 30, 75000m });
data.AddRow(new object[] { "Bob", 25, 65000m });
data.AddRow(new object[] { "Carol", 35, 85000m });

// Convert to JSON string (async shorthand)
var jsonString = await data.JsonStringSinkAsync();
Console.WriteLine(jsonString);

// Synchronous version
var jsonOutput = data.JsonStringSink();

// Write to file
await File.WriteAllTextAsync("output/employees.json", jsonString);
```

Output for single record:
```json
{
  "Name": "Alice",
  "Age": 30,
  "Salary": 75000
}
```

Output for multiple records:
```json
[
  {
    "Name": "Alice",
    "Age": 30,
    "Salary": 75000
  },
  {
    "Name": "Bob",
    "Age": 25,
    "Salary": 65000
  }
]
```

### Error Handling

Configure global error handling for JSON operations:

```csharp
var configuration = new JsonConnectorConfiguration
{
    Source = new Uri("file:///data/employees.json"),
    ErrorHandler = (exception) =>
    {
        Console.WriteLine($"JSON Error: {exception.Message}");
        // Log to file, send alert, etc.
    }
};

var connector = new JsonDataConnector(configuration);

try
{
    var data = await connector.GetDataAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load JSON: {ex.Message}");
}
```

### Working with JSON Data

Once loaded, use the full DataBlock API for data manipulation:

```csharp
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.JsonConnector;
using Datafication.Sinks.Connectors.JsonConnector;

// Load JSON file
var sales = await DataBlock.Connector.LoadJsonAsync("data/sales.json");

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

// Export results to JSON
var resultJson = await result.JsonStringSinkAsync();
await File.WriteAllTextAsync("output/top_products.json", resultJson);
```

## Configuration Reference

### JsonConnectorConfiguration

Configuration class for JSON data sources.

**Properties:**

- **`Source`** (Uri, required): Location of the JSON data source
  - File path: `new Uri("file:///C:/data/file.json")`
  - HTTP/HTTPS URL: `new Uri("https://api.example.com/data")`
  - Relative path: Resolved from `AppDomain.CurrentDomain.BaseDirectory`

- **`Id`** (string, auto-generated): Unique identifier for the configuration
  - Automatically generated as GUID if not specified
  - Can be set explicitly for tracking purposes

- **`ErrorHandler`** (Action<Exception>?, optional): Global exception handler
  - Provides centralized error handling for JSON operations
  - Invoked when exceptions occur during data retrieval

**Example:**

```csharp
var config = new JsonConnectorConfiguration
{
    Source = new Uri("file:///data/employees.json"),
    Id = "employees-connector",
    ErrorHandler = ex => Console.WriteLine($"Error: {ex.Message}")
};
```

## API Reference

For complete API documentation, see the [Datafication.Connectors.JsonConnector API Reference](https://datafication.co/help/api/reference/Datafication.Connectors.JsonConnector.html).

### Core Classes

**JsonDataConnector**
- **Constructor**
  - `JsonDataConnector(JsonConnectorConfiguration configuration)` - Creates connector with validation
  - Throws `ArgumentNullException` if configuration is null
  - Throws `ArgumentException` if configuration validation fails
- **Methods**
  - `Task<DataBlock> GetDataAsync()` - Loads entire JSON into memory as DataBlock
    - Supports JSON objects `{}` and arrays `[]`
    - Throws `InvalidOperationException` for unsupported JSON root types
  - `Task<IStorageDataBlock> GetStorageDataAsync(IStorageDataBlock target, int batchSize = 10000)` - Streams JSON data in batches
    - Performs schema inference from first batch
    - Converts complex nested structures to strings in batch mode
  - `string GetConnectorId()` - Returns unique connector identifier
- **Properties**
  - `JsonConnectorConfiguration Configuration` - Current configuration

**JsonConnectorConfiguration**
- **Properties**
  - `Uri Source` - JSON source location (file or HTTP URL)
  - `string Id` - Unique identifier (auto-generated if not set)
  - `Action<Exception>? ErrorHandler` - Error handler delegate

**JsonStringSink**
- Implements `IDataSink<string>`
- **Methods**
  - `Task<string> Transform(DataBlock dataBlock)` - Converts DataBlock to formatted JSON string
    - Single record produces JSON object `{...}`
    - Multiple records produce JSON array `[{...}, {...}]`
    - Nested DataBlocks converted to nested JSON objects
    - Array-indexed columns (e.g., "items[0]") converted to JSON arrays

**JsonConnectorValidator**
- Validates `JsonConnectorConfiguration` instances
- **Methods**
  - `ValidationResult Validate(IDataConnectorConfiguration configuration)` - Validates configuration
  - **Validation Checks:**
    - Configuration is not null and correct type
    - `Id` is not null or empty
    - `Source` is not null
    - For file URIs: file exists
    - URL schemes must be http, https, or file

**JsonDataProvider**
- Factory class implementing `IDataConnectorFactory`
- **Methods**
  - `IDataConnector CreateDataConnector(IDataConnectorConfiguration configuration)` - Creates JsonDataConnector instances

### Extension Methods

**JsonConnectorExtensions** (namespace: `Datafication.Extensions.Connectors.JsonConnector`)

```csharp
// Async shorthand methods
Task<DataBlock> LoadJsonAsync(this ConnectorExtensions ext, string source)
Task<DataBlock> LoadJsonAsync(this ConnectorExtensions ext, JsonConnectorConfiguration config)

// Synchronous shorthand methods
DataBlock LoadJson(this ConnectorExtensions ext, string source)
DataBlock LoadJson(this ConnectorExtensions ext, JsonConnectorConfiguration config)
```

**JsonStringSinkExtension** (namespace: `Datafication.Sinks.Connectors.JsonConnector`)

```csharp
// Convert DataBlock to JSON
Task<string> JsonStringSinkAsync(this DataBlock dataBlock)
string JsonStringSink(this DataBlock dataBlock)
```

## Common Patterns

### Loading JSON API Data for Analysis

```csharp
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.JsonConnector;

// Load data from a REST API
var issues = await DataBlock.Connector.LoadJsonAsync(
    "https://api.github.com/repos/microsoft/dotnet/issues"
);

// Analyze issue data
var openIssues = issues
    .Where("state", "open")
    .Select("title", "created_at", "user.login")
    .Sort(SortDirection.Descending, "created_at");

Console.WriteLine($"Open Issues: {openIssues.RowCount}");
Console.WriteLine(await openIssues.TextTableAsync());
```

### ETL Pipeline with JSON

```csharp
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.JsonConnector;
using Datafication.Sinks.Connectors.JsonConnector;

// Extract: Load JSON from API
var rawData = await DataBlock.Connector.LoadJsonAsync("https://api.example.com/sales");

// Transform: Clean and process
var transformed = rawData
    .DropNulls(DropNullMode.Any)
    .Where("Status", "Cancelled", ComparisonOperator.NotEquals)
    .Compute("NetRevenue", "Revenue - Discount")
    .Compute("ProfitMargin", "NetRevenue / Revenue")
    .Select("OrderId", "ProductName", "NetRevenue", "ProfitMargin");

// Load: Export to JSON
var outputJson = await transformed.JsonStringSinkAsync();
await File.WriteAllTextAsync("output/processed_sales.json", outputJson);

Console.WriteLine($"Processed {transformed.RowCount} orders");
```

### JSON to VelocityDataBlock

```csharp
using Datafication.Storage.Velocity;
using Datafication.Connectors.JsonConnector;

// Load JSON configuration
var jsonConfig = new JsonConnectorConfiguration
{
    Source = new Uri("file:///data/large_events.json")
};

// Create VelocityDataBlock with high-throughput options
using var velocityBlock = new VelocityDataBlock(
    "data/events.dfc",
    VelocityOptions.CreateHighThroughput()
);

// Stream JSON data directly to VelocityDataBlock
var connector = new JsonDataConnector(jsonConfig);
await connector.GetStorageDataAsync(velocityBlock, batchSize: 50000);
await velocityBlock.FlushAsync();

Console.WriteLine($"Loaded {velocityBlock.RowCount} rows into VelocityDataBlock");

// Now query efficiently with SIMD acceleration
var recentEvents = velocityBlock
    .Where("EventType", "Login")
    .GroupByAggregate("UserId", "EventId", AggregationType.Count, "LoginCount")
    .Execute();
```

## Performance Tips

1. **Use Streaming for Large Files**: For JSON files with millions of records, use `GetStorageDataAsync` to stream data directly to VelocityDataBlock instead of loading everything into memory:
   ```csharp
   await connector.GetStorageDataAsync(velocityBlock, batchSize: 100000);
   ```

2. **Adjust Batch Size**: Tune the batch size based on available memory and JSON structure complexity:
   - Simple flat objects: Use larger batch sizes (50,000 - 100,000)
   - Complex nested structures: Use smaller batch sizes (5,000 - 25,000)

3. **Automatic Schema Inference**: The connector automatically detects column types from JSON content, which may add slight overhead. Schema is inferred from the first batch in streaming mode.

4. **Remote JSON Caching**: When loading from URLs repeatedly, consider downloading once and caching locally:
   ```csharp
   // Download once
   if (!File.Exists("cache/data.json"))
   {
       var webData = await DataBlock.Connector.LoadJsonAsync("https://api.example.com/data");
       await File.WriteAllTextAsync("cache/data.json", await webData.JsonStringSinkAsync());
   }

   // Use cached version
   var data = await DataBlock.Connector.LoadJsonAsync("cache/data.json");
   ```

5. **Nested Structure Handling**: Complex nested JSON objects and arrays are automatically converted to string representations in batch mode. For deep nested analysis, consider flattening the structure before processing.

6. **Error Handler Usage**: Configure the `ErrorHandler` property for production scenarios to capture and log parsing errors without crashing the entire load process.

7. **Memory Management**: For large JSON processing pipelines, dispose intermediate DataBlocks to free memory:
   ```csharp
   using (var rawData = await connector.GetDataAsync())
   {
       var processed = rawData.Where(...).Select(...);
       // rawData automatically disposed here
   }
   ```

## License

This library is licensed under the **Datafication SDK License Agreement**. See the [LICENSE](./LICENSE) file for details.

**Summary:**
- **Free Use**: Organizations with fewer than 5 developers AND annual revenue under $500,000 USD may use the SDK without a commercial license
- **Commercial License Required**: Organizations with 5+ developers OR annual revenue exceeding $500,000 USD must obtain a commercial license
- **Open Source Exemption**: Open source projects meeting specific criteria may be exempt from developer count limits

For commercial licensing inquiries, contact [support@datafication.co](mailto:support@datafication.co).

---

**Datafication.JsonConnector** - Seamlessly connect JSON data to the Datafication ecosystem.

## Samples

The following samples are available in the [samples directory](./samples/):

| Sample | Description |
|--------|-------------|
| [JsonBasicLoad](./samples/JsonBasicLoad/) | Basic JSON loading, schema inspection, filtering, sorting |
| [JsonRemoteApi](./samples/JsonRemoteApi/) | Loading JSON from HTTP/HTTPS URLs (REST APIs) |
| [JsonExport](./samples/JsonExport/) | Converting DataBlocks back to JSON using JsonStringSink |
| [JsonConfiguration](./samples/JsonConfiguration/) | Complete JsonConnectorConfiguration usage |
| [JsonErrorHandling](./samples/JsonErrorHandling/) | Error handling patterns and graceful degradation |
| [JsonToVelocity](./samples/JsonToVelocity/) | Saving JSON to VelocityDataBlock for high-performance storage |
| [JsonETLPipeline](./samples/JsonETLPipeline/) | Full ETL: Extract JSON, Transform, Load to JSON |
