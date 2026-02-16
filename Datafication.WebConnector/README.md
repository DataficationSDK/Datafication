# Datafication.WebConnector

[![NuGet](https://img.shields.io/nuget/v/Datafication.WebConnector.svg)](https://www.nuget.org/packages/Datafication.WebConnector)

A web scraping connector for .NET that provides seamless integration between web pages and the Datafication.Core DataBlock API.

## Description

Datafication.WebConnector is a specialized connector library that bridges web content and the Datafication.Core ecosystem. It provides robust HTML parsing with multiple extraction strategies: HTML tables, CSS selectors, links, images, and page metadata (Open Graph, Twitter Cards, JSON-LD). The connector supports both HTTP-only and headless browser rendering via Puppeteer, enabling scraping of JavaScript-heavy single-page applications. Additional sinks enable exporting DataBlocks as PDF documents or screenshot images.

### Key Features

- **HTML Table Extraction**: Parse HTML tables with automatic header detection, column merging, and metadata tracking
- **CSS Selector Scraping**: Flexibly extract any structured data using CSS selectors with sub-selectors for nested content
- **Link Extraction**: Extract and filter links with support for internal/external filtering, regex patterns, and deduplication
- **Image Extraction**: Extract images with lazy-load detection, size filtering, and srcset/background image support
- **Page Metadata**: Extract Open Graph, Twitter Cards, JSON-LD structured data, and standard meta tags for SEO analysis
- **Browser Rendering**: Optional Puppeteer-based rendering for JavaScript-heavy pages and SPAs
- **HTTP Configuration**: Customizable User-Agent, headers, cookies, timeouts, and redirect handling
- **Screenshot Export**: Render DataBlocks as PNG/JPEG images with customizable styling
- **PDF Export**: Generate PDF reports from DataBlocks with title pages and formatting
- **Factory Pattern**: Create connectors using the WebConnectorFactory for simplified instantiation
- **Error Handling**: Global error handler configuration for graceful exception management
- **Validation**: Built-in configuration validation ensures correct setup before processing
- **Cross-Platform**: Works on Windows, Linux, and macOS

## Table of Contents

- [Description](#description)
  - [Key Features](#key-features)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
  - [Extracting HTML Tables](#extracting-html-tables)
  - [CSS Selector Scraping](#css-selector-scraping)
  - [Extracting Links](#extracting-links)
  - [Extracting Images](#extracting-images)
  - [Extracting Page Metadata](#extracting-page-metadata)
  - [Browser Rendering for SPAs](#browser-rendering-for-spas)
  - [Using the Factory Pattern](#using-the-factory-pattern)
  - [Error Handling](#error-handling)
  - [Screenshot Export](#screenshot-export)
  - [PDF Export](#pdf-export)
- [Configuration Reference](#configuration-reference)
  - [WebConnectorConfigurationBase](#webconnectorconfigurationbase)
  - [WebRequestOptions](#webrequestoptions)
  - [BrowserOptions](#browseroptions)
  - [HtmlTableConnectorConfiguration](#htmltableconnectorconfiguration)
  - [CssSelectorConnectorConfiguration](#cssselectorconnectorconfiguration)
  - [LinkExtractorConnectorConfiguration](#linkextractorconnectorconfiguration)
  - [ImageExtractorConnectorConfiguration](#imageextractorconnectorconfiguration)
  - [PageMetadataConnectorConfiguration](#pagemetadataconnectorconfiguration)
- [API Reference](#api-reference)
  - [Connector Classes](#connector-classes)
  - [Sink Classes](#sink-classes)
  - [Extension Methods](#extension-methods)
  - [Factory Methods](#factory-methods)
- [Common Patterns](#common-patterns)
  - [SEO Analysis Pipeline](#seo-analysis-pipeline)
  - [Product Catalog Scraping](#product-catalog-scraping)
  - [Link Audit and Broken Link Detection](#link-audit-and-broken-link-detection)
  - [Image Asset Inventory](#image-asset-inventory)
- [Performance Tips](#performance-tips)
- [License](#license)

## Installation

> **Note**: Datafication.WebConnector is currently in pre-release. The packages are now available on nuget.org.

```bash
dotnet add package Datafication.WebConnector
```

**Running the Samples:**

```bash
cd samples/WebTableExtraction
dotnet run
```

**Dependencies:**
- AngleSharp - for HTML parsing
- PuppeteerSharp - for browser rendering (optional, required for UseBrowser mode)

## Usage Examples

### Extracting HTML Tables

Extract HTML tables from web pages with automatic header detection:

```csharp
using Datafication.Connectors.WebConnector.Connectors;

// Create configuration for table extraction
var configuration = new HtmlTableConnectorConfiguration
{
    Source = new Uri("https://example.com/data-tables.html"),
    TableSelector = "table.data-table",  // Target specific tables
    FirstRowIsHeader = true,
    IncludeTableMetadata = true,  // Adds TableIndex, TableId, RowIndex columns
    TrimCellValues = true
};

// Create connector and extract data
var connector = new HtmlTableConnector(configuration);
var data = await connector.GetDataAsync();

Console.WriteLine($"Extracted {data.RowCount} rows from table");
Console.WriteLine(await data.TextTableAsync());
```

**Merging Multiple Tables:**

```csharp
var configuration = new HtmlTableConnectorConfiguration
{
    Source = new Uri("https://example.com/multi-table-page.html"),
    TableSelector = "table",  // Match all tables
    MergeTables = true,  // Combine all tables into one DataBlock
    IncludeTableMetadata = true  // Track which table each row came from
};

var connector = new HtmlTableConnector(configuration);
var allTables = await connector.GetDataAsync();

// Analyze data by source table
var tableStats = allTables
    .GroupBy("TableIndex")
    .Info();
Console.WriteLine(await tableStats.TextTableAsync());
```

### CSS Selector Scraping

Use CSS selectors to extract any structured content:

```csharp
using Datafication.Connectors.WebConnector.Connectors;

// Scrape product listings
var configuration = new CssSelectorConnectorConfiguration
{
    Source = new Uri("https://example.com/products"),
    Selector = ".product-card",  // Each matched element becomes a row

    // Extract text content from nested elements
    SubSelectors = new Dictionary<string, string>
    {
        { "ProductName", "h2.title" },
        { "Price", ".price-value" },
        { "Description", ".product-description" }
    },

    // Extract attributes from nested elements
    AttributeSubSelectors = new Dictionary<string, string>
    {
        { "ImageUrl", "img|src" },          // selector|attribute format
        { "ProductLink", "a.details|href" },
        { "DataId", ".|data-product-id" }   // "." means current element
    },

    IncludeElementIndex = true,
    IncludeInnerText = false,  // We're using SubSelectors instead
    TrimValues = true,
    MaxElements = 100  // Limit results
};

var connector = new CssSelectorConnector(configuration);
var products = await connector.GetDataAsync();

Console.WriteLine($"Found {products.RowCount} products");
Console.WriteLine(await products.TextTableAsync());
```

**Extracting Article Listings:**

```csharp
var configuration = new CssSelectorConnectorConfiguration
{
    Source = new Uri("https://news.example.com/"),
    Selector = "article.news-item",
    SubSelectors = new Dictionary<string, string>
    {
        { "Headline", "h2" },
        { "Summary", ".summary" },
        { "Author", ".author-name" },
        { "Date", "time" }
    },
    AttributeSubSelectors = new Dictionary<string, string>
    {
        { "ArticleUrl", "a|href" },
        { "PublishDate", "time|datetime" }
    },
    IncludeOuterHtml = false,
    IncludeInnerHtml = false
};

var connector = new CssSelectorConnector(configuration);
var articles = await connector.GetDataAsync();
```

### Extracting Links

Extract and filter links from web pages:

```csharp
using Datafication.Connectors.WebConnector.Connectors;

// Extract all internal links
var configuration = new LinkExtractorConnectorConfiguration
{
    Source = new Uri("https://example.com/"),
    InternalLinksOnly = true,  // Only links to same domain
    RemoveDuplicates = true,
    IncludeLinkText = true,
    IncludeTitle = true,
    IncludeRel = true,
    ExcludePatterns = new List<string>
    {
        @"^javascript:",
        @"^mailto:",
        @"^tel:",
        @"^#$",
        @"/login",
        @"/logout"
    }
};

var connector = new LinkExtractorConnector(configuration);
var links = await connector.GetDataAsync();

Console.WriteLine($"Found {links.RowCount} internal links");
```

**Filter by URL Pattern:**

```csharp
// Extract only PDF document links
var configuration = new LinkExtractorConnectorConfiguration
{
    Source = new Uri("https://example.com/resources"),
    UrlPattern = @"\.pdf$",  // Regex pattern
    IncludeLinkText = true
};

var connector = new LinkExtractorConnector(configuration);
var pdfLinks = await connector.GetDataAsync();
```

**Extract External Links Only:**

```csharp
var configuration = new LinkExtractorConnectorConfiguration
{
    Source = new Uri("https://example.com/"),
    ExternalLinksOnly = true,
    LinkSelector = "article a[href]",  // Scope to article content only
    IncludeRel = true  // Check for nofollow, sponsored, etc.
};

var connector = new LinkExtractorConnector(configuration);
var externalLinks = await connector.GetDataAsync();

// Analyze external link patterns
var domainCounts = externalLinks
    .GroupBy("Href")
    .Info();
```

### Extracting Images

Extract images and their metadata:

```csharp
using Datafication.Connectors.WebConnector.Connectors;

var configuration = new ImageExtractorConnectorConfiguration
{
    Source = new Uri("https://example.com/gallery"),
    ImageSelector = "img",
    IncludeDataSrc = true,  // Detect lazy-loaded images
    IncludeSrcset = true,   // Include responsive image info
    IncludeParentInfo = true,
    RemoveDuplicates = true,
    ResolveUrls = true,  // Convert relative URLs to absolute
    MinWidth = 100,      // Filter out small images/icons
    MinHeight = 100,
    ExcludedExtensions = new List<string> { ".svg", ".ico" }
};

var connector = new ImageExtractorConnector(configuration);
var images = await connector.GetDataAsync();

Console.WriteLine($"Found {images.RowCount} images");
Console.WriteLine(await images.Head(10).TextTableAsync());
```

**Include Background Images:**

```csharp
var configuration = new ImageExtractorConnectorConfiguration
{
    Source = new Uri("https://example.com/"),
    IncludeBackgroundImages = true,
    BackgroundImageSelector = ".hero, .banner, .featured",  // Scope search
    AllowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".webp" }
};

var connector = new ImageExtractorConnector(configuration);
var allImages = await connector.GetDataAsync();
```

### Extracting Page Metadata

Extract SEO metadata, Open Graph, Twitter Cards, and JSON-LD:

```csharp
using Datafication.Connectors.WebConnector.Connectors;

var configuration = new PageMetadataConnectorConfiguration
{
    Source = new Uri("https://example.com/article/123"),
    ExtractStandardMeta = true,   // title, description, keywords, etc.
    ExtractOpenGraph = true,      // og:title, og:image, etc.
    ExtractTwitterCard = true,    // twitter:card, twitter:title, etc.
    ExtractJsonLd = true,         // Schema.org structured data
    ExtractLinkTags = true,       // canonical, favicon, feeds, etc.
    SingleRowResult = true        // One row with all metadata as columns
};

var connector = new PageMetadataConnector(configuration);
var metadata = await connector.GetDataAsync();

Console.WriteLine("Page Metadata:");
Console.WriteLine(await metadata.TextTableAsync());

// Access specific metadata values
var ogTitle = metadata.GetColumn("OG_title").GetValue<string>(0);
var ogImage = metadata.GetColumn("OG_image").GetValue<string>(0);
var jsonLd = metadata.GetColumn("JsonLd").GetValue<string>(0);
```

**Name/Value Pair Format:**

```csharp
var configuration = new PageMetadataConnectorConfiguration
{
    Source = new Uri("https://example.com/"),
    SingleRowResult = false,  // Returns Name/Value pairs instead
    ExtractAllMetaTags = true  // Include all meta tags
};

var connector = new PageMetadataConnector(configuration);
var metadata = await connector.GetDataAsync();

// Result has Name and Value columns
Console.WriteLine(await metadata.TextTableAsync());
```

### Browser Rendering for SPAs

Use Puppeteer for JavaScript-rendered content:

```csharp
var configuration = new HtmlTableConnectorConfiguration
{
    Source = new Uri("https://spa-example.com/data"),
    UseBrowser = true,  // Enable Puppeteer rendering

    BrowserOptions = new BrowserOptions
    {
        Headless = true,
        ViewportWidth = 1920,
        ViewportHeight = 1080,
        PageLoadTimeoutMs = 60000,
        WaitStrategy = BrowserWaitStrategy.NetworkIdle,  // Wait for AJAX
        PostLoadDelayMs = 2000,  // Additional wait for animations

        // Execute JavaScript after page load
        PostLoadScript = @"
            // Click 'Load More' button if present
            const btn = document.querySelector('.load-more');
            if (btn) btn.click();
        "
    }
};

var connector = new HtmlTableConnector(configuration);
var data = await connector.GetDataAsync();
```

**Browser with Custom Headers and Cookies:**

```csharp
var configuration = new CssSelectorConnectorConfiguration
{
    Source = new Uri("https://authenticated-site.com/dashboard"),
    Selector = ".data-item",
    UseBrowser = true,

    HttpOptions = new WebRequestOptions
    {
        UserAgent = "Custom Bot 1.0",
        Headers = new Dictionary<string, string>
        {
            { "Authorization", "Bearer token123" }
        },
        Cookies = new Dictionary<string, string>
        {
            { "session_id", "abc123" },
            { "user_pref", "dark_mode" }
        }
    },

    BrowserOptions = new BrowserOptions
    {
        WaitStrategy = BrowserWaitStrategy.NetworkIdle
    }
};

var connector = new CssSelectorConnector(configuration);
var data = await connector.GetDataAsync();
```

### Using the Factory Pattern

Use WebConnectorFactory for simplified connector creation:

```csharp
using Datafication.Connectors.WebConnector.Factories;

// Quick creation with defaults
var tableConnector = WebConnectorFactory.CreateHtmlTableConnector(
    new Uri("https://example.com/table.html"));
var tables = await tableConnector.GetDataAsync();

var linkExtractor = WebConnectorFactory.CreateLinkExtractor(
    new Uri("https://example.com/"));
var links = await linkExtractor.GetDataAsync();

var cssConnector = WebConnectorFactory.CreateCssSelectorConnector(
    new Uri("https://example.com/products"),
    ".product-item");
var products = await cssConnector.GetDataAsync();

var metadataConnector = WebConnectorFactory.CreatePageMetadataConnector(
    new Uri("https://example.com/article"));
var metadata = await metadataConnector.GetDataAsync();

var imageExtractor = WebConnectorFactory.CreateImageExtractor(
    new Uri("https://example.com/gallery"));
var images = await imageExtractor.GetDataAsync();
```

**Factory with Configuration:**

```csharp
using Datafication.Core.Connectors;

// Use factory interface for dynamic connector creation
IDataConnectorFactory factory = new WebConnectorFactory();

var config = new HtmlTableConnectorConfiguration
{
    Source = new Uri("https://example.com/data"),
    MergeTables = true
};

IDataConnector connector = factory.CreateDataConnector(config);
var data = await connector.GetDataAsync();
```

### Error Handling

Configure global error handling:

```csharp
var configuration = new HtmlTableConnectorConfiguration
{
    Source = new Uri("https://example.com/tables"),
    ErrorHandler = (exception) =>
    {
        Console.WriteLine($"Web Connector Error: {exception.Message}");
        // Log to file, send alert, etc.
    }
};

var connector = new HtmlTableConnector(configuration);

try
{
    var data = await connector.GetDataAsync();
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP error: {ex.Message}");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Request timed out: {ex.Message}");
}
```

### Screenshot Export

Render DataBlocks as images:

```csharp
using Datafication.Sinks.Connectors.WebConnector;

// Create or load a DataBlock
var data = await connector.GetDataAsync();

// Quick screenshot (async)
var imageBytes = await data.ScreenshotAsync(rowLimit: 50, title: "Data Preview");
await File.WriteAllBytesAsync("output.png", imageBytes);

// Save directly to file
await data.ScreenshotToFileAsync(
    "report.png",
    rowLimit: 100,
    title: "Sales Report",
    format: ScreenshotFormat.Png);

// JPEG with quality setting
await data.ScreenshotToFileAsync(
    "report.jpg",
    format: ScreenshotFormat.Jpeg,
    quality: 85);

// High-resolution (2x scale)
var highResBytes = await data.ScreenshotHighResAsync(
    rowLimit: 50,
    title: "High-Res Preview");
```

**Advanced Screenshot Options:**

```csharp
var sink = new ScreenshotSink
{
    RowLimit = 100,
    Title = "Quarterly Report",
    Subtitle = "Q4 2024 Sales Data",
    Format = ScreenshotFormat.Png,
    ViewportWidth = 1400,
    FullPage = true,
    DeviceScaleFactor = 2,  // High DPI
    BackgroundColor = "#f5f5f5",
    HeaderBackgroundColor = "#2563eb",
    HeaderTextColor = "#ffffff",
    UseAlternateRowColor = true,
    CustomCss = @"
        .title { color: #1e40af; }
        td { font-size: 12px; }
    "
};

var imageBytes = await sink.Transform(dataBlock);
```

### PDF Export

Generate PDF reports from DataBlocks:

```csharp
using Datafication.Sinks.Connectors.WebConnector;

var data = await connector.GetDataAsync();

var pdfSink = new PdfSink
{
    Title = "Data Export Report",
    Author = "Datafication System",
    Description = "Automated data extraction from web sources",
    RowLimit = 1000,
    LandscapeOrientation = true
};

using var pdfStream = await pdfSink.Transform(data);
using var fileStream = File.Create("report.pdf");
await pdfStream.CopyToAsync(fileStream);

Console.WriteLine("PDF report generated");
```

## Configuration Reference

### WebConnectorConfigurationBase

Base configuration shared by all web connectors.

**Properties:**

- **`Source`** (Uri, required): The URL to scrape
  - Must be an absolute HTTP or HTTPS URL
  - File URIs are not supported

- **`Id`** (string, auto-generated): Unique identifier for the configuration

- **`UseBrowser`** (bool, default: false): Whether to use Puppeteer for rendering
  - `false`: HTTP-only fetching (faster, no JavaScript)
  - `true`: Full browser rendering (supports SPAs)

- **`HttpOptions`** (WebRequestOptions): HTTP request configuration

- **`BrowserOptions`** (BrowserOptions): Browser rendering configuration

- **`ErrorHandler`** (Action<Exception>?, optional): Global exception handler

### WebRequestOptions

HTTP request configuration used when `UseBrowser` is false.

**Properties:**

- **`UserAgent`** (string): User-Agent header
  - Default: Modern Chrome user agent string

- **`TimeoutSeconds`** (int, default: 30): Request timeout

- **`FollowRedirects`** (bool, default: true): Follow HTTP redirects

- **`MaxRedirects`** (int, default: 10): Maximum redirects to follow

- **`Headers`** (Dictionary<string, string>): Custom HTTP headers

- **`Cookies`** (Dictionary<string, string>): Cookies to send

- **`Accept`** (string): Accept header value

- **`AcceptLanguage`** (string?): Accept-Language header value

### BrowserOptions

Puppeteer browser configuration used when `UseBrowser` is true.

**Properties:**

- **`Headless`** (bool, default: true): Run browser without visible window

- **`ViewportWidth`** (int, default: 1920): Browser viewport width

- **`ViewportHeight`** (int, default: 1080): Browser viewport height

- **`PageLoadTimeoutMs`** (int, default: 30000): Page load timeout

- **`WaitStrategy`** (BrowserWaitStrategy): When to consider page "loaded"
  - `Load`: Wait for load event (fastest)
  - `DOMContentLoaded`: Wait for DOM ready
  - `NetworkIdle`: Wait for network idle (most reliable)

- **`PostLoadDelayMs`** (int, default: 0): Additional delay after page load

- **`PostLoadScript`** (string?): JavaScript to execute after load

- **`ExecutablePath`** (string?): Custom browser executable path

- **`LaunchArgs`** (string[]?): Additional browser launch arguments

### HtmlTableConnectorConfiguration

Configuration for HTML table extraction.

**Properties:**

- **`TableSelector`** (string, default: "table"): CSS selector for tables

- **`TableIndex`** (int?, default: null): Extract specific table by index (0-based)

- **`FirstRowIsHeader`** (bool, default: true): Treat first row as headers

- **`UseTheadForHeaders`** (bool, default: true): Use `<thead>` for headers when available

- **`IncludeTableMetadata`** (bool, default: true): Add TableIndex, TableId, TableClass, RowIndex columns

- **`MergeTables`** (bool, default: false): Combine all matched tables

- **`SkipEmptyRows`** (bool, default: true): Skip rows with all empty cells

- **`TrimCellValues`** (bool, default: true): Trim whitespace from cell values

### CssSelectorConnectorConfiguration

Configuration for CSS selector-based scraping.

**Properties:**

- **`Selector`** (string, default: "*"): Primary CSS selector for elements

- **`SubSelectors`** (Dictionary<string, string>): Map column names to CSS selectors for text extraction

- **`AttributeSubSelectors`** (Dictionary<string, string>): Map column names to "selector|attribute" for attribute extraction

- **`Attributes`** (List<string>): Attribute names to extract from matched elements

- **`IncludeInnerText`** (bool, default: true): Include text content column

- **`IncludeInnerHtml`** (bool, default: false): Include inner HTML column

- **`IncludeOuterHtml`** (bool, default: false): Include outer HTML column

- **`IncludeTagName`** (bool, default: true): Include tag name column

- **`IncludeElementIndex`** (bool, default: true): Include element index column

- **`MaxElements`** (int?, default: null): Limit number of elements

- **`TrimValues`** (bool, default: true): Trim whitespace from values

### LinkExtractorConnectorConfiguration

Configuration for link extraction.

**Properties:**

- **`LinkSelector`** (string, default: "a[href]"): CSS selector for links

- **`InternalLinksOnly`** (bool, default: false): Only include same-domain links

- **`ExternalLinksOnly`** (bool, default: false): Only include different-domain links

- **`UrlPattern`** (string?, default: null): Regex pattern to filter URLs

- **`RemoveDuplicates`** (bool, default: true): Remove duplicate URLs

- **`ExcludePatterns`** (List<string>): Regex patterns to exclude (default excludes javascript:, mailto:, tel:, #)

- **`IncludeLinkText`** (bool, default: true): Include anchor text

- **`IncludeTitle`** (bool, default: true): Include title attribute

- **`IncludeRel`** (bool, default: true): Include rel attribute

- **`IncludeTarget`** (bool, default: true): Include target attribute

- **`IncludeAnchorId`** (bool, default: false): Include anchor id attribute

- **`IncludeAnchorClass`** (bool, default: false): Include anchor class attribute

### ImageExtractorConnectorConfiguration

Configuration for image extraction.

**Properties:**

- **`ImageSelector`** (string, default: "img"): CSS selector for images

- **`IncludeBackgroundImages`** (bool, default: false): Extract CSS background images

- **`BackgroundImageSelector`** (string, default: "*"): Scope for background image search

- **`MinWidth`** (int?, default: null): Minimum image width filter

- **`MinHeight`** (int?, default: null): Minimum image height filter

- **`AllowedExtensions`** (List<string>): Only include these extensions

- **`ExcludedExtensions`** (List<string>): Exclude these extensions

- **`IncludeDataSrc`** (bool, default: true): Detect lazy-loaded images

- **`IncludeSrcset`** (bool, default: true): Include srcset and sizes

- **`IncludeParentInfo`** (bool, default: true): Include parent element info

- **`RemoveDuplicates`** (bool, default: true): Remove duplicate URLs

- **`ResolveUrls`** (bool, default: true): Convert relative URLs to absolute

### PageMetadataConnectorConfiguration

Configuration for page metadata extraction.

**Properties:**

- **`ExtractStandardMeta`** (bool, default: true): Extract standard meta tags

- **`ExtractOpenGraph`** (bool, default: true): Extract Open Graph tags

- **`ExtractTwitterCard`** (bool, default: true): Extract Twitter Card tags

- **`ExtractJsonLd`** (bool, default: true): Extract JSON-LD structured data

- **`ExtractLinkTags`** (bool, default: true): Extract link tags (canonical, favicon, etc.)

- **`CustomMetaTags`** (Dictionary<string, string>): Custom meta tags to extract

- **`ExtractAllMetaTags`** (bool, default: false): Extract all meta tags

- **`SingleRowResult`** (bool, default: true): Return single row (vs name/value pairs)

## API Reference

For complete API documentation, see the [Datafication.Connectors.WebConnector API Reference](https://datafication.co/help/api/reference/Datafication.Connectors.WebConnector.html).

### Connector Classes

**HtmlTableConnector**
- **Constructor**: `HtmlTableConnector(HtmlTableConnectorConfiguration configuration)`
- **Methods**:
  - `Task<DataBlock> GetDataAsync()` - Extract tables as DataBlock
  - `Task<IStorageDataBlock> GetStorageDataAsync(IStorageDataBlock target, int batchSize = 10000)`
  - `string GetConnectorId()` - Get unique identifier

**CssSelectorConnector**
- **Constructor**: `CssSelectorConnector(CssSelectorConnectorConfiguration configuration)`
- **Methods**: Same as HtmlTableConnector

**LinkExtractorConnector**
- **Constructor**: `LinkExtractorConnector(LinkExtractorConnectorConfiguration configuration)`
- **Methods**: Same as HtmlTableConnector
- **Output Columns**: Href, IsExternal, Text, Title, Rel, Target, AnchorId, AnchorClass

**ImageExtractorConnector**
- **Constructor**: `ImageExtractorConnector(ImageExtractorConnectorConfiguration configuration)`
- **Methods**: Same as HtmlTableConnector
- **Output Columns**: ElementIndex, Src, Alt, Title, Width, Height, Loading, IsBackground, Srcset, Sizes, DataSrc, ParentTag, ParentClass, FileExtension

**PageMetadataConnector**
- **Constructor**: `PageMetadataConnector(PageMetadataConnectorConfiguration configuration)`
- **Methods**: Same as HtmlTableConnector

### Sink Classes

**ScreenshotSink** (namespace: `Datafication.Sinks.Connectors.WebConnector`)
- **Implements**: `IDataSink<byte[]>`
- **Properties**:
  - `int RowLimit` (default: 100)
  - `ScreenshotFormat Format` (default: Png)
  - `int ViewportWidth` (default: 1200)
  - `bool FullPage` (default: true)
  - `string BackgroundColor` (default: "#ffffff")
  - `int? Quality` (JPEG only)
  - `double DeviceScaleFactor` (default: 1)
  - `string? Title`, `string? Subtitle`
  - `string? CustomCss`
  - `bool UseAlternateRowColor` (default: true)
  - `string HeaderBackgroundColor`, `string HeaderTextColor`
- **Methods**:
  - `Task<byte[]> Transform(DataBlock dataBlock)`

**PdfSink** (namespace: `Datafication.Sinks.Connectors.WebConnector`)
- **Implements**: `IDataSink<Stream>`
- **Properties**:
  - `int RowLimit` (default: 1000)
  - `bool LandscapeOrientation` (default: true)
  - `string? Title`, `string? Author`, `string? Description`
- **Methods**:
  - `Task<Stream> Transform(DataBlock dataBlock)`

### Extension Methods

**ScreenshotSinkExtension** (namespace: `Datafication.Sinks.Connectors.WebConnector`)

```csharp
// Async methods
Task<byte[]> ScreenshotAsync(this DataBlock dataBlock, int rowLimit = 100, string? title = null)
Task ScreenshotToFileAsync(this DataBlock dataBlock, string filePath, int rowLimit = 100, string? title = null, ScreenshotFormat format = Png, int? quality = null)
Task<byte[]> ScreenshotHighResAsync(this DataBlock dataBlock, int rowLimit = 100, string? title = null)

// Synchronous methods
byte[] Screenshot(this DataBlock dataBlock, int rowLimit = 100, string? title = null)
void ScreenshotToFile(this DataBlock dataBlock, string filePath, ...)
```

### Factory Methods

**WebConnectorFactory** (namespace: `Datafication.Connectors.WebConnector.Factories`)

```csharp
// Instance method (IDataConnectorFactory interface)
IDataConnector CreateDataConnector(IDataConnectorConfiguration configuration)

// Static factory methods
static HtmlTableConnector CreateHtmlTableConnector(Uri source)
static LinkExtractorConnector CreateLinkExtractor(Uri source)
static CssSelectorConnector CreateCssSelectorConnector(Uri source, string selector)
static PageMetadataConnector CreatePageMetadataConnector(Uri source)
static ImageExtractorConnector CreateImageExtractor(Uri source)
```

## Common Patterns

### SEO Analysis Pipeline

```csharp
// Extract metadata from multiple pages
var urls = new[]
{
    "https://example.com/",
    "https://example.com/about",
    "https://example.com/products"
};

var allMetadata = new DataBlock();
allMetadata.AddColumn(new DataColumn("Url", typeof(string)));
allMetadata.AddColumn(new DataColumn("Title", typeof(string)));
allMetadata.AddColumn(new DataColumn("Description", typeof(string)));
allMetadata.AddColumn(new DataColumn("OG_title", typeof(string)));
allMetadata.AddColumn(new DataColumn("OG_image", typeof(string)));
allMetadata.AddColumn(new DataColumn("Canonical", typeof(string)));
allMetadata.AddColumn(new DataColumn("HasJsonLd", typeof(bool)));

foreach (var url in urls)
{
    var config = new PageMetadataConnectorConfiguration
    {
        Source = new Uri(url),
        SingleRowResult = true
    };

    var connector = new PageMetadataConnector(config);
    var metadata = await connector.GetDataAsync();

    allMetadata.AddRow(new object[]
    {
        url,
        metadata.GetColumn("Title").GetValue<string>(0) ?? "",
        metadata.GetColumn("Description").GetValue<string>(0) ?? "",
        metadata.GetColumn("OG_title").GetValue<string>(0) ?? "",
        metadata.GetColumn("OG_image").GetValue<string>(0) ?? "",
        metadata.GetColumn("Canonical").GetValue<string>(0) ?? "",
        !string.IsNullOrEmpty(metadata.GetColumn("JsonLd").GetValue<string>(0))
    });
}

Console.WriteLine("SEO Analysis:");
Console.WriteLine(await allMetadata.TextTableAsync());
```

### Product Catalog Scraping

```csharp
// Scrape e-commerce product listings
var config = new CssSelectorConnectorConfiguration
{
    Source = new Uri("https://store.example.com/products?page=1"),
    Selector = ".product-card",
    SubSelectors = new Dictionary<string, string>
    {
        { "Name", ".product-name" },
        { "Price", ".price" },
        { "OriginalPrice", ".original-price" },
        { "Rating", ".rating-value" },
        { "ReviewCount", ".review-count" }
    },
    AttributeSubSelectors = new Dictionary<string, string>
    {
        { "ProductUrl", "a.product-link|href" },
        { "ImageUrl", "img|src" },
        { "ProductId", ".|data-product-id" }
    },
    UseBrowser = true,  // For JavaScript-rendered content
    BrowserOptions = new BrowserOptions
    {
        WaitStrategy = BrowserWaitStrategy.NetworkIdle
    }
};

var connector = new CssSelectorConnector(config);
var products = await connector.GetDataAsync();

// Clean and transform data
var cleanedProducts = products
    .Compute("PriceValue", "REPLACE(Price, '$', '')")
    .Compute("HasDiscount", "OriginalPrice != ''")
    .Select("ProductId", "Name", "PriceValue", "HasDiscount", "Rating", "ProductUrl", "ImageUrl");

Console.WriteLine($"Found {cleanedProducts.RowCount} products");
Console.WriteLine(await cleanedProducts.Head(10).TextTableAsync());
```

### Link Audit and Broken Link Detection

```csharp
// Extract all links for auditing
var config = new LinkExtractorConnectorConfiguration
{
    Source = new Uri("https://example.com/"),
    RemoveDuplicates = true,
    IncludeLinkText = true,
    IncludeRel = true
};

var connector = new LinkExtractorConnector(config);
var links = await connector.GetDataAsync();

// Categorize links
var internalLinks = links.Where("IsExternal", false);
var externalLinks = links.Where("IsExternal", true);
var nofollowLinks = links.Where("Rel", "nofollow", ComparisonOperator.Contains);

Console.WriteLine($"Internal links: {internalLinks.RowCount}");
Console.WriteLine($"External links: {externalLinks.RowCount}");
Console.WriteLine($"Nofollow links: {nofollowLinks.RowCount}");

// Export for further analysis
var linksCsv = await links.CsvStringSinkAsync();
await File.WriteAllTextAsync("link_audit.csv", linksCsv);
```

### Image Asset Inventory

```csharp
// Create inventory of all images on a site
var config = new ImageExtractorConnectorConfiguration
{
    Source = new Uri("https://example.com/"),
    IncludeDataSrc = true,
    IncludeSrcset = true,
    IncludeParentInfo = true,
    RemoveDuplicates = true,
    ResolveUrls = true
};

var connector = new ImageExtractorConnector(config);
var images = await connector.GetDataAsync();

// Analyze image usage
var byExtension = images
    .GroupBy("FileExtension")
    .Info();

var missingAlt = images
    .Where("Alt", "", ComparisonOperator.Equals)
    .Select("Src", "ParentTag", "ParentClass");

Console.WriteLine("Images by extension:");
Console.WriteLine(await byExtension.TextTableAsync());

Console.WriteLine($"\nImages missing alt text: {missingAlt.RowCount}");
Console.WriteLine(await missingAlt.Head(10).TextTableAsync());
```

## Performance Tips

1. **Use HTTP-Only When Possible**: Browser rendering (`UseBrowser = true`) is significantly slower than HTTP-only fetching. Only enable browser mode when the page requires JavaScript rendering.

2. **Choose the Right Wait Strategy**: When using browser mode, `BrowserWaitStrategy.NetworkIdle` is most reliable but slowest. Use `DOMContentLoaded` or `Load` for faster extraction when content doesn't require AJAX.

3. **Limit Result Sets**: Use `MaxElements` in CSS selector configuration or filter results early to reduce memory usage:
   ```csharp
   var config = new CssSelectorConnectorConfiguration
   {
       MaxElements = 100  // Limit to first 100 elements
   };
   ```

4. **Scope Your Selectors**: Use specific CSS selectors to reduce parsing overhead:
   ```csharp
   TableSelector = "table#data-table"  // Faster than "table"
   LinkSelector = "article a[href]"    // Faster than "a[href]"
   ```

5. **Remove Unnecessary Columns**: Disable metadata columns if not needed:
   ```csharp
   IncludeTableMetadata = false
   IncludeElementIndex = false
   IncludeParentInfo = false
   ```

6. **Enable Deduplication**: Use `RemoveDuplicates = true` for links and images to reduce data volume.

7. **Set Appropriate Timeouts**: Adjust timeouts based on expected page load times:
   ```csharp
   HttpOptions = new WebRequestOptions
   {
       TimeoutSeconds = 15  // Fail fast on slow pages
   }
   ```

8. **Cache Browser Instance**: Puppeteer downloads Chromium on first use. The connector caches this, but initial runs will be slower. Consider pre-warming in production environments.

9. **Dispose DataBlocks**: For large scraping operations, dispose intermediate DataBlocks:
   ```csharp
   using (var rawData = await connector.GetDataAsync())
   {
       var processed = rawData.Where(...).Select(...);
       // rawData disposed after processing
   }
   ```

10. **Use Factory for Simple Cases**: When using default configurations, the factory methods provide cleaner code with less overhead:
    ```csharp
    var connector = WebConnectorFactory.CreateHtmlTableConnector(uri);
    ```

## License

This library is licensed under the **Datafication SDK License Agreement**. See the [LICENSE](./LICENSE) file for details.

**Summary:**
- **Free Use**: Organizations with fewer than 5 developers AND annual revenue under $500,000 USD may use the SDK without a commercial license
- **Commercial License Required**: Organizations with 5+ developers OR annual revenue exceeding $500,000 USD must obtain a commercial license
- **Open Source Exemption**: Open source projects meeting specific criteria may be exempt from developer count limits

For commercial licensing inquiries, contact [support@datafication.co](mailto:support@datafication.co).

**Third-Party Libraries:**
- AngleSharp - MIT License
- PuppeteerSharp - MIT License

---

**Datafication.WebConnector** - Seamlessly connect web content to the Datafication ecosystem.

For more examples and documentation, visit our [samples directory](../../samples/).
