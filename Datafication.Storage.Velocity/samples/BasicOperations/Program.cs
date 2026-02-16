using Datafication.Storage.Velocity;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Storage.Velocity Basic Operations Sample ===\n");

await RunSampleAsync();

async Task RunSampleAsync()
{
    var dataPath = Path.Combine(Path.GetTempPath(), "velocity_samples");

    // Clean up any stale files from previous runs
    if (Directory.Exists(dataPath))
        Directory.Delete(dataPath, recursive: true);
    Directory.CreateDirectory(dataPath);

    // 1. Create a new VelocityDataBlock
    Console.WriteLine("1. Creating a new VelocityDataBlock:");
    var filePath = Path.Combine(dataPath, "employees.dfc");
    using var velocityBlock = new VelocityDataBlock(filePath);

    velocityBlock.AddColumn(new DataColumn("EmployeeId", typeof(int)));
    velocityBlock.AddColumn(new DataColumn("Name", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Department", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Salary", typeof(decimal)));

    Console.WriteLine($"   Created VelocityDataBlock at: {filePath}");
    Console.WriteLine($"   Columns: {string.Join(", ", velocityBlock.Schema.GetColumnNames())}\n");

    // Add some data
    velocityBlock.AddRow(new object[] { 1, "Alice Johnson", "Engineering", 95000m });
    velocityBlock.AddRow(new object[] { 2, "Bob Smith", "Marketing", 72000m });
    velocityBlock.AddRow(new object[] { 3, "Carol White", "Engineering", 88000m });
    await velocityBlock.FlushAsync();

    // 2. Create with VelocityOptions
    Console.WriteLine("2. Creating with VelocityOptions:");
    var options = new VelocityOptions
    {
        PrimaryKeyColumn = "ProductId",
        DefaultCompression = VelocityCompressionType.LZ4,
        EnableAutoCompression = true,
        AutoCompactionEnabled = true
    };
    var productsPath = Path.Combine(dataPath, "products.dfc");
    using var productsBlock = new VelocityDataBlock(productsPath, options);
    productsBlock.AddColumn(new DataColumn("ProductId", typeof(int)));
    productsBlock.AddColumn(new DataColumn("Name", typeof(string)));
    productsBlock.AddColumn(new DataColumn("Price", typeof(decimal)));
    Console.WriteLine($"   Created with primary key: ProductId");
    Console.WriteLine($"   Compression: {options.DefaultCompression}\n");

    // 3. Factory methods
    Console.WriteLine("3. Using factory methods:");
    var enterprisePath = Path.Combine(dataPath, "orders_enterprise.dfc");
    using var enterpriseBlock = VelocityDataBlock.CreateEnterprise(enterprisePath, "OrderId");
    enterpriseBlock.AddColumn(new DataColumn("OrderId", typeof(int)));
    enterpriseBlock.AddColumn(new DataColumn("Amount", typeof(decimal)));
    Console.WriteLine("   CreateEnterprise: Optimized for frequent updates");

    var highThroughputPath = Path.Combine(dataPath, "events_ht.dfc");
    using var htBlock = VelocityDataBlock.CreateHighThroughput(highThroughputPath, "EventId");
    htBlock.AddColumn(new DataColumn("EventId", typeof(int)));
    htBlock.AddColumn(new DataColumn("EventType", typeof(string)));
    Console.WriteLine("   CreateHighThroughput: Optimized for high-speed ingestion\n");

    // 4. Save a DataBlock to DFC format
    Console.WriteLine("4. Saving a DataBlock to DFC format:");
    var sourceBlock = new DataBlock();
    sourceBlock.AddColumn(new DataColumn("Id", typeof(int)));
    sourceBlock.AddColumn(new DataColumn("Value", typeof(string)));
    sourceBlock.AddRow(new object[] { 1, "First" });
    sourceBlock.AddRow(new object[] { 2, "Second" });

    var savedPath = Path.Combine(dataPath, "saved_data.dfc");
    using var savedBlock = await VelocityDataBlock.SaveAsync(
        savedPath,
        sourceBlock,
        new VelocityOptions { PrimaryKeyColumn = "Id" }
    );
    Console.WriteLine($"   Saved {savedBlock.RowCount} rows to: {savedPath}\n");

    // 5. Open an existing DFC file
    Console.WriteLine("5. Opening an existing DFC file:");
    using var reopened = await VelocityDataBlock.OpenAsync(savedPath);
    Console.WriteLine($"   Opened file with {reopened.RowCount} rows");
    Console.WriteLine($"   Columns: {string.Join(", ", reopened.Schema.GetColumnNames())}\n");

    // 6. Basic row and column access
    Console.WriteLine("6. Basic row and column access:");
    Console.WriteLine($"   RowCount: {velocityBlock.RowCount}");
    Console.WriteLine($"   HasColumn('Name'): {velocityBlock.HasColumn("Name")}");
    Console.WriteLine($"   GetValue(0, 1) [Name column]: {velocityBlock.GetValue(0, 1)}");
    Console.WriteLine($"   GetValue(0, 3) [Salary column]: {velocityBlock.GetValue(0, 3):C}\n");

    // 7. Iterate using row cursor
    Console.WriteLine("7. Iterating with GetRowCursor:");
    var cursor = velocityBlock.GetRowCursor("Name", "Department", "Salary");
    while (cursor.MoveNext())
    {
        var name = cursor.GetValue("Name");
        var dept = cursor.GetValue("Department");
        var salary = cursor.GetValue("Salary");
        Console.WriteLine($"   {name} - {dept}: {salary:C}");
    }

    // Dispose VelocityDataBlocks before cleanup
    velocityBlock.Dispose();
    productsBlock.Dispose();
    enterpriseBlock.Dispose();
    htBlock.Dispose();
    savedBlock.Dispose();
    reopened.Dispose();

    // Cleanup
    Directory.Delete(dataPath, recursive: true);

    Console.WriteLine("\n=== Sample Complete ===");
}
