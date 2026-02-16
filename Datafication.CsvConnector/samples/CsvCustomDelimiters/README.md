# CsvCustomDelimiters Sample

Demonstrates how to load CSV files with custom delimiters using the `CsvConnectorConfiguration.Separator` property.

## Overview

This sample shows how to:
- Load semicolon-separated values (common in European locales)
- Load tab-separated values (TSV files)
- Load pipe-delimited files
- Configure the separator through `CsvConnectorConfiguration`

## Key Features Demonstrated

### Semicolon-Separated Values

```csharp
var config = new CsvConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.csv"),
    Separator = ';',
    HeaderRow = true
};

var connector = new CsvDataConnector(config);
var data = await connector.GetDataAsync();
```

### Tab-Separated Values (TSV)

```csharp
var config = new CsvConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.tsv"),
    Separator = '\t',
    HeaderRow = true
};

var connector = new CsvDataConnector(config);
var data = await connector.GetDataAsync();
```

### Pipe-Delimited Values

```csharp
var config = new CsvConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.csv"),
    Separator = '|',
    HeaderRow = true
};

var connector = new CsvDataConnector(config);
var data = await connector.GetDataAsync();
```

## Common Separators

| Separator | Character | Use Case |
|-----------|-----------|----------|
| Comma | `,` | Standard CSV (default) |
| Semicolon | `;` | European locales, Excel exports |
| Tab | `\t` | TSV files, database exports |
| Pipe | `\|` | Data with commas in values |

## How to Run

```bash
cd CsvCustomDelimiters
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.CsvConnector Custom Delimiters Sample ===

1. Loading semicolon-separated CSV (European format)...
   Loaded 30 products
   Columns: ProductId, Name, Category, Price, InStock, Supplier

2. Loading tab-separated values (TSV)...
   Loaded 40 log entries
   Columns: Timestamp, Level, Message, Source, Duration

3. Loading pipe-delimited values...
   Loaded 25 sensor readings
   Columns: SensorId, Location, Temperature, Humidity, Timestamp

=== Summary ===
   Semicolon-separated (;): 30 products loaded
   Tab-separated (\t):      40 log entries loaded
   Pipe-separated (|):      25 sensor readings loaded

=== Sample Complete ===
```

## Data Files

- **products_euro.csv** - Semicolon-separated product catalog (30 rows)
- **server_logs.tsv** - Tab-separated server log entries (40 rows)
- **sensor_data.csv** - Pipe-delimited IoT sensor readings (25 rows)

## Related Samples

- **CsvBasicLoad** - Loading standard comma-separated files
- **CsvNoHeaders** - Loading files without header rows
- **CsvFullConfiguration** - Using all configuration options together
