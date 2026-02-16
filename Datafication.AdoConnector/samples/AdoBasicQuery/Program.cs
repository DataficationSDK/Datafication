using System.Data.Common;
using Datafication.Core.Data;
using Datafication.Extensions.Connectors.AdoConnector;
using Microsoft.Data.Sqlite;

Console.WriteLine("=== Datafication.AdoConnector Basic Query Sample ===\n");

// Register SQLite provider (required once at startup)
DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);

// Create SQLite database with sample data
var dbPath = Path.Combine(AppContext.BaseDirectory, "employees.db");
var connectionString = $"Data Source={dbPath}";

Console.WriteLine("Setting up SQLite database with sample data...\n");
await SetupDatabaseAsync(connectionString);

// 1. Load data using async shorthand (recommended)
Console.WriteLine("1. Loading data asynchronously...");
var employeesAsync = await DataBlock.Connector.LoadSqliteAsync(
    connectionString,
    "SELECT * FROM Employees"
);
Console.WriteLine($"   Loaded {employeesAsync.RowCount} rows with {employeesAsync.Schema.Count} columns\n");

// 2. Display schema information
Console.WriteLine("2. Schema Information:");
foreach (var colName in employeesAsync.Schema.GetColumnNames())
{
    var column = employeesAsync.GetColumn(colName);
    Console.WriteLine($"   - {colName}: {column.DataType.GetClrType().Name}");
}
Console.WriteLine();

// 3. Load data using sync shorthand (alternative)
Console.WriteLine("3. Loading data synchronously...");
var employeesSync = DataBlock.Connector.LoadSqlite(
    connectionString,
    "SELECT Id, Name, Department FROM Employees"
);
Console.WriteLine($"   Loaded {employeesSync.RowCount} rows\n");

// 4. Display sample data using row cursor
Console.WriteLine("4. First 5 employees:");
Console.WriteLine("   " + new string('-', 75));
Console.WriteLine($"   {"Id",-5} {"Name",-20} {"Department",-15} {"Salary",-12} {"Active",-8}");
Console.WriteLine("   " + new string('-', 75));

var cursor = employeesAsync.GetRowCursor("Id", "Name", "Department", "Salary", "IsActive");
int rowCount = 0;
while (cursor.MoveNext() && rowCount < 5)
{
    var id = cursor.GetValue("Id");
    var name = cursor.GetValue("Name");
    var dept = cursor.GetValue("Department");
    var salary = cursor.GetValue("Salary");
    var active = cursor.GetValue("IsActive");
    Console.WriteLine($"   {id,-5} {name,-20} {dept,-15} {salary,-12:C0} {active,-8}");
    rowCount++;
}
Console.WriteLine("   " + new string('-', 75));
Console.WriteLine($"   ... and {employeesAsync.RowCount - 5} more rows\n");

// 5. Basic filtering example
Console.WriteLine("5. Filtering: Engineering department employees...");
var engineers = employeesAsync.Where("Department", "Engineering");
Console.WriteLine($"   Found {engineers.RowCount} engineers\n");

// 6. Basic sorting example
Console.WriteLine("6. Sorting: Top 3 highest salaries...");
var topEarners = employeesAsync
    .Sort(SortDirection.Descending, "Salary")
    .Head(3);

Console.WriteLine("   " + new string('-', 45));
Console.WriteLine($"   {"Name",-25} {"Salary",-15}");
Console.WriteLine("   " + new string('-', 45));

var topCursor = topEarners.GetRowCursor("Name", "Salary");
while (topCursor.MoveNext())
{
    var name = topCursor.GetValue("Name");
    var salary = topCursor.GetValue("Salary");
    Console.WriteLine($"   {name,-25} {salary,-15:C0}");
}
Console.WriteLine("   " + new string('-', 45));

// Cleanup
File.Delete(dbPath);

Console.WriteLine("\n=== Sample Complete ===");

// Helper method to set up the SQLite database
async Task SetupDatabaseAsync(string connString)
{
    using var connection = new SqliteConnection(connString);
    await connection.OpenAsync();

    // Create table
    using var createCmd = connection.CreateCommand();
    createCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Employees (
            Id INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            Department TEXT NOT NULL,
            Salary REAL NOT NULL,
            HireDate TEXT NOT NULL,
            IsActive INTEGER NOT NULL
        )";
    await createCmd.ExecuteNonQueryAsync();

    // Insert sample data
    var employees = new[]
    {
        (1, "Alice Johnson", "Engineering", 95000.00, "2020-03-15", 1),
        (2, "Bob Smith", "Sales", 75000.00, "2019-07-22", 1),
        (3, "Carol Williams", "Engineering", 105000.00, "2018-01-10", 1),
        (4, "David Brown", "Marketing", 68000.00, "2021-05-08", 1),
        (5, "Eve Davis", "Engineering", 88000.00, "2020-11-30", 1),
        (6, "Frank Miller", "Sales", 72000.00, "2019-04-18", 0),
        (7, "Grace Wilson", "HR", 65000.00, "2022-02-14", 1),
        (8, "Henry Taylor", "Engineering", 115000.00, "2017-09-05", 1),
        (9, "Ivy Anderson", "Marketing", 71000.00, "2021-08-20", 1),
        (10, "Jack Thomas", "Sales", 79000.00, "2018-12-01", 1)
    };

    foreach (var (id, name, dept, salary, hireDate, active) in employees)
    {
        using var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO Employees (Id, Name, Department, Salary, HireDate, IsActive)
            VALUES ($id, $name, $dept, $salary, $hireDate, $active)";
        insertCmd.Parameters.AddWithValue("$id", id);
        insertCmd.Parameters.AddWithValue("$name", name);
        insertCmd.Parameters.AddWithValue("$dept", dept);
        insertCmd.Parameters.AddWithValue("$salary", salary);
        insertCmd.Parameters.AddWithValue("$hireDate", hireDate);
        insertCmd.Parameters.AddWithValue("$active", active);
        await insertCmd.ExecuteNonQueryAsync();
    }
}
