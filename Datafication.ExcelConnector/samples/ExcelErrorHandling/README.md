# ExcelErrorHandling Sample

This sample demonstrates robust error handling patterns when working with the Excel connector, including validation, try-catch patterns, retries, and fallbacks.

## Features Demonstrated

1. **ErrorHandler Callback** - Capture errors via configuration callback
2. **ExcelConnectorValidator** - Pre-validate configurations before loading
3. **Try-Catch Patterns** - Standard exception handling approaches
4. **Retry Pattern** - Retry on transient failures
5. **Fallback Pattern** - Use alternative data sources on failure
6. **Error Aggregation** - Collect errors across multiple operations

## Error Handling Approaches

### ErrorHandler Callback
```csharp
var config = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    ErrorHandler = (ex) => Console.WriteLine($"Error: {ex.Message}")
};
```

### Pre-Validation
```csharp
var validator = new ExcelConnectorValidator();
var result = validator.Validate(config);
if (!result.IsValid)
{
    Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");
    return;
}
```

### Validate-then-Load Pattern
```csharp
async Task<DataBlock?> SafeLoad(ExcelConnectorConfiguration config)
{
    var validator = new ExcelConnectorValidator();
    if (!validator.Validate(config).IsValid) return null;

    try
    {
        return await DataBlock.Connector.LoadExcelAsync(config);
    }
    catch (Exception ex)
    {
        // Handle or log error
        return null;
    }
}
```

### Retry Pattern
```csharp
async Task<DataBlock?> LoadWithRetry(ExcelConnectorConfiguration config, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await DataBlock.Connector.LoadExcelAsync(config);
        }
        catch when (attempt < maxRetries)
        {
            await Task.Delay(1000); // Wait before retry
        }
    }
    return null;
}
```

## Running the Sample

```bash
cd samples/ExcelErrorHandling
dotnet run
```

## Common Error Types

| Error | Cause | Solution |
|-------|-------|----------|
| `ArgumentException` | Invalid configuration | Use ExcelConnectorValidator before loading |
| `FileNotFoundException` | Source file doesn't exist | Verify file path |
| `Exception` (Sheet not found) | Invalid SheetName/SheetIndex | Check available sheets |

## Best Practices

1. **Always validate** configurations before loading to provide clear error messages
2. **Use ErrorHandler** for logging or monitoring without changing control flow
3. **Implement retry logic** for transient failures (network files, locked files)
4. **Provide fallbacks** for non-critical data sources
5. **Aggregate errors** when loading multiple sheets to provide complete feedback
