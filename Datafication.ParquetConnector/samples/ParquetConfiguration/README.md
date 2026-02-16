# ParquetConfiguration Sample

Demonstrates all configuration options for the Datafication.ParquetConnector library.

## Overview

This sample shows how to:
- Create full configurations with all properties
- Use minimal configurations with defaults
- Set custom connector IDs for identification
- Configure error handlers for centralized error handling
- Reuse configurations across multiple connectors
- Use configurations with extension methods

## Key Features Demonstrated

### Full Configuration

```csharp
var config = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath),
    Id = "sales-data-2024",
    ErrorHandler = (ex) =>
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
};

var connector = new ParquetDataConnector(config);
var data = await connector.GetDataAsync();
```

### Minimal Configuration

```csharp
var config = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath)
};
// Id will be auto-generated, no error handler
```

### Custom Connector ID

```csharp
var config = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath),
    Id = "quarterly-sales-report"
};

var connector = new ParquetDataConnector(config);
Console.WriteLine(connector.GetConnectorId()); // "quarterly-sales-report"
```

### Using with Extension Methods

```csharp
var config = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + parquetPath),
    Id = "extension-load"
};

var data = await DataBlock.Connector.LoadParquetAsync(config);
```

## Configuration Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Source | Uri | Yes | Location of the Parquet file (file:// or http(s)://) |
| Id | string? | No | Unique identifier for the connector (auto-generated if not specified) |
| ErrorHandler | Action\<Exception\>? | No | Global exception handler callback |

## How to Run

```bash
cd ParquetConfiguration
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.ParquetConnector Configuration Sample ===

1. Creating full configuration with all options...
   Source: file:///path/to/SalesRecords50k.parquet
   Id: sales-data-2024
   ErrorHandler: Configured

2. Loading data with full configuration...
   Loaded 50000 rows
   Connector ID: sales-data-2024

3. Minimal configuration (defaults)...
   Source: file:///path/to/SalesRecords50k.parquet
   Id: (auto-generated)
   ErrorHandler: Not set

4. Configuration with custom Id...
   Connector ID: quarterly-sales-report

5. Using extension method with configuration...
   Loaded 50000 rows via extension method

6. Configuration reuse pattern...
   Connector 1 ID: reusable-config
   Connector 2 ID: reusable-config
   Same ID: True

7. Sample data from configured load:
   [Table showing sample data]

8. Configuration Properties Reference:
   [Table showing property reference]

=== Sample Complete ===
```

## Related Samples

- **ParquetBasicLoad** - Simple loading without configuration
- **ParquetErrorHandling** - Comprehensive error handling patterns
- **ParquetToVelocity** - Using configuration for streaming operations
