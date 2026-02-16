# ParquetErrorHandling Sample

Demonstrates comprehensive error handling patterns for the Datafication.ParquetConnector library.

## Overview

This sample shows how to:
- Handle file not found errors
- Use global ErrorHandler callbacks for centralized logging
- Implement robust try-catch patterns
- Validate paths before loading
- Create fallback patterns with multiple sources
- Implement retry logic with exponential backoff

## Key Features Demonstrated

### Basic Try-Catch

```csharp
try
{
    var data = await DataBlock.Connector.LoadParquetAsync(
        new Uri("file://" + parquetPath));
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Global ErrorHandler

```csharp
var config = new ParquetConnectorConfiguration
{
    Source = new Uri("file://" + path),
    ErrorHandler = (ex) =>
    {
        // Log to monitoring system
        Logger.Error(ex);
    }
};
```

### Safe Loading Pattern

```csharp
async Task<DataBlock?> SafeLoadParquetAsync(string path)
{
    try
    {
        if (!File.Exists(path))
            return null;

        return await DataBlock.Connector.LoadParquetAsync(
            new Uri("file://" + path));
    }
    catch
    {
        return null;
    }
}
```

### Fallback Pattern

```csharp
async Task<DataBlock?> LoadWithFallbackAsync(params string[] paths)
{
    foreach (var path in paths)
    {
        try
        {
            if (File.Exists(path))
                return await DataBlock.Connector.LoadParquetAsync(
                    new Uri("file://" + path));
        }
        catch { /* Try next source */ }
    }
    return null;
}
```

### Retry with Exponential Backoff

```csharp
async Task<DataBlock?> LoadWithRetryAsync(string path, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await DataBlock.Connector.LoadParquetAsync(
                new Uri("file://" + path));
        }
        catch
        {
            if (attempt < maxRetries)
            {
                var delay = (int)Math.Pow(2, attempt) * 100;
                await Task.Delay(delay);
            }
        }
    }
    return null;
}
```

## Error Handling Patterns

| Pattern | Use Case |
|---------|----------|
| Try-catch | General error recovery |
| ErrorHandler callback | Centralized logging/monitoring |
| Pre-validation | Fail fast before I/O operations |
| Fallback sources | High availability requirements |
| Retry with backoff | Transient failures (network) |

## How to Run

```bash
cd ParquetErrorHandling
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.ParquetConnector Error Handling Sample ===

1. Handling File Not Found errors...
   [Caught] FileNotFoundException: ...

2. Using global ErrorHandler callback...
   [ErrorHandler] Captured: FileNotFoundException
   [Caught] Exception still thrown after handler: FileNotFoundException
   Total errors captured by handler: 1

3. Robust loading with try-catch pattern...
   [SafeLoad] File does not exist: nonexistent.parquet
   [SafeLoad] Successfully loaded 50000 rows
   Result 1 (nonexistent): null
   Result 2 (valid): Loaded

4. Validation before loading...
   [Validation] Path is empty
   [Validation] File not found: nonexistent.parquet
   [Validation] Valid: SalesRecords50k.parquet

5. Fallback pattern with multiple sources...
   [Fallback] Skipping (not found): nonexistent.parquet
   [Fallback] Skipping (not found): backup.parquet
   [Fallback] Loaded from: SalesRecords50k.parquet
   Final result: 50000 rows

6. Retry pattern demonstration...
   [Retry] Attempt 1 of 3...
   [Retry] Success on attempt 1
   Result: 50000 rows

7. Error Handling Best Practices:
   [Summary table]

=== Sample Complete ===
```

## Related Samples

- **ParquetBasicLoad** - Simple loading patterns
- **ParquetConfiguration** - Configuration options including ErrorHandler
- **ParquetToVelocity** - Error handling in streaming scenarios
