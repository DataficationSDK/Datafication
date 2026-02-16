using Datafication.Core.Data;
using Datafication.Connectors.JsonConnector;
using Datafication.Extensions.Connectors.JsonConnector;

Console.WriteLine("=== Datafication.JsonConnector Error Handling Sample ===\n");

// Get path to data directory
var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");

// 1. Basic try-catch pattern
Console.WriteLine("1. Basic Try-Catch Pattern...");
try
{
    var validPath = Path.Combine(dataPath, "employees.json");
    var data = await DataBlock.Connector.LoadJsonAsync(validPath);
    Console.WriteLine($"   Success: Loaded {data.RowCount} rows\n");
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}\n");
}

// 2. File not found handling
Console.WriteLine("2. File Not Found Handling...");
try
{
    var missingPath = Path.Combine(dataPath, "nonexistent.json");
    var data = await DataBlock.Connector.LoadJsonAsync(missingPath);
    Console.WriteLine($"   Loaded: {data.RowCount} rows");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"   FileNotFoundException: {ex.Message}");
}
catch (DirectoryNotFoundException ex)
{
    Console.WriteLine($"   DirectoryNotFoundException: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"   Caught: {ex.GetType().Name} - {ex.Message}");
}
Console.WriteLine();

// 3. Invalid JSON handling
Console.WriteLine("3. Invalid JSON Handling...");
try
{
    var malformedPath = Path.Combine(dataPath, "malformed.json");
    var data = await DataBlock.Connector.LoadJsonAsync(malformedPath);
    Console.WriteLine($"   Loaded: {data.RowCount} rows");
}
catch (Newtonsoft.Json.JsonReaderException ex)
{
    Console.WriteLine($"   JsonReaderException: {ex.Message}");
    Console.WriteLine($"   Line: {ex.LineNumber}, Position: {ex.LinePosition}");
}
catch (Exception ex)
{
    Console.WriteLine($"   Caught: {ex.GetType().Name} - {ex.Message}");
}
Console.WriteLine();

// 4. Using ErrorHandler callback
Console.WriteLine("4. ErrorHandler Callback Pattern...");
var errorLog = new List<string>();
var config = new JsonConnectorConfiguration
{
    Id = "error-demo",
    Source = new Uri("file://" + Path.Combine(dataPath, "nonexistent.json").Replace('\\', '/')),
    ErrorHandler = (ex) =>
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}";
        errorLog.Add(logEntry);
        Console.WriteLine($"   [LOGGED] {ex.GetType().Name}");
    }
};

try
{
    var connector = new JsonDataConnector(config);
    var data = await connector.GetDataAsync();
}
catch (Exception)
{
    // Error was already logged by ErrorHandler
    Console.WriteLine($"   Total errors logged: {errorLog.Count}");
}
Console.WriteLine();

// 5. HTTP error handling (simulated with invalid URL)
Console.WriteLine("5. HTTP Error Handling...");
try
{
    // This will fail because the URL doesn't exist
    var data = await DataBlock.Connector.LoadJsonAsync("https://invalid.example.com/api/data.json");
    Console.WriteLine($"   Loaded: {data.RowCount} rows");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"   HttpRequestException: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"   Caught: {ex.GetType().Name} - {ex.Message}");
}
Console.WriteLine();

// 6. Graceful degradation with fallback
Console.WriteLine("6. Graceful Degradation with Fallback...");
DataBlock? employees = null;

// Try primary source
try
{
    var primaryPath = Path.Combine(dataPath, "employees_primary.json"); // doesn't exist
    employees = await DataBlock.Connector.LoadJsonAsync(primaryPath);
    Console.WriteLine("   Loaded from primary source");
}
catch
{
    Console.WriteLine("   Primary source failed, trying fallback...");
}

// Try fallback source
if (employees == null)
{
    try
    {
        var fallbackPath = Path.Combine(dataPath, "employees.json"); // exists
        employees = await DataBlock.Connector.LoadJsonAsync(fallbackPath);
        Console.WriteLine($"   Loaded from fallback: {employees.RowCount} rows");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Fallback also failed: {ex.Message}");
    }
}

// Use default empty DataBlock if all sources fail
employees ??= new DataBlock();
Console.WriteLine($"   Final result: {employees.RowCount} rows\n");

// 7. Comprehensive error handling pattern
Console.WriteLine("7. Comprehensive Error Handling Pattern...");
async Task<DataBlock?> SafeLoadJsonAsync(string path, Action<Exception>? onError = null)
{
    try
    {
        return await DataBlock.Connector.LoadJsonAsync(path);
    }
    catch (FileNotFoundException ex)
    {
        onError?.Invoke(ex);
        Console.WriteLine($"   [SafeLoad] File not found: {Path.GetFileName(path)}");
        return null;
    }
    catch (Newtonsoft.Json.JsonException ex)
    {
        onError?.Invoke(ex);
        Console.WriteLine($"   [SafeLoad] Invalid JSON in: {Path.GetFileName(path)}");
        return null;
    }
    catch (HttpRequestException ex)
    {
        onError?.Invoke(ex);
        Console.WriteLine($"   [SafeLoad] HTTP error: {ex.Message}");
        return null;
    }
    catch (Exception ex)
    {
        onError?.Invoke(ex);
        Console.WriteLine($"   [SafeLoad] Unexpected error: {ex.GetType().Name}");
        return null;
    }
}

// Test the safe load pattern
var result1 = await SafeLoadJsonAsync(Path.Combine(dataPath, "employees.json"));
Console.WriteLine($"   Valid file: {(result1 != null ? $"{result1.RowCount} rows" : "failed")}");

var result2 = await SafeLoadJsonAsync(Path.Combine(dataPath, "missing.json"));
Console.WriteLine($"   Missing file: {(result2 != null ? $"{result2.RowCount} rows" : "null (handled)")}");

var result3 = await SafeLoadJsonAsync(Path.Combine(dataPath, "malformed.json"));
Console.WriteLine($"   Malformed file: {(result3 != null ? $"{result3.RowCount} rows" : "null (handled)")}");

Console.WriteLine("\n=== Sample Complete ===");
