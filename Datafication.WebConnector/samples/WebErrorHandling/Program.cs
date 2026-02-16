using Datafication.Core.Data;
using Datafication.Connectors.WebConnector.Connectors;
using Datafication.Connectors.WebConnector.Configuration;

Console.WriteLine("=== Datafication.WebConnector Error Handling Sample ===\n");

// 1. Basic try-catch pattern
Console.WriteLine("1. Basic try-catch pattern for handling errors...");
try
{
    var invalidConfig = new LinkExtractorConnectorConfiguration
    {
        Source = new Uri("https://thisdomaindoesnotexist12345.com/page")
    };
    var connector = new LinkExtractorConnector(invalidConfig);
    var data = await connector.GetDataAsync();
    Console.WriteLine($"   Extracted {data.RowCount} links");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"   Caught HttpRequestException: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"   Caught Exception ({ex.GetType().Name}): {ex.Message}");
}
Console.WriteLine();

// 2. Using ErrorHandler callback
Console.WriteLine("2. Using ErrorHandler callback for logging...");
var errorLog = new List<string>();

var callbackConfig = new LinkExtractorConnectorConfiguration
{
    Source = new Uri("https://httpbin.org/status/404"),  // Will return 404
    ErrorHandler = (ex) =>
    {
        var msg = $"[{DateTime.Now:HH:mm:ss}] Error logged: {ex.GetType().Name} - {ex.Message}";
        errorLog.Add(msg);
        Console.WriteLine($"   ErrorHandler invoked!");
    }
};

try
{
    var connector = new LinkExtractorConnector(callbackConfig);
    var data = await connector.GetDataAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"   Exception still propagated: {ex.GetType().Name}");
}

Console.WriteLine($"   Error log entries: {errorLog.Count}");
foreach (var entry in errorLog)
{
    Console.WriteLine($"   {entry}");
}
Console.WriteLine();

// 3. Configuring timeout for slow pages
Console.WriteLine("3. Configuring timeouts via HttpOptions...");
var timeoutConfig = new PageMetadataConnectorConfiguration
{
    Source = new Uri("https://httpbin.org/delay/3"),  // 3 second delay
    HttpOptions = new WebRequestOptions
    {
        TimeoutSeconds = 5  // Allow 5 seconds (enough for the delay)
    }
};

try
{
    Console.WriteLine("   Fetching page with 3s delay, 5s timeout...");
    var connector = new PageMetadataConnector(timeoutConfig);
    var data = await connector.GetDataAsync();
    Console.WriteLine($"   Success! Extracted {data.Schema.Count} fields");
}
catch (TaskCanceledException)
{
    Console.WriteLine("   Request timed out!");
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}
Console.WriteLine();

// 4. Short timeout example
Console.WriteLine("4. Demonstrating timeout failure...");
var shortTimeoutConfig = new PageMetadataConnectorConfiguration
{
    Source = new Uri("https://httpbin.org/delay/5"),  // 5 second delay
    HttpOptions = new WebRequestOptions
    {
        TimeoutSeconds = 2  // Only 2 seconds (will timeout)
    }
};

try
{
    Console.WriteLine("   Fetching page with 5s delay, 2s timeout...");
    var connector = new PageMetadataConnector(shortTimeoutConfig);
    var data = await connector.GetDataAsync();
    Console.WriteLine($"   Success! (unexpected)");
}
catch (TaskCanceledException)
{
    Console.WriteLine("   Request timed out as expected!");
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.GetType().Name} - {ex.Message}");
}
Console.WriteLine();

// 5. Retry pattern with exponential backoff
Console.WriteLine("5. Retry pattern with exponential backoff...");
async Task<DataBlock?> FetchWithRetryAsync(Uri url, int maxRetries = 3)
{
    var errors = new List<Exception>();

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Console.WriteLine($"   Attempt {attempt} of {maxRetries}...");
            var config = new LinkExtractorConnectorConfiguration
            {
                Source = url,
                HttpOptions = new WebRequestOptions { TimeoutSeconds = 10 }
            };
            var connector = new LinkExtractorConnector(config);
            return await connector.GetDataAsync();
        }
        catch (Exception ex)
        {
            errors.Add(ex);
            Console.WriteLine($"   Attempt {attempt} failed: {ex.GetType().Name}");

            if (attempt < maxRetries)
            {
                var delay = (int)Math.Pow(2, attempt) * 1000;  // Exponential backoff
                Console.WriteLine($"   Waiting {delay}ms before retry...");
                await Task.Delay(delay);
            }
        }
    }

    Console.WriteLine($"   All {maxRetries} attempts failed");
    return null;
}

// Test retry with a working URL
var retryResult = await FetchWithRetryAsync(new Uri("https://books.toscrape.com/"));
Console.WriteLine($"   Retry result: {(retryResult != null ? $"{retryResult.RowCount} links" : "failed")}\n");

// 6. Fallback URL pattern
Console.WriteLine("6. Fallback URL pattern...");
async Task<DataBlock?> FetchWithFallbackAsync(Uri[] urls)
{
    foreach (var url in urls)
    {
        try
        {
            Console.WriteLine($"   Trying: {url.Host}...");
            var config = new LinkExtractorConnectorConfiguration
            {
                Source = url,
                HttpOptions = new WebRequestOptions { TimeoutSeconds = 10 }
            };
            var connector = new LinkExtractorConnector(config);
            var data = await connector.GetDataAsync();
            Console.WriteLine($"   Success with {url.Host}!");
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Failed: {ex.GetType().Name}");
        }
    }
    return null;
}

var fallbackUrls = new[]
{
    new Uri("https://invalid-domain-12345.com/"),  // Will fail
    new Uri("https://books.toscrape.com/")         // Should work
};

var fallbackResult = await FetchWithFallbackAsync(fallbackUrls);
Console.WriteLine($"   Fallback result: {(fallbackResult != null ? $"{fallbackResult.RowCount} links" : "all failed")}\n");

// 7. Custom headers for authentication
Console.WriteLine("7. Custom HTTP headers configuration...");
var headerConfig = new PageMetadataConnectorConfiguration
{
    Source = new Uri("https://httpbin.org/headers"),
    HttpOptions = new WebRequestOptions
    {
        Headers = new Dictionary<string, string>
        {
            { "X-Custom-Header", "MyValue" },
            { "Authorization", "Bearer mock-token" }
        },
        UserAgent = "Datafication.WebConnector/1.0"
    }
};

try
{
    var connector = new PageMetadataConnector(headerConfig);
    var data = await connector.GetDataAsync();
    Console.WriteLine($"   Request with custom headers succeeded");
    Console.WriteLine($"   Extracted {data.Schema.Count} metadata fields");
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}
Console.WriteLine();

// 8. Valid extraction for comparison
Console.WriteLine("8. Successful extraction for comparison...");
var validConfig = new LinkExtractorConnectorConfiguration
{
    Source = new Uri("https://books.toscrape.com/"),
    HttpOptions = new WebRequestOptions
    {
        TimeoutSeconds = 30,
        FollowRedirects = true,
        MaxRedirects = 5
    }
};

try
{
    var connector = new LinkExtractorConnector(validConfig);
    var data = await connector.GetDataAsync();
    Console.WriteLine($"   Successfully extracted {data.RowCount} links from valid URL");
}
catch (Exception ex)
{
    Console.WriteLine($"   Unexpected error: {ex.Message}");
}
Console.WriteLine();

// 9. Best practices summary
Console.WriteLine("9. Error Handling Best Practices:");
Console.WriteLine("   - Always wrap GetDataAsync() in try-catch");
Console.WriteLine("   - Use ErrorHandler for logging before exceptions propagate");
Console.WriteLine("   - Configure appropriate timeouts via HttpOptions");
Console.WriteLine("   - Implement retry logic for transient failures");
Console.WriteLine("   - Consider fallback URLs for redundancy");
Console.WriteLine("   - Validate URLs and configuration before calling");

Console.WriteLine("\n=== Sample Complete ===");
