# CsvBasicLoad Sample

Demonstrates the simplest patterns for loading CSV files using the Datafication.CsvConnector library.

## Overview

This sample shows how to:
- Load CSV files using the shorthand `LoadCsvAsync()` method
- Load CSV files using the synchronous `LoadCsv()` method
- Inspect the schema and data types of loaded data
- Display data using row cursors
- Perform basic filtering and sorting operations

## Key Features Demonstrated

### Asynchronous Loading (Recommended)

```csharp
var data = await DataBlock.Connector.LoadCsvAsync("path/to/file.csv");
Console.WriteLine($"Loaded {data.RowCount} rows");
```

### Synchronous Loading

```csharp
var data = DataBlock.Connector.LoadCsv("path/to/file.csv");
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
cd CsvBasicLoad
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.CsvConnector Basic Load Sample ===

1. Loading CSV asynchronously...
   Loaded 50 rows with 7 columns

2. Schema Information:
   - Id: Int32
   - Name: String
   - Department: String
   - Salary: Decimal
   - StartDate: DateTime
   - IsActive: Boolean
   - Email: String

3. Loading CSV synchronously...
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

This sample uses `data/employees.csv` which contains 50 employee records with the following columns:
- Id (integer)
- Name (string)
- Department (string)
- Salary (decimal)
- StartDate (date)
- IsActive (boolean)
- Email (string)

## Related Samples

- **CsvCustomDelimiters** - Loading CSV files with different separators
- **CsvNoHeaders** - Loading CSV files without header rows
- **CsvWriteBack** - Exporting DataBlocks back to CSV format
