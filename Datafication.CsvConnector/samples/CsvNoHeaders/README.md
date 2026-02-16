# CsvNoHeaders Sample

Demonstrates how to load CSV files that don't have a header row using the `CsvConnectorConfiguration.HeaderRow` property.

## Overview

This sample shows how to:
- Load CSV files without header rows
- Work with auto-generated column names (Column_1, Column_2, etc.)
- Perform operations using auto-generated column names
- Understand the difference between `HeaderRow=true` and `HeaderRow=false`

## Key Features Demonstrated

### Loading Without Headers

```csharp
var config = new CsvConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.csv"),
    HeaderRow = false  // First row is data, not headers
};

var connector = new CsvDataConnector(config);
var data = await connector.GetDataAsync();
```

### Auto-Generated Column Names

When `HeaderRow=false`, columns are automatically named:
- Column_1
- Column_2
- Column_3
- etc.

### Working with Auto-Generated Names

```csharp
// Access data using auto-generated names
var cursor = data.GetRowCursor("Column_1", "Column_2");
while (cursor.MoveNext())
{
    var value1 = cursor.GetValue("Column_1");
    var value2 = cursor.GetValue("Column_2");
}

// Filter using auto-generated names
var filtered = data.Where("Column_4", true);

// Compute using auto-generated names
var computed = data.Compute("Result", "Column_2 * Column_3");
```

## How to Run

```bash
cd CsvNoHeaders
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.CsvConnector No Headers Sample ===

1. Loading CSV without header row...
   Loaded 20 rows with 4 columns
   Auto-generated column names:
   - Column_1: Int32
   - Column_2: Int32
   - Column_3: Decimal
   - Column_4: Boolean

2. First 5 rows with auto-generated column names:
   [Table showing data]

3. Filtering and calculations with auto-generated names...
   Rows where Column_4 is true: 12
   Added computed column 'Calculated' = Column_2 * Column_3

4. Comparison: Loading same file WITH HeaderRow=true...
   Row count: 19 (one less - first row became headers!)
   Column names (from first data row - incorrect!)

=== Sample Complete ===
```

## Data File

**raw_numbers.csv** - A CSV file without headers containing:
- 20 rows of numeric data
- 4 columns: integer, integer, decimal, boolean
- No header row (pure data)

## When to Use HeaderRow=false

- Legacy data files without column headers
- Machine-generated data exports
- Fixed-format data files
- When column positions are known but names aren't provided

## Related Samples

- **CsvBasicLoad** - Loading standard CSV files with headers
- **CsvCustomDelimiters** - Using different separators
- **CsvFullConfiguration** - All configuration options together
