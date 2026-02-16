# ExcelBasicLoad Sample

This sample demonstrates the fundamental operations for loading and working with Excel data using Datafication.ExcelConnector.

## Features Demonstrated

1. **Async Loading** - Load Excel files asynchronously using `LoadExcelAsync()`
2. **Sync Loading** - Load Excel files synchronously using `LoadExcel()`
3. **Schema Inspection** - Examine column names and data types
4. **Row Cursor** - Iterate through data row by row
5. **Where** - Filter data by column values
6. **Sort** - Order data by column
7. **Head** - Get first N rows
8. **Combined Operations** - Chain multiple operations together

## Data Used

- **Sheet**: Customers (200 rows)
- **Key Columns**: CustomerID, FirstName, LastName, Email, State, Segment, IsActive

## Running the Sample

```bash
cd samples/ExcelBasicLoad
dotnet run
```

## Expected Output

```
=== Datafication.ExcelConnector Basic Load Sample ===

1. Async Loading (LoadExcelAsync)
--------------------------------------------------
   Loaded sheet: First sheet (default)
   Total rows: 200
   Total columns: 7

2. Sync Loading (LoadExcel)
--------------------------------------------------
   Loaded rows: 200
   Loaded columns: 7

3. Schema Inspection
--------------------------------------------------
   Columns:
     - CustomerID (Double)
     - FirstName (String)
     - LastName (String)
     ...

...

=== Sample Complete ===
```

## Key Code Snippets

### Async Loading (Recommended)
```csharp
var customers = await DataBlock.Connector.LoadExcelAsync(excelPath);
```

### Sync Loading
```csharp
var customers = DataBlock.Connector.LoadExcel(excelPath);
```

### Chained Operations
```csharp
var result = customers
    .Where("Segment", "Enterprise")
    .Sort("LastName")
    .Head(5);
```
