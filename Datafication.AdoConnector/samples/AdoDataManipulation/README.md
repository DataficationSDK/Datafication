# AdoDataManipulation Sample

Demonstrates DataBlock operations on data loaded from a database.

## Overview

This sample shows how to:
- Load data from multiple database tables
- Filter data with `Where()`
- Sort data with `Sort()` and limit with `Head()`/`Tail()`
- Add calculated columns with `Compute()`
- Project specific columns with `Select()`
- Group and aggregate with `GroupByAggregate()`
- Join DataBlocks with `Merge()`

## Key Features Demonstrated

### Filtering

```csharp
var orders2024 = orders.Where("OrderDate", "2024", ComparisonOperator.StartsWith);
```

### Sorting and Limiting

```csharp
var topOrders = orders
    .Sort(SortDirection.Descending, "Amount")
    .Head(5);
```

### Computed Columns

```csharp
var ordersWithTax = orders
    .Compute("Tax", "Amount * 0.08")
    .Compute("Total", "Amount + Tax");
```

### Column Projection

```csharp
var projected = orders.Select("OrderId", "CustomerId", "Total");
```

### GroupBy and Aggregation

```csharp
var customerRevenue = orders
    .GroupByAggregate("CustomerId", "Amount", AggregationType.Sum, "TotalRevenue")
    .Sort(SortDirection.Descending, "TotalRevenue");
```

### Merging DataBlocks

```csharp
var ordersWithCustomers = orders.Merge(
    customers,
    "CustomerId",      // Left key
    "CustomerId",      // Right key
    MergeMode.Left     // Join type
);
```

## How to Run

```bash
cd AdoDataManipulation
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.AdoConnector Data Manipulation Sample ===

Setting up SQLite database with Orders and Customers...

1. Loading orders from database...
   Loaded 12 orders

2. Filtering: Orders from 2024...
   Found 10 orders in 2024

3. Sorting: Top 5 orders by amount...
   ----------------------------------------------------------------------
   OrderId    CustomerId   Amount          OrderDate
   ----------------------------------------------------------------------
   7          C005         $3,200.00       2024-03-05
   12         C005         $2,450.00       2023-12-05
   3          C003         $2,100.00       2024-02-01
   ...

4. Computing: Adding Tax and Total columns...
   Sample with calculated columns:
   ------------------------------------------------------------
   OrderId    Amount       Tax          Total
   ------------------------------------------------------------
   1          $1,250.00    $100.00      $1,350.00
   ...

5. Selecting: Project only OrderId, CustomerId, and Total...
   Projected to 3 columns: OrderId, CustomerId, Total

6. Aggregating: Total revenue by customer...
   ----------------------------------------
   CustomerId      Total Revenue
   ----------------------------------------
   C005            $5,650.00
   C001            $3,775.25
   ...

7. Merging: Joining orders with customer names...
   Loaded 5 customers
   Merged result has 12 rows and 7 columns

8. Sample of merged data:
   --------------------------------------------------------------------------------
   OrderId    CustomerName         Amount       Region     Status
   --------------------------------------------------------------------------------
   1          Acme Corp            $1,250.00    West       Completed
   ...

=== Sample Complete ===
```

## Database Schema

**Customers**
| Column | Type | Description |
|--------|------|-------------|
| CustomerId | TEXT | Primary key |
| CustomerName | TEXT | Customer name |
| Region | TEXT | Geographic region |

**Orders**
| Column | Type | Description |
|--------|------|-------------|
| OrderId | INTEGER | Primary key |
| CustomerId | TEXT | Foreign key to Customers |
| Amount | REAL | Order amount |
| OrderDate | TEXT | Date of order |
| Status | TEXT | Order status |

## Related Samples

- **AdoBasicQuery** - Simple query execution
- **AdoParameterizedQueries** - Secure parameterized queries
- **AdoStreamingLargeData** - Streaming large datasets
