# ETLPipeline Sample

This sample demonstrates a complete ETL (Extract, Transform, Load) pipeline using Datafication.Core's method chaining capabilities.

## Overview

The ETLPipeline sample shows how to:
- Extract data from a source (simulated)
- Transform data through multiple operations
- Load processed data
- Chain operations together for a complete pipeline

## Key Features Demonstrated

### ETL Pipeline Pattern

A complete ETL pipeline using method chaining:

```csharp
var processed = rawData
    // Extract: Select relevant columns
    .Select("ProductId", "ProductName", "Category", "Price", "Quantity", "Cost", "Status")
    
    // Transform: Clean data
    .Where("Status", "Active")
    .DropNulls(DropNullMode.Any, "Price", "Quantity", "Cost")
    .FillNulls(FillMethod.Mean, "Price")
    .FillNulls(FillMethod.Mean, "Cost")
    
    // Transform: Compute new columns
    .Compute("Revenue", "Price * Quantity")
    .Compute("TotalCost", "Cost * Quantity")
    .Compute("Profit", "Revenue - TotalCost")
    .Compute("ProfitMargin", "Profit / Revenue")
    
    // Transform: Filter and sort
    .Where("Profit", 0m, ComparisonOperator.GreaterThan)
    .Sort(SortDirection.Descending, "ProfitMargin")
    
    // Load: Limit results
    .Head(10);
```

### Pipeline Stages

1. **Extract**: Select relevant columns from source data
2. **Transform**: 
   - Filter data (Where)
   - Clean data (DropNulls, FillNulls)
   - Compute new columns (Compute)
   - Sort data (Sort)
3. **Load**: Limit and output processed data (Head)

## How to Run

```bash
cd ETLPipeline
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates:
- A complete ETL pipeline from raw data to processed results
- Method chaining for readable pipeline code
- Data cleaning and transformation operations
- Computed columns and filtering
- Transformation summary statistics

## Related Samples

- **DataTransformation** - Learn about individual transformation operations
- **DataQuality** - Learn about data quality operations
- **ExpressionsAndComputedColumns** - Learn about computed columns
- **FilteringAndSorting** - Learn about filtering and sorting

