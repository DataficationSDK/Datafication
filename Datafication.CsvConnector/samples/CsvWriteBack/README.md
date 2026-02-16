# CsvWriteBack Sample

Demonstrates how to export DataBlocks back to CSV format using `CsvStringSink` and `CsvStringSinkAsync` extension methods.

## Overview

This sample shows how to:
- Convert DataBlocks to CSV strings
- Write transformed data to CSV files
- Perform complete ETL (Extract-Transform-Load) workflows
- Verify roundtrip data integrity

## Key Features Demonstrated

### Async CSV Export

```csharp
using Datafication.Sinks.Connectors.CsvConnector;

var data = await DataBlock.Connector.LoadCsvAsync("input.csv");
var csvString = await data.CsvStringSinkAsync();
await File.WriteAllTextAsync("output.csv", csvString);
```

### Sync CSV Export

```csharp
var csvString = data.CsvStringSink();
File.WriteAllText("output.csv", csvString);
```

### Complete ETL Pipeline

```csharp
// Extract
var rawData = await DataBlock.Connector.LoadCsvAsync("source.csv");

// Transform
var transformed = rawData
    .Where("Department", "Engineering")
    .Compute("Bonus", "Salary * 0.15")
    .Select("Name", "Salary", "Bonus");

// Load
var csv = await transformed.CsvStringSinkAsync();
await File.WriteAllTextAsync("output.csv", csv);
```

### Roundtrip Verification

```csharp
// Export
var csvOutput = await data.CsvStringSinkAsync();
await File.WriteAllTextAsync("exported.csv", csvOutput);

// Reload and verify
var reloaded = await DataBlock.Connector.LoadCsvAsync("exported.csv");
Console.WriteLine($"Roundtrip: {reloaded.RowCount} rows verified");
```

## How to Run

```bash
cd CsvWriteBack
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.CsvConnector Write Back Sample ===

1. Loading original CSV data...
   Loaded 50 employees

2. Transforming data...
   Filtered to Engineering: 18 employees
   Added computed columns: AnnualBonus, TotalCompensation

3. Converting to CSV string (async)...
   [Preview of CSV output]

4. Converting to CSV string (sync)...
   Generated [N] characters

5. Writing transformed data to file...
   Written to: /tmp/engineering_compensation.csv

6. Roundtrip verification (load the exported file)...
   Reloaded 18 rows with 6 columns

7. Complete ETL Pipeline example...
   ETL complete: Top 10 earners exported

=== Sample Complete ===
```

## Data Flow

```
employees.csv (50 rows)
       │
       ▼
  [Filter: Engineering]
       │
       ▼
  [Compute: Bonus columns]
       │
       ▼
  [Select: Specific columns]
       │
       ▼
engineering_compensation.csv (18 rows)
```

## Use Cases

- **Data Export**: Save analysis results to CSV for sharing
- **ETL Pipelines**: Extract, transform, and reload data
- **Data Backup**: Create CSV snapshots of processed data
- **Integration**: Export data for other systems
- **Reporting**: Generate CSV reports from transformed data

## Related Samples

- **CsvBasicLoad** - Loading CSV files
- **CsvFullConfiguration** - Complete configuration options
- **CsvToVelocity** - Streaming to high-performance storage
