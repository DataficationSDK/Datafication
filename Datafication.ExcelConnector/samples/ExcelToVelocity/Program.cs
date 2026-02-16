using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ExcelConnector;
using Datafication.Connectors.ExcelConnector;
using Datafication.Storage.Velocity;

Console.WriteLine("=== Datafication.ExcelConnector to Velocity Sample ===\n");

var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data");
var excelPath = Path.Combine(dataPath, "demo_excel_connector_data.xlsx");
var outputPath = Path.Combine(AppContext.BaseDirectory, "output");

// Create output directory and clean up any existing .dfc files
if (Directory.Exists(outputPath))
{
    foreach (var file in Directory.GetFiles(outputPath, "*.dfc"))
    {
        File.Delete(file);
    }
}
Directory.CreateDirectory(outputPath);

var velocityPath = Path.Combine(outputPath, "web_events.dfc");
var ordersVelocityPath = Path.Combine(outputPath, "orders.dfc");
var velocityFiles = new Dictionary<string, string>();

// ============================================================================
// 1. STREAM EXCEL DATA TO VELOCITYDATABLOCK - Using GetStorageDataAsync
// ============================================================================
Console.WriteLine("1. Stream Excel Data to VelocityDataBlock");
Console.WriteLine(new string('-', 50));

// Create configuration for WebEvents sheet (2,000 rows)
var webEventsConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "WebEvents",
    HasHeader = true
};

// Create connector
var connector = new ExcelDataConnector(webEventsConfig);

// Create VelocityDataBlock with default options (auto-compaction disabled for streaming)
var options = VelocityOptions.CreateDefault();
options.AutoCompactionEnabled = false;
long webEventsRowCount;
{
    using var velocityBlock = new VelocityDataBlock(velocityPath, options);

    // Stream data from Excel to Velocity with batch processing
    Console.WriteLine("   Streaming WebEvents data to Velocity...");
    await connector.GetStorageDataAsync(velocityBlock, batchSize: 500);
    await velocityBlock.FlushAsync();

    webEventsRowCount = velocityBlock.RowCount;
    Console.WriteLine($"   Rows streamed: {webEventsRowCount}");
}
Console.WriteLine($"   Output file: {velocityPath}");
Console.WriteLine($"   File size: {new FileInfo(velocityPath).Length:N0} bytes");
Console.WriteLine();

// ============================================================================
// 2. QUERY THE VELOCITYDATABLOCK - Filter and analyze
// ============================================================================
Console.WriteLine("2. Query VelocityDataBlock");
Console.WriteLine(new string('-', 50));

{
    // Reopen the velocity file for querying
    using var queryBlock = new VelocityDataBlock(velocityPath);

    Console.WriteLine($"   Total rows: {queryBlock.RowCount}");
    Console.WriteLine($"   Schema columns: {string.Join(", ", queryBlock.Schema.GetColumnNames())}");

    // Filter for page_view events
    var pageViews = queryBlock.Where("EventType", "page_view").Execute();
    Console.WriteLine($"   Page views: {pageViews.RowCount}");

    // Filter for purchase events
    var purchases = queryBlock.Where("EventType", "purchase").Execute();
    Console.WriteLine($"   Purchases: {purchases.RowCount}");

    // Filter by device type
    var mobileEvents = queryBlock.Where("Device", "mobile").Execute();
    Console.WriteLine($"   Mobile events: {mobileEvents.RowCount}");
}
Console.WriteLine();

// ============================================================================
// 3. BATCH PROCESSING WITH CUSTOM SIZE
// ============================================================================
Console.WriteLine("3. Batch Processing with Custom Size");
Console.WriteLine(new string('-', 50));

var ordersConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Orders",
    HasHeader = true
};

var ordersConnector = new ExcelDataConnector(ordersConfig);

// Create with default options (auto-compaction disabled for streaming)
var ordersOptions = VelocityOptions.CreateDefault();
ordersOptions.AutoCompactionEnabled = false;
{
    using var ordersVelocity = new VelocityDataBlock(ordersVelocityPath, ordersOptions);

    // Stream with smaller batch size for demonstration
    await ordersConnector.GetStorageDataAsync(ordersVelocity, batchSize: 100);
    await ordersVelocity.FlushAsync();

    Console.WriteLine($"   Orders streamed: {ordersVelocity.RowCount}");
    Console.WriteLine($"   Batch size used: 100");
}
Console.WriteLine();

// ============================================================================
// 4. MULTIPLE SHEETS TO SEPARATE VELOCITY FILES
// ============================================================================
Console.WriteLine("4. Multiple Sheets to Velocity Files");
Console.WriteLine(new string('-', 50));

var sheetsToConvert = new[] { "Customers", "Products", "OrderItems" };

foreach (var sheetName in sheetsToConvert)
{
    var sheetVelocityPath = Path.Combine(outputPath, $"{sheetName.ToLower()}.dfc");
    velocityFiles[sheetName] = sheetVelocityPath;

    var config = new ExcelConnectorConfiguration
    {
        Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
        SheetName = sheetName,
        HasHeader = true
    };

    var sheetConnector = new ExcelDataConnector(config);
    var sheetOptions = VelocityOptions.CreateDefault();
    sheetOptions.AutoCompactionEnabled = false;

    using (var sheetVelocity = new VelocityDataBlock(sheetVelocityPath, sheetOptions))
    {
        await sheetConnector.GetStorageDataAsync(sheetVelocity, batchSize: 500);
        await sheetVelocity.FlushAsync();

        Console.WriteLine($"   {sheetName}: {sheetVelocity.RowCount} rows -> {sheetVelocityPath}");
    }
}
Console.WriteLine();

// ============================================================================
// 5. VERIFY AND ANALYZE VELOCITY FILES
// ============================================================================
Console.WriteLine("5. Verify and Analyze Velocity Files");
Console.WriteLine(new string('-', 50));

foreach (var (sheetName, path) in velocityFiles)
{
    using (var verifyBlock = new VelocityDataBlock(path))
    {
        var stats = await verifyBlock.GetStorageStatsAsync();

        Console.WriteLine($"   {sheetName}:");
        Console.WriteLine($"     Rows: {stats.ActiveRows}");
        Console.WriteLine($"     Size: {stats.EstimatedSizeBytes:N0} bytes");
    }
}
Console.WriteLine();

// ============================================================================
// 6. ADVANCED: ANALYTICS ON VELOCITY DATA
// ============================================================================
Console.WriteLine("6. Analytics on Velocity Data");
Console.WriteLine(new string('-', 50));

{
    // Reopen web events for analytics
    using var analyticsBlock = new VelocityDataBlock(velocityPath);

    // Count events by type
    Console.WriteLine("   Event counts by type:");
    var eventTypes = new[] { "page_view", "click", "purchase", "add_to_cart" };
    foreach (var eventType in eventTypes)
    {
        var count = analyticsBlock.Where("EventType", eventType).Execute();
        Console.WriteLine($"     {eventType}: {count.RowCount}");
    }

    // Count events by device
    Console.WriteLine("\n   Event counts by device:");
    var devices = new[] { "desktop", "mobile", "tablet" };
    foreach (var device in devices)
    {
        var count = analyticsBlock.Where("Device", device).Execute();
        Console.WriteLine($"     {device}: {count.RowCount}");
    }
}
Console.WriteLine();

// ============================================================================
// 7. OUTPUT SUMMARY
// ============================================================================
Console.WriteLine("7. Output Summary");
Console.WriteLine(new string('-', 50));

var outputFiles = Directory.GetFiles(outputPath, "*.dfc");
long totalSize = 0;

Console.WriteLine($"   Output directory: {outputPath}");
Console.WriteLine($"   DFC files created: {outputFiles.Length}");
Console.WriteLine();
Console.WriteLine("   File listing:");

foreach (var file in outputFiles.OrderBy(f => f))
{
    var info = new FileInfo(file);
    totalSize += info.Length;
    Console.WriteLine($"     {info.Name,-25} {info.Length,12:N0} bytes");
}
Console.WriteLine(new string('-', 50));
Console.WriteLine($"     {"TOTAL",-25} {totalSize,12:N0} bytes");

Console.WriteLine("\n=== Sample Complete ===");
