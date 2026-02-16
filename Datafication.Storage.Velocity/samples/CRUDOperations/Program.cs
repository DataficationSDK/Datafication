using Datafication.Storage.Velocity;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Storage.Velocity CRUD Operations Sample ===\n");

await RunSampleAsync();

async Task RunSampleAsync()
{
    var dataPath = Path.Combine(Path.GetTempPath(), "velocity_crud_sample");

    // Clean up any stale files from previous runs
    if (Directory.Exists(dataPath))
        Directory.Delete(dataPath, recursive: true);
    Directory.CreateDirectory(dataPath);

    var filePath = Path.Combine(dataPath, "products.dfc");

    using var velocityBlock = new VelocityDataBlock(filePath);

    // Set up schema: ProductId(0), Name(1), Category(2), Price(3), InStock(4)
    velocityBlock.AddColumn(new DataColumn("ProductId", typeof(int)));
    velocityBlock.AddColumn(new DataColumn("Name", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Category", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Price", typeof(decimal)));
    velocityBlock.AddColumn(new DataColumn("InStock", typeof(bool)));

    // 1. Create - Adding rows
    Console.WriteLine("1. CREATE - Adding rows:");
    velocityBlock.AddRow(new object[] { 1, "Laptop", "Electronics", 999.99m, true });
    velocityBlock.AddRow(new object[] { 2, "Mouse", "Electronics", 29.99m, true });
    velocityBlock.AddRow(new object[] { 3, "Keyboard", "Electronics", 79.99m, false });
    velocityBlock.AddRow(new object[] { 4, "Monitor", "Electronics", 399.99m, true });
    velocityBlock.AddRow(new object[] { 5, "Headphones", "Audio", 149.99m, true });
    await velocityBlock.FlushAsync();

    Console.WriteLine($"   Added 5 products");
    Console.WriteLine($"   RowCount: {velocityBlock.RowCount}\n");
    PrintDataBlock(velocityBlock, "ProductId", "Name", "Price", "InStock");

    // 2. Read - Accessing values using GetValue
    Console.WriteLine("\n2. READ - Accessing values with GetValue(row, columnIndex):");
    Console.WriteLine($"   GetValue(0, 1) [Name]: {velocityBlock.GetValue(0, 1)}");
    Console.WriteLine($"   GetValue(0, 3) [Price]: {velocityBlock.GetValue(0, 3):C}");
    Console.WriteLine($"   GetValue(2, 4) [InStock]: {velocityBlock.GetValue(2, 4)}\n");

    // 3. Update - Modifying an entire row
    Console.WriteLine("3. UPDATE - Modifying row at index 2:");
    Console.WriteLine($"   Before: {velocityBlock.GetValue(2, 1)} - {velocityBlock.GetValue(2, 3):C} - InStock: {velocityBlock.GetValue(2, 4)}");

    velocityBlock.UpdateRow(2, new object[] { 3, "Mechanical Keyboard", "Electronics", 129.99m, true });

    Console.WriteLine($"   After:  {velocityBlock.GetValue(2, 1)} - {velocityBlock.GetValue(2, 3):C} - InStock: {velocityBlock.GetValue(2, 4)}\n");

    // 4. Update - Updating row 0 with new price
    Console.WriteLine("4. UPDATE - Updating row 0 with new price:");
    Console.WriteLine($"   Before: {velocityBlock.GetValue(0, 1)} - {velocityBlock.GetValue(0, 3):C}");

    // Update entire row with modified price
    velocityBlock.UpdateRow(0, new object[] { 1, "Laptop", "Electronics", 899.99m, true });

    Console.WriteLine($"   After:  {velocityBlock.GetValue(0, 1)} - {velocityBlock.GetValue(0, 3):C}\n");

    // 5. Delete - Removing a row
    Console.WriteLine("5. DELETE - Removing row at index 1:");
    Console.WriteLine($"   Row count before: {velocityBlock.RowCount}");
    Console.WriteLine($"   Removing: {velocityBlock.GetValue(1, 1)}");

    velocityBlock.RemoveRow(1);

    Console.WriteLine($"   Row count after: {velocityBlock.RowCount}\n");

    // 6. Batch append
    Console.WriteLine("6. BATCH APPEND - Adding multiple rows efficiently:");
    var batch = new DataBlock();
    batch.AddColumn(new DataColumn("ProductId", typeof(int)));
    batch.AddColumn(new DataColumn("Name", typeof(string)));
    batch.AddColumn(new DataColumn("Category", typeof(string)));
    batch.AddColumn(new DataColumn("Price", typeof(decimal)));
    batch.AddColumn(new DataColumn("InStock", typeof(bool)));

    batch.AddRow(new object[] { 6, "Webcam", "Electronics", 89.99m, true });
    batch.AddRow(new object[] { 7, "USB Hub", "Electronics", 34.99m, true });
    batch.AddRow(new object[] { 8, "Speakers", "Audio", 199.99m, false });

    await velocityBlock.AppendBatchAsync(batch);
    await velocityBlock.FlushAsync();

    Console.WriteLine($"   Appended 3 rows in batch");
    Console.WriteLine($"   New row count: {velocityBlock.RowCount}\n");

    // 7. Final state
    Console.WriteLine("7. Final state of data:");
    PrintDataBlock(velocityBlock, "ProductId", "Name", "Category", "Price", "InStock");

    // Dispose the VelocityDataBlock before cleanup
    velocityBlock.Dispose();

    // Cleanup
    Directory.Delete(dataPath, recursive: true);

    Console.WriteLine("\n=== Sample Complete ===");
}

static void PrintDataBlock(VelocityDataBlock data, params string[] columns)
{
    // Header
    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{col,-20} ");
    }
    Console.WriteLine();

    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{new string('-', 20)} ");
    }
    Console.WriteLine();

    // Rows
    var cursor = data.GetRowCursor(columns);
    while (cursor.MoveNext())
    {
        Console.Write("   ");
        foreach (var col in columns)
        {
            var value = cursor.GetValue(col);
            string formatted = value switch
            {
                decimal d => $"{d:C}",
                bool b => b ? "Yes" : "No",
                _ => value?.ToString() ?? "null"
            };
            Console.Write($"{formatted,-20} ");
        }
        Console.WriteLine();
    }
}
