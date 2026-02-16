# Datafication.AdoConnector

[![NuGet](https://img.shields.io/nuget/v/Datafication.AdoConnector.svg)](https://www.nuget.org/packages/Datafication.AdoConnector)

An ADO.NET database connector for .NET that provides seamless integration between relational databases and the Datafication.Core DataBlock API.

## Description

Datafication.AdoConnector is a universal database connector library that bridges ADO.NET-compatible databases and the Datafication.Core ecosystem. It provides connectivity to any database with an ADO.NET provider including SQL Server, PostgreSQL, MySQL, SQLite, and Oracle. The connector supports async queries, parameterized SQL, stored procedures, and streaming batch processing for large datasets.

### Key Features

- **Universal Provider Support**: Connect to any ADO.NET-compatible database (SQL Server, PostgreSQL, MySQL, SQLite, Oracle, etc.)
- **Async Operations**: Fully asynchronous query execution with `GetDataAsync`
- **Parameterized Queries**: Built-in support for parameterized SQL to prevent SQL injection
- **Stored Procedures**: Execute stored procedures with parameter support
- **Streaming Support**: Efficient batch loading for large result sets with `GetStorageDataAsync`
- **Automatic Type Mapping**: Maps database column types to .NET types automatically
- **Configurable Timeouts**: Set command timeout for long-running queries
- **Error Handling**: Global error handler configuration for graceful exception management
- **Validation**: Built-in configuration validation ensures correct setup before connecting
- **Factory Pattern**: `AdoDataProvider` factory for creating connectors from configurations
- **Shorthand API**: Convenient extension methods like `DataBlock.Connector.LoadSqlServerAsync()` for quick data loading
- **Cross-Platform**: Works on Windows, Linux, and macOS

## Table of Contents

- [Description](#description)
  - [Key Features](#key-features)
- [Installation](#installation)
- [Provider Setup](#provider-setup)
- [Loading Data (Shorthand)](#loading-data-shorthand)
- [Usage Examples](#usage-examples)
  - [Basic Query Execution](#basic-query-execution)
  - [Parameterized Queries](#parameterized-queries)
  - [Executing Stored Procedures](#executing-stored-procedures)
  - [Streaming Large Result Sets](#streaming-large-result-sets)
  - [Error Handling](#error-handling)
  - [Working with Query Results](#working-with-query-results)
  - [Using the Factory Pattern](#using-the-factory-pattern)
- [Configuration Reference](#configuration-reference)
  - [AdoConnectorConfiguration](#adoconnectorconfiguration)
- [API Reference](#api-reference)
  - [Core Classes](#core-classes)
  - [Extension Methods](#extension-methods)
- [Common Patterns](#common-patterns)
  - [ETL Pipeline from Database](#etl-pipeline-from-database)
  - [Database to VelocityDataBlock](#database-to-velocitydatablock)
  - [Data Analysis from SQL Query](#data-analysis-from-sql-query)
  - [Multi-Database Integration](#multi-database-integration)
- [Supported Providers](#supported-providers)
- [Performance Tips](#performance-tips)
- [Security Considerations](#security-considerations)
- [License](#license)

## Installation

> **Note**: Datafication.AdoConnector is currently in pre-release. The packages are now available on nuget.org.

```bash
dotnet add package Datafication.AdoConnector
```

**Running the Samples:**

```bash
cd samples/AdoBasicQuery
dotnet run
```

## Provider Setup

ADO.NET providers must be registered before use. Each database requires its specific provider package:

**SQL Server (Microsoft.Data.SqlClient):**
```bash
dotnet add package Microsoft.Data.SqlClient
```

```csharp
DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
```

**PostgreSQL (Npgsql):**
```bash
dotnet add package Npgsql
```

```csharp
DbProviderFactories.RegisterFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);
```

**MySQL (MySql.Data):**
```bash
dotnet add package MySql.Data
```

```csharp
DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySql.Data.MySqlClient.MySqlClientFactory.Instance);
```

**SQLite (Microsoft.Data.Sqlite):**
```bash
dotnet add package Microsoft.Data.Sqlite
```

```csharp
DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", Microsoft.Data.Sqlite.SqliteFactory.Instance);
```

## Loading Data (Shorthand)

The simplest way to load data from a database is using the shorthand extension methods:

```csharp
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.AdoConnector;

// SQL Server
var employees = await DataBlock.Connector.LoadSqlServerAsync(
    "Server=localhost;Database=MyDB;Trusted_Connection=True;",
    "SELECT * FROM Employees"
);

// PostgreSQL
var users = await DataBlock.Connector.LoadPostgresAsync(
    "Host=localhost;Database=mydb;Username=user;Password=pass;",
    "SELECT * FROM users"
);

// SQLite
var products = await DataBlock.Connector.LoadSqliteAsync(
    "Data Source=app.db",
    "SELECT * FROM products"
);

// MySQL
var orders = await DataBlock.Connector.LoadMySqlAsync(
    "Server=localhost;Database=shop;Uid=user;Pwd=pass;",
    "SELECT * FROM orders"
);
```

### Shorthand with Parameters

Use parameterized queries with the shorthand methods:

```csharp
// SQL Server with parameters
var engineers = await DataBlock.Connector.LoadSqlServerAsync(
    "Server=localhost;Database=MyDB;Trusted_Connection=True;",
    "SELECT * FROM Employees WHERE Department = @Dept AND Salary > @MinSalary",
    new Dictionary<string, object>
    {
        { "@Dept", "Engineering" },
        { "@MinSalary", 75000 }
    }
);

// PostgreSQL with parameters
var recentUsers = await DataBlock.Connector.LoadPostgresAsync(
    "Host=localhost;Database=mydb;Username=user;Password=pass;",
    "SELECT * FROM users WHERE created_at > @since",
    new Dictionary<string, object>
    {
        { "@since", DateTime.Now.AddDays(-30) }
    }
);
```

### Synchronous Versions

All shorthand methods have synchronous versions:

```csharp
// Sync versions (without Async suffix)
var data1 = DataBlock.Connector.LoadSqlServer(connectionString, query);
var data2 = DataBlock.Connector.LoadPostgres(connectionString, query);
var data3 = DataBlock.Connector.LoadSqlite(connectionString, query);
var data4 = DataBlock.Connector.LoadMySql(connectionString, query);

// With parameters
var data5 = DataBlock.Connector.LoadSqlServer(connectionString, query, parameters);
```

### Full Configuration via Shorthand

For advanced scenarios (stored procedures, custom timeouts), use the configuration overload:

```csharp
var config = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=MyDB;Trusted_Connection=True;",
    CommandText = "sp_GetEmployeeReport",
    CommandType = CommandType.StoredProcedure,
    CommandTimeout = 120
};

var report = await DataBlock.Connector.LoadAdoAsync(config);
```

## Usage Examples

### Basic Query Execution

Execute a simple SQL query and load results into a DataBlock:

```csharp
using Datafication.Connectors.AdoConnector;
using System.Data.Common;

// Register the provider (do this once at startup)
DbProviderFactories.RegisterFactory(
    "Microsoft.Data.SqlClient",
    Microsoft.Data.SqlClient.SqlClientFactory.Instance
);

// Create configuration
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=MyDB;Trusted_Connection=True;",
    CommandText = "SELECT * FROM Employees"
};

// Create connector and execute query
var connector = new AdoDataConnector(configuration);
var employees = await connector.GetDataAsync();

Console.WriteLine($"Loaded {employees.RowCount} employees");
Console.WriteLine(await employees.TextTableAsync());
```

### Parameterized Queries

Use parameterized queries to prevent SQL injection:

```csharp
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=MyDB;Trusted_Connection=True;",
    CommandText = @"
        SELECT EmployeeId, FirstName, LastName, Department, Salary
        FROM Employees
        WHERE Department = @Department AND Salary > @MinSalary",
    Parameters = new Dictionary<string, object>
    {
        { "@Department", "Engineering" },
        { "@MinSalary", 75000 }
    }
};

var connector = new AdoDataConnector(configuration);
var engineers = await connector.GetDataAsync();

Console.WriteLine($"Found {engineers.RowCount} engineers with salary > $75,000");
```

### Executing Stored Procedures

Call stored procedures with parameters:

```csharp
using System.Data;

var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=MyDB;Trusted_Connection=True;",
    CommandText = "sp_GetEmployeesByDepartment",
    CommandType = CommandType.StoredProcedure,
    CommandTimeout = 60, // 60 second timeout for long-running procedures
    Parameters = new Dictionary<string, object>
    {
        { "@DepartmentId", 5 },
        { "@IncludeInactive", false }
    }
};

var connector = new AdoDataConnector(configuration);
var departmentEmployees = await connector.GetDataAsync();

Console.WriteLine($"Retrieved {departmentEmployees.RowCount} employees");
```

### Streaming Large Result Sets

For large datasets, stream results directly to VelocityDataBlock in batches:

```csharp
using Datafication.Storage.Velocity;

// Create VelocityDataBlock for efficient large-scale storage
var velocityBlock = new VelocityDataBlock("data/large_dataset.dfc");

// Configure database query
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=MyDB;Trusted_Connection=True;",
    CommandText = "SELECT * FROM Transactions WHERE TransactionDate >= '2024-01-01'",
    CommandTimeout = 300 // 5 minutes for large queries
};

// Stream database results in batches of 50,000 rows
var connector = new AdoDataConnector(configuration);
await connector.GetStorageDataAsync(velocityBlock, batchSize: 50000);

Console.WriteLine($"Streamed {velocityBlock.RowCount} transactions to storage");
await velocityBlock.FlushAsync();
```

### Error Handling

Configure global error handling for database operations:

```csharp
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=MyDB;Trusted_Connection=True;",
    CommandText = "SELECT * FROM Employees",
    ErrorHandler = (exception) =>
    {
        Console.WriteLine($"Database Error: {exception.Message}");
        // Log to file, send alert, etc.
    }
};

var connector = new AdoDataConnector(configuration);

try
{
    var data = await connector.GetDataAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to execute query: {ex.Message}");
}
```

### Working with Query Results

Once loaded, use the full DataBlock API for data manipulation:

```csharp
// Load sales data from database
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=SalesDB;Trusted_Connection=True;",
    CommandText = @"
        SELECT OrderId, CustomerId, ProductName, Quantity, UnitPrice, OrderDate, Region
        FROM Orders
        WHERE OrderDate >= '2024-01-01'"
};

var connector = new AdoDataConnector(configuration);
var orders = await connector.GetDataAsync();

// Filter, transform, and analyze
var westRegionSales = orders
    .Where("Region", "West")
    .Compute("TotalPrice", "Quantity * UnitPrice")
    .Compute("Month", "MONTH(OrderDate)")
    .Select("OrderId", "ProductName", "TotalPrice", "Month")
    .Sort(SortDirection.Descending, "TotalPrice")
    .Head(100);

Console.WriteLine("Top 100 West Region Orders:");
Console.WriteLine(await westRegionSales.TextTableAsync());

// Aggregate by month
var monthlyTotals = orders
    .Compute("Revenue", "Quantity * UnitPrice")
    .GroupBy("Region")
    .Aggregate(
        new[] { "Revenue" },
        new Dictionary<string, AggregationType>
        {
            { "Revenue", AggregationType.Sum }
        }
    )
    .Sort(SortDirection.Descending, "sum_Revenue");

Console.WriteLine("Revenue by Region:");
Console.WriteLine(await monthlyTotals.TextTableAsync());
```

### Using the Factory Pattern

Create connectors using the factory pattern for dependency injection scenarios:

```csharp
using Datafication.Factories.AdoConnector;

// Create the factory
var factory = new AdoDataProvider();

// Create configuration
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=MyDB;Trusted_Connection=True;",
    CommandText = "SELECT * FROM Products"
};

// Create connector via factory
var connector = factory.CreateDataConnector(configuration);
var products = await connector.GetDataAsync();

Console.WriteLine($"Loaded {products.RowCount} products");
```

## Configuration Reference

### AdoConnectorConfiguration

Configuration class for ADO.NET database connections.

**Properties:**

- **`ProviderName`** (string, required): ADO.NET provider invariant name
  - SQL Server: `"Microsoft.Data.SqlClient"`
  - PostgreSQL: `"Npgsql"`
  - MySQL: `"MySql.Data.MySqlClient"`
  - SQLite: `"Microsoft.Data.Sqlite"`
  - The provider must be registered via `DbProviderFactories.RegisterFactory()`

- **`ConnectionString`** (string, required): Database connection string
  - Format depends on the provider being used
  - Connection is automatically created, opened, and disposed

- **`CommandText`** (string, required): SQL command or stored procedure name
  - Can be a SELECT statement, stored procedure name, or other SQL command
  - Use `CommandType` to specify text vs stored procedure

- **`CommandType`** (CommandType, default: Text): Type of command
  - `CommandType.Text`: SQL query text (default)
  - `CommandType.StoredProcedure`: Stored procedure call

- **`CommandTimeout`** (int, default: 30): Command timeout in seconds
  - Time to wait before terminating command execution
  - Use higher values for long-running queries

- **`Parameters`** (Dictionary<string, object>?, optional): Query parameters
  - Key: Parameter name with provider prefix (e.g., `@ParamName` for SQL Server)
  - Value: Parameter value (null values converted to DBNull.Value)

- **`Id`** (string, auto-generated): Unique identifier for the configuration
  - Automatically generated as GUID if not specified

- **`ErrorHandler`** (Action<Exception>?, optional): Global exception handler
  - Provides centralized error handling for database operations

**Example:**

```csharp
var config = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=MyDB;Trusted_Connection=True;",
    CommandText = "SELECT * FROM Employees WHERE Department = @Dept",
    CommandType = CommandType.Text,
    CommandTimeout = 60,
    Parameters = new Dictionary<string, object>
    {
        { "@Dept", "Sales" }
    },
    Id = "employees-connector",
    ErrorHandler = ex => Console.WriteLine($"Error: {ex.Message}")
};
```

## API Reference

For complete API documentation, see the [Datafication.Connectors.AdoConnector API Reference](https://datafication.co/help/api/reference/Datafication.Connectors.AdoConnector.html).

### Core Classes

**AdoDataConnector**
- **Constructor**
  - `AdoDataConnector(AdoConnectorConfiguration configuration)` - Creates connector with validation
- **Methods**
  - `Task<DataBlock> GetDataAsync()` - Executes query and loads results into memory as DataBlock
  - `Task<IStorageDataBlock> GetStorageDataAsync(IStorageDataBlock target, int batchSize = 10000)` - Streams query results in batches
  - `string GetConnectorId()` - Returns unique connector identifier
- **Properties**
  - `AdoConnectorConfiguration Configuration` - Current configuration
- **Notes**
  - Automatically manages database connections (open/close/dispose)
  - Maps database column types to .NET types automatically
  - Supports parameterized queries for SQL injection prevention

**AdoConnectorConfiguration**
- **Properties**
  - `string ProviderName` - ADO.NET provider name (required)
  - `string ConnectionString` - Database connection string (required)
  - `string CommandText` - SQL command or stored procedure (required)
  - `CommandType CommandType` - Command type (default: Text)
  - `int CommandTimeout` - Timeout in seconds (default: 30)
  - `Dictionary<string, object>? Parameters` - Query parameters
  - `string Id` - Unique identifier (auto-generated)
  - `Action<Exception>? ErrorHandler` - Error handler

**AdoDataProvider**
- Factory class implementing `IDataConnectorFactory`
- **Methods**
  - `IDataConnector CreateDataConnector(IDataConnectorConfiguration configuration)` - Creates AdoDataConnector

**AdoConnectorValidator**
- Validates `AdoConnectorConfiguration` instances
- **Methods**
  - `ValidationResult Validate(IDataConnectorConfiguration configuration)` - Validates configuration
- **Validation Rules**
  - Configuration cannot be null
  - Id is required
  - ProviderName is required and must be registered
  - ConnectionString is required
  - CommandText is required

### Extension Methods

**AdoConnectorExtensions** (namespace: `Datafication.Extensions.Connectors.AdoConnector`)

Provides shorthand methods for loading data from common databases.

**SQL Server Methods:**
```csharp
Task<DataBlock> LoadSqlServerAsync(this ConnectorExtensions ext, string connectionString, string query)
Task<DataBlock> LoadSqlServerAsync(this ConnectorExtensions ext, string connectionString, string query, Dictionary<string, object> parameters)
DataBlock LoadSqlServer(this ConnectorExtensions ext, string connectionString, string query)
DataBlock LoadSqlServer(this ConnectorExtensions ext, string connectionString, string query, Dictionary<string, object> parameters)
```

**PostgreSQL Methods:**
```csharp
Task<DataBlock> LoadPostgresAsync(this ConnectorExtensions ext, string connectionString, string query)
Task<DataBlock> LoadPostgresAsync(this ConnectorExtensions ext, string connectionString, string query, Dictionary<string, object> parameters)
DataBlock LoadPostgres(this ConnectorExtensions ext, string connectionString, string query)
DataBlock LoadPostgres(this ConnectorExtensions ext, string connectionString, string query, Dictionary<string, object> parameters)
```

**SQLite Methods:**
```csharp
Task<DataBlock> LoadSqliteAsync(this ConnectorExtensions ext, string connectionString, string query)
Task<DataBlock> LoadSqliteAsync(this ConnectorExtensions ext, string connectionString, string query, Dictionary<string, object> parameters)
DataBlock LoadSqlite(this ConnectorExtensions ext, string connectionString, string query)
DataBlock LoadSqlite(this ConnectorExtensions ext, string connectionString, string query, Dictionary<string, object> parameters)
```

**MySQL Methods:**
```csharp
Task<DataBlock> LoadMySqlAsync(this ConnectorExtensions ext, string connectionString, string query)
Task<DataBlock> LoadMySqlAsync(this ConnectorExtensions ext, string connectionString, string query, Dictionary<string, object> parameters)
DataBlock LoadMySql(this ConnectorExtensions ext, string connectionString, string query)
DataBlock LoadMySql(this ConnectorExtensions ext, string connectionString, string query, Dictionary<string, object> parameters)
```

**Generic Configuration Methods:**
```csharp
Task<DataBlock> LoadAdoAsync(this ConnectorExtensions ext, AdoConnectorConfiguration configuration)
DataBlock LoadAdo(this ConnectorExtensions ext, AdoConnectorConfiguration configuration)
```

## Common Patterns

### ETL Pipeline from Database

```csharp
// Extract: Load from database
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=SourceDB;Trusted_Connection=True;",
    CommandText = "SELECT * FROM RawOrders WHERE ProcessedDate IS NULL"
};

var connector = new AdoDataConnector(configuration);
var rawData = await connector.GetDataAsync();

// Transform: Clean and enrich
var transformed = rawData
    .DropNulls(DropNullMode.Any)
    .Where("Status", "Cancelled", ComparisonOperator.NotEquals)
    .Compute("NetAmount", "Amount - Discount")
    .Compute("Tax", "NetAmount * 0.08")
    .Compute("Total", "NetAmount + Tax")
    .Select("OrderId", "CustomerId", "NetAmount", "Tax", "Total", "OrderDate");

// Load: Export to CSV or another system
var outputCsv = await transformed.CsvStringSinkAsync();
await File.WriteAllTextAsync("output/processed_orders.csv", outputCsv);

Console.WriteLine($"Processed {transformed.RowCount} orders");
```

### Database to VelocityDataBlock

```csharp
using Datafication.Storage.Velocity;

// Configure database source
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Npgsql",
    ConnectionString = "Host=localhost;Database=analytics;Username=user;Password=pass;",
    CommandText = @"
        SELECT event_id, user_id, event_type, event_data, created_at
        FROM user_events
        WHERE created_at >= CURRENT_DATE - INTERVAL '30 days'",
    CommandTimeout = 300
};

// Create VelocityDataBlock with primary key
var velocityBlock = VelocityDataBlock.CreateEnterprise(
    "data/user_events.dfc",
    primaryKeyColumn: "event_id"
);

// Stream database results directly to VelocityDataBlock
var connector = new AdoDataConnector(configuration);
await connector.GetStorageDataAsync(velocityBlock, batchSize: 100000);
await velocityBlock.FlushAsync();

Console.WriteLine($"Loaded {velocityBlock.RowCount} events into VelocityDataBlock");

// Now query efficiently with deferred execution
var topUsers = velocityBlock
    .GroupByAggregate("user_id", "event_id", AggregationType.Count, "event_count")
    .Sort(SortDirection.Descending, "event_count")
    .Head(100)
    .Execute();

Console.WriteLine("Top 100 most active users:");
Console.WriteLine(await topUsers.TextTableAsync());
```

### Data Analysis from SQL Query

```csharp
// Load sales data
var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=localhost;Database=SalesDB;Trusted_Connection=True;",
    CommandText = @"
        SELECT
            s.SaleId, s.SaleDate, s.Amount, s.Quantity,
            p.ProductName, p.Category,
            c.CustomerName, c.Region
        FROM Sales s
        JOIN Products p ON s.ProductId = p.ProductId
        JOIN Customers c ON s.CustomerId = c.CustomerId
        WHERE s.SaleDate >= '2024-01-01'"
};

var connector = new AdoDataConnector(configuration);
var sales = await connector.GetDataAsync();

// Regional performance analysis
var regionalStats = sales
    .GroupBy("Region")
    .Aggregate(
        new[] { "Amount", "Quantity" },
        new Dictionary<string, AggregationType>
        {
            { "Amount", AggregationType.Sum },
            { "Quantity", AggregationType.Sum }
        }
    )
    .Compute("AvgOrderValue", "sum_Amount / sum_Quantity")
    .Sort(SortDirection.Descending, "sum_Amount");

Console.WriteLine("Sales by Region:");
Console.WriteLine(await regionalStats.TextTableAsync());

// Category breakdown
var categoryStats = sales
    .GroupBy("Category")
    .Aggregate(
        new[] { "Amount" },
        new Dictionary<string, AggregationType>
        {
            { "Amount", AggregationType.Sum }
        }
    )
    .Sort(SortDirection.Descending, "sum_Amount");

Console.WriteLine("\nSales by Category:");
Console.WriteLine(await categoryStats.TextTableAsync());
```

### Multi-Database Integration

```csharp
// Load from SQL Server
var sqlServerConfig = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = "Server=sqlserver;Database=Orders;Trusted_Connection=True;",
    CommandText = "SELECT OrderId, CustomerId, Amount FROM Orders"
};

var sqlConnector = new AdoDataConnector(sqlServerConfig);
var orders = await sqlConnector.GetDataAsync();

// Load from PostgreSQL
var postgresConfig = new AdoConnectorConfiguration
{
    ProviderName = "Npgsql",
    ConnectionString = "Host=postgres;Database=Customers;Username=user;Password=pass;",
    CommandText = "SELECT customer_id, customer_name, region FROM customers"
};

var pgConnector = new AdoDataConnector(postgresConfig);
var customers = await pgConnector.GetDataAsync();

// Merge data from different databases
var merged = orders.Merge(
    customers,
    "CustomerId",
    "customer_id",
    MergeType.Left
);

Console.WriteLine($"Merged {merged.RowCount} records from SQL Server and PostgreSQL");
Console.WriteLine(await merged.Head(10).TextTableAsync());
```

## Supported Providers

| Database | Provider Package | Provider Name |
|----------|------------------|---------------|
| SQL Server | Microsoft.Data.SqlClient | `"Microsoft.Data.SqlClient"` |
| PostgreSQL | Npgsql | `"Npgsql"` |
| MySQL | MySql.Data | `"MySql.Data.MySqlClient"` |
| SQLite | Microsoft.Data.Sqlite | `"Microsoft.Data.Sqlite"` |
| Oracle | Oracle.ManagedDataAccess | `"Oracle.ManagedDataAccess.Client"` |
| SQL Server (legacy) | System.Data.SqlClient | `"System.Data.SqlClient"` |

## Performance Tips

1. **Use Streaming for Large Result Sets**: For queries returning millions of rows, use `GetStorageDataAsync` to stream data directly to VelocityDataBlock
   ```csharp
   await connector.GetStorageDataAsync(velocityBlock, batchSize: 100000);
   ```

2. **Optimize Batch Size**: Tune batch size based on row width and available memory
   - Narrow rows (few columns): Use larger batch sizes (50,000 - 100,000)
   - Wide rows (many columns): Use smaller batch sizes (10,000 - 25,000)

3. **Set Appropriate Timeouts**: Increase `CommandTimeout` for long-running queries
   ```csharp
   CommandTimeout = 300  // 5 minutes
   ```

4. **Use SELECT Column Lists**: Specify only needed columns instead of `SELECT *`
   ```csharp
   CommandText = "SELECT Id, Name, Amount FROM Orders"  // Better than SELECT *
   ```

5. **Leverage Database Filtering**: Filter data at the database level rather than loading everything
   ```csharp
   // Good: Filter in SQL
   CommandText = "SELECT * FROM Orders WHERE OrderDate >= '2024-01-01'"

   // Avoid: Loading all data then filtering
   CommandText = "SELECT * FROM Orders"  // Then .Where() in code
   ```

6. **Use Parameterized Queries**: Parameters are not just for security - they also help query plan caching
   ```csharp
   Parameters = new Dictionary<string, object> { { "@Date", startDate } }
   ```

7. **Index Your Queries**: Ensure database tables have appropriate indexes for WHERE clauses and JOINs

8. **Connection Pooling**: ADO.NET providers typically enable connection pooling by default - take advantage of it by reusing connection strings

9. **Dispose DataBlocks**: For large processing pipelines, dispose intermediate DataBlocks
   ```csharp
   using (var rawData = await connector.GetDataAsync())
   {
       var processed = rawData.Where(...).Select(...);
       // rawData automatically disposed here
   }
   ```

## Security Considerations

1. **Always Use Parameterized Queries**: Never concatenate user input into SQL strings
   ```csharp
   // CORRECT - Use parameters
   CommandText = "SELECT * FROM Users WHERE Username = @Username"
   Parameters = new Dictionary<string, object> { { "@Username", userInput } }

   // DANGEROUS - Never do this
   CommandText = $"SELECT * FROM Users WHERE Username = '{userInput}'"
   ```

2. **Protect Connection Strings**: Store connection strings securely
   - Use environment variables or secret managers
   - Never commit connection strings to source control
   - Use integrated authentication where possible

3. **Principle of Least Privilege**: Use database accounts with minimal required permissions
   - Read-only accounts for analytics queries
   - Avoid using sa/root accounts

4. **Validate Provider Registration**: The connector validates that the provider is registered, helping prevent injection of malicious providers

## License

This library is licensed under the **Datafication SDK License Agreement**. See the [LICENSE](./LICENSE) file for details.

**Summary:**
- **Free Use**: Organizations with fewer than 5 developers AND annual revenue under $500,000 USD may use the SDK without a commercial license
- **Commercial License Required**: Organizations with 5+ developers OR annual revenue exceeding $500,000 USD must obtain a commercial license
- **Open Source Exemption**: Open source projects meeting specific criteria may be exempt from developer count limits

For commercial licensing inquiries, contact [support@datafication.co](mailto:support@datafication.co).

---

**Datafication.AdoConnector** - Universal ADO.NET connectivity for the Datafication ecosystem.

For more examples and documentation, visit our [samples directory](../../samples/).
