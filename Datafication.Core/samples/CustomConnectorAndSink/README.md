# CustomConnectorAndSink Sample

This sample demonstrates how to create custom connectors and sinks in Datafication.Core.

## Overview

The CustomConnectorAndSink sample shows how to:
- Implement the `IDataConnector` interface to create a custom connector
- Implement the `IDataSink<T>` interface to create a custom sink
- Use custom connectors and sinks in data pipelines

## Key Features Demonstrated

### Custom Connector

A custom connector implements the `IDataConnector` interface:

```csharp
public class CustomXmlConnector : IDataConnector
{
    public string GetConnectorId() { ... }
    public Task<DataBlock> GetDataAsync() { ... }
    public Task<IStorageDataBlock> GetStorageDataAsync(IStorageDataBlock target, int batchSize = 10000) { ... }
}
```

#### IDataConnector Interface

- `GetConnectorId()` - Returns a unique identifier for the connector
- `GetDataAsync()` - Retrieves data asynchronously and returns a DataBlock
- `GetStorageDataAsync()` - Retrieves data and appends to a storage DataBlock in batches

#### Example: XML Connector

The sample includes a `CustomXmlConnector` that:
- Loads data from XML files
- Parses XML structure
- Infers data types
- Returns a DataBlock

### Custom Sink

A custom sink implements the `IDataSink<T>` interface:

```csharp
public class CustomHtmlSink : IDataSink<string>
{
    public Task<string> Transform(DataBlock dataBlock) { ... }
}
```

#### IDataSink<T> Interface

- `Transform(DataBlock dataBlock)` - Transforms a DataBlock into type T

#### Example: HTML Sink

The sample includes a `CustomHtmlSink` that:
- Transforms a DataBlock into an HTML table
- Formats values appropriately
- Escapes HTML special characters

### Using Custom Connector and Sink

```csharp
// Use custom connector
var xmlConnector = new CustomXmlConnector("data.xml");
var data = await xmlConnector.GetDataAsync();

// Use custom sink
var htmlSink = new CustomHtmlSink();
var html = await htmlSink.Transform(data);

// Use in pipeline
var processed = await xmlConnector.GetDataAsync();
var result = processed
    .Compute("Revenue", "Price * Quantity")
    .Where("Revenue", 1000m, ComparisonOperator.GreaterThan);
var htmlOutput = await htmlSink.Transform(result);
```

## Implementation Details

### CustomXmlConnector

- Loads XML files using `XDocument`
- Infers column types from data
- Handles type conversion
- Returns a DataBlock with parsed data

### CustomHtmlSink

- Generates HTML table markup
- Formats numeric and date values
- Escapes HTML special characters
- Returns HTML as a string

## How to Run

```bash
cd CustomConnectorAndSink
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates:
- Loading data from XML using a custom connector
- Transforming data to HTML using a custom sink
- Using custom connector and sink in a data pipeline
- Saving HTML output to a file

## Related Samples

- **BasicOperations** - Learn basic DataBlock operations
- **ETLPipeline** - See connectors and sinks in a complete pipeline
- **ExpressionsAndComputedColumns** - Learn about computed columns used in the pipeline

## Extending This Sample

You can extend this sample by:
- Creating connectors for other formats (JSON, Excel, database, etc.)
- Creating sinks for other output formats (CSV, Markdown, PDF, etc.)
- Adding configuration options to connectors and sinks
- Implementing batch processing for large datasets

