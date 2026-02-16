# QueryOperations Sample

This sample demonstrates how to query data using the Datafication.Server.Core REST API, including filtering, sorting, grouping, and aggregations.

## Overview

- Filter data with WHERE clauses (equals, greater than, less than, contains)
- Sort data with ORDER BY (ascending/descending)
- Group data with GROUP BY and aggregations (SUM, AVG, COUNT, MIN, MAX)
- Select specific columns to return

## How to Run

```bash
cd QueryOperations
dotnet restore
dotnet run
```

The server will start at `http://localhost:5000`.

## Query API

All queries use the `POST /api/data/datablocks/{id}/query` endpoint with a JSON body.

### Query Request Structure

```json
{
  "where": [...],    // Filter conditions (array)
  "sort": {...},     // Sort specification (object)
  "groupBy": {...},  // Grouping with aggregations
  "select": [...]    // Columns to return
}
```

## Filter Examples (WHERE)

### Equals

Filter sales where Category equals "Electronics":

```bash
curl -X POST http://localhost:5000/api/data/datablocks/sales/query \
  -H "Content-Type: application/json" \
  -d '{"where": [{"column": "Category", "operator": "eq", "value": "Electronics"}]}'
```

### Greater Than

Filter sales where Total is greater than 3000:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/sales/query \
  -H "Content-Type: application/json" \
  -d '{"where": [{"column": "Total", "operator": "gt", "value": "3000"}]}'
```

### Contains

Filter products containing "top":

```bash
curl -X POST http://localhost:5000/api/data/datablocks/sales/query \
  -H "Content-Type: application/json" \
  -d '{"where": [{"column": "Product", "operator": "contains", "value": "top"}]}'
```

### Multiple Conditions (AND)

Filter Electronics in North region:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/sales/query \
  -H "Content-Type: application/json" \
  -d '{"where": [{"column": "Category", "operator": "eq", "value": "Electronics"}, {"column": "Region", "operator": "eq", "value": "North"}]}'
```

## Sort Examples (ORDER BY)

### Sort Descending

Sort by Total in descending order:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/sales/query \
  -H "Content-Type: application/json" \
  -d '{"sort": {"column": "Total", "direction": "desc"}}'
```

## GroupBy Examples (Aggregations)

### Sum by Category

```bash
curl -X POST http://localhost:5000/api/data/datablocks/sales/query \
  -H "Content-Type: application/json" \
  -d '{"groupBy": {"column": "Category", "aggregations": {"Total": "sum"}}}'
```

### Multiple Aggregations

```bash
curl -X POST http://localhost:5000/api/data/datablocks/sales/query \
  -H "Content-Type: application/json" \
  -d '{"groupBy": {"column": "Category", "aggregations": {"Total": "sum", "Quantity": "sum", "Id": "count"}}}'
```

### GroupBy with Result Column Names

```bash
curl -X POST http://localhost:5000/api/data/datablocks/sales/query \
  -H "Content-Type: application/json" \
  -d '{"groupBy": {"column": "Category", "aggregations": {"Total": "sum", "Id": "count"}, "resultColumnNames": {"Total": "total_sales", "Id": "num_transactions"}}}'
```

## Select Specific Columns

Return only Product, Quantity, and Total:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/sales/query \
  -H "Content-Type: application/json" \
  -d '{"select": ["Product", "Quantity", "Total"]}'
```

## Combined Query

Filter, sort, and select in one query:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/sales/query \
  -H "Content-Type: application/json" \
  -d '{"where": [{"column": "Category", "operator": "eq", "value": "Electronics"}], "sort": {"column": "Total", "direction": "desc"}, "select": ["Product", "Quantity", "Total"]}'
```

## C# Client Example

```csharp
using System.Text;
using System.Text.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// Filter Electronics, sort by Total descending
var query = new
{
    where = new[]
    {
        new { column = "Category", @operator = "eq", value = "Electronics" }
    },
    sort = new { column = "Total", direction = "desc" },
    select = new[] { "Product", "Quantity", "Total" }
};

var json = JsonSerializer.Serialize(query);
var content = new StringContent(json, Encoding.UTF8, "application/json");
var response = await client.PostAsync("/api/data/datablocks/sales/query", content);
var result = await response.Content.ReadAsStringAsync();
Console.WriteLine(result);
```

## Available Filter Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `eq` | Equals | `{"column": "Name", "operator": "eq", "value": "Alice"}` |
| `neq` | Not equals | `{"column": "Status", "operator": "neq", "value": "Inactive"}` |
| `gt` | Greater than | `{"column": "Price", "operator": "gt", "value": "100"}` |
| `gte` | Greater than or equal | `{"column": "Age", "operator": "gte", "value": "18"}` |
| `lt` | Less than | `{"column": "Quantity", "operator": "lt", "value": "10"}` |
| `lte` | Less than or equal | `{"column": "Score", "operator": "lte", "value": "50"}` |
| `contains` | Contains substring | `{"column": "Name", "operator": "contains", "value": "son"}` |
| `startswith` | Starts with | `{"column": "Code", "operator": "startswith", "value": "A"}` |
| `endswith` | Ends with | `{"column": "Email", "operator": "endswith", "value": ".com"}` |

## Available Aggregation Functions

| Function | Description |
|----------|-------------|
| `sum` | Sum of values |
| `mean` / `avg` | Average of values |
| `count` | Count of rows |
| `min` | Minimum value |
| `max` | Maximum value |

## Related Samples

- **BasicServer** - Start here if you're new to Datafication.Server.Core
- **RowOperations** - Learn how to add, update, and delete rows
- **AdvancedQueries** - Learn about window functions, computed columns, and joins
