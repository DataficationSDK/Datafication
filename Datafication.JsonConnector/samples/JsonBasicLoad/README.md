# JsonBasicLoad Sample

Demonstrates the simplest patterns for loading JSON files using the Datafication.JsonConnector library.

## Overview

This sample shows how to:
- Load JSON files using the shorthand `LoadJsonAsync()` method
- Load JSON files using the synchronous `LoadJson()` method
- Inspect the schema and data types of loaded data
- Display data using row cursors
- Perform basic filtering and sorting operations

## Key Features Demonstrated

### Asynchronous Loading (Recommended)

```csharp
var data = await DataBlock.Connector.LoadJsonAsync("path/to/file.json");
Console.WriteLine($"Loaded {data.RowCount} rows");
```

### Synchronous Loading

```csharp
var data = DataBlock.Connector.LoadJson("path/to/file.json");
```

### Schema Inspection

```csharp
foreach (var colName in data.Schema.GetColumnNames())
{
    var column = data.GetColumn(colName);
    Console.WriteLine($"{colName}: {column.DataType.GetClrType().Name}");
}
```

### Row Cursor Iteration

```csharp
var cursor = data.GetRowCursor("Name", "Department", "Salary");
while (cursor.MoveNext())
{
    var name = cursor.GetValue("Name");
    var dept = cursor.GetValue("Department");
    var salary = cursor.GetValue("Salary");
}
```

## How to Run

```bash
cd JsonBasicLoad
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.JsonConnector Basic Load Sample ===

1. Loading JSON asynchronously...
   Loaded 50 rows with 7 columns

2. Schema Information:
   - Id: Int64
   - Name: String
   - Department: String
   - Salary: Double
   - StartDate: String
   - IsActive: Boolean
   - Email: String

3. Loading JSON synchronously...
   Loaded 50 rows

4. First 10 employees:
   [Table showing employee data]

5. Filtering: Engineering department employees...
   Found 18 engineers

6. Sorting: Top 5 highest salaries...
   [Table showing top earners]

=== Sample Complete ===
```

## Data File

This sample uses `data/employees.json` which contains 50 employee records with the following columns:
- Id (integer)
- Name (string)
- Department (string)
- Salary (decimal)
- StartDate (date string)
- IsActive (boolean)
- Email (string)

## Related Samples

- **JsonConfiguration** - Advanced configuration options for JSON loading
- **JsonExport** - Exporting DataBlocks back to JSON format
- **JsonRemoteApi** - Loading JSON from HTTP/HTTPS URLs
