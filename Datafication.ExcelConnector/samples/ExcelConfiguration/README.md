# ExcelConfiguration Sample

This sample demonstrates all configuration options available in `ExcelConnectorConfiguration` for fine-tuning how Excel data is loaded.

## Features Demonstrated

1. **UseColumns** - Load only specific columns
2. **NRows** - Limit the number of rows loaded
3. **Combined UseColumns + NRows** - Use both together for efficient loading
4. **SkipRows** - Skip rows at the beginning of the sheet
5. **HeaderRow** - Specify which row contains column headers
6. **HasHeader = false** - Handle sheets without header rows
7. **ExcelConnectorValidator** - Validate configurations before loading

## Configuration Options Reference

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| Source | Uri | - | File path or URL (required) |
| SheetName | string? | null | Sheet name to load |
| SheetIndex | int? | null | Sheet index (0-based) |
| HasHeader | bool | true | First row contains headers |
| HeaderRow | int | 0 | Offset after SkipRows for header row |
| SkipRows | int | 0 | Rows to skip before reading |
| UseColumns | string? | null | Comma-separated column names to load |
| NRows | int? | null | Maximum rows to read |
| ErrorHandler | Action<Exception>? | null | Error callback function |

## Data Used

- **Orders** (500 rows) - For UseColumns demo
- **WebEvents** (2,000 rows) - For NRows demo
- **Products** (50 rows) - For SkipRows/HeaderRow/HasHeader demos

## Running the Sample

```bash
cd samples/ExcelConfiguration
dotnet run
```

## Key Code Snippets

### UseColumns - Select Specific Columns
```csharp
var config = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Orders",
    HasHeader = true,
    UseColumns = "OrderID, CustomerID, Total"
};
var orders = await DataBlock.Connector.LoadExcelAsync(config);
```

### NRows - Limit Rows Loaded
```csharp
var config = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "WebEvents",
    HasHeader = true,
    NRows = 100
};
var events = await DataBlock.Connector.LoadExcelAsync(config);
```

### HasHeader = false (Auto-generated Column Names)
```csharp
var config = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",
    HasHeader = false  // Columns will be named Column1, Column2, etc.
};
var products = await DataBlock.Connector.LoadExcelAsync(config);
```

### Validate Configuration
```csharp
var validator = new ExcelConnectorValidator();
var result = validator.Validate(config);

if (!result.IsValid)
{
    Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");
}
```

## Use Cases

- **UseColumns**: When you only need a subset of columns, reducing memory usage
- **NRows**: For previewing large files or loading sample data
- **SkipRows**: When Excel files have metadata rows before the actual data
- **HasHeader = false**: For files without headers or when treating headers as data
- **Validator**: Pre-flight checks before loading to provide better error messages
