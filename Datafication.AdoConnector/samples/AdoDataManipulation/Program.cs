using System.Data.Common;
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.AdoConnector;
using Microsoft.Data.Sqlite;

Console.WriteLine("=== Datafication.AdoConnector Data Manipulation Sample ===\n");

// Register SQLite provider
DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);

// Create SQLite database with sample data
var dbPath = Path.Combine(AppContext.BaseDirectory, "sales.db");
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("Setting up SQLite database with Orders and Customers...\n");
await SetupDatabaseAsync(connectionString);

// 1. Load orders from database
Console.WriteLine("1. Loading orders from database...");
var orders = await DataBlock.Connector.LoadSqliteAsync(
    connectionString,
    "SELECT * FROM Orders"
);
Console.WriteLine($"   Loaded {orders.RowCount} orders\n");

// 2. Filter: Orders from 2024
Console.WriteLine("2. Filtering: Orders from 2024...");
var orders2024 = orders.Where("OrderDate", "2024", ComparisonOperator.StartsWith);
Console.WriteLine($"   Found {orders2024.RowCount} orders in 2024\n");

// 3. Sort: Top 5 orders by amount
Console.WriteLine("3. Sorting: Top 5 orders by amount...");
var topOrders = orders
    .Sort(SortDirection.Descending, "Amount")
    .Head(5);

Console.WriteLine("   " + new string('-', 70));
Console.WriteLine($"   {"OrderId",-10} {"CustomerId",-12} {"Amount",-15} {"OrderDate",-15}");
Console.WriteLine("   " + new string('-', 70));

var cursor = topOrders.GetRowCursor("OrderId", "CustomerId", "Amount", "OrderDate");
while (cursor.MoveNext())
{
    Console.WriteLine($"   {cursor.GetValue("OrderId"),-10} {cursor.GetValue("CustomerId"),-12} {cursor.GetValue("Amount"),-15:C2} {cursor.GetValue("OrderDate"),-15}");
}
Console.WriteLine("   " + new string('-', 70) + "\n");

// 4. Compute: Add calculated columns
Console.WriteLine("4. Computing: Adding Tax and Total columns...");
var ordersWithTax = orders
    .Compute("Tax", "Amount * 0.08")
    .Compute("Total", "Amount + Tax");

Console.WriteLine("   Sample with calculated columns:");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine($"   {"OrderId",-10} {"Amount",-12} {"Tax",-12} {"Total",-12}");
Console.WriteLine("   " + new string('-', 60));

var taxCursor = ordersWithTax.Head(5).GetRowCursor("OrderId", "Amount", "Tax", "Total");
while (taxCursor.MoveNext())
{
    Console.WriteLine($"   {taxCursor.GetValue("OrderId"),-10} {taxCursor.GetValue("Amount"),-12:C2} {taxCursor.GetValue("Tax"),-12:C2} {taxCursor.GetValue("Total"),-12:C2}");
}
Console.WriteLine("   " + new string('-', 60) + "\n");

// 5. Select: Project specific columns
Console.WriteLine("5. Selecting: Project only OrderId, CustomerId, and Total...");
var projected = ordersWithTax.Select("OrderId", "CustomerId", "Total");
Console.WriteLine($"   Projected to {projected.Schema.Count} columns: {string.Join(", ", projected.Schema.GetColumnNames())}\n");

// 6. GroupBy and Aggregate: Revenue by customer
Console.WriteLine("6. Aggregating: Total revenue by customer...");
var customerRevenue = orders
    .GroupByAggregate("CustomerId", "Amount", AggregationType.Sum, "TotalRevenue")
    .Sort(SortDirection.Descending, "TotalRevenue");

Console.WriteLine("   " + new string('-', 40));
Console.WriteLine($"   {"CustomerId",-15} {"Total Revenue",-20}");
Console.WriteLine("   " + new string('-', 40));

var revenueCursor = customerRevenue.GetRowCursor("CustomerId", "TotalRevenue");
while (revenueCursor.MoveNext())
{
    Console.WriteLine($"   {revenueCursor.GetValue("CustomerId"),-15} {revenueCursor.GetValue("TotalRevenue"),-20:C2}");
}
Console.WriteLine("   " + new string('-', 40) + "\n");

// 7. Load customers and merge with orders
Console.WriteLine("7. Merging: Joining orders with customer names...");
var customers = await DataBlock.Connector.LoadSqliteAsync(
    connectionString,
    "SELECT * FROM Customers"
);
Console.WriteLine($"   Loaded {customers.RowCount} customers");

var ordersWithCustomers = orders.Merge(
    customers,
    "CustomerId",
    "CustomerId",
    MergeMode.Left
);
Console.WriteLine($"   Merged result has {ordersWithCustomers.RowCount} rows and {ordersWithCustomers.Schema.Count} columns\n");

// 8. Display merged data sample
Console.WriteLine("8. Sample of merged data:");
Console.WriteLine("   " + new string('-', 80));
Console.WriteLine($"   {"OrderId",-10} {"CustomerName",-20} {"Amount",-12} {"Region",-10} {"Status",-10}");
Console.WriteLine("   " + new string('-', 80));

var mergedCursor = ordersWithCustomers.Head(5).GetRowCursor("OrderId", "CustomerName", "Amount", "Region", "Status");
while (mergedCursor.MoveNext())
{
    Console.WriteLine($"   {mergedCursor.GetValue("OrderId"),-10} {mergedCursor.GetValue("CustomerName"),-20} {mergedCursor.GetValue("Amount"),-12:C2} {mergedCursor.GetValue("Region"),-10} {mergedCursor.GetValue("Status"),-10}");
}
Console.WriteLine("   " + new string('-', 80));

// Cleanup
File.Delete(dbPath);

Console.WriteLine("\n=== Sample Complete ===");

// Helper method to set up the SQLite database
async Task SetupDatabaseAsync(string connString)
{
    using var connection = new SqliteConnection(connString);
    await connection.OpenAsync();

    // Create Customers table
    using var createCustomersCmd = connection.CreateCommand();
    createCustomersCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Customers (
            CustomerId TEXT PRIMARY KEY,
            CustomerName TEXT NOT NULL,
            Region TEXT NOT NULL
        )";
    await createCustomersCmd.ExecuteNonQueryAsync();

    // Create Orders table
    using var createOrdersCmd = connection.CreateCommand();
    createOrdersCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Orders (
            OrderId INTEGER PRIMARY KEY,
            CustomerId TEXT NOT NULL,
            Amount REAL NOT NULL,
            OrderDate TEXT NOT NULL,
            Status TEXT NOT NULL
        )";
    await createOrdersCmd.ExecuteNonQueryAsync();

    // Insert customers
    var customers = new[]
    {
        ("C001", "Acme Corp", "West"),
        ("C002", "TechStart Inc", "East"),
        ("C003", "GlobalTrade LLC", "West"),
        ("C004", "QuickServe Co", "Central"),
        ("C005", "DataDriven Ltd", "East")
    };

    foreach (var (id, name, region) in customers)
    {
        using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Customers VALUES ($id, $name, $region)";
        insertCmd.Parameters.AddWithValue("$id", id);
        insertCmd.Parameters.AddWithValue("$name", name);
        insertCmd.Parameters.AddWithValue("$region", region);
        await insertCmd.ExecuteNonQueryAsync();
    }

    // Insert orders
    var orders = new[]
    {
        (1, "C001", 1250.00, "2024-01-15", "Completed"),
        (2, "C002", 850.50, "2024-01-18", "Completed"),
        (3, "C003", 2100.00, "2024-02-01", "Completed"),
        (4, "C001", 975.25, "2024-02-10", "Completed"),
        (5, "C004", 450.00, "2024-02-15", "Completed"),
        (6, "C002", 1800.75, "2024-03-01", "Pending"),
        (7, "C005", 3200.00, "2024-03-05", "Completed"),
        (8, "C003", 680.50, "2024-03-10", "Completed"),
        (9, "C001", 1550.00, "2024-03-15", "Pending"),
        (10, "C004", 920.00, "2024-03-20", "Completed"),
        (11, "C002", 1100.25, "2023-11-10", "Completed"),
        (12, "C005", 2450.00, "2023-12-05", "Completed")
    };

    foreach (var (id, customerId, amount, date, status) in orders)
    {
        using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Orders VALUES ($id, $customerId, $amount, $date, $status)";
        insertCmd.Parameters.AddWithValue("$id", id);
        insertCmd.Parameters.AddWithValue("$customerId", customerId);
        insertCmd.Parameters.AddWithValue("$amount", amount);
        insertCmd.Parameters.AddWithValue("$date", date);
        insertCmd.Parameters.AddWithValue("$status", status);
        await insertCmd.ExecuteNonQueryAsync();
    }
}
