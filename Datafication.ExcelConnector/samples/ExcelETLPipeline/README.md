# ExcelETLPipeline Sample

This capstone sample demonstrates a complete **Extract-Transform-Load (ETL)** pipeline using Datafication.ExcelConnector. It showcases all major features of the Excel connector and DataBlock API working together in a real-world scenario.

## Features Demonstrated

### Extract Phase
- Multi-sheet loading using `LoadExcelAsync()`
- Sheet selection by name
- Configuration with `ExcelConnectorConfiguration`

### Transform Phase
- **Merge (JOIN)** - Enrich orders with customer data
- **GroupByAggregate** - Calculate revenue by segment, state, and status
- **Where** - Filter data by column values
- **Select** - Project specific columns
- **Sort + Head** - Rank and get top N results

### Load Phase
- **ExcelSinkAsync** - Export analysis results to Excel files
- Multiple report generation

## Running the Sample

```bash
cd samples/ExcelETLPipeline
dotnet run
```

## Pipeline Flow

```
┌─────────────────────────────────────────────────────────┐
│  EXTRACT                                                │
│  • Customers (200 rows)                                 │
│  • Products (50 rows)                                   │
│  • Orders (500 rows)                                    │
│  • OrderItems (1,497 rows)                              │
└────────────────────────┬────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│  TRANSFORM                                              │
│  • Merge (Join orders with customers)                   │
│  • GroupBy + Aggregate (Revenue analysis)               │
│  • Where (Filtering)                                    │
│  • Select (Column selection)                            │
│  • Sort + Head (Top N analysis)                         │
└────────────────────────┬────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│  LOAD                                                   │
│  • 7 Excel reports exported                             │
│  • Ready for business consumption                       │
└─────────────────────────────────────────────────────────┘
```

## Output Reports

| Report | Description |
|--------|-------------|
| revenue_by_segment.xlsx | Total revenue by customer segment |
| orders_by_status.xlsx | Order count by status (Shipped, Pending, etc.) |
| top_states_revenue.xlsx | Top 5 states by revenue |
| customer_segments.xlsx | Customer count by segment |
| enterprise_customers.xlsx | List of Enterprise segment customers |
| product_categories.xlsx | Product count by category |
| enriched_orders.xlsx | Orders with customer data merged |

## Key Code Snippets

### Extract: Multi-Sheet Loading
```csharp
var customers = await DataBlock.Connector.LoadExcelAsync(new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Customers",
    HasHeader = true
});

var orders = await DataBlock.Connector.LoadExcelAsync(new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Orders",
    HasHeader = true
});
```

### Transform: Join (Merge)
```csharp
// Enrich orders with customer information
var enrichedOrders = orders.Merge(customers, "CustomerID", MergeMode.Left);
```

### Transform: GroupBy + Aggregate
```csharp
// Revenue by customer segment
var revenueBySegment = enrichedOrders
    .GroupByAggregate("Segment", "Total", AggregationType.Sum, "TotalRevenue");
```

### Transform: Filter + Select + Sort
```csharp
// Top 5 states by revenue
var topStates = revenueByState
    .Sort("StateRevenue", ascending: false)
    .Head(5);

// Enterprise customers with selected columns
var enterpriseCustomers = customers
    .Where("Segment", "Enterprise")
    .Select("CustomerID", "FirstName", "LastName", "Email", "State");
```

### Load: Export to Excel
```csharp
var bytes = await revenueBySegment.ExcelSinkAsync();
await File.WriteAllBytesAsync("output/revenue_report.xlsx", bytes);
```

## Business Use Cases

This pattern is applicable to many real-world scenarios:

1. **Sales Reporting** - Combine sales, customer, and product data
2. **Financial Analysis** - Aggregate transactions by category/period
3. **Customer Analytics** - Segment analysis and customer profiling
4. **Inventory Management** - Product and order analysis
5. **Marketing Analysis** - Campaign and customer behavior analysis

## Best Practices

1. **Extract**: Load only the columns you need using `UseColumns`
2. **Transform**: Chain operations for efficient memory usage
3. **Load**: Use async methods (`ExcelSinkAsync`) for better performance
4. **Error Handling**: Validate configurations before loading
5. **Memory**: For large datasets, consider streaming to VelocityDataBlock
