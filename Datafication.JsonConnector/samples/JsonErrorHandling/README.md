# JsonErrorHandling Sample

Demonstrates error handling patterns and graceful degradation when working with the Datafication.JsonConnector library.

## Overview

This sample shows how to:
- Use basic try-catch patterns for error handling
- Handle specific exception types (FileNotFoundException, JsonException, etc.)
- Configure ErrorHandler callbacks in JsonConnectorConfiguration
- Handle HTTP errors when loading from remote URLs
- Implement graceful degradation with fallback sources
- Create reusable safe-load utility methods

## Key Features Demonstrated

### Basic Try-Catch Pattern

```csharp
try
{
    var data = await DataBlock.Connector.LoadJsonAsync("path/to/file.json");
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

### ErrorHandler Callback

```csharp
var config = new JsonConnectorConfiguration
{
    Source = new Uri("file:///path/to/file.json"),
    ErrorHandler = (ex) =>
    {
        // Log to file, monitoring system, etc.
        Logger.Error($"JSON load failed: {ex.Message}");
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
}
```

### Graceful Degradation

```csharp
DataBlock? data = null;

// Try primary source
try
{
    data = await DataBlock.Connector.LoadJsonAsync(primaryPath);
}
catch { }

// Try fallback source
if (data == null)
{
    try
    {
        data = await DataBlock.Connector.LoadJsonAsync(fallbackPath);
    }
    catch { }
}

// Use empty DataBlock if all sources fail
data ??= new DataBlock();
```

### Safe Load Pattern

```csharp
async Task<DataBlock?> SafeLoadJsonAsync(string path)
{
    try
    {
        return await DataBlock.Connector.LoadJsonAsync(path);
    }
    catch (FileNotFoundException) { return null; }
    catch (JsonException) { return null; }
    catch (HttpRequestException) { return null; }
}
```

## Common Exception Types

| Exception Type | Cause |
|---------------|-------|
| `FileNotFoundException` | Local file does not exist |
| `DirectoryNotFoundException` | Parent directory does not exist |
| `JsonReaderException` | Malformed JSON syntax |
| `JsonSerializationException` | JSON structure doesn't match expected format |
| `HttpRequestException` | Network error or HTTP status error |
| `ArgumentNullException` | Source or configuration is null |
| `ArgumentException` | Invalid configuration values |

## How to Run

```bash
cd JsonErrorHandling
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.JsonConnector Error Handling Sample ===

1. Basic Try-Catch Pattern...
   Success: Loaded 50 rows

2. File Not Found Handling...
   Caught: [Exception type] - [message]

3. Invalid JSON Handling...
   JsonReaderException: [parsing error details]
   Line: X, Position: Y

4. ErrorHandler Callback Pattern...
   [LOGGED] [Exception type]
   Total errors logged: 1

5. HTTP Error Handling...
   Caught: [HTTP error details]

6. Graceful Degradation with Fallback...
   Primary source failed, trying fallback...
   Loaded from fallback: 50 rows
   Final result: 50 rows

7. Comprehensive Error Handling Pattern...
   Valid file: 50 rows
   Missing file: null (handled)
   Malformed file: null (handled)

=== Sample Complete ===
```

## Data Files

- `data/employees.json` - Valid JSON file (50 records)
- `data/malformed.json` - Intentionally malformed JSON for testing

## Related Samples

- **JsonConfiguration** - Configuration options including ErrorHandler
- **JsonRemoteApi** - HTTP error handling for remote APIs
- **JsonBasicLoad** - Basic loading without error handling
