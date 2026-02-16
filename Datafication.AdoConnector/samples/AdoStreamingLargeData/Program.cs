using System.Data.Common;
using System.Diagnostics;
using Datafication.Connectors.AdoConnector;
using Datafication.Core.Data;
using Datafication.Storage.Velocity;
using Microsoft.Data.Sqlite;

Console.WriteLine("=== Datafication.AdoConnector Streaming Large Data Sample ===\n");

// Register SQLite provider
DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);

// Setup paths
var dbPath = Path.Combine(AppContext.BaseDirectory, "transactions.db");
var connectionString = $"Data Source={dbPath}";
var velocityPath = Path.Combine(Path.GetTempPath(), "ado_velocity_sample");

// Clean up previous runs
if (Directory.Exists(velocityPath))
{
    Directory.Delete(velocityPath, recursive: true);
}
Directory.CreateDirectory(velocityPath);

// ============================================================================
// 1. Create SQLite database with large dataset
// ============================================================================
Console.WriteLine("1. Creating SQLite database with 10,000 transactions...");
var stopwatch = Stopwatch.StartNew();
await SetupLargeDatabaseAsync(connectionString);
stopwatch.Stop();
Console.WriteLine($"   Database created in {stopwatch.ElapsedMilliseconds:N0} ms\n");

// ============================================================================
// 2. Configure ADO connector
// ============================================================================
Console.WriteLine("2. Configuring ADO connector...");

var configuration = new AdoConnectorConfiguration
{
    ProviderName = "Microsoft.Data.Sqlite",
    ConnectionString = connectionString,
    CommandText = "SELECT * FROM Transactions",
    CommandTimeout = 300,  // 5 minutes for large queries
    Id = "transactions-connector"
};

var connector = new AdoDataConnector(configuration);
Console.WriteLine($"   Connector ID: {connector.GetConnectorId()}");
Console.WriteLine($"   Command timeout: {configuration.CommandTimeout} seconds\n");

// ============================================================================
// 3. Create VelocityDataBlock for high-performance storage
// ============================================================================
Console.WriteLine("3. Creating VelocityDataBlock...");

var dfcPath = Path.Combine(velocityPath, "transactions.dfc");
var velocityOptions = VelocityOptions.CreateHighThroughput();

using var velocityBlock = new VelocityDataBlock(dfcPath, velocityOptions);
Console.WriteLine($"   Storage path: {dfcPath}");
Console.WriteLine($"   Mode: High Throughput\n");

// ============================================================================
// 4. Stream database data to VelocityDataBlock
// ============================================================================
Console.WriteLine("4. Streaming database to VelocityDataBlock...");
Console.WriteLine("   (Using batch processing for memory efficiency)\n");

stopwatch.Restart();

// Stream with batch size of 1000 rows
const int batchSize = 1000;
await connector.GetStorageDataAsync(velocityBlock, batchSize);
await velocityBlock.FlushAsync();

stopwatch.Stop();

Console.WriteLine($"   Batch size: {batchSize:N0} rows");
Console.WriteLine($"   Total rows loaded: {velocityBlock.RowCount:N0}");
Console.WriteLine($"   Stream time: {stopwatch.ElapsedMilliseconds:N0} ms");
Console.WriteLine($"   Throughput: {velocityBlock.RowCount / stopwatch.Elapsed.TotalSeconds:N0} rows/sec\n");

// ============================================================================
// 5. Display storage statistics
// ============================================================================
Console.WriteLine("5. Storage statistics...");

var stats = await velocityBlock.GetStorageStatsAsync();
Console.WriteLine($"   Total rows: {stats.TotalRows:N0}");
Console.WriteLine($"   Active rows: {stats.ActiveRows:N0}");
Console.WriteLine($"   Deleted rows: {stats.DeletedRows:N0}");
Console.WriteLine($"   Storage files: {stats.StorageFiles}");
Console.WriteLine($"   Estimated size: {stats.EstimatedSizeBytes:N0} bytes\n");

// ============================================================================
// 6. Query the VelocityDataBlock (SIMD-accelerated)
// ============================================================================
Console.WriteLine("6. Querying VelocityDataBlock...");

stopwatch.Restart();
var highValueTransactions = velocityBlock
    .Where("Amount", 500.0, ComparisonOperator.GreaterThan)
    .Execute();
stopwatch.Stop();

Console.WriteLine($"   High-value transactions (>$500): {highValueTransactions.RowCount:N0}");
Console.WriteLine($"   Query time: {stopwatch.ElapsedMilliseconds} ms\n");

// ============================================================================
// 7. Aggregation example
// ============================================================================
Console.WriteLine("7. Aggregation: Transactions by category...");

var categoryStats = velocityBlock
    .GroupByAggregate("Category", "Amount", AggregationType.Sum, "TotalAmount")
    .Execute();

Console.WriteLine("   " + new string('-', 45));
Console.WriteLine($"   {"Category",-20} {"Total Amount",-20}");
Console.WriteLine("   " + new string('-', 45));

var categoryCursor = categoryStats.GetRowCursor("Category", "TotalAmount");
while (categoryCursor.MoveNext())
{
    Console.WriteLine($"   {categoryCursor.GetValue("Category"),-20} {categoryCursor.GetValue("TotalAmount"),-20:C2}");
}
Console.WriteLine("   " + new string('-', 45) + "\n");

// ============================================================================
// 8. Sample data preview
// ============================================================================
Console.WriteLine("8. Sample data (first 5 rows):");
Console.WriteLine("   " + new string('-', 85));
Console.WriteLine($"   {"TxnId",-8} {"Date",-12} {"CustomerId",-12} {"Amount",-12} {"Category",-15} {"Status",-10}");
Console.WriteLine("   " + new string('-', 85));

var sampleCursor = velocityBlock.GetRowCursor("TransactionId", "TransactionDate", "CustomerId", "Amount", "Category", "Status");
int count = 0;
while (sampleCursor.MoveNext() && count < 5)
{
    Console.WriteLine($"   {sampleCursor.GetValue("TransactionId"),-8} {sampleCursor.GetValue("TransactionDate"),-12} {sampleCursor.GetValue("CustomerId"),-12} {sampleCursor.GetValue("Amount"),-12:C2} {sampleCursor.GetValue("Category"),-15} {sampleCursor.GetValue("Status"),-10}");
    count++;
}
Console.WriteLine("   " + new string('-', 85) + "\n");

// ============================================================================
// 9. Batch size recommendations
// ============================================================================
Console.WriteLine("9. Batch size recommendations for GetStorageDataAsync:");
Console.WriteLine("   +-----------------------+--------------------------------------+");
Console.WriteLine("   | Data Characteristics  | Recommended Batch Size               |");
Console.WriteLine("   +-----------------------+--------------------------------------+");
Console.WriteLine("   | Narrow rows (<10 cols)| 50,000 - 100,000 rows                |");
Console.WriteLine("   | Medium rows (10-50)   | 10,000 - 25,000 rows                 |");
Console.WriteLine("   | Wide rows (50+ cols)  | 5,000 - 10,000 rows                  |");
Console.WriteLine("   | Limited memory        | 1,000 - 5,000 rows                   |");
Console.WriteLine("   +-----------------------+--------------------------------------+\n");

// Cleanup
File.Delete(dbPath);
Directory.Delete(velocityPath, recursive: true);

Console.WriteLine("=== Summary ===");
Console.WriteLine($"   Database rows: 10,000");
Console.WriteLine($"   VelocityDataBlock rows: {velocityBlock.RowCount:N0}");
Console.WriteLine("   Benefits of streaming to VelocityDataBlock:");
Console.WriteLine("   - Memory efficient (processes in batches)");
Console.WriteLine("   - Disk-backed storage (handles datasets larger than RAM)");
Console.WriteLine("   - SIMD-accelerated queries");
Console.WriteLine("   - Persistent storage (data survives restarts)");

Console.WriteLine("\n=== Sample Complete ===");

// Helper method to set up large database
async Task SetupLargeDatabaseAsync(string connString)
{
    using var connection = new SqliteConnection(connString);
    await connection.OpenAsync();

    // Create table
    using var createCmd = connection.CreateCommand();
    createCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Transactions (
            TransactionId INTEGER PRIMARY KEY,
            TransactionDate TEXT NOT NULL,
            CustomerId TEXT NOT NULL,
            Amount REAL NOT NULL,
            Category TEXT NOT NULL,
            Status TEXT NOT NULL
        )";
    await createCmd.ExecuteNonQueryAsync();

    // Insert 10,000 transactions
    var random = new Random(42); // Seed for reproducibility
    var categories = new[] { "Electronics", "Clothing", "Food", "Services", "Entertainment" };
    var statuses = new[] { "Completed", "Pending", "Refunded" };
    var baseDate = new DateTime(2024, 1, 1);

    using var transaction = connection.BeginTransaction();

    for (int i = 1; i <= 10000; i++)
    {
        using var insertCmd = connection.CreateCommand();
        insertCmd.Transaction = transaction;
        insertCmd.CommandText = @"
            INSERT INTO Transactions (TransactionId, TransactionDate, CustomerId, Amount, Category, Status)
            VALUES ($id, $date, $customerId, $amount, $category, $status)";

        insertCmd.Parameters.AddWithValue("$id", i);
        insertCmd.Parameters.AddWithValue("$date", baseDate.AddDays(random.Next(365)).ToString("yyyy-MM-dd"));
        insertCmd.Parameters.AddWithValue("$customerId", $"C{random.Next(1, 501):D4}");
        insertCmd.Parameters.AddWithValue("$amount", Math.Round(random.NextDouble() * 1000 + 10, 2));
        insertCmd.Parameters.AddWithValue("$category", categories[random.Next(categories.Length)]);
        insertCmd.Parameters.AddWithValue("$status", statuses[random.Next(statuses.Length)]);

        await insertCmd.ExecuteNonQueryAsync();
    }

    await transaction.CommitAsync();
}
