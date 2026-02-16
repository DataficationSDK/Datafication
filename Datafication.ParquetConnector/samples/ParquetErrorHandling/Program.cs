using Datafication.Core.Data;
using Datafication.Connectors.ParquetConnector;
using Datafication.Extensions.Connectors.ParquetConnector;

Console.WriteLine("=== Datafication.ParquetConnector Error Handling Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
var validParquetPath = Path.GetFullPath(Path.Combine(dataPath, "SalesRecords50k.parquet"));
var nonExistentPath = Path.GetFullPath(Path.Combine(dataPath, "nonexistent.parquet"));

// 1. File Not Found error handling
Console.WriteLine("1. Handling File Not Found errors...");
try
{
    var data = await DataBlock.Connector.LoadParquetAsync(
        new Uri("file://" + nonExistentPath));
    Console.WriteLine($"   Loaded {data.RowCount} rows");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"   [Caught] FileNotFoundException: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"   [Caught] {ex.GetType().Name}: {ex.Message}");
}
Console.WriteLine();

// 2. Global ErrorHandler demonstration
Console.WriteLine("2. Using global ErrorHandler callback...");
var globalErrors = new List<Exception>();

var configWithHandler = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + nonExistentPath),
    ErrorHandler = (ex) =>
    {
        globalErrors.Add(ex);
        Console.WriteLine($"   [ErrorHandler] Captured: {ex.GetType().Name}");
    }
};

try
{
    var connector = new ParquetDataConnector(configWithHandler);
    var data = await connector.GetDataAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"   [Caught] Exception still thrown after handler: {ex.GetType().Name}");
}
Console.WriteLine($"   Total errors captured by handler: {globalErrors.Count}\n");

// 3. Try-catch pattern for robust loading
Console.WriteLine("3. Robust loading with try-catch pattern...");

async Task<DataBlock?> SafeLoadParquetAsync(string path)
{
    try
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"   [SafeLoad] File does not exist: {Path.GetFileName(path)}");
            return null;
        }

        var data = await DataBlock.Connector.LoadParquetAsync(new Uri("file://" + path));
        Console.WriteLine($"   [SafeLoad] Successfully loaded {data.RowCount} rows");
        return data;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   [SafeLoad] Failed: {ex.GetType().Name}");
        return null;
    }
}

var result1 = await SafeLoadParquetAsync(nonExistentPath);
var result2 = await SafeLoadParquetAsync(validParquetPath);
Console.WriteLine($"   Result 1 (nonexistent): {(result1 != null ? "Loaded" : "null")}");
Console.WriteLine($"   Result 2 (valid): {(result2 != null ? "Loaded" : "null")}\n");

// 4. Validation before loading
Console.WriteLine("4. Validation before loading...");

bool ValidateParquetPath(string path)
{
    if (string.IsNullOrEmpty(path))
    {
        Console.WriteLine("   [Validation] Path is empty");
        return false;
    }

    if (!File.Exists(path))
    {
        Console.WriteLine($"   [Validation] File not found: {Path.GetFileName(path)}");
        return false;
    }

    if (!path.EndsWith(".parquet", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"   [Validation] Not a .parquet file: {Path.GetFileName(path)}");
        return false;
    }

    Console.WriteLine($"   [Validation] Valid: {Path.GetFileName(path)}");
    return true;
}

ValidateParquetPath("");
ValidateParquetPath(nonExistentPath);
ValidateParquetPath(validParquetPath);
Console.WriteLine();

// 5. Fallback pattern with multiple sources
Console.WriteLine("5. Fallback pattern with multiple sources...");

async Task<DataBlock?> LoadWithFallbackAsync(params string[] paths)
{
    foreach (var path in paths)
    {
        try
        {
            if (File.Exists(path))
            {
                var data = await DataBlock.Connector.LoadParquetAsync(new Uri("file://" + path));
                Console.WriteLine($"   [Fallback] Loaded from: {Path.GetFileName(path)}");
                return data;
            }
            Console.WriteLine($"   [Fallback] Skipping (not found): {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [Fallback] Error on {Path.GetFileName(path)}: {ex.GetType().Name}");
        }
    }

    Console.WriteLine("   [Fallback] All sources failed");
    return null;
}

var fallbackData = await LoadWithFallbackAsync(
    nonExistentPath,
    Path.Combine(dataPath, "backup.parquet"),
    validParquetPath
);
Console.WriteLine($"   Final result: {(fallbackData != null ? $"{fallbackData.RowCount} rows" : "null")}\n");

// 6. Retry pattern with exponential backoff
Console.WriteLine("6. Retry pattern demonstration...");

async Task<DataBlock?> LoadWithRetryAsync(string path, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Console.WriteLine($"   [Retry] Attempt {attempt} of {maxRetries}...");
            var data = await DataBlock.Connector.LoadParquetAsync(new Uri("file://" + path));
            Console.WriteLine($"   [Retry] Success on attempt {attempt}");
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [Retry] Attempt {attempt} failed: {ex.GetType().Name}");

            if (attempt < maxRetries)
            {
                var delay = (int)Math.Pow(2, attempt) * 100; // Exponential backoff
                Console.WriteLine($"   [Retry] Waiting {delay}ms before retry...");
                await Task.Delay(delay);
            }
        }
    }

    Console.WriteLine("   [Retry] All retries exhausted");
    return null;
}

// Demonstrate with valid file (succeeds immediately)
var retryResult = await LoadWithRetryAsync(validParquetPath);
Console.WriteLine($"   Result: {(retryResult != null ? $"{retryResult.RowCount} rows" : "null")}\n");

// 7. Error handling best practices summary
Console.WriteLine("7. Error Handling Best Practices:");
Console.WriteLine("   " + new string('-', 65));
Console.WriteLine("   Pattern                     | Use Case");
Console.WriteLine("   " + new string('-', 65));
Console.WriteLine("   Try-catch                   | General error recovery");
Console.WriteLine("   ErrorHandler callback       | Centralized logging/monitoring");
Console.WriteLine("   Pre-validation              | Fail fast before I/O operations");
Console.WriteLine("   Fallback sources            | High availability requirements");
Console.WriteLine("   Retry with backoff          | Transient failures (network)");
Console.WriteLine("   " + new string('-', 65));

Console.WriteLine("\n=== Sample Complete ===");
