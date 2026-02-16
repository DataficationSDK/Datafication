# ExcelSheetSelection Sample

This sample demonstrates the various methods for selecting which sheet to load from an Excel workbook.

## Features Demonstrated

1. **Default Behavior** - Loads the first sheet when no sheet is specified
2. **Sheet by Name** - Select a specific sheet using `SheetName`
3. **Sheet by Index** - Select a sheet using zero-based `SheetIndex`
4. **Precedence Rules** - SheetName takes precedence when both are specified
5. **Multi-Sheet Loading** - Load data from multiple sheets into separate DataBlocks

## Data Used

| Sheet Index | Sheet Name | Rows |
|-------------|------------|------|
| 0 | Customers | 200 |
| 1 | Products | 50 |
| 2 | Orders | 500 |
| 3 | OrderItems | 1,497 |
| 4 | WebEvents | 2,000 |
| 5 | DataDictionary | 7 |
| 6 | KPIs | 4 |

## Running the Sample

```bash
cd samples/ExcelSheetSelection
dotnet run
```

## Expected Output

```
=== Datafication.ExcelConnector Sheet Selection Sample ===

1. Default Behavior (First Sheet)
--------------------------------------------------
   Loaded first sheet by default
   Rows: 200
   First column: CustomerID

2. Sheet Selection by Name
--------------------------------------------------
   Sheet: Customers
   Rows: 200
   Columns: CustomerID, FirstName, LastName, Email, State, Segment, IsActive

   Sheet: Products
   Rows: 50
   Columns: ProductID, ProductName, Category, Brand, UnitCost, ListPrice

...

=== Sample Complete ===
```

## Key Code Snippets

### Select Sheet by Name
```csharp
var config = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",
    HasHeader = true
};
var products = await DataBlock.Connector.LoadExcelAsync(config);
```

### Select Sheet by Index
```csharp
var config = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetIndex = 2,  // Zero-based index
    HasHeader = true
};
var orders = await DataBlock.Connector.LoadExcelAsync(config);
```

### Multi-Sheet Loading
```csharp
var sheets = new[] { "Customers", "Products", "Orders" };
var data = new Dictionary<string, DataBlock>();

foreach (var sheet in sheets)
{
    var config = new ExcelConnectorConfiguration
    {
        Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
        SheetName = sheet,
        HasHeader = true
    };
    data[sheet] = await DataBlock.Connector.LoadExcelAsync(config);
}
```

## Important Notes

- When neither `SheetName` nor `SheetIndex` is specified, the first sheet (index 0) is loaded
- `SheetName` takes precedence over `SheetIndex` when both are specified
- Sheet indices are zero-based (0, 1, 2, ...)
- An exception is thrown if the specified sheet does not exist
