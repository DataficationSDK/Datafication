# WebTableExtraction Sample

Demonstrates extracting HTML tables from web pages using the HtmlTableConnector.

## Overview

This sample shows how to:
- Extract HTML tables from web pages
- Use CSS selectors to target specific tables
- Enable table metadata (TableIndex, TableId, TableClass, RowIndex)
- Merge multiple tables into a single DataBlock
- Extract specific tables by index
- Handle tables with and without headers

## Key Features Demonstrated

### Basic Table Extraction

```csharp
var configuration = new HtmlTableConnectorConfiguration
{
    Source = new Uri("https://en.wikipedia.org/wiki/..."),
    TableSelector = "table.wikitable",
    FirstRowIsHeader = true,
    TrimCellValues = true
};

var connector = new HtmlTableConnector(configuration);
var data = await connector.GetDataAsync();
```

### Table Metadata

```csharp
var configuration = new HtmlTableConnectorConfiguration
{
    Source = url,
    IncludeTableMetadata = true  // Adds TableIndex, TableId, TableClass, RowIndex
};
```

### Merging Multiple Tables

```csharp
var configuration = new HtmlTableConnectorConfiguration
{
    Source = url,
    TableSelector = "table.wikitable",
    MergeTables = true,  // Combine all matching tables
    IncludeTableMetadata = true  // Track which table each row came from
};

var connector = new HtmlTableConnector(configuration);
var mergedData = await connector.GetDataAsync();

// Analyze by source table
var tableGroups = mergedData.GroupBy("TableIndex");
```

### Extracting Specific Table by Index

```csharp
var configuration = new HtmlTableConnectorConfiguration
{
    Source = url,
    TableSelector = "table.wikitable",
    TableIndex = 0  // Only extract the first matching table
};
```

## How to Run

```bash
cd WebTableExtraction
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.WebConnector Table Extraction Sample ===

Target URL: https://en.wikipedia.org/wiki/List_of_countries_by_population_(United_Nations)

1. Basic table extraction (first table only)...
   Extracted ~240 rows with 4 columns

2. Schema Information:
   - Rank: String
   - Country / Area: String
   - UN continental region: String
   - Population: String

3. First 10 rows of data:
   [Table showing top 10 countries by population]

4. Extracting with table metadata enabled...
   Schema now includes metadata columns:
   - TableIndex
   - TableId
   - TableClass
   - RowIndex
   - Rank

5. Merging multiple tables from page...
   Merged data: [total rows] total rows

6. Analyzing merged data by source table...
   Found data from N different tables:
   - Table 0: X rows
   - Table 1: Y rows

7. Extracting specific table by index (TableIndex = 0)...
   Table 0 contains X rows

8. Handling tables without headers...
   Generated column names:
   - Column_0
   - Column_1
   - Column_2
   - Column_3
   - Column_4

=== Sample Complete ===
```

## Target Website

This sample uses Wikipedia's "List of countries by population" page which provides multiple well-structured HTML tables with population data. Wikipedia tables are stable, publicly accessible, and demonstrate real-world table structures.

## Configuration Options

| Property | Default | Description |
|----------|---------|-------------|
| `TableSelector` | `"table"` | CSS selector to target tables |
| `TableIndex` | `null` | Extract specific table by index (0-based) |
| `FirstRowIsHeader` | `true` | Use first row as column headers |
| `UseTheadForHeaders` | `true` | Prefer `<thead>` for headers when available |
| `IncludeTableMetadata` | `true` | Add TableIndex, TableId, TableClass, RowIndex columns |
| `MergeTables` | `false` | Combine all matching tables |
| `SkipEmptyRows` | `true` | Skip rows with all empty cells |
| `TrimCellValues` | `true` | Trim whitespace from cell values |

## Related Samples

- **WebProductScraping** - CSS selector-based extraction
- **WebMetadataExtraction** - Page metadata extraction
- **WebFactoryPattern** - Using factory methods for quick connector creation
