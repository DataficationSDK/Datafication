# S3ErrorHandling Sample

Demonstrates error handling patterns for robust S3 data loading operations.

## Overview

This sample shows how to:
- Handle common S3 errors (not found, access denied, invalid bucket)
- Use the `ErrorHandler` configuration property
- Implement retry patterns with exponential backoff
- Validate configuration before loading
- Handle unsupported file types

## Key Features Demonstrated

### Basic Try-Catch Pattern

```csharp
try
{
    using var connector = new S3DataConnector(config);
    var data = await connector.GetDataAsync();
}
catch (Amazon.S3.AmazonS3Exception s3Ex) when (s3Ex.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine("Object not found");
}
catch (Amazon.S3.AmazonS3Exception s3Ex) when (s3Ex.StatusCode == HttpStatusCode.Forbidden)
{
    Console.WriteLine("Access denied - check credentials");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Using ErrorHandler Configuration

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "my-bucket",
    ObjectKey = "data.csv",
    ErrorHandler = (Exception ex) =>
    {
        // Log error, send notification, etc.
        Console.WriteLine($"Error: {ex.Message}");
        // Processing continues with empty DataBlock
    }
};

using var connector = new S3DataConnector(config);
var data = await connector.GetDataAsync();  // Returns empty if error occurred
```

### Retry Pattern with Exponential Backoff

```csharp
async Task<DataBlock?> LoadWithRetryAsync(S3ConnectorConfiguration config, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            using var connector = new S3DataConnector(config);
            return await connector.GetDataAsync();
        }
        catch (Exception) when (attempt < maxRetries)
        {
            var delay = (int)Math.Pow(2, attempt) * 100;  // 200ms, 400ms, 800ms
            await Task.Delay(delay);
        }
    }
    return null;
}
```

### Pre-Load Validation

```csharp
void ValidateConfiguration(S3ConnectorConfiguration config)
{
    if (string.IsNullOrWhiteSpace(config.Region))
        throw new ArgumentException("Region is required");

    if (string.IsNullOrWhiteSpace(config.BucketName))
        throw new ArgumentException("BucketName is required");

    if (config.ObjectKey?.EndsWith("/") == true && !config.AllowMultipleSegments)
        throw new ArgumentException("Prefix pattern requires AllowMultipleSegments = true");
}
```

## Common Exception Types

| Exception | Cause | Handling |
|-----------|-------|----------|
| `AmazonS3Exception` (404) | Object not found | Check ObjectKey path |
| `AmazonS3Exception` (403) | Access denied | Check credentials/permissions |
| `AmazonS3Exception` (301) | Bucket in different region | Update Region config |
| `NotSupportedException` | Unsupported file type | Use supported formats |
| `ArgumentException` | Invalid configuration | Validate before creating connector |
| `InvalidOperationException` | Mixed file types in prefix | Use consistent file types |

## How to Run

```bash
cd S3ErrorHandling
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.S3Connector Error Handling Sample ===

1. Basic Try-Catch Pattern
   Success: Loaded 218 rows

2. Handling Non-Existent Objects
   Handled: Object not found (404)

3. Handling Invalid Bucket
   Handled: S3 error (NoSuchBucket)

4. Using ErrorHandler Configuration
   [ErrorHandler] Caught: Exception
   Returned empty DataBlock: 0 rows

5. Retry Pattern (Demonstration)
   Attempt 1 of 3...
   Success after retry: 218 rows

6. Pre-Load Validation
   Configuration is valid
   Validation issues:
   - ObjectKey looks like a prefix but AllowMultipleSegments is false

7. Handling Unsupported File Types
   Handled: File type not supported...

8. Error Summary
   Errors caught during demo: 1

=== Sample Complete ===
```

## Best Practices

1. **Always dispose connectors** - Use `using` statements or try-finally
2. **Validate early** - Check configuration before creating connector
3. **Use specific catch blocks** - Handle different error types appropriately
4. **Implement retries** - Network operations can have transient failures
5. **Log errors** - Use ErrorHandler for centralized error logging
6. **Fail gracefully** - Return empty results instead of crashing when appropriate

## Related Samples

- **S3BasicLoad** - Basic loading without error handling
- **S3Configuration** - All configuration options
- **S3MultiSegment** - Error handling in multi-segment mode
- **S3ToVelocity** - Error handling with streaming
