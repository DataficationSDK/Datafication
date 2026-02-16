# AdoFullConfiguration Sample

Demonstrates all configuration options available in AdoConnectorConfiguration.

## Overview

This sample shows how to:
- Configure all properties of AdoConnectorConfiguration
- Validate configurations before use
- Handle validation errors
- Use error handlers for exception management
- Configure timeouts for long-running queries
- Work with different command types

## Key Features Demonstrated

### Full Configuration

```csharp
var configuration = new AdoConnectorConfiguration
{
    // Required
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = "Data Source=mydb.db",
    CommandText = "SELECT * FROM Employees WHERE Department = @Dept",

    // Optional
    CommandType = CommandType.Text,     // Default: Text
    CommandTimeout = 120,               // Default: 30 seconds
    Id = "employees-connector",         // Default: Auto-generated GUID

    // Parameters
    Parameters = new Dictionary<string, object>
    {
        { "@Dept", "Engineering" }
    },

    // Error handling
    ErrorHandler = (exception) =>
    {
        Console.WriteLine($"Error: {exception.Message}");
    }
};
```

### Configuration Validation

```csharp
var validator = new AdoConnectorValidator();
var result = validator.Validate(configuration);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### Error Handler

```csharp
ErrorHandler = (exception) =>
{
    // Log to file, send alert, etc.
    logger.LogError(exception, "Database query failed");
}
```

### Stored Procedure Configuration

```csharp
var spConfig = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.SqlClient",
    ConnectionString = connectionString,
    CommandText = "sp_GetEmployeesByDepartment",
    CommandType = CommandType.StoredProcedure,
    CommandTimeout = 60,
    Parameters = new Dictionary<string, object>
    {
        { "@DepartmentId", 5 },
        { "@IncludeInactive", false }
    }
};
```

## How to Run

```bash
cd AdoFullConfiguration
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.AdoConnector Full Configuration Sample ===

Setting up SQLite database...

1. Creating full configuration with all options...
   Configuration created with:
   - ProviderName: Microsoft.Data.Sqlite
   - CommandType: Text
   - CommandTimeout: 120 seconds
   - Parameters: 1
   - Id: employees-by-department
   - ErrorHandler: Configured

2. Validating configuration...
   Validation result: Valid

3. Demonstrating validation errors...
   Configuration with missing fields:
   - Valid: False
   - Error: ProviderName is required and cannot be empty.
   - Error: CommandText is required and cannot be empty.

   Configuration with invalid provider:
   - Valid: False
   - Error: Provider 'NonExistent.Provider' is not registered...

4. Executing query with valid configuration...
   Connector ID: employees-by-department
   Loaded 4 engineers

5. Demonstrating error handler...
   Error caught by handler: SQLite Error 1: 'no such table: NonExistentTable'
   Exception propagated: SQLite Error 1: 'no such table: NonExistentTable'

=== Sample Complete ===
```

## Configuration Reference

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| ProviderName | string | Yes | - | ADO.NET provider invariant name |
| ConnectionString | string | Yes | - | Database connection string |
| CommandText | string | Yes | - | SQL query or stored procedure name |
| CommandType | CommandType | No | Text | Type of command to execute |
| CommandTimeout | int | No | 30 | Timeout in seconds |
| Parameters | Dictionary | No | null | Query parameters |
| Id | string | No | GUID | Unique identifier |
| ErrorHandler | Action | No | null | Exception handler callback |

## Validation Rules

The `AdoConnectorValidator` checks:
1. Configuration is not null
2. Configuration is correct type (AdoConnectorConfiguration)
3. Id is not empty
4. ProviderName is not empty and is registered
5. ConnectionString is not empty
6. CommandText is not empty

## Best Practices

| Practice | Recommendation |
|----------|----------------|
| Connection strings | Use environment variables or secret managers |
| Provider registration | Register once at application startup |
| Timeouts | Set based on expected query duration |
| Error handlers | Log errors for debugging and monitoring |
| Configuration IDs | Use descriptive names for easier debugging |
| Parameters | Always use for user input (SQL injection prevention) |

## Related Samples

- **AdoBasicQuery** - Simple query execution
- **AdoParameterizedQueries** - Secure parameterized queries
- **AdoFactoryPattern** - Factory pattern for dependency injection
