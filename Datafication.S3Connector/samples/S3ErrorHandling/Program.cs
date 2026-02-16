using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.S3Connector Error Handling Sample ===\n");
Console.WriteLine("This sample demonstrates error handling patterns for S3 operations.\n");

// Track errors for summary
var errors = new List<string>();

// 1. Basic Try-Catch Pattern
Console.WriteLine("1. Basic Try-Catch Pattern");
Console.WriteLine("   " + new string('-', 60));

var validConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/1763.csv"
};

var validConnector = new S3DataConnector(validConfig);
try
{
    var data = await validConnector.GetDataAsync();
    Console.WriteLine($"   Success: Loaded {data.RowCount} rows\n");
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}\n");
    errors.Add($"Basic load: {ex.Message}");
}
finally
{
    validConnector.Dispose();
}

// 2. Handling Non-Existent Objects
Console.WriteLine("2. Handling Non-Existent Objects");
Console.WriteLine("   " + new string('-', 60));

var missingConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "this-file-does-not-exist.csv"
};

var missingConnector = new S3DataConnector(missingConfig);
try
{
    var data = await missingConnector.GetDataAsync();
    Console.WriteLine($"   Loaded: {data.RowCount} rows");
}
catch (Amazon.S3.AmazonS3Exception s3Ex) when (s3Ex.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    Console.WriteLine($"   Handled: Object not found (404)");
    Console.WriteLine($"   Message: {s3Ex.Message}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}\n");
    errors.Add($"Missing object: {ex.Message}");
}
finally
{
    missingConnector.Dispose();
}

// 3. Handling Invalid Bucket
Console.WriteLine("3. Handling Invalid Bucket");
Console.WriteLine("   " + new string('-', 60));

var invalidBucketConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "this-bucket-definitely-does-not-exist-12345",
    ObjectKey = "test.csv"
};

var invalidConnector = new S3DataConnector(invalidBucketConfig);
try
{
    var data = await invalidConnector.GetDataAsync();
    Console.WriteLine($"   Loaded: {data.RowCount} rows");
}
catch (Amazon.S3.AmazonS3Exception s3Ex)
{
    Console.WriteLine($"   Handled: S3 error ({s3Ex.ErrorCode})");
    Console.WriteLine($"   Status: {s3Ex.StatusCode}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}\n");
}
finally
{
    invalidConnector.Dispose();
}

// 4. Using ErrorHandler Configuration
Console.WriteLine("4. Using ErrorHandler Configuration");
Console.WriteLine("   " + new string('-', 60));

var errorHandlerConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "non-existent-file.csv",
    ErrorHandler = (Exception ex) =>
    {
        Console.WriteLine($"   [ErrorHandler] Caught: {ex.GetType().Name}");
        Console.WriteLine($"   [ErrorHandler] Message: {ex.Message}");
        errors.Add($"ErrorHandler caught: {ex.GetType().Name}");
    }
};

var errorConnector = new S3DataConnector(errorHandlerConfig);
try
{
    var data = await errorConnector.GetDataAsync();
    Console.WriteLine($"   Returned empty DataBlock: {data.RowCount} rows\n");
}
finally
{
    errorConnector.Dispose();
}

// 5. Retry Pattern with Exponential Backoff
Console.WriteLine("5. Retry Pattern (Demonstration)");
Console.WriteLine("   " + new string('-', 60));

async Task<DataBlock?> LoadWithRetryAsync(S3ConnectorConfiguration config, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        var connector = new S3DataConnector(config);
        try
        {
            Console.WriteLine($"   Attempt {attempt} of {maxRetries}...");
            return await connector.GetDataAsync();
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            var delay = (int)Math.Pow(2, attempt) * 100;  // 200ms, 400ms, 800ms
            Console.WriteLine($"   Failed, retrying in {delay}ms...");
            await Task.Delay(delay);
        }
        finally
        {
            connector.Dispose();
        }
    }
    return null;
}

var retryConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "csv/by_year/1763.csv"
};

var result = await LoadWithRetryAsync(retryConfig);
if (result != null)
{
    Console.WriteLine($"   Success after retry: {result.RowCount} rows\n");
}

// 6. Validation Before Load
Console.WriteLine("6. Pre-Load Validation");
Console.WriteLine("   " + new string('-', 60));

void ValidateConfiguration(S3ConnectorConfiguration config)
{
    var issues = new List<string>();

    if (string.IsNullOrWhiteSpace(config.Region))
        issues.Add("Region is required");

    if (string.IsNullOrWhiteSpace(config.BucketName))
        issues.Add("BucketName is required");

    if (string.IsNullOrWhiteSpace(config.ObjectKey))
        issues.Add("ObjectKey is required");

    // Check for common mistakes
    if (config.ObjectKey?.EndsWith("/") == true && !config.AllowMultipleSegments)
        issues.Add("ObjectKey looks like a prefix but AllowMultipleSegments is false");

    if (issues.Any())
    {
        Console.WriteLine("   Validation issues:");
        foreach (var issue in issues)
        {
            Console.WriteLine($"   - {issue}");
        }
    }
    else
    {
        Console.WriteLine("   Configuration is valid");
    }
}

ValidateConfiguration(validConfig);
Console.WriteLine();

var prefixConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "bucket",
    ObjectKey = "data/partitioned/"
};
ValidateConfiguration(prefixConfig);
Console.WriteLine();

// 7. Handling Unsupported File Types
Console.WriteLine("7. Handling Unsupported File Types");
Console.WriteLine("   " + new string('-', 60));

var unsupportedConfig = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "readme.txt"  // Plain text, not a supported format
};

var unsupportedConnector = new S3DataConnector(unsupportedConfig);
try
{
    var data = await unsupportedConnector.GetDataAsync();
    Console.WriteLine($"   Loaded: {data.RowCount} rows");
}
catch (NotSupportedException ex)
{
    Console.WriteLine($"   Handled: {ex.Message}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}\n");
}
finally
{
    unsupportedConnector.Dispose();
}

// 8. Summary
Console.WriteLine("8. Error Summary");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine($"   Errors caught during demo: {errors.Count}");
foreach (var error in errors)
{
    Console.WriteLine($"   - {error}");
}

Console.WriteLine("\n=== Sample Complete ===");
