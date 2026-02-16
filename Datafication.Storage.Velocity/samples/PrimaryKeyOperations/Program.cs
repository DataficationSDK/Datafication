using Datafication.Storage.Velocity;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Storage.Velocity Primary Key Operations Sample ===\n");

await RunSampleAsync();

async Task RunSampleAsync()
{
    var dataPath = Path.Combine(Path.GetTempPath(), "velocity_pk_sample");

    // Clean up any stale files from previous runs
    if (Directory.Exists(dataPath))
        Directory.Delete(dataPath, recursive: true);
    Directory.CreateDirectory(dataPath);

    var filePath = Path.Combine(dataPath, "customers.dfc");

    // Create with primary key
    var options = new VelocityOptions { PrimaryKeyColumn = "CustomerId" };
    using var velocityBlock = new VelocityDataBlock(filePath, options);

    velocityBlock.AddColumn(new DataColumn("CustomerId", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Name", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Email", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Balance", typeof(decimal)));

    // Add initial data
    velocityBlock.AddRow(new object[] { "CUST-001", "Alice Johnson", "alice@example.com", 1500.00m });
    velocityBlock.AddRow(new object[] { "CUST-002", "Bob Smith", "bob@example.com", 2300.50m });
    velocityBlock.AddRow(new object[] { "CUST-003", "Carol White", "carol@example.com", 850.25m });
    velocityBlock.AddRow(new object[] { "CUST-004", "David Brown", "david@example.com", 3200.00m });
    velocityBlock.AddRow(new object[] { "CUST-005", "Eve Davis", "eve@example.com", 1750.75m });
    await velocityBlock.FlushAsync();

    Console.WriteLine("1. Initial data with primary key 'CustomerId':");
    PrintDataBlock(velocityBlock);

    // 2. Find row ID by primary key (O(1) lookup)
    Console.WriteLine("\n2. FindRowIdAsync - O(1) primary key lookup:");
    var rowId = await velocityBlock.FindRowIdAsync("CUST-003");
    if (rowId.HasValue)
    {
        Console.WriteLine($"   Found CUST-003 at RowId: {rowId.Value}");
        Console.WriteLine($"   Name: {velocityBlock.GetValue(rowId.Value, "Name")}");
        Console.WriteLine($"   Balance: {velocityBlock.GetValue(rowId.Value, "Balance"):C}");
    }

    // 3. Update by primary key
    Console.WriteLine("\n3. UpdateRowAsync by primary key:");
    var bobRowId = await velocityBlock.FindRowIdAsync("CUST-002");
    if (bobRowId.HasValue)
        Console.WriteLine($"   Before: CUST-002 Balance = {velocityBlock.GetValue(bobRowId.Value, "Balance"):C}");

    await velocityBlock.UpdateRowAsync("CUST-002", new object[] { "CUST-002", "Bob Smith", "bob.smith@example.com", 2800.00m });

    bobRowId = await velocityBlock.FindRowIdAsync("CUST-002");
    if (bobRowId.HasValue)
    {
        Console.WriteLine($"   After:  CUST-002 Balance = {velocityBlock.GetValue(bobRowId.Value, "Balance"):C}");
        Console.WriteLine($"   Email updated to: {velocityBlock.GetValue(bobRowId.Value, "Email")}");
    }

    // 4. Update by VelocityRowId (even faster)
    Console.WriteLine("\n4. UpdateRowAsync by VelocityRowId:");
    var aliceRowId = await velocityBlock.FindRowIdAsync("CUST-001");
    if (aliceRowId.HasValue)
    {
        Console.WriteLine($"   Before: CUST-001 Balance = {velocityBlock.GetValue(aliceRowId.Value, "Balance"):C}");

        await velocityBlock.UpdateRowAsync(aliceRowId.Value, new object[] { "CUST-001", "Alice Johnson-Smith", "alice@example.com", 1650.00m });

        Console.WriteLine($"   After:  CUST-001 Balance = {velocityBlock.GetValue(aliceRowId.Value, "Balance"):C}");
        Console.WriteLine($"   Name updated to: {velocityBlock.GetValue(aliceRowId.Value, "Name")}");
    }

    // 5. Delete by primary key
    Console.WriteLine("\n5. DeleteRowAsync by primary key:");
    Console.WriteLine($"   Row count before: {velocityBlock.RowCount}");
    Console.WriteLine($"   Deleting CUST-004...");

    await velocityBlock.DeleteRowAsync("CUST-004");

    Console.WriteLine($"   Row count after: {velocityBlock.RowCount}");

    // 6. Batch updates by primary key
    Console.WriteLine("\n6. Batch UpdateRowsAsync by primary keys:");
    var updates = new Dictionary<string, object[]>
    {
        ["CUST-001"] = new object[] { "CUST-001", "Alice Johnson-Smith", "alice@example.com", 1800.00m },
        ["CUST-003"] = new object[] { "CUST-003", "Carol White", "carol@example.com", 950.00m },
        ["CUST-005"] = new object[] { "CUST-005", "Eve Davis", "eve.davis@example.com", 2000.00m }
    };

    await velocityBlock.UpdateRowsAsync(updates);
    Console.WriteLine($"   Updated 3 customers in batch");

    // 7. Batch delete by primary keys
    Console.WriteLine("\n7. Batch DeleteRowsAsync by primary keys:");
    Console.WriteLine($"   Row count before: {velocityBlock.RowCount}");

    await velocityBlock.DeleteRowsAsync(new[] { "CUST-003", "CUST-005" });

    Console.WriteLine($"   Deleted CUST-003 and CUST-005");
    Console.WriteLine($"   Row count after: {velocityBlock.RowCount}");

    // 8. Check if row is deleted
    Console.WriteLine("\n8. IsRowDeleted check:");
    var deletedRowId = await velocityBlock.FindRowIdAsync("CUST-004");
    if (deletedRowId.HasValue)
    {
        Console.WriteLine($"   CUST-004 IsRowDeleted: {velocityBlock.IsRowDeleted(deletedRowId.Value)}");
    }
    else
    {
        Console.WriteLine($"   CUST-004 not found in index (deleted)");
    }

    // 9. Primary key index statistics
    Console.WriteLine("\n9. Primary key index statistics:");
    var (indexedKeys, indexBuilt, segments) = velocityBlock.GetPrimaryKeyIndexStats();
    Console.WriteLine($"   Indexed keys: {indexedKeys}");
    Console.WriteLine($"   Index built: {indexBuilt}");
    Console.WriteLine($"   Segments: {segments}");

    // 10. Final state
    Console.WriteLine("\n10. Final state:");
    await velocityBlock.FlushAsync(); // Refresh reader to see all updates
    PrintDataBlock(velocityBlock);

    // Dispose the VelocityDataBlock before cleanup
    velocityBlock.Dispose();

    // Cleanup
    Directory.Delete(dataPath, recursive: true);

    Console.WriteLine("\n=== Sample Complete ===");
}

static void PrintDataBlock(VelocityDataBlock data)
{
    var columns = data.Schema.GetColumnNames();

    Console.Write("    ");
    foreach (var col in columns)
    {
        Console.Write($"{col,-20} ");
    }
    Console.WriteLine();

    Console.Write("    ");
    foreach (var col in columns)
    {
        Console.Write($"{new string('-', 20)} ");
    }
    Console.WriteLine();

    var cursor = data.GetRowCursor(columns.ToArray());
    while (cursor.MoveNext())
    {
        Console.Write("    ");
        foreach (var col in columns)
        {
            var value = cursor.GetValue(col);
            string formatted = value switch
            {
                decimal d => $"{d:C}",
                _ => value?.ToString() ?? "null"
            };
            Console.Write($"{formatted,-20} ");
        }
        Console.WriteLine();
    }
}
