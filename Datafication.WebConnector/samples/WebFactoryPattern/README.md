# WebFactoryPattern Sample

Demonstrates using WebConnectorFactory for simplified connector creation and dependency injection patterns.

## Overview

This sample shows how to:
- Create connectors quickly using static factory methods
- Use the factory instance for configuration-based creation
- Implement IDataConnectorFactory for dependency injection
- Compare factory vs direct instantiation approaches
- Process multiple sources efficiently

## Key Features Demonstrated

### Quick Connector Creation (Static Methods)

```csharp
// One-liner for each connector type
var tableConnector = WebConnectorFactory.CreateHtmlTableConnector(url);
var linkConnector = WebConnectorFactory.CreateLinkExtractor(url);
var cssConnector = WebConnectorFactory.CreateCssSelectorConnector(url, ".product");
var metaConnector = WebConnectorFactory.CreatePageMetadataConnector(url);
var imageConnector = WebConnectorFactory.CreateImageExtractor(url);

// Then use the connector
var data = await tableConnector.GetDataAsync();
```

### Factory with Custom Configuration

```csharp
var factory = new WebConnectorFactory();

var config = new HtmlTableConnectorConfiguration
{
    Source = url,
    TableSelector = "table.wikitable",
    FirstRowIsHeader = true,
    IncludeTableMetadata = true
};

IDataConnector connector = factory.CreateDataConnector(config);
var data = await connector.GetDataAsync();
```

### IDataConnectorFactory Interface

```csharp
// For dependency injection
IDataConnectorFactory factory = new WebConnectorFactory();

// Create any connector type through unified interface
var configs = new IDataConnectorConfiguration[]
{
    new LinkExtractorConnectorConfiguration { Source = url1 },
    new ImageExtractorConnectorConfiguration { Source = url2 },
    new PageMetadataConnectorConfiguration { Source = url3 }
};

foreach (var config in configs)
{
    var connector = factory.CreateDataConnector(config);
    var data = await connector.GetDataAsync();
}
```

### Factory vs Direct Instantiation

```csharp
// Factory approach (concise)
var connector = WebConnectorFactory.CreateLinkExtractor(url);

// Direct instantiation (verbose)
var config = new LinkExtractorConnectorConfiguration { Source = url };
var connector = new LinkExtractorConnector(config);
```

## How to Run

```bash
cd WebFactoryPattern
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.WebConnector Factory Pattern Sample ===

Target URLs:
  - Books: https://books.toscrape.com/
  - Quotes: https://quotes.toscrape.com/
  - Wikipedia: https://en.wikipedia.org/wiki/...

1. Quick table extraction via WebConnectorFactory...
   Extracted ~240 rows from table

2. Quick link extraction via WebConnectorFactory...
   Extracted ~100 unique links

3. Quick CSS selector extraction via WebConnectorFactory...
   Extracted 20 product elements

4. Quick metadata extraction via WebConnectorFactory...
   Extracted ~15 metadata fields

5. Quick image extraction via WebConnectorFactory...
   Extracted ~25 images

6. Factory instance with custom configuration...
   Custom table connector: X rows

7. Using IDataConnectorFactory interface pattern...
   LinkExtractorConnectorConfiguration: X rows
   ImageExtractorConnectorConfiguration: Y rows
   PageMetadataConnectorConfiguration: Z rows

8. Comparison: Factory vs Direct instantiation...
   [Code comparison]

9. Processing multiple sources with factory...
   Link counts per site:
   - books.toscrape.com: X links
   - quotes.toscrape.com: Y links

10. Factory Pattern Benefits:
    - Simplified connector creation with sensible defaults
    - Consistent interface (IDataConnectorFactory)
    - Easy dependency injection support
    - Reduced boilerplate code
    - Type-safe configuration selection

=== Sample Complete ===
```

## Factory Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `CreateHtmlTableConnector(Uri)` | `HtmlTableConnector` | Table extraction with defaults |
| `CreateLinkExtractor(Uri)` | `LinkExtractorConnector` | Link extraction with defaults |
| `CreateCssSelectorConnector(Uri, string)` | `CssSelectorConnector` | CSS selector with specified selector |
| `CreatePageMetadataConnector(Uri)` | `PageMetadataConnector` | Metadata extraction with defaults |
| `CreateImageExtractor(Uri)` | `ImageExtractorConnector` | Image extraction with defaults |
| `CreateDataConnector(IDataConnectorConfiguration)` | `IDataConnector` | Configuration-based creation |

## When to Use Factory Pattern

### Use Static Factory Methods When:
- You need quick one-off extractions
- Default settings are acceptable
- Writing scripts or prototypes
- Minimal configuration needed

### Use Factory Instance When:
- You need custom configurations
- Implementing dependency injection
- Building reusable components
- Testing with mock factories

### Use IDataConnectorFactory Interface When:
- Building plugin architectures
- Implementing IoC containers
- Writing testable code
- Abstracting connector creation

## Dependency Injection Example

```csharp
// In Startup/Program.cs
services.AddSingleton<IDataConnectorFactory, WebConnectorFactory>();

// In your service class
public class WebScrapingService
{
    private readonly IDataConnectorFactory _factory;

    public WebScrapingService(IDataConnectorFactory factory)
    {
        _factory = factory;
    }

    public async Task<DataBlock> ExtractLinksAsync(Uri url)
    {
        var config = new LinkExtractorConnectorConfiguration { Source = url };
        var connector = _factory.CreateDataConnector(config);
        return await connector.GetDataAsync();
    }
}
```

## Related Samples

- **WebTableExtraction** - Detailed table extraction options
- **WebProductScraping** - CSS selector configuration
- **WebLinkExtraction** - Link extractor configuration
- **WebErrorHandling** - Error handling patterns
