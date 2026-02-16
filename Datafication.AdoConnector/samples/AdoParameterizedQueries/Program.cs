using System.Data.Common;
using Datafication.Connectors.AdoConnector;
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.AdoConnector;
using Microsoft.Data.Sqlite;

Console.WriteLine("=== Datafication.AdoConnector Parameterized Queries Sample ===\n");

// Register SQLite provider
DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);

// Create SQLite database with sample data
var dbPath = Path.Combine(AppContext.BaseDirectory, "products.db");
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("Setting up SQLite database with sample data...\n");
await SetupDatabaseAsync(connectionString);

// 1. Simple parameterized query with shorthand API
Console.WriteLine("1. Filter by category using shorthand with parameters...");
var electronics = await DataBlock.Connector.LoadSqliteAsync(
    connectionString,
    "SELECT * FROM Products WHERE Category = @Category",
    new Dictionary<string, object>
    {
        { "@Category", "Electronics" }
    }
);
Console.WriteLine($"   Found {electronics.RowCount} electronics products\n");

DisplayProducts(electronics, 5);

// 2. Multiple parameters with shorthand API
Console.WriteLine("\n2. Filter by category and minimum price...");
var expensiveElectronics = await DataBlock.Connector.LoadSqliteAsync(
    connectionString,
    "SELECT * FROM Products WHERE Category = @Category AND Price > @MinPrice",
    new Dictionary<string, object>
    {
        { "@Category", "Electronics" },
        { "@MinPrice", 500.00 }
    }
);
Console.WriteLine($"   Found {expensiveElectronics.RowCount} electronics over $500\n");

DisplayProducts(expensiveElectronics, 5);

// 3. Full configuration with parameters
Console.WriteLine("\n3. Using full configuration with parameters...");
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandText = @"
        SELECT * FROM Products
        WHERE InStock = @InStock
        AND Price BETWEEN @MinPrice AND @MaxPrice
        ORDER BY Price DESC",
    Parameters = new Dictionary<string, object>
    {
        { "@InStock", 1 },
        { "@MinPrice", 100.00 },
        { "@MaxPrice", 1000.00 }
    }
};

var connector = new AdoDataConnector(configuration);
var priceRange = await connector.GetDataAsync();
Console.WriteLine($"   Found {priceRange.RowCount} in-stock products priced $100-$1000\n");

DisplayProducts(priceRange, 5);

// 4. Different parameter types
Console.WriteLine("\n4. Using different parameter types...");
var searchDate = DateTime.Parse("2024-01-01");
var multiTypeQuery = await DataBlock.Connector.LoadSqliteAsync(
    connectionString,
    @"SELECT * FROM Products
      WHERE InStock = @InStock
      AND DateAdded >= @SinceDate
      AND Name LIKE @NamePattern",
    new Dictionary<string, object>
    {
        { "@InStock", 1 },                          // Boolean as integer
        { "@SinceDate", searchDate.ToString("yyyy-MM-dd") },  // Date
        { "@NamePattern", "%Pro%" }                 // String with wildcards
    }
);
Console.WriteLine($"   Found {multiTypeQuery.RowCount} in-stock 'Pro' products added since 2024\n");

DisplayProducts(multiTypeQuery, 5);

// 5. Demonstrate SQL injection prevention
Console.WriteLine("\n5. SQL Injection Prevention Demonstration:");
Console.WriteLine("   Attempting to search with malicious input...");

// This malicious input would break a concatenated query, but is safe with parameters
var maliciousInput = "'; DROP TABLE Products; --";
var safeQuery = await DataBlock.Connector.LoadSqliteAsync(
    connectionString,
    "SELECT * FROM Products WHERE Category = @Category",
    new Dictionary<string, object>
    {
        { "@Category", maliciousInput }
    }
);
Console.WriteLine($"   Query executed safely! Found {safeQuery.RowCount} products");
Console.WriteLine("   (The malicious input was treated as a literal string value)\n");

// Verify table still exists
var allProducts = await DataBlock.Connector.LoadSqliteAsync(
    connectionString,
    "SELECT COUNT(*) as TotalCount FROM Products"
);
var countCursor = allProducts.GetRowCursor("TotalCount");
countCursor.MoveNext();
Console.WriteLine($"   Products table intact with {countCursor.GetValue("TotalCount")} records");

// Cleanup
File.Delete(dbPath);

Console.WriteLine("\n=== Sample Complete ===");

// Helper method to display products
void DisplayProducts(DataBlock products, int limit)
{
    Console.WriteLine("   " + new string('-', 80));
    Console.WriteLine($"   {"Id",-5} {"Name",-25} {"Category",-15} {"Price",-12} {"InStock",-8}");
    Console.WriteLine("   " + new string('-', 80));

    var cursor = products.GetRowCursor("Id", "Name", "Category", "Price", "InStock");
    int count = 0;
    while (cursor.MoveNext() && count < limit)
    {
        var id = cursor.GetValue("Id");
        var name = cursor.GetValue("Name");
        var category = cursor.GetValue("Category");
        var price = cursor.GetValue("Price");
        var inStock = cursor.GetValue("InStock");
        Console.WriteLine($"   {id,-5} {name,-25} {category,-15} {price,-12:C2} {(Convert.ToInt32(inStock) == 1 ? "Yes" : "No"),-8}");
        count++;
    }
    Console.WriteLine("   " + new string('-', 80));
}

// Helper method to set up the SQLite database
async Task SetupDatabaseAsync(string connString)
{
    using var connection = new SqliteConnection(connString);
    await connection.OpenAsync();

    // Create table
    using var createCmd = connection.CreateCommand();
    createCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Products (
            Id INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            Category TEXT NOT NULL,
            Price REAL NOT NULL,
            InStock INTEGER NOT NULL,
            DateAdded TEXT NOT NULL
        )";
    await createCmd.ExecuteNonQueryAsync();

    // Insert sample data
    var products = new[]
    {
        (1, "Laptop Pro 15", "Electronics", 1299.99, 1, "2024-02-15"),
        (2, "Wireless Mouse", "Electronics", 49.99, 1, "2024-01-10"),
        (3, "USB-C Hub", "Electronics", 79.99, 1, "2024-03-01"),
        (4, "4K Monitor", "Electronics", 599.99, 1, "2024-01-20"),
        (5, "Mechanical Keyboard", "Electronics", 149.99, 0, "2023-11-15"),
        (6, "Office Chair Pro", "Furniture", 399.99, 1, "2024-02-01"),
        (7, "Standing Desk", "Furniture", 549.99, 1, "2024-01-05"),
        (8, "Desk Lamp", "Furniture", 45.99, 1, "2023-12-20"),
        (9, "Notebook Set", "Stationery", 12.99, 1, "2024-03-10"),
        (10, "Pen Pack", "Stationery", 8.99, 1, "2024-02-28"),
        (11, "Tablet Pro", "Electronics", 899.99, 1, "2024-03-05"),
        (12, "Wireless Earbuds Pro", "Electronics", 199.99, 1, "2024-02-10"),
        (13, "Smart Watch", "Electronics", 349.99, 0, "2023-10-01"),
        (14, "Filing Cabinet", "Furniture", 189.99, 1, "2024-01-15"),
        (15, "Bookshelf", "Furniture", 129.99, 1, "2024-02-20")
    };

    foreach (var (id, name, category, price, inStock, dateAdded) in products)
    {
        using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO Products (Id, Name, Category, Price, InStock, DateAdded)
            VALUES ($id, $name, $category, $price, $inStock, $dateAdded)";
        insertCmd.Parameters.AddWithValue("$id", id);
        insertCmd.Parameters.AddWithValue("$name", name);
        insertCmd.Parameters.AddWithValue("$category", category);
        insertCmd.Parameters.AddWithValue("$price", price);
        insertCmd.Parameters.AddWithValue("$inStock", inStock);
        insertCmd.Parameters.AddWithValue("$dateAdded", dateAdded);
        await insertCmd.ExecuteNonQueryAsync();
    }
}
