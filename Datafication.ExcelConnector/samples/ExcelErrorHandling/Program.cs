using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ExcelConnector;
using Datafication.Connectors.ExcelConnector;

Console.WriteLine("=== Datafication.ExcelConnector Error Handling Sample ===\n");

var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data");
var excelPath = Path.Combine(dataPath, "demo_excel_connector_data.xlsx");
var errorLog = new List<string>();

// ============================================================================
// 1. ERROR HANDLER CALLBACK - Capture errors via callback
// ============================================================================
Console.WriteLine("1. ErrorHandler Callback");
Console.WriteLine(new string('-', 50));

// Set up an error handler
void LogError(Exception ex)
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    var message = $"[{timestamp}] {ex.GetType().Name}: {ex.Message}";
    errorLog.Add(message);
    Console.WriteLine($"   Error captured: {ex.GetType().Name}");
}

// Try to load a non-existent file with error handler
var badFileConfig = new ExcelConnectorConfiguration
{
    Source = new Uri("/nonexistent/path/data.xlsx", UriKind.Relative),
    SheetName = "Sheet1",
    HasHeader = true,
    ErrorHandler = LogError
};

try
{
    // Note: Validator will fail before ErrorHandler is called
    // ErrorHandler is mainly for runtime errors during data reading
    var configValidator = new ExcelConnectorValidator();
    var validationResult = configValidator.Validate(badFileConfig);
    if (!validationResult.IsValid)
    {
        Console.WriteLine($"   Validation failed (expected): {validationResult.Errors.First()}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   Exception: {ex.Message}");
}
Console.WriteLine();

// ============================================================================
// 2. PRE-VALIDATION WITH ExcelConnectorValidator
// ============================================================================
Console.WriteLine("2. Pre-Validation with ExcelConnectorValidator");
Console.WriteLine(new string('-', 50));

var validator = new ExcelConnectorValidator();

// Test various invalid configurations
var testCases = new (string Name, ExcelConnectorConfiguration Config)[]
{
    ("Null Source", new ExcelConnectorConfiguration { Source = null!, SheetName = "Test" }),
    ("Non-existent File", new ExcelConnectorConfiguration { Source = new Uri("/fake/path.xlsx", UriKind.Relative) }),
    ("Valid Config", new ExcelConnectorConfiguration { Source = new Uri(excelPath, UriKind.RelativeOrAbsolute), SheetName = "Customers" })
};

foreach (var testCase in testCases)
{
    var result = validator.Validate(testCase.Config);
    Console.WriteLine($"   {testCase.Name}: IsValid = {result.IsValid}");
    if (!result.IsValid)
    {
        Console.WriteLine($"     -> {result.Errors.First()}");
    }
}
Console.WriteLine();

// ============================================================================
// 3. TRY-CATCH PATTERNS - Standard exception handling
// ============================================================================
Console.WriteLine("3. Try-Catch Patterns");
Console.WriteLine(new string('-', 50));

// Pattern 1: Validate before loading
Console.WriteLine("   Pattern 1: Validate-then-Load");
async Task<DataBlock?> SafeLoadWithValidation(ExcelConnectorConfiguration config)
{
    var validationRes = validator.Validate(config);
    if (!validationRes.IsValid)
    {
        Console.WriteLine($"     Validation failed: {string.Join(", ", validationRes.Errors)}");
        return null;
    }

    try
    {
        return await DataBlock.Connector.LoadExcelAsync(config);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"     Load failed: {ex.Message}");
        return null;
    }
}

var safeResult = await SafeLoadWithValidation(new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Customers"
});
Console.WriteLine($"     Result: {(safeResult != null ? $"Loaded {safeResult.RowCount} rows" : "Failed")}");
Console.WriteLine();

// Pattern 2: Try-catch with specific exception types
Console.WriteLine("   Pattern 2: Specific Exception Types");
async Task<DataBlock?> LoadWithSpecificHandling(string path, string sheetName)
{
    try
    {
        var config = new ExcelConnectorConfiguration
        {
            Source = new Uri(path, UriKind.RelativeOrAbsolute),
            SheetName = sheetName,
            HasHeader = true
        };
        return await DataBlock.Connector.LoadExcelAsync(config);
    }
    catch (FileNotFoundException ex)
    {
        Console.WriteLine($"     File not found: {ex.FileName}");
        return null;
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"     Invalid argument: {ex.Message}");
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"     Unexpected error: {ex.GetType().Name} - {ex.Message}");
        return null;
    }
}

// This will fail validation (ArgumentException)
await LoadWithSpecificHandling("/fake/file.xlsx", "Sheet1");
Console.WriteLine();

// ============================================================================
// 4. RETRY PATTERN - Retry on transient failures
// ============================================================================
Console.WriteLine("4. Retry Pattern");
Console.WriteLine(new string('-', 50));

async Task<DataBlock?> LoadWithRetry(ExcelConnectorConfiguration config, int maxRetries = 3, int delayMs = 1000)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Console.WriteLine($"     Attempt {attempt} of {maxRetries}...");
            return await DataBlock.Connector.LoadExcelAsync(config);
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            Console.WriteLine($"     Failed: {ex.Message}. Retrying...");
            await Task.Delay(delayMs);
        }
    }

    Console.WriteLine("     All retries exhausted.");
    return null;
}

// Successful retry (first attempt works)
var retryConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",
    HasHeader = true
};
var retryResult = await LoadWithRetry(retryConfig, maxRetries: 3);
Console.WriteLine($"     Result: {(retryResult != null ? $"Loaded {retryResult.RowCount} rows" : "Failed")}");
Console.WriteLine();

// ============================================================================
// 5. FALLBACK PATTERN - Use alternative data on failure
// ============================================================================
Console.WriteLine("5. Fallback Pattern");
Console.WriteLine(new string('-', 50));

async Task<DataBlock> LoadWithFallback(string primarySheet, string fallbackSheet)
{
    var primaryConfig = new ExcelConnectorConfiguration
    {
        Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
        SheetName = primarySheet,
        HasHeader = true
    };

    try
    {
        var result = await DataBlock.Connector.LoadExcelAsync(primaryConfig);
        Console.WriteLine($"     Primary sheet '{primarySheet}' loaded successfully");
        return result;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"     Primary failed: {ex.Message}");
        Console.WriteLine($"     Falling back to '{fallbackSheet}'...");

        var fallbackConfig = new ExcelConnectorConfiguration
        {
            Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
            SheetName = fallbackSheet,
            HasHeader = true
        };
        return await DataBlock.Connector.LoadExcelAsync(fallbackConfig);
    }
}

// Test with a valid sheet (no fallback needed)
var fallbackResult = await LoadWithFallback("Products", "Customers");
Console.WriteLine($"     Result: {fallbackResult.RowCount} rows");
Console.WriteLine();

// ============================================================================
// 6. ERROR AGGREGATION - Collect errors across multiple operations
// ============================================================================
Console.WriteLine("6. Error Aggregation (Multi-Sheet Loading)");
Console.WriteLine(new string('-', 50));

async Task<Dictionary<string, DataBlock>> LoadMultipleSheetsWithErrors(string[] sheets)
{
    var results = new Dictionary<string, DataBlock>();
    var errors = new List<(string Sheet, string Error)>();

    foreach (var sheet in sheets)
    {
        try
        {
            var config = new ExcelConnectorConfiguration
            {
                Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
                SheetName = sheet,
                HasHeader = true
            };
            results[sheet] = await DataBlock.Connector.LoadExcelAsync(config);
            Console.WriteLine($"     [OK] Loaded {sheet}: {results[sheet].RowCount} rows");
        }
        catch (Exception ex)
        {
            errors.Add((sheet, ex.Message));
            Console.WriteLine($"     [FAIL] {sheet}: {ex.Message}");
        }
    }

    if (errors.Count > 0)
    {
        Console.WriteLine($"\n     Summary: {results.Count} succeeded, {errors.Count} failed");
    }

    return results;
}

var sheetNames = new[] { "Customers", "Products", "Orders", "NonExistentSheet" };
var multiResults = await LoadMultipleSheetsWithErrors(sheetNames);
Console.WriteLine();

// ============================================================================
// 7. ERROR LOG SUMMARY
// ============================================================================
Console.WriteLine("7. Error Log Summary");
Console.WriteLine(new string('-', 50));

if (errorLog.Count > 0)
{
    Console.WriteLine("   Captured errors:");
    foreach (var log in errorLog)
    {
        Console.WriteLine($"     {log}");
    }
}
else
{
    Console.WriteLine("   No errors captured via ErrorHandler callback.");
    Console.WriteLine("   (Validation errors are returned, not thrown)");
}

Console.WriteLine("\n=== Sample Complete ===");
