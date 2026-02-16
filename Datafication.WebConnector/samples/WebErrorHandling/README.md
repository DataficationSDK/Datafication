# WebErrorHandling Sample

Demonstrates error handling patterns and HTTP configuration for robust web scraping.

## Overview

This sample shows how to:
- Handle exceptions with try-catch patterns
- Use ErrorHandler callback for logging
- Configure timeouts via HttpOptions
- Implement retry logic with exponential backoff
- Use fallback URLs for redundancy
- Configure custom HTTP headers and cookies

## Key Features Demonstrated

### Basic Try-Catch Pattern

```csharp
try
{
    var connector = new LinkExtractorConnector(config);
    var data = await connector.GetDataAsync();
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (TaskCanceledException)
{
    Console.WriteLine("Request timed out");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### ErrorHandler Callback

```csharp
var config = new LinkExtractorConnectorConfiguration
{
    Source = url,
    ErrorHandler = (ex) =>
    {
        // Log error before it propagates
        logger.Error($"Web request failed: {ex.Message}");
        // Can also collect errors for batch reporting
    }
};
```

### Timeout Configuration

```csharp
var config = new PageMetadataConnectorConfiguration
{
    Source = url,
    HttpOptions = new WebRequestOptions
    {
        TimeoutSeconds = 15  // Custom timeout
    }
};
```

### Retry Pattern with Exponential Backoff

```csharp
async Task<DataBlock?> FetchWithRetryAsync(Uri url, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var config = new LinkExtractorConnectorConfiguration { Source = url };
            var connector = new LinkExtractorConnector(config);
            return await connector.GetDataAsync();
        }
        catch (Exception)
        {
            if (attempt < maxRetries)
            {
                var delay = (int)Math.Pow(2, attempt) * 1000;
                await Task.Delay(delay);
            }
        }
    }
    return null;
}
```

### Fallback URL Pattern

```csharp
async Task<DataBlock?> FetchWithFallbackAsync(Uri[] urls)
{
    foreach (var url in urls)
    {
        try
        {
            var connector = new LinkExtractorConnector(
                new LinkExtractorConnectorConfiguration { Source = url });
            return await connector.GetDataAsync();
        }
        catch { /* try next URL */ }
    }
    return null;
}
```

### Custom HTTP Headers

```csharp
var config = new PageMetadataConnectorConfiguration
{
    Source = url,
    HttpOptions = new WebRequestOptions
    {
        Headers = new Dictionary<string, string>
        {
            { "X-API-Key", "your-api-key" },
            { "Authorization", "Bearer token" }
        },
        Cookies = new Dictionary<string, string>
        {
            { "session_id", "abc123" }
        },
        UserAgent = "MyApp/1.0"
    }
};
```

## How to Run

```bash
cd WebErrorHandling
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.WebConnector Error Handling Sample ===

1. Basic try-catch pattern for handling errors...
   Caught Exception: No such host is known

2. Using ErrorHandler callback for logging...
   ErrorHandler invoked!
   Exception still propagated: HttpRequestException
   Error log entries: 1

3. Configuring timeouts via HttpOptions...
   Fetching page with 3s delay, 5s timeout...
   Success! Extracted X fields

4. Demonstrating timeout failure...
   Fetching page with 5s delay, 2s timeout...
   Request timed out as expected!

5. Retry pattern with exponential backoff...
   Attempt 1 of 3...
   Retry result: X links

6. Fallback URL pattern...
   Trying: invalid-domain...
   Failed: HttpRequestException
   Trying: books.toscrape.com...
   Success!
   Fallback result: X links

7. Custom HTTP headers configuration...
   Request with custom headers succeeded

8. Successful extraction for comparison...
   Successfully extracted X links

9. Error Handling Best Practices:
   [Summary displayed]

=== Sample Complete ===
```

## HttpOptions Configuration

| Property | Default | Description |
|----------|---------|-------------|
| `TimeoutSeconds` | `30` | Request timeout in seconds |
| `UserAgent` | Chrome UA | User-Agent header string |
| `FollowRedirects` | `true` | Follow HTTP redirects |
| `MaxRedirects` | `10` | Maximum redirects to follow |
| `Headers` | `{}` | Custom HTTP headers |
| `Cookies` | `{}` | Request cookies |
| `Accept` | HTML/XML | Accept header value |
| `AcceptLanguage` | `"en-US"` | Accept-Language header |

## Common Exceptions

| Exception | Cause |
|-----------|-------|
| `HttpRequestException` | Network errors, DNS failures, connection refused |
| `TaskCanceledException` | Request timeout |
| `ArgumentException` | Invalid configuration |
| `ArgumentNullException` | Missing required configuration |
| `InvalidOperationException` | Invalid state or operation |

## Best Practices

1. **Always Use Try-Catch**: Web requests can fail for many reasons
2. **Log with ErrorHandler**: Capture errors before they propagate
3. **Set Appropriate Timeouts**: Balance between patience and responsiveness
4. **Implement Retries**: Transient failures are common
5. **Consider Fallbacks**: For critical data, have backup sources
6. **Validate Early**: Check configuration before making requests
7. **Handle Specific Exceptions**: Different errors need different handling
8. **Monitor and Alert**: Track error rates in production

## Retry Strategies

### Simple Retry
```csharp
for (int i = 0; i < 3; i++) { try { ... } catch { } }
```

### Exponential Backoff
```csharp
delay = Math.Pow(2, attempt) * baseDelay
```

### Circuit Breaker (conceptual)
```csharp
if (consecutiveFailures > threshold) return cached;
```

## Related Samples

- **WebFactoryPattern** - Simplified connector creation
- **WebTableExtraction** - Basic extraction patterns
- **WebProductScraping** - Full extraction workflow
