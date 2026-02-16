# AdoFactoryPattern Sample

Demonstrates the factory pattern for creating database connectors, ideal for dependency injection scenarios.

## Overview

This sample shows how to:
- Create and use the `AdoDataProvider` factory
- Create multiple connectors from a single factory instance
- Implement dependency injection patterns
- Reuse configuration patterns
- Execute parallel queries using factory-created connectors

## Key Features Demonstrated

### Create Factory Instance

```csharp
IDataConnectorFactory factory = new AdoDataProvider();
```

### Create Connectors from Factory

```csharp
var config = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandText = "SELECT * FROM Employees",
    Id = "employees-connector"
};

IDataConnector connector = factory.CreateDataConnector(config);
var data = await connector.GetDataAsync();
```

### Dependency Injection Pattern

```csharp
public class DataService
{
    private readonly IDataConnectorFactory _factory;
    private readonly string _connectionString;

    public DataService(IDataConnectorFactory factory, string connectionString)
    {
        _factory = factory;
        _connectionString = connectionString;
    }

    public async Task<DataBlock> GetEmployeeReportAsync()
    {
        var config = new AdoConnectorConfiguration
        {
            ProviderName = "Microsoft.Data.Sqlite",
            ConnectionString = _connectionString,
            CommandText = "SELECT * FROM Employees WHERE IsActive = 1"
        };

        var connector = _factory.CreateDataConnector(config);
        return await connector.GetDataAsync();
    }
}
```

### Configuration Reuse

```csharp
// Base configuration
var baseConfig = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandTimeout = 60
};

// Create specific configurations
var employeesConfig = new AdoConnectorConfiguration
{
    ProviderName = baseConfig.ProviderName,
    ConnectionString = baseConfig.ConnectionString,
    CommandTimeout = baseConfig.CommandTimeout,
    CommandText = "SELECT * FROM Employees"
};
```

### Parallel Query Execution

```csharp
var tasks = queryConfigs.Select(config =>
{
    var connector = factory.CreateDataConnector(config);
    return connector.GetDataAsync();
});

var results = await Task.WhenAll(tasks);
```

## How to Run

```bash
cd AdoFactoryPattern
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.AdoConnector Factory Pattern Sample ===

Setting up SQLite database with multiple tables...

1. Creating AdoDataProvider factory...
   Factory created: AdoDataProvider
   Implements: IDataConnectorFactory

2. Creating multiple connectors from factory...
   Created connector: employees-connector
   Created connector: products-connector
   Created connector: orders-connector

3. Executing queries using factory-created connectors...
   Employees: 5 rows
   Products: 5 rows
   Orders: 6 rows

4. Configuration reuse pattern...
   active-employees: 4 rows
   in-stock-products: 4 rows

5. Factory validation demonstration...
   Factory rejected invalid config: Invalid configuration: ...

6. Dependency injection simulation...
   Employee report: 4 rows
   Product report: 5 rows

7. Parallel query execution using factory...
   Parallel execution results:
   - parallel-employees: 5 rows
   - parallel-products: 5 rows
   - parallel-orders: 6 rows

=== Sample Complete ===
```

## Factory Pattern Benefits

| Benefit | Description |
|---------|-------------|
| Abstraction | Code depends on `IDataConnectorFactory`, not concrete implementation |
| Testability | Easy to mock the factory for unit testing |
| Validation | Factory validates configuration before creating connectors |
| Consistency | Single point for connector creation logic |
| Flexibility | Swap implementations without changing consuming code |

## Integration with DI Containers

### ASP.NET Core Example

```csharp
// In Program.cs or Startup.cs
services.AddSingleton<IDataConnectorFactory, AdoDataProvider>();

// In a controller or service
public class ReportController
{
    private readonly IDataConnectorFactory _factory;

    public ReportController(IDataConnectorFactory factory)
    {
        _factory = factory;
    }
}
```

### Manual DI

```csharp
var factory = new AdoDataProvider();
var service = new DataService(factory, connectionString);
```

## Database Schema

**Employees**
| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Name | TEXT | Employee name |
| Department | TEXT | Department |
| Salary | REAL | Salary |
| IsActive | INTEGER | Active status |

**Products**
| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Name | TEXT | Product name |
| Price | REAL | Price |
| InStock | INTEGER | Stock status |

**Orders**
| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| ProductId | INTEGER | Product reference |
| Quantity | INTEGER | Order quantity |
| OrderDate | TEXT | Order date |

## Related Samples

- **AdoFullConfiguration** - All configuration options
- **AdoBasicQuery** - Simple query execution
- **AdoStreamingLargeData** - Streaming large datasets
