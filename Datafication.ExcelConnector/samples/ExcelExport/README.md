# ExcelExport Sample

This sample demonstrates how to export DataBlocks to Excel files using the `ExcelSink` and `ExcelSinkAsync` extension methods.

## Features Demonstrated

1. **Simple Export (Async)** - Export a DataBlock to Excel using `ExcelSinkAsync()`
2. **Sync Export** - Export using synchronous `ExcelSink()` method
3. **Transform Then Export** - Apply Where, Select, Sort operations before exporting
4. **Aggregated Data Export** - Export grouped/aggregated results
5. **Sorted and Filtered Export** - Export filtered and sorted subsets
6. **Roundtrip Verification** - Load, export, reload and verify data integrity
7. **Multiple Exports** - Create multiple report files from one data source

## Running the Sample

```bash
cd samples/ExcelExport
dotnet run
```

## Output Files

The sample creates an `output` directory with the following Excel files:

| File | Description |
|------|-------------|
| products_export.xlsx | All products (direct export) |
| customers_export.xlsx | All customers (sync export) |
| enterprise_customers.xlsx | Enterprise segment customers only |
| customers_by_segment.xlsx | Customer count by segment (aggregated) |
| premium_electronics.xlsx | Top 10 electronics by price |
| roundtrip_test.xlsx | Verification test file |
| all_orders.xlsx | All orders |
| shipped_orders.xlsx | Shipped orders only |
| pending_orders.xlsx | Pending orders only |
| cancelled_orders.xlsx | Cancelled orders only |

## Key Code Snippets

### Simple Export
```csharp
var products = await DataBlock.Connector.LoadExcelAsync(config);
var excelBytes = await products.ExcelSinkAsync();
await File.WriteAllBytesAsync("output/products.xlsx", excelBytes);
```

### Sync Export
```csharp
var customers = DataBlock.Connector.LoadExcel(config);
var excelBytes = customers.ExcelSink();
File.WriteAllBytes("output/customers.xlsx", excelBytes);
```

### Transform Then Export
```csharp
var report = customers
    .Where("Segment", "Enterprise")
    .Select("CustomerID", "FirstName", "LastName", "Email")
    .Sort("LastName");

var bytes = await report.ExcelSinkAsync();
await File.WriteAllBytesAsync("output/report.xlsx", bytes);
```

### Export Aggregated Data
```csharp
var segmentCounts = customers
    .GroupByAggregate("Segment", "CustomerID", AggregationType.Count, "CustomerCount");

var bytes = await segmentCounts.ExcelSinkAsync();
await File.WriteAllBytesAsync("output/segment_summary.xlsx", bytes);
```

## Return Value

Both `ExcelSinkAsync()` and `ExcelSink()` return `byte[]` representing the complete Excel file. This allows you to:

- Write directly to disk with `File.WriteAllBytesAsync()`
- Return from an API endpoint as a download
- Store in a database as binary data
- Send via email attachment
- Upload to cloud storage

## Best Practices

1. **Use async methods** (`ExcelSinkAsync`) for better performance in web applications
2. **Apply transformations first** to reduce file size and include only needed data
3. **Verify roundtrips** when data integrity is critical
4. **Create output directories** before writing files
5. **Consider file naming** to indicate the content and date/time
