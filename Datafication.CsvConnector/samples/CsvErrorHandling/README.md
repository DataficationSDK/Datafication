# CsvErrorHandling Sample

Demonstrates error handling patterns and the `CsvConnectorConfiguration.ErrorHandler` property for robust CSV operations.

## Overview

This sample shows how to:
- Handle file not found errors
- Use the global error handler configuration
- Implement try-catch patterns for CSV loading
- Validate files before loading
- Create fallback and retry patterns

## Key Features Demonstrated

### Global Error Handler

The `ErrorHandler` is invoked when exceptions occur during CSV loading (file not found, I/O errors, etc.). The exception is still propagated after the handler is called.

```csharp
var config = new CsvConnectorConfiguration
{
    Source = new Uri("file:///path/to/data.csv"),
    ErrorHandler = (ex) =>
    {
        Console.WriteLine($"Error: {ex.Message}");
        // Log to file, send alert, etc.
    }
};

var connector = new CsvDataConnector(config);
var data = await connector.GetDataAsync();
```

### Parser Behavior with Malformed Data

The CSV parser is **lenient** and recovers gracefully from malformed rows without throwing exceptions. It handles:
- Missing fields (padded with null)
- Extra fields (ignored)
- Unbalanced quotes
- Special characters

```csharp
// The parser will load what it can, recovering from issues
var data = await DataBlock.Connector.LoadCsvAsync("malformed.csv");
Console.WriteLine($"Loaded {data.RowCount} rows");
```

### Try-Catch Pattern

```csharp
try
{
    var data = await DataBlock.Connector.LoadCsvAsync(path);
}
catch (FileNotFoundException)
{
    Console.WriteLine("File not found");
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("Access denied");
}
catch (IOException ex)
{
    Console.WriteLine($"IO Error: {ex.Message}");
}
```

### File Validation

```csharp
if (File.Exists(csvPath))
{
    var data = await DataBlock.Connector.LoadCsvAsync(csvPath);
}
else
{
    Console.WriteLine("File does not exist");
}
```

### Fallback Pattern

```csharp
async Task<DataBlock> LoadWithFallbackAsync(string primary, string fallback)
{
    try
    {
        return await DataBlock.Connector.LoadCsvAsync(primary);
    }
    catch
    {
        return await DataBlock.Connector.LoadCsvAsync(fallback);
    }
}
```

### Retry Pattern

```csharp
async Task<DataBlock?> LoadWithRetryAsync(string path, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await DataBlock.Connector.LoadCsvAsync(path);
        }
        catch when (attempt < maxRetries)
        {
            await Task.Delay(100 * attempt); // Exponential backoff
        }
    }
    return null;
}
```

## How to Run

```bash
cd CsvErrorHandling
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.CsvConnector Error Handling Sample ===

1. Handling 'File Not Found' errors...
   Caught FileNotFoundException: [message]

2. Using Global Error Handler...
   The ErrorHandler is invoked when exceptions occur during loading.

   [ErrorHandler] FileNotFoundException: [message]
   ErrorHandler was invoked: True

   Loading malformed.csv (parser is lenient with formatting issues)...
   Loaded 10 rows (parser recovered gracefully)

3. Try-Catch pattern for robust CSV loading...
   Successfully loaded 50 rows

4. Validating file before loading...
   employees.csv: EXISTS
   missing.csv: NOT FOUND
   products_euro.csv: EXISTS

5. Graceful degradation with fallback...
   Primary failed, using fallback
   Loaded 50 rows via fallback

6. Loading with retry logic...
   Attempt 1/3...
   Success: Loaded 50 rows

=== Sample Complete ===
```

## Common Exception Types

| Exception | Cause |
|-----------|-------|
| `FileNotFoundException` | CSV file does not exist |
| `UnauthorizedAccessException` | No permission to read file |
| `IOException` | File is locked or I/O error |
| `ArgumentException` | Invalid configuration |

> **Note:** The CSV parser is lenient with malformed data. It recovers gracefully from missing fields, extra fields, and unbalanced quotes without throwing exceptions.

## Best Practices

1. **Always use try-catch** - External file operations can fail
2. **Use ErrorHandler** - Centralized logging and monitoring
3. **Validate first** - Check file existence before loading
4. **Implement fallbacks** - Have backup data sources
5. **Add retry logic** - Handle transient failures
6. **Log errors** - Track issues for debugging

## Related Samples

- **CsvBasicLoad** - Simple CSV loading
- **CsvFullConfiguration** - All configuration options
- **CsvToVelocity** - High-performance streaming with error handling
