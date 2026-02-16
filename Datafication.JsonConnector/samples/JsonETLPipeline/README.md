# JsonETLPipeline Sample

Demonstrates a complete ETL (Extract, Transform, Load) pipeline using the Datafication.JsonConnector library.

## Overview

This sample shows how to:
- Extract data from multiple JSON sources
- Transform data using Where, Compute, Sort, Select, and GroupByAggregate
- Load transformed results back to JSON files
- Track pipeline timing and statistics
- Build multi-stage data processing workflows

## Key Features Demonstrated

### Extract Phase

```csharp
// Load from multiple sources
var employees = await DataBlock.Connector.LoadJsonAsync("employees.json");
var products = await DataBlock.Connector.LoadJsonAsync("products.json");
var sales = await DataBlock.Connector.LoadJsonAsync("sales.json");
```

### Transform Phase

```csharp
// Filter
var activeEngineers = employees
    .Where("Department", "Engineering")
    .Where("IsActive", true);

// Compute new columns
var salesWithTotal = sales.Compute("TotalValue", (row) =>
    Convert.ToDouble(row["Quantity"]) * Convert.ToDouble(row["UnitPrice"]));

// Aggregate
var salesByRegion = salesWithTotal.GroupByAggregate(
    groupByColumns: new[] { "Region" },
    aggregations: new[]
    {
        ("TotalValue", AggregationType.Sum, "TotalRevenue"),
        ("TransactionId", AggregationType.Count, "TransactionCount"),
        ("TotalValue", AggregationType.Average, "AvgOrderValue")
    }
);

// Sort and limit
var topProducts = productSales
    .Sort(SortDirection.Descending, "TotalSales")
    .Head(10);
```

### Load Phase

```csharp
// Export to JSON
var regionJson = await salesByRegion.JsonStringSinkAsync();
await File.WriteAllTextAsync("region_summary.json", regionJson);
```

## Pipeline Stages

| Stage | Operation | Description |
|-------|-----------|-------------|
| Extract | LoadJsonAsync | Load data from JSON files or APIs |
| Transform | Where | Filter rows by condition |
| Transform | Compute | Add calculated columns |
| Transform | GroupByAggregate | Aggregate data by groups |
| Transform | Sort | Order results |
| Transform | Select | Choose specific columns |
| Load | JsonStringSinkAsync | Export to JSON format |

## How to Run

```bash
cd JsonETLPipeline
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.JsonConnector ETL Pipeline Sample ===

=== EXTRACT PHASE ===

1. Extracting employees from local JSON...
   Loaded 50 employees in Xms

2. Extracting products from local JSON...
   Loaded 30 products in Xms

3. Extracting sales transactions...
   Loaded 1200 sales in Xms

=== TRANSFORM PHASE ===

4. Filtering active engineering employees...
   Found 17 active engineers in Xms

5. Computing total value for sales...
   Added TotalValue column to 1200 rows in Xms

6. Aggregating sales by region...
   Created 4 region summaries in Xms
   [Region summary table]

7. Finding top products by sales volume...
   Identified top 10 products in Xms

8. Sorting employees by salary...
   Sorted 50 employees in Xms

9. Aggregating employee stats by department...
   Created 6 department summaries in Xms
   [Department stats table]

=== LOAD PHASE ===

10. Exporting region summary to JSON...
    Saved to output_region_summary.json

11. Exporting top products to JSON...
    Saved to output_top_products.json

12. Exporting department statistics to JSON...
    Saved to output_department_stats.json

=== PIPELINE SUMMARY ===

   Sources processed:     3 (employees, products, sales)
   Total records loaded:  1,280
   Outputs generated:     3
   Transformations:       6

=== Sample Complete ===
```

## Data Files

### Input
- `data/employees.json` - 50 employee records
- `data/products.json` - 30 product records
- `data/sales_large.json` - 1200 sales transactions

### Output
- `data/output_region_summary.json` - Sales aggregated by region
- `data/output_top_products.json` - Top 10 products by revenue
- `data/output_department_stats.json` - Employee stats by department

## Related Samples

- **JsonBasicLoad** - Simple data loading
- **JsonExport** - JSON export patterns
- **JsonToVelocity** - Streaming to VelocityDataBlock for large datasets
