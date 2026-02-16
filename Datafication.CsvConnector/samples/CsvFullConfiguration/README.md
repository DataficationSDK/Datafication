# CsvFullConfiguration Sample

Demonstrates comprehensive use of all `CsvConnectorConfiguration` properties in various scenarios.

## Overview

This sample shows how to:
- Configure all available properties
- Create different configuration patterns
- Reuse configurations across multiple loads
- Access configuration from connector instances

## Key Features Demonstrated

### Complete Configuration

```csharp
var config = new CsvConnectorConfiguration
{
    // Required
    Source = new Uri("file:///path/to/data.csv"),

    // Optional (with defaults shown)
    Separator = ',',
    HeaderRow = true,
    Id = "my-connector",
    ErrorHandler = (ex) => Console.WriteLine($"Error: {ex.Message}")
};

var connector = new CsvDataConnector(config);
var data = await connector.GetDataAsync();
```

### Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Source` | Uri | Required | CSV file path or URL |
| `Separator` | char | `,` | Field delimiter character |
| `HeaderRow` | bool | `true` | First row contains headers |
| `Id` | string | Auto-GUID | Unique connector identifier |
| `ErrorHandler` | Action<Exception> | null | Error callback |

### Common Patterns

**Minimal Configuration:**
```csharp
var config = new CsvConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.csv")
};
```

**European Format:**
```csharp
var config = new CsvConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.csv"),
    Separator = ';'
};
```

**Headerless Data:**
```csharp
var config = new CsvConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.csv"),
    HeaderRow = false
};
```

**Production with Monitoring:**
```csharp
var config = new CsvConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.csv"),
    Id = $"prod-{DateTime.UtcNow:yyyyMMdd}",
    ErrorHandler = (ex) => Logger.Error(ex)
};
```

### Accessing Configuration

```csharp
var connector = new CsvDataConnector(config);

// Get connector ID
var id = connector.GetConnectorId();

// Access configuration
var source = connector.Configuration.Source;
var separator = connector.Configuration.Separator;
```

## How to Run

```bash
cd CsvFullConfiguration
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.CsvConnector Full Configuration Sample ===

1. Creating a fully configured CsvConnectorConfiguration...
   Configuration Properties:
   - Source: file:///path/to/employees.csv
   - Separator: ','
   - HeaderRow: True
   - Id: employees-connector-001
   - ErrorHandler: Configured

2. Creating connector and loading data...
   Connector ID: employees-connector-001
   Loaded 50 rows with 7 columns

3. Accessing configuration from connector instance...
   [Configuration details]

4. Common configuration patterns...
   [Pattern examples]

5. Reusing configuration for multiple loads...
   First load: 50 rows
   Second load: 50 rows

6. Sample data from full configuration load...
   [Data table]

=== Sample Complete ===
```

## Best Practices

1. **Always set Source** - It's the only required property
2. **Use custom Id** - Makes logging and debugging easier
3. **Configure ErrorHandler** - For production monitoring
4. **Reuse configurations** - Create once, use many times
5. **Match Separator to file format** - Avoid parsing errors

## Source URI Formats

```csharp
// Local file (absolute path)
Source = new Uri("file:///C:/data/file.csv")

// Local file (using Path.GetFullPath)
Source = new Uri(Path.GetFullPath("data/file.csv"))

// HTTP URL
Source = new Uri("https://example.com/data.csv")
```

## Related Samples

- **CsvBasicLoad** - Simple loading with shorthand API
- **CsvCustomDelimiters** - Different separator characters
- **CsvNoHeaders** - HeaderRow property usage
- **CsvErrorHandling** - ErrorHandler patterns
