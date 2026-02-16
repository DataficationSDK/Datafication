# AdvancedQueries Sample

This sample demonstrates advanced query features including window functions, computed columns, and data joins/merges.

## Overview

- **Window Functions** - Moving averages, cumulative sums, ranking, lag/lead
- **Computed Columns** - Create new columns from expressions
- **Data Joins** - Left, right, inner, and full joins between DataBlocks

## How to Run

```bash
cd AdvancedQueries
dotnet restore
dotnet run
```

The server will start at `http://localhost:5000`.

## Available DataBlocks

| ID | Description | Use Case |
|----|-------------|----------|
| `stocks` | Daily stock prices (TECH, BANK) | Window functions |
| `orders` | Customer orders | Joins |
| `customers` | Customer info | Join target |
| `products` | Product catalog | Join target |

## Window Functions

Window functions perform calculations across a set of rows that are related to the current row.

### Moving Average

Calculate 3-day moving average of stock prices:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/stocks/query \
  -H "Content-Type: application/json" \
  -d '{
    "window": [{
      "column": "Price",
      "function": "movingavg",
      "windowSize": 3,
      "resultColumn": "MovingAvg3Day"
    }]
  }'
```

### Cumulative Sum

Calculate cumulative volume:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/stocks/query \
  -H "Content-Type: application/json" \
  -d '{
    "window": [{
      "column": "Volume",
      "function": "cumsum",
      "resultColumn": "CumulativeVolume"
    }]
  }'
```

### Ranking

Rank prices within each symbol:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/stocks/query \
  -H "Content-Type: application/json" \
  -d '{
    "window": [{
      "column": "Price",
      "function": "rank",
      "orderByColumn": "Price",
      "partitionByColumns": ["Symbol"],
      "resultColumn": "PriceRank"
    }]
  }'
```

### LAG (Previous Value)

Get the previous day's price:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/stocks/query \
  -H "Content-Type: application/json" \
  -d '{
    "window": [{
      "column": "Price",
      "function": "lag",
      "offset": 1,
      "resultColumn": "PreviousPrice"
    }]
  }'
```

### LEAD (Next Value)

Get the next day's price:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/stocks/query \
  -H "Content-Type: application/json" \
  -d '{
    "window": [{
      "column": "Price",
      "function": "lead",
      "offset": 1,
      "resultColumn": "NextPrice"
    }]
  }'
```

### Available Window Functions

| Function | Description |
|----------|-------------|
| `movingavg` | Moving average over window |
| `movingsum` | Moving sum over window |
| `movingmin` | Moving minimum over window |
| `movingmax` | Moving maximum over window |
| `cumsum` | Cumulative sum |
| `cumavg` | Cumulative average |
| `cummin` | Cumulative minimum |
| `cummax` | Cumulative maximum |
| `lag` | Value from previous row |
| `lead` | Value from next row |
| `rank` | Rank within partition |
| `denserank` | Dense rank (no gaps) |
| `rownumber` | Row number within partition |

## Computed Columns

Create new columns using expressions.

### Basic Expression

Calculate total value (Price * Volume):

```bash
curl -X POST http://localhost:5000/api/data/datablocks/stocks/query \
  -H "Content-Type: application/json" \
  -d '{
    "compute": [{
      "resultColumn": "TotalValue",
      "expression": "[Price] * [Volume]"
    }]
  }'
```

### Multiple Computed Columns

```bash
curl -X POST http://localhost:5000/api/data/datablocks/stocks/query \
  -H "Content-Type: application/json" \
  -d '{
    "compute": [
      { "resultColumn": "TotalValue", "expression": "[Price] * [Volume]" },
      { "resultColumn": "PricePlus10Pct", "expression": "[Price] * 1.1" }
    ]
  }'
```

## Data Joins (Merge)

Combine data from multiple DataBlocks.

### Left Join

Join orders with customers (include all orders, even if no matching customer):

```bash
curl -X POST http://localhost:5000/api/data/datablocks/orders/query \
  -H "Content-Type: application/json" \
  -d '{
    "merge": {
      "otherDataBlockId": "customers",
      "keyColumn": "CustomerId",
      "otherKeyColumn": "CustomerId",
      "mode": "left"
    }
  }'
```

### Inner Join

Join orders with products (only matching rows):

```bash
curl -X POST http://localhost:5000/api/data/datablocks/orders/query \
  -H "Content-Type: application/json" \
  -d '{
    "merge": {
      "otherDataBlockId": "products",
      "keyColumn": "ProductId",
      "otherKeyColumn": "ProductId",
      "mode": "inner"
    }
  }'
```

### Right Join

```bash
curl -X POST http://localhost:5000/api/data/datablocks/orders/query \
  -H "Content-Type: application/json" \
  -d '{
    "merge": {
      "otherDataBlockId": "customers",
      "keyColumn": "CustomerId",
      "otherKeyColumn": "CustomerId",
      "mode": "right"
    }
  }'
```

### Full Join

```bash
curl -X POST http://localhost:5000/api/data/datablocks/orders/query \
  -H "Content-Type: application/json" \
  -d '{
    "merge": {
      "otherDataBlockId": "customers",
      "keyColumn": "CustomerId",
      "otherKeyColumn": "CustomerId",
      "mode": "full"
    }
  }'
```

### Join Modes

| Mode | Description |
|------|-------------|
| `inner` | Only matching rows from both (default) |
| `left` | All rows from left, matching from right |
| `right` | All rows from right, matching from left |
| `full` | All rows from both sides |

## C# Client Example

```csharp
using System.Text;
using System.Text.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// Window function: 3-day moving average
var windowQuery = new
{
    window = new[]
    {
        new
        {
            column = "Price",
            function = "movingavg",
            windowSize = 3,
            resultColumn = "MovingAvg"
        }
    }
};

var json = JsonSerializer.Serialize(windowQuery);
var content = new StringContent(json, Encoding.UTF8, "application/json");
var response = await client.PostAsync("/api/data/datablocks/stocks/query", content);
var result = await response.Content.ReadAsStringAsync();
Console.WriteLine(result);

// Join orders with customers
var joinQuery = new
{
    merge = new
    {
        otherDataBlockId = "customers",
        keyColumn = "CustomerId",
        otherKeyColumn = "CustomerId",
        mode = "left"
    }
};

json = JsonSerializer.Serialize(joinQuery);
content = new StringContent(json, Encoding.UTF8, "application/json");
response = await client.PostAsync("/api/data/datablocks/orders/query", content);
result = await response.Content.ReadAsStringAsync();
Console.WriteLine(result);
```

## Combining Operations

You can combine multiple operations in a single query:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/stocks/query \
  -H "Content-Type: application/json" \
  -d '{
    "where": [{"column": "Symbol", "operator": "eq", "value": "TECH"}],
    "window": [{
      "column": "Price",
      "function": "movingavg",
      "windowSize": 3,
      "resultColumn": "MovingAvg"
    }],
    "compute": [{
      "resultColumn": "TotalValue",
      "expression": "[Price] * [Volume]"
    }],
    "sort": {"column": "Date", "direction": "asc"}
  }'
```

## Related Samples

- **QueryOperations** - Basic filtering, sorting, and aggregations
- **BasicServer** - Start here if you're new to Datafication.Server.Core
- **ConnectorsAndSinks** - Loading external data and exporting
