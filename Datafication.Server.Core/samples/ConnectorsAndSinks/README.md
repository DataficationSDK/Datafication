# ConnectorsAndSinks Sample

This sample demonstrates loading data from external sources (CSV, JSON) and outputting data in different formats using registered sinks.

## Overview

- Load data from CSV files
- Load data from JSON files
- Output data as JSON, CSV, HTML, or Markdown
- Register and use sinks for output format transformation

## How to Run

```bash
cd ConnectorsAndSinks
dotnet restore
dotnet run
```

The server will start at `http://localhost:5000`.

## Sample Data Files

The sample includes data files in the `data/` directory:

- `products.csv` - Product catalog (10 products)
- `orders.json` - Order data (8 orders)

## Registering Sinks

Sinks are registered during service configuration using `options.RegisterSink<TSink>(sinkKey)`:

```csharp
builder.Services.AddDataficationServer(
    options =>
    {
        options.AllowAnonymousAccess = true;
        options.RoutePrefix = "api/data";

        // Register sinks for output formats
        // Note: "json" format has a built-in fallback, but you can register a custom JSON sink
        options.RegisterSink<HtmlTableSink>("html");
        options.RegisterSink<CsvStringSink>("csv");
        options.RegisterSink<MarkdownTableSink>("markdown");
    },
    authOptions =>
    {
        authOptions.AddPolicy("DataBlockAccess", policy => policy.RequireAssertion(_ => true));
    });
```

## List Registered Sinks

```bash
curl http://localhost:5000/api/data/sinks
```

Response:
```json
["html", "csv", "markdown"]
```

## Output Formats

### JSON (Default)

```bash
curl http://localhost:5000/api/data/datablocks/products
```

Response:
```json
{
  "columns": ["ProductId", "ProductName", "Category", "Price", "InStock"],
  "rows": [
    [1, "Laptop Pro", "Electronics", 1299.99, true],
    ...
  ]
}
```

### CSV

```bash
curl "http://localhost:5000/api/data/datablocks/products?format=csv"
```

Response:
```csv
ProductId,ProductName,Category,Price,InStock
1,Laptop Pro,Electronics,1299.99,True
2,Wireless Mouse,Electronics,29.99,True
...
```

### HTML

```bash
curl "http://localhost:5000/api/data/datablocks/summary?format=html"
```

Response:
```html
<table class='datafication-table'>
  <thead>
    <tr class='datafication-tr'>
      <th class='datafication-th'>Category</th>
      <th class='datafication-th'>ProductCount</th>
      <th class='datafication-th'>TotalValue</th>
    </tr>
  </thead>
  <tbody>
    <tr class='datafication-tr'><td class='datafication-td'>Electronics</td><td class='datafication-td'>6</td><td class='datafication-td'>1984.94</td></tr>
    <tr class='datafication-tr'><td class='datafication-td'>Furniture</td><td class='datafication-td'>4</td><td class='datafication-td'>1199.96</td></tr>
  </tbody>
</table>
```

### Markdown

```bash
curl "http://localhost:5000/api/data/datablocks/summary?format=markdown"
```

Response:
```markdown
| Category | ProductCount | TotalValue |
|---|---|---|
| Electronics | 6 | 1984.94 |
| Furniture | 4 | 1199.96 |
```

## Query with Output Format

You can specify the output format for query results:

```bash
curl -X POST "http://localhost:5000/api/data/datablocks/products/query?format=csv" \
  -H "Content-Type: application/json" \
  -d '{
    "where": [{"column": "Category", "operator": "eq", "value": "Electronics"}],
    "select": ["ProductName", "Price"]
  }'
```

## Loading Data from Files

### CSV Loading Pattern

```csharp
public static DataBlock LoadProductsFromCsv()
{
    var block = new DataBlock();
    block.AddColumn(new DataColumn("ProductId", typeof(int)));
    block.AddColumn(new DataColumn("ProductName", typeof(string)));
    block.AddColumn(new DataColumn("Category", typeof(string)));
    block.AddColumn(new DataColumn("Price", typeof(decimal)));
    block.AddColumn(new DataColumn("InStock", typeof(bool)));

    var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "products.csv");
    var lines = File.ReadAllLines(dataPath).Skip(1); // Skip header
    foreach (var line in lines)
    {
        var parts = line.Split(',');
        block.AddRow(new object[]
        {
            int.Parse(parts[0]),
            parts[1],
            parts[2],
            decimal.Parse(parts[3]),
            bool.Parse(parts[4])
        });
    }

    return block;
}

// Register the DataBlock
var products = DataLoaders.LoadProductsFromCsv();
registry.RegisterDataBlock("products", products, new DataBlockMetadata
{
    Name = "Products",
    Description = "Product catalog loaded from CSV",
    Tags = new[] { "products", "csv", "catalog" },
    RegisteredAt = DateTime.UtcNow
});
```

### JSON Loading Pattern

```csharp
public static DataBlock LoadOrdersFromJson()
{
    var block = new DataBlock();
    block.AddColumn(new DataColumn("OrderId", typeof(int)));
    block.AddColumn(new DataColumn("CustomerId", typeof(int)));
    block.AddColumn(new DataColumn("ProductId", typeof(int)));
    block.AddColumn(new DataColumn("Quantity", typeof(int)));
    block.AddColumn(new DataColumn("OrderDate", typeof(string)));

    var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");
    var json = File.ReadAllText(dataPath);
    var orders = JsonSerializer.Deserialize<List<OrderDto>>(json);

    foreach (var order in orders)
    {
        block.AddRow(new object[]
        {
            order.OrderId,
            order.CustomerId,
            order.ProductId,
            order.Quantity,
            order.OrderDate
        });
    }

    return block;
}
```

## Available Sink Types

### From Datafication.Core

- `HtmlTableSink` - Renders DataBlock as an HTML table with styling
- `MarkdownTableSink` - Renders DataBlock as a Markdown table
- `TextTableSink` - Renders DataBlock as a plain text table

### From Datafication.CsvConnector

- `CsvStringSink` - Renders DataBlock as CSV text

## C# Client Example

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// Get as JSON (default)
var jsonResponse = await client.GetStringAsync("/api/data/datablocks/products");
Console.WriteLine("JSON: " + jsonResponse);

// Get as CSV
var csvResponse = await client.GetStringAsync("/api/data/datablocks/products?format=csv");
Console.WriteLine("CSV: " + csvResponse);

// Get as HTML
var htmlResponse = await client.GetStringAsync("/api/data/datablocks/summary?format=html");
Console.WriteLine("HTML: " + htmlResponse);

// Get as Markdown
var mdResponse = await client.GetStringAsync("/api/data/datablocks/summary?format=markdown");
Console.WriteLine("Markdown: " + mdResponse);

// Query with CSV format
var query = new { select = new[] { "ProductName", "Price" } };
var json = JsonSerializer.Serialize(query);
var content = new StringContent(json, Encoding.UTF8, "application/json");
var queryResponse = await client.PostAsync("/api/data/datablocks/products/query?format=csv", content);
var result = await queryResponse.Content.ReadAsStringAsync();
Console.WriteLine("Query CSV: " + result);
```

## Data File Formats

### products.csv

```csv
ProductId,ProductName,Category,Price,InStock
1,Laptop Pro,Electronics,1299.99,true
2,Wireless Mouse,Electronics,29.99,true
...
```

### orders.json

```json
[
  {"orderId": 1001, "customerId": 1, "productId": 1, "quantity": 1, "orderDate": "2024-01-15"},
  {"orderId": 1002, "customerId": 2, "productId": 3, "quantity": 2, "orderDate": "2024-01-16"},
  ...
]
```

## Related Samples

- **BasicServer** - Start here if you're new to Datafication.Server.Core
- **QueryOperations** - Learn about filtering and aggregations
- **AdvancedQueries** - Learn about window functions and joins
