using System.Data.Common;
using Datafication.Connectors.AdoConnector;
using Datafication.Core.Connectors;
using Datafication.Core.Factories;
using Datafication.Factories.AdoConnector;
using Microsoft.Data.Sqlite;

Console.WriteLine("=== Datafication.AdoConnector Factory Pattern Sample ===\n");

// Register SQLite provider
DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);

// Create SQLite database with multiple tables
var dbPath = Path.Combine(AppContext.BaseDirectory, "factory_demo.db");
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("Setting up SQLite database with multiple tables...\n");
await SetupDatabaseAsync(connectionString);

// ============================================================================
// 1. Create Factory Instance
// ============================================================================
Console.WriteLine("1. Creating AdoDataProvider factory...");

IDataConnectorFactory factory = new AdoDataProvider();
Console.WriteLine("   Factory created: AdoDataProvider");
Console.WriteLine("   Implements: IDataConnectorFactory\n");

// ============================================================================
// 2. Create Multiple Connectors from Factory
// ============================================================================
Console.WriteLine("2. Creating multiple connectors from factory...");

// Employees connector
var employeesConfig = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandText = "SELECT * FROM Employees",
    Id = "employees-connector"
};

var employeesConnector = factory.CreateDataConnector(employeesConfig);
Console.WriteLine($"   Created connector: {employeesConnector.GetConnectorId()}");

// Products connector
var productsConfig = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandText = "SELECT * FROM Products",
    Id = "products-connector"
};

var productsConnector = factory.CreateDataConnector(productsConfig);
Console.WriteLine($"   Created connector: {productsConnector.GetConnectorId()}");

// Orders connector
var ordersConfig = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandText = "SELECT * FROM Orders",
    Id = "orders-connector"
};

var ordersConnector = factory.CreateDataConnector(ordersConfig);
Console.WriteLine($"   Created connector: {ordersConnector.GetConnectorId()}\n");

// ============================================================================
// 3. Execute Queries Using Factory-Created Connectors
// ============================================================================
Console.WriteLine("3. Executing queries using factory-created connectors...\n");

var employees = await employeesConnector.GetDataAsync();
Console.WriteLine($"   Employees: {employees.RowCount} rows");

var products = await productsConnector.GetDataAsync();
Console.WriteLine($"   Products: {products.RowCount} rows");

var orders = await ordersConnector.GetDataAsync();
Console.WriteLine($"   Orders: {orders.RowCount} rows\n");

// ============================================================================
// 4. Configuration Reuse Pattern
// ============================================================================
Console.WriteLine("4. Configuration reuse pattern...");

// Base configuration
var baseConfig = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandTimeout = 60
};

// Create configurations for different queries
var configs = new Dictionary<string, AdoConnectorConfiguration>
{
    ["active-employees"] = new AdoConnectorConfiguration
    {
        ProviderName = baseConfig.ProviderName,
        ConnectionString = baseConfig.ConnectionString,
        CommandTimeout = baseConfig.CommandTimeout,
        CommandText = "SELECT * FROM Employees WHERE IsActive = 1",
        Id = "active-employees"
    },
    ["in-stock-products"] = new AdoConnectorConfiguration
    {
        ProviderName = baseConfig.ProviderName,
        ConnectionString = baseConfig.ConnectionString,
        CommandTimeout = baseConfig.CommandTimeout,
        CommandText = "SELECT * FROM Products WHERE InStock = 1",
        Id = "in-stock-products"
    }
};

foreach (var (name, config) in configs)
{
    var connector = factory.CreateDataConnector(config);
    var data = await connector.GetDataAsync();
    Console.WriteLine($"   {name}: {data.RowCount} rows");
}
Console.WriteLine();

// ============================================================================
// 5. Factory Validation
// ============================================================================
Console.WriteLine("5. Factory validation demonstration...");

try
{
    var invalidConfig = new AdoConnectorConfiguration
    {
        ProviderName = "",  // Invalid: empty
        ConnectionString = connectionString,
        CommandText = "SELECT 1"
    };

    factory.CreateDataConnector(invalidConfig);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"   Factory rejected invalid config: {ex.Message}\n");
}

// ============================================================================
// 6. Dependency Injection Simulation
// ============================================================================
Console.WriteLine("6. Dependency injection simulation...");
Console.WriteLine("   Simulating a service that receives IDataConnectorFactory...\n");

// Simulate a service class that depends on the factory
var dataService = new DataService(factory, connectionString);

var employeeReport = await dataService.GetEmployeeReportAsync();
Console.WriteLine($"   Employee report: {employeeReport.RowCount} rows");

var productReport = await dataService.GetProductReportAsync();
Console.WriteLine($"   Product report: {productReport.RowCount} rows\n");

// ============================================================================
// 7. Parallel Query Execution
// ============================================================================
Console.WriteLine("7. Parallel query execution using factory...");

var queryConfigs = new[]
{
    new AdoConnectorConfiguration
    {
        ProviderName = "Microsoft.Data.Sqlite",
        ConnectionString = connectionString,
        CommandText = "SELECT * FROM Employees",
        Id = "parallel-employees"
    },
    new AdoConnectorConfiguration
    {
        ProviderName = "Microsoft.Data.Sqlite",
        ConnectionString = connectionString,
        CommandText = "SELECT * FROM Products",
        Id = "parallel-products"
    },
    new AdoConnectorConfiguration
    {
        ProviderName = "Microsoft.Data.Sqlite",
        ConnectionString = connectionString,
        CommandText = "SELECT * FROM Orders",
        Id = "parallel-orders"
    }
};

var tasks = queryConfigs.Select(config =>
{
    var connector = factory.CreateDataConnector(config);
    return connector.GetDataAsync();
});

var results = await Task.WhenAll(tasks);

Console.WriteLine("   Parallel execution results:");
for (int i = 0; i < results.Length; i++)
{
    Console.WriteLine($"   - {queryConfigs[i].Id}: {results[i].RowCount} rows");
}
Console.WriteLine();

// ============================================================================
// 8. Factory Pattern Benefits
// ============================================================================
Console.WriteLine("8. Factory pattern benefits:");
Console.WriteLine("   +----------------------------------+----------------------------------------+");
Console.WriteLine("   | Benefit                          | Description                            |");
Console.WriteLine("   +----------------------------------+----------------------------------------+");
Console.WriteLine("   | Abstraction                      | Code depends on interface, not impl    |");
Console.WriteLine("   | Testability                      | Easy to mock IDataConnectorFactory     |");
Console.WriteLine("   | Validation                       | Factory validates before creation      |");
Console.WriteLine("   | Consistency                      | Single point of connector creation     |");
Console.WriteLine("   | Flexibility                      | Swap implementations without changes   |");
Console.WriteLine("   +----------------------------------+----------------------------------------+");

// Cleanup
File.Delete(dbPath);

Console.WriteLine("\n=== Sample Complete ===");

// Helper method to set up the SQLite database
async Task SetupDatabaseAsync(string connString)
{
    using var connection = new SqliteConnection(connString);
    await connection.OpenAsync();

    // Create Employees table
    using var createEmployeesCmd = connection.CreateCommand();
    createEmployeesCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Employees (
            Id INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            Department TEXT NOT NULL,
            Salary REAL NOT NULL,
            IsActive INTEGER NOT NULL
        )";
    await createEmployeesCmd.ExecuteNonQueryAsync();

    // Create Products table
    using var createProductsCmd = connection.CreateCommand();
    createProductsCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Products (
            Id INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            Price REAL NOT NULL,
            InStock INTEGER NOT NULL
        )";
    await createProductsCmd.ExecuteNonQueryAsync();

    // Create Orders table
    using var createOrdersCmd = connection.CreateCommand();
    createOrdersCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Orders (
            Id INTEGER PRIMARY KEY,
            ProductId INTEGER NOT NULL,
            Quantity INTEGER NOT NULL,
            OrderDate TEXT NOT NULL
        )";
    await createOrdersCmd.ExecuteNonQueryAsync();

    // Insert sample data
    var employees = new[]
    {
        (1, "Alice Johnson", "Engineering", 95000.00, 1),
        (2, "Bob Smith", "Sales", 75000.00, 1),
        (3, "Carol Williams", "Engineering", 105000.00, 1),
        (4, "David Brown", "Marketing", 68000.00, 0),
        (5, "Eve Davis", "Engineering", 88000.00, 1)
    };

    foreach (var (id, name, dept, salary, active) in employees)
    {
        using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Employees VALUES ($id, $name, $dept, $salary, $active)";
        insertCmd.Parameters.AddWithValue("$id", id);
        insertCmd.Parameters.AddWithValue("$name", name);
        insertCmd.Parameters.AddWithValue("$dept", dept);
        insertCmd.Parameters.AddWithValue("$salary", salary);
        insertCmd.Parameters.AddWithValue("$active", active);
        await insertCmd.ExecuteNonQueryAsync();
    }

    var products = new[]
    {
        (1, "Laptop Pro", 1299.99, 1),
        (2, "Wireless Mouse", 49.99, 1),
        (3, "USB-C Hub", 79.99, 1),
        (4, "Monitor 4K", 599.99, 0),
        (5, "Keyboard", 149.99, 1)
    };

    foreach (var (id, name, price, inStock) in products)
    {
        using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Products VALUES ($id, $name, $price, $inStock)";
        insertCmd.Parameters.AddWithValue("$id", id);
        insertCmd.Parameters.AddWithValue("$name", name);
        insertCmd.Parameters.AddWithValue("$price", price);
        insertCmd.Parameters.AddWithValue("$inStock", inStock);
        await insertCmd.ExecuteNonQueryAsync();
    }

    var orders = new[]
    {
        (1, 1, 2, "2024-01-15"),
        (2, 2, 5, "2024-01-18"),
        (3, 3, 3, "2024-02-01"),
        (4, 1, 1, "2024-02-10"),
        (5, 5, 2, "2024-02-15"),
        (6, 2, 10, "2024-03-01")
    };

    foreach (var (id, productId, quantity, date) in orders)
    {
        using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Orders VALUES ($id, $productId, $quantity, $date)";
        insertCmd.Parameters.AddWithValue("$id", id);
        insertCmd.Parameters.AddWithValue("$productId", productId);
        insertCmd.Parameters.AddWithValue("$quantity", quantity);
        insertCmd.Parameters.AddWithValue("$date", date);
        await insertCmd.ExecuteNonQueryAsync();
    }
}

// Simulated service class that uses dependency injection
class DataService
{
    private readonly IDataConnectorFactory _factory;
    private readonly string _connectionString;

    public DataService(IDataConnectorFactory factory, string connectionString)
    {
        _factory = factory;
        _connectionString = connectionString;
    }

    public async Task<Datafication.Core.Data.DataBlock> GetEmployeeReportAsync()
    {
        var config = new AdoConnectorConfiguration
        {
            ProviderName = "Microsoft.Data.Sqlite",
            ConnectionString = _connectionString,
            CommandText = "SELECT Name, Department, Salary FROM Employees WHERE IsActive = 1",
            Id = "employee-report"
        };

        var connector = _factory.CreateDataConnector(config);
        return await connector.GetDataAsync();
    }

    public async Task<Datafication.Core.Data.DataBlock> GetProductReportAsync()
    {
        var config = new AdoConnectorConfiguration
        {
            ProviderName = "Microsoft.Data.Sqlite",
            ConnectionString = _connectionString,
            CommandText = "SELECT Name, Price, InStock FROM Products",
            Id = "product-report"
        };

        var connector = _factory.CreateDataConnector(config);
        return await connector.GetDataAsync();
    }
}
