# JsonConfiguration Sample

Demonstrates complete usage of `JsonConnectorConfiguration` and `JsonDataConnector` for fine-grained control over JSON data loading.

## Overview

This sample shows how to:
- Create minimal configurations with just a source
- Configure custom connector IDs for tracking
- Set up error handlers for exception logging
- Build production-ready configurations
- Use configurations with extension methods
- Reuse configurations for multiple data sources

## Key Features Demonstrated

### Minimal Configuration

```csharp
var config = new JsonConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.json")
};
var connector = new JsonDataConnector(config);
var data = await connector.GetDataAsync();
```

### Configuration with Custom ID

```csharp
var config = new JsonConnectorConfiguration
{
    Id = "my-custom-connector-id",
    Source = new Uri("file:///path/to/data.json")
};
```

### Configuration with Error Handler

```csharp
var config = new JsonConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.json"),
    ErrorHandler = (ex) =>
    {
        Console.WriteLine($"Error: {ex.Message}");
        // Log to file, monitoring system, etc.
    }
};
```

### Using Configuration with Extension Methods

```csharp
var config = new JsonConnectorConfiguration
{
    Id = "configured-load",
    Source = new Uri("file:///path/to/data.json"),
    ErrorHandler = (ex) => LogError(ex)
};

var data = await DataBlock.Connector.LoadJsonAsync(config);
```

## Configuration Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Unique identifier for the connector instance. Auto-generated GUID if not specified. |
| `Source` | `Uri` | The URI to the JSON data source. Supports `file://` and `http://`/`https://` schemes. |
| `ErrorHandler` | `Action<Exception>` | Optional callback invoked when an exception occurs during data loading. |

## How to Run

```bash
cd JsonConfiguration
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.JsonConnector Configuration Sample ===

1. Minimal Configuration (Source only)...
   Config ID: [auto-generated-guid]
   Source: file:///path/to/employees.json
   Loaded: 50 rows

2. Configuration with Custom ID...
   Config ID: employee-data-source
   Connector ID: employee-data-source

3. Configuration with Error Handler...
   Loaded: 50 rows
   Errors logged: 0

4. Production-Ready Configuration...
   ID: json-connector-20240115-143022
   Source: file:///path/to/employees.json
   ErrorHandler: Configured

5. Using Configuration with Extension Methods...
   Loaded via extension: 50 rows

6. Reusing Configuration for Multiple Loads...
   First load: 50 rows
   Second load (products): 30 rows

7. JsonConnectorConfiguration Properties Summary:
   [Property table]

=== Sample Complete ===
```

## Data Files

This sample uses:
- `data/employees.json` - 50 employee records
- `data/products.json` - 30 product records

## Related Samples

- **JsonBasicLoad** - Simple loading without explicit configuration
- **JsonErrorHandling** - Advanced error handling patterns
- **JsonRemoteApi** - Loading from HTTP endpoints with configuration
