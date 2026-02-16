# AdoBasicQuery Sample

Demonstrates the simplest patterns for loading data from a database using the Datafication.AdoConnector library.

## Overview

This sample shows how to:
- Register an ADO.NET provider (SQLite)
- Load data using the shorthand `LoadSqliteAsync()` method
- Load data using the synchronous `LoadSqlite()` method
- Inspect the schema and data types of loaded data
- Display data using row cursors
- Perform basic filtering and sorting operations

## Key Features Demonstrated

### Provider Registration

```csharp
DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);
```

### Asynchronous Loading (Recommended)

```csharp
var data = await DataBlock.Connector.LoadSqliteAsync(
    "Data Source=mydb.db",
    "SELECT * FROM Employees"
);
Console.WriteLine($"Loaded {data.RowCount} rows");
```

### Synchronous Loading

```csharp
var data = DataBlock.Connector.LoadSqlite(
    "Data Source=mydb.db",
    "SELECT Id, Name FROM Employees"
);
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
cd AdoBasicQuery
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.AdoConnector Basic Query Sample ===

Setting up SQLite database with sample data...

1. Loading data asynchronously...
   Loaded 10 rows with 6 columns

2. Schema Information:
   - Id: Int64
   - Name: String
   - Department: String
   - Salary: Double
   - HireDate: String
   - IsActive: Int64

3. Loading data synchronously...
   Loaded 10 rows

4. First 5 employees:
   ---------------------------------------------------------------------------
   Id    Name                 Department      Salary       Active
   ---------------------------------------------------------------------------
   1     Alice Johnson        Engineering     $95,000      1
   2     Bob Smith            Sales           $75,000      1
   3     Carol Williams       Engineering     $105,000     1
   4     David Brown          Marketing       $68,000      1
   5     Eve Davis            Engineering     $88,000      1
   ---------------------------------------------------------------------------
   ... and 5 more rows

5. Filtering: Engineering department employees...
   Found 4 engineers

6. Sorting: Top 3 highest salaries...
   ---------------------------------------------
   Name                      Salary
   ---------------------------------------------
   Henry Taylor              $115,000
   Carol Williams            $105,000
   Alice Johnson             $95,000
   ---------------------------------------------

=== Sample Complete ===
```

## Database Schema

This sample creates an in-memory SQLite database with the following table:

**Employees**
| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Name | TEXT | Employee name |
| Department | TEXT | Department name |
| Salary | REAL | Annual salary |
| HireDate | TEXT | Date hired (ISO format) |
| IsActive | INTEGER | 1 = active, 0 = inactive |

## Related Samples

- **AdoParameterizedQueries** - Using parameters in SQL queries for security
- **AdoDataManipulation** - Advanced filtering, sorting, and aggregation
- **AdoFullConfiguration** - All configuration options
