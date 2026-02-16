# AdoParameterizedQueries Sample

Demonstrates secure parameterized queries to prevent SQL injection attacks.

## Overview

This sample shows how to:
- Use parameterized queries with the shorthand API
- Use parameterized queries with full configuration
- Work with different parameter types (strings, numbers, dates)
- Prevent SQL injection attacks
- Build dynamic queries safely

## Key Features Demonstrated

### Shorthand API with Parameters

```csharp
var results = await DataBlock.Connector.LoadSqliteAsync(
    connectionString,
    "SELECT * FROM Products WHERE Category = @Category",
    new Dictionary<string, object>
    {
        { "@Category", "Electronics" }
    }
);
```

### Multiple Parameters

```csharp
var results = await DataBlock.Connector.LoadSqliteAsync(
    connectionString,
    "SELECT * FROM Products WHERE Category = @Category AND Price > @MinPrice",
    new Dictionary<string, object>
    {
        { "@Category", "Electronics" },
        { "@MinPrice", 500.00 }
    }
);
```

### Full Configuration with Parameters

```csharp
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandText = @"
        SELECT * FROM Products
        WHERE InStock = @InStock
        AND Price BETWEEN @MinPrice AND @MaxPrice",
    Parameters = new Dictionary<string, object>
    {
        { "@InStock", 1 },
        { "@MinPrice", 100.00 },
        { "@MaxPrice", 1000.00 }
    }
};

var connector = new AdoDataConnector(configuration);
var data = await connector.GetDataAsync();
```

### Different Parameter Types

```csharp
var parameters = new Dictionary<string, object>
{
    { "@InStock", 1 },                              // Boolean as integer
    { "@SinceDate", "2024-01-01" },                 // Date string
    { "@NamePattern", "%Pro%" }                     // String with wildcards
};
```

## How to Run

```bash
cd AdoParameterizedQueries
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.AdoConnector Parameterized Queries Sample ===

Setting up SQLite database with sample data...

1. Filter by category using shorthand with parameters...
   Found 8 electronics products

   --------------------------------------------------------------------------------
   Id    Name                      Category        Price        InStock
   --------------------------------------------------------------------------------
   1     Laptop Pro 15             Electronics     $1,299.99    Yes
   2     Wireless Mouse            Electronics     $49.99       Yes
   ...

2. Filter by category and minimum price...
   Found 3 electronics over $500

3. Using full configuration with parameters...
   Found 7 in-stock products priced $100-$1000

4. Using different parameter types...
   Found 2 in-stock 'Pro' products added since 2024

5. SQL Injection Prevention Demonstration:
   Attempting to search with malicious input...
   Query executed safely! Found 0 products
   (The malicious input was treated as a literal string value)

   Products table intact with 15 records

=== Sample Complete ===
```

## Database Schema

**Products**
| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Name | TEXT | Product name |
| Category | TEXT | Product category |
| Price | REAL | Product price |
| InStock | INTEGER | 1 = in stock, 0 = out of stock |
| DateAdded | TEXT | Date added (ISO format) |

## Security Notes

**Always use parameterized queries:**
```csharp
// CORRECT - Parameters are safe
CommandText = "SELECT * FROM Users WHERE Username = @Username"
Parameters = { { "@Username", userInput } }

// DANGEROUS - Never concatenate user input
CommandText = $"SELECT * FROM Users WHERE Username = '{userInput}'"
```

Parameters prevent SQL injection by:
- Treating user input as literal values, not SQL code
- Properly escaping special characters
- Enforcing correct data types

## Related Samples

- **AdoBasicQuery** - Basic query execution without parameters
- **AdoFullConfiguration** - All configuration options
- **AdoDataManipulation** - Working with query results
