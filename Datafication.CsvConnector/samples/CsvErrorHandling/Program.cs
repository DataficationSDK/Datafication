using Datafication.Core.Data;
using Datafication.Connectors.CsvConnector;
using Datafication.Extensions.Connectors.CsvConnector;

Console.WriteLine("=== Datafication.CsvConnector Error Handling Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");

// Track errors for demonstration
var errorLog = new List<string>();

// ============================================================================
// 1. File Not Found handling
// ============================================================================
Console.WriteLine("1. Handling 'File Not Found' errors...");

try
{
    var nonExistentPath = Path.Combine(dataPath, "does_not_exist.csv");
    var data = await DataBlock.Connector.LoadCsvAsync(nonExistentPath);
    Console.WriteLine($"   Loaded {data.RowCount} rows");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"   Caught FileNotFoundException: {ex.Message}");
    errorLog.Add("FileNotFoundException: File not found");
}
catch (Exception ex)
{
    Console.WriteLine($"   Caught Exception: {ex.GetType().Name} - {ex.Message}");
    errorLog.Add($"{ex.GetType().Name}: {ex.Message}");
}
Console.WriteLine();

// ============================================================================
// 2. Using Global Error Handler
// ============================================================================
Console.WriteLine("2. Using Global Error Handler...");
Console.WriteLine("   The ErrorHandler is invoked when exceptions occur during loading.\n");

var globalErrors = new List<Exception>();

// Test ErrorHandler with a file that doesn't exist
var errorConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "nonexistent.csv"))),
    HeaderRow = true,
    ErrorHandler = (ex) =>
    {
        globalErrors.Add(ex);
        Console.WriteLine($"   [ErrorHandler] {ex.GetType().Name}: {ex.Message}");
    }
};

try
{
    var connector = new CsvDataConnector(errorConfig);
    await connector.GetDataAsync();
}
catch
{
    // Exception still propagates after ErrorHandler is called
}

Console.WriteLine($"   ErrorHandler was invoked: {globalErrors.Count > 0}\n");

// Also show that malformed data loads successfully (parser is lenient)
Console.WriteLine("   Loading malformed.csv (parser is lenient with formatting issues)...");
var malformedConfig = new CsvConnectorConfiguration
{
    Source = new Uri(Path.GetFullPath(Path.Combine(dataPath, "malformed.csv"))),
    HeaderRow = true
};

try
{
    var connector = new CsvDataConnector(malformedConfig);
    var data = await connector.GetDataAsync();
    Console.WriteLine($"   Loaded {data.RowCount} rows (parser recovered gracefully)\n");
}
catch (Exception ex)
{
    Console.WriteLine($"   Exception: {ex.Message}\n");
}

// ============================================================================
// 3. Try-Catch Pattern for CSV Loading
// ============================================================================
Console.WriteLine("3. Try-Catch pattern for robust CSV loading...");

DataBlock? safeData = null;
var loadSuccess = false;

try
{
    var safePath = Path.Combine(dataPath, "employees.csv");
    safeData = await DataBlock.Connector.LoadCsvAsync(safePath);
    loadSuccess = true;
    Console.WriteLine($"   Successfully loaded {safeData.RowCount} rows");
}
catch (FileNotFoundException)
{
    Console.WriteLine("   Error: CSV file not found");
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("   Error: Access denied to CSV file");
}
catch (IOException ex)
{
    Console.WriteLine($"   Error: IO error - {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: Unexpected error - {ex.Message}");
}

if (loadSuccess && safeData != null)
{
    Console.WriteLine($"   Processing {safeData.RowCount} records...");
}
Console.WriteLine();

// ============================================================================
// 4. Validation Before Loading
// ============================================================================
Console.WriteLine("4. Validating file before loading...");

var filesToCheck = new[]
{
    "employees.csv",
    "missing.csv",
    "products_euro.csv"
};

foreach (var fileName in filesToCheck)
{
    var filePath = Path.Combine(dataPath, fileName);

    if (File.Exists(filePath))
    {
        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"   {fileName}: EXISTS ({fileInfo.Length} bytes)");
    }
    else
    {
        Console.WriteLine($"   {fileName}: NOT FOUND");
    }
}
Console.WriteLine();

// ============================================================================
// 5. Graceful Degradation Pattern
// ============================================================================
Console.WriteLine("5. Graceful degradation with fallback...");

async Task<DataBlock> LoadWithFallbackAsync(string primaryPath, string fallbackPath)
{
    try
    {
        Console.WriteLine($"   Attempting primary: {Path.GetFileName(primaryPath)}");
        return await DataBlock.Connector.LoadCsvAsync(primaryPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Primary failed: {ex.GetType().Name}");
        Console.WriteLine($"   Attempting fallback: {Path.GetFileName(fallbackPath)}");
        return await DataBlock.Connector.LoadCsvAsync(fallbackPath);
    }
}

try
{
    var result = await LoadWithFallbackAsync(
        Path.Combine(dataPath, "missing.csv"),     // Will fail
        Path.Combine(dataPath, "employees.csv")    // Fallback
    );
    Console.WriteLine($"   Loaded {result.RowCount} rows via fallback\n");
}
catch (Exception ex)
{
    Console.WriteLine($"   Both primary and fallback failed: {ex.Message}\n");
}

// ============================================================================
// 6. Loading with Retry Logic
// ============================================================================
Console.WriteLine("6. Loading with retry logic...");

async Task<DataBlock?> LoadWithRetryAsync(string path, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Console.WriteLine($"   Attempt {attempt}/{maxRetries}...");
            return await DataBlock.Connector.LoadCsvAsync(path);
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            Console.WriteLine($"   Attempt {attempt} failed: {ex.GetType().Name}");
            await Task.Delay(100 * attempt); // Exponential backoff
        }
    }

    Console.WriteLine($"   All {maxRetries} attempts failed");
    return null;
}

var retryResult = await LoadWithRetryAsync(Path.Combine(dataPath, "employees.csv"));
if (retryResult != null)
{
    Console.WriteLine($"   Success: Loaded {retryResult.RowCount} rows\n");
}

// ============================================================================
// Summary
// ============================================================================
Console.WriteLine("=== Error Handling Best Practices ===");
Console.WriteLine("   1. Always use try-catch when loading external files");
Console.WriteLine("   2. Use ErrorHandler for centralized logging");
Console.WriteLine("   3. Validate file existence before loading");
Console.WriteLine("   4. Implement fallback/retry patterns for reliability");
Console.WriteLine("   5. Handle specific exceptions (FileNotFound, IO, etc.)");
Console.WriteLine();
Console.WriteLine($"   Total errors captured in this demo: {errorLog.Count + globalErrors.Count}");

Console.WriteLine("\n=== Sample Complete ===");
