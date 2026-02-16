# WebLinkExtraction Sample

Demonstrates extracting and filtering links from web pages using the LinkExtractorConnector.

## Overview

This sample shows how to:
- Extract all links from a web page
- Filter internal vs external links
- Use URL patterns to match specific link types
- Exclude unwanted link patterns
- Target specific page sections with CSS selectors
- Extract anchor metadata (id, class, rel, target)
- Analyze link distribution

## Key Features Demonstrated

### Extract All Links

```csharp
var configuration = new LinkExtractorConnectorConfiguration
{
    Source = new Uri("https://books.toscrape.com/"),
    LinkSelector = "a[href]",
    RemoveDuplicates = true,
    IncludeLinkText = true,
    IncludeTitle = true
};

var connector = new LinkExtractorConnector(configuration);
var links = await connector.GetDataAsync();
```

### Internal Links Only

```csharp
var configuration = new LinkExtractorConnectorConfiguration
{
    Source = url,
    InternalLinksOnly = true  // Only same-domain links
};
```

### External Links Only

```csharp
var configuration = new LinkExtractorConnectorConfiguration
{
    Source = url,
    ExternalLinksOnly = true  // Only different-domain links
};
```

### Filter by URL Pattern

```csharp
var configuration = new LinkExtractorConnectorConfiguration
{
    Source = url,
    UrlPattern = @"\.pdf$"  // Regex pattern for PDF links
};

// Or match category pages
var categoryConfig = new LinkExtractorConnectorConfiguration
{
    Source = url,
    UrlPattern = @"/category/"
};
```

### Exclude Patterns

```csharp
var configuration = new LinkExtractorConnectorConfiguration
{
    Source = url,
    ExcludePatterns = new List<string>
    {
        @"^javascript:",  // JavaScript links
        @"^mailto:",      // Email links
        @"^tel:",         // Phone links
        @"^#$",           // Empty anchors
        @"/admin/"        // Admin section
    }
};
```

### Target Specific Sections

```csharp
var configuration = new LinkExtractorConnectorConfiguration
{
    Source = url,
    LinkSelector = "nav.main-nav a"  // Only navigation links
};
```

## How to Run

```bash
cd WebLinkExtraction
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.WebConnector Link Extraction Sample ===

Target URL: https://books.toscrape.com/

1. Extracting all links from page...
   Found ~100 unique links

2. Schema Information:
   - LinkIndex: Int32
   - Url: String
   - LinkText: String
   - Title: String
   - Rel: String
   - Target: String

3. First 10 links:
   [Table showing links with text and URLs]

4. Extracting internal links only...
   Found ~95 internal links

5. Extracting external links only...
   Found ~5 external links

6. Filtering by URL pattern (category links)...
   Found ~50 category links

7. Using custom exclusion patterns...
   After exclusions: N links remain

8. Extracting navigation links using specific selector...
   Found N navigation links

9. Analyzing links by URL pattern...
   Link distribution:
   - Product pages: X
   - Category pages: Y
   - Homepage links: Z

10. Extracting anchor metadata (id, class)...
    Schema includes id and class columns

=== Sample Complete ===
```

## Target Website

This sample uses [books.toscrape.com](https://books.toscrape.com/) which provides various link types:
- Product links to book detail pages
- Category navigation links
- Pagination links
- Footer/header navigation

## Configuration Options

| Property | Default | Description |
|----------|---------|-------------|
| `LinkSelector` | `"a[href]"` | CSS selector for link elements |
| `InternalLinksOnly` | `false` | Extract only same-domain links |
| `ExternalLinksOnly` | `false` | Extract only different-domain links |
| `UrlPattern` | `null` | Regex pattern to match URLs |
| `RemoveDuplicates` | `true` | Remove duplicate URLs |
| `ExcludePatterns` | `[javascript:, mailto:, tel:, #$]` | Patterns to exclude |
| `IncludeLinkText` | `true` | Include anchor text |
| `IncludeTitle` | `true` | Include title attribute |
| `IncludeRel` | `true` | Include rel attribute |
| `IncludeTarget` | `true` | Include target attribute |
| `IncludeAnchorId` | `false` | Include anchor id attribute |
| `IncludeAnchorClass` | `false` | Include anchor class attribute |

## Use Cases

- **SEO Auditing**: Analyze internal link structure
- **Broken Link Detection**: Extract all links for validation
- **Site Mapping**: Build navigation structure
- **Content Discovery**: Find all related pages
- **Competitive Analysis**: Extract outbound links

## Related Samples

- **WebImageExtraction** - Extract images instead of links
- **WebProductScraping** - CSS selector scraping for product data
- **WebMetadataExtraction** - Page metadata for SEO analysis
