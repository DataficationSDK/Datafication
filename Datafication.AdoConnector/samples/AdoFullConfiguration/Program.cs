using System.Data;
using System.Data.Common;
using Datafication.Connectors.AdoConnector;
using Datafication.Core.Data;
using Microsoft.Data.Sqlite;

Console.WriteLine("=== Datafication.AdoConnector Full Configuration Sample ===\n");

// Register SQLite provider
DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);

// Create SQLite database
var dbPath = Path.Combine(AppContext.BaseDirectory, "config_demo.db");
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("Setting up SQLite database...\n");
await SetupDatabaseAsync(connectionString);

// ============================================================================
// 1. Full Configuration with All Options
// ============================================================================
Console.WriteLine("1. Creating full configuration with all options...");

var errorMessages = new List<string>();

var fullConfiguration = new AdoConnectorConfiguration
{
    // Required: Provider name (must be registered)
    ProviderName = "Microsoft.Data.Sqlite",

    // Required: Connection string
    ConnectionString = connectionString,

    // Required: SQL command or stored procedure name
    CommandText = "SELECT * FROM Employees WHERE Department = @Dept",

    // Optional: Command type (Text is default)
    CommandType = CommandType.Text,

    // Optional: Command timeout in seconds (default is 30)
    CommandTimeout = 120,

    // Optional: Query parameters
    Parameters = new Dictionary<string, object>
    {
        { "@Dept", "Engineering" }
    },

    // Optional: Custom identifier (auto-generated if not specified)
    Id = "employees-by-department",

    // Optional: Global error handler
    ErrorHandler = (exception) =>
    {
        errorMessages.Add($"[{DateTime.Now:HH:mm:ss}] Error: {exception.Message}");
        Console.WriteLine($"   ErrorHandler called: {exception.Message}");
    }
};

Console.WriteLine("   Configuration created with:");
Console.WriteLine($"   - ProviderName: {fullConfiguration.ProviderName}");
Console.WriteLine($"   - CommandType: {fullConfiguration.CommandType}");
Console.WriteLine($"   - CommandTimeout: {fullConfiguration.CommandTimeout} seconds");
Console.WriteLine($"   - Parameters: {fullConfiguration.Parameters?.Count ?? 0}");
Console.WriteLine($"   - Id: {fullConfiguration.Id}");
Console.WriteLine($"   - ErrorHandler: {(fullConfiguration.ErrorHandler != null ? "Configured" : "Not configured")}\n");

// ============================================================================
// 2. Configuration Validation
// ============================================================================
Console.WriteLine("2. Validating configuration...");

var validator = new AdoConnectorValidator();
var validationResult = validator.Validate(fullConfiguration);

Console.WriteLine($"   Validation result: {(validationResult.IsValid ? "Valid" : "Invalid")}");
if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"   - {error}");
    }
}
Console.WriteLine();

// ============================================================================
// 3. Demonstrate Validation Errors
// ============================================================================
Console.WriteLine("3. Demonstrating validation errors...");

// Missing required fields
var invalidConfig = new AdoConnectorConfiguration
{
    ProviderName = "",  // Empty - will fail
    ConnectionString = connectionString,
    CommandText = ""    // Empty - will fail
};

var invalidResult = validator.Validate(invalidConfig);
Console.WriteLine($"   Configuration with missing fields:");
Console.WriteLine($"   - Valid: {invalidResult.IsValid}");
foreach (var error in invalidResult.Errors)
{
    Console.WriteLine($"   - Error: {error}");
}
Console.WriteLine();

// Invalid provider
var badProviderConfig = new AdoConnectorConfiguration
{
    ProviderName = "NonExistent.Provider",
    ConnectionString = connectionString,
    CommandText = "SELECT 1"
};

var badProviderResult = validator.Validate(badProviderConfig);
Console.WriteLine($"   Configuration with invalid provider:");
Console.WriteLine($"   - Valid: {badProviderResult.IsValid}");
foreach (var error in badProviderResult.Errors)
{
    Console.WriteLine($"   - Error: {error}");
}
Console.WriteLine();

// ============================================================================
// 4. Execute with Valid Configuration
// ============================================================================
Console.WriteLine("4. Executing query with valid configuration...");

var connector = new AdoDataConnector(fullConfiguration);
Console.WriteLine($"   Connector ID: {connector.GetConnectorId()}");

var engineers = await connector.GetDataAsync();
Console.WriteLine($"   Loaded {engineers.RowCount} engineers\n");

// Display results
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine($"   {"Id",-5} {"Name",-20} {"Department",-15} {"Salary",-12}");
Console.WriteLine("   " + new string('-', 60));

var cursor = engineers.GetRowCursor("Id", "Name", "Department", "Salary");
while (cursor.MoveNext())
{
    Console.WriteLine($"   {cursor.GetValue("Id"),-5} {cursor.GetValue("Name"),-20} {cursor.GetValue("Department"),-15} {cursor.GetValue("Salary"),-12:C0}");
}
Console.WriteLine("   " + new string('-', 60) + "\n");

// ============================================================================
// 5. Error Handler Demonstration
// ============================================================================
Console.WriteLine("5. Demonstrating error handler...");

var errorConfig = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandText = "SELECT * FROM NonExistentTable",
    ErrorHandler = (exception) =>
    {
        Console.WriteLine($"   Error caught by handler: {exception.Message}");
    }
};

var errorConnector = new AdoDataConnector(errorConfig);

try
{
    await errorConnector.GetDataAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"   Exception propagated: {ex.Message}\n");
}

// ============================================================================
// 6. Custom Timeout Demonstration
// ============================================================================
Console.WriteLine("6. Timeout configuration...");
Console.WriteLine("   Different timeout scenarios:");
Console.WriteLine("   - Quick queries: 30 seconds (default)");
Console.WriteLine("   - Complex reports: 120-300 seconds");
Console.WriteLine("   - ETL operations: 600+ seconds");
Console.WriteLine("   - Current configuration: 120 seconds\n");

// ============================================================================
// 7. Command Type Options
// ============================================================================
Console.WriteLine("7. Command type options:");
Console.WriteLine("   - CommandType.Text (default): SQL query text");
Console.WriteLine("   - CommandType.StoredProcedure: Stored procedure name");
Console.WriteLine("   - CommandType.TableDirect: Direct table access (provider-specific)");
Console.WriteLine();

// Example stored procedure configuration (for documentation)
Console.WriteLine("   Stored procedure example:");
Console.WriteLine("   ```");
Console.WriteLine("   var spConfig = new AdoConnectorConfiguration");
Console.WriteLine("   {");
Console.WriteLine("       ProviderName = \"Microsoft.Data.SqlClient\",");
Console.WriteLine("       ConnectionString = connectionString,");
Console.WriteLine("       CommandText = \"sp_GetEmployeesByDepartment\",");
Console.WriteLine("       CommandType = CommandType.StoredProcedure,");
Console.WriteLine("       Parameters = new Dictionary<string, object>");
Console.WriteLine("       {");
Console.WriteLine("           { \"@DepartmentId\", 5 }");
Console.WriteLine("       }");
Console.WriteLine("   };");
Console.WriteLine("   ```\n");

// ============================================================================
// 8. Configuration Best Practices
// ============================================================================
Console.WriteLine("8. Configuration best practices:");
Console.WriteLine("   +----------------------------------+----------------------------------------+");
Console.WriteLine("   | Practice                         | Recommendation                         |");
Console.WriteLine("   +----------------------------------+----------------------------------------+");
Console.WriteLine("   | Connection strings               | Use environment variables or secrets   |");
Console.WriteLine("   | Provider registration            | Do once at application startup         |");
Console.WriteLine("   | Timeouts                         | Set based on expected query duration   |");
Console.WriteLine("   | Error handlers                   | Log errors for debugging               |");
Console.WriteLine("   | Configuration IDs                | Use descriptive names for debugging    |");
Console.WriteLine("   | Parameters                       | Always use for user input (security)   |");
Console.WriteLine("   +----------------------------------+----------------------------------------+");

// Cleanup
File.Delete(dbPath);

Console.WriteLine("\n=== Sample Complete ===");

// Helper method to set up the SQLite database
async Task SetupDatabaseAsync(string connString)
{
    using var connection = new SqliteConnection(connString);
    await connection.OpenAsync();

    using var createCmd = connection.CreateCommand();
    createCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Employees (
            Id INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            Department TEXT NOT NULL,
            Salary REAL NOT NULL
        )";
    await createCmd.ExecuteNonQueryAsync();

    var employees = new[]
    {
        (1, "Alice Johnson", "Engineering", 95000.00),
        (2, "Bob Smith", "Sales", 75000.00),
        (3, "Carol Williams", "Engineering", 105000.00),
        (4, "David Brown", "Marketing", 68000.00),
        (5, "Eve Davis", "Engineering", 88000.00),
        (6, "Frank Miller", "Sales", 72000.00),
        (7, "Grace Wilson", "Engineering", 115000.00),
        (8, "Henry Taylor", "HR", 65000.00)
    };

    foreach (var (id, name, dept, salary) in employees)
    {
        using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Employees VALUES ($id, $name, $dept, $salary)";
        insertCmd.Parameters.AddWithValue("$id", id);
        insertCmd.Parameters.AddWithValue("$name", name);
        insertCmd.Parameters.AddWithValue("$dept", dept);
        insertCmd.Parameters.AddWithValue("$salary", salary);
        await insertCmd.ExecuteNonQueryAsync();
    }
}
