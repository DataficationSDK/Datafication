# WebMetadataExtraction Sample

Demonstrates extracting page metadata for SEO analysis using the PageMetadataConnector.

## Overview

This sample shows how to:
- Extract all page metadata with default settings
- Extract standard meta tags (description, keywords, author, etc.)
- Extract Open Graph protocol tags for social sharing
- Extract Twitter Card metadata
- Extract JSON-LD structured data (Schema.org)
- Extract link tags (canonical, favicon, feed URLs)
- Use custom meta tag extraction
- Choose between single-row and name/value pair output formats

## Key Features Demonstrated

### Extract All Metadata

```csharp
var configuration = new PageMetadataConnectorConfiguration
{
    Source = new Uri("https://quotes.toscrape.com/"),
    ExtractStandardMeta = true,
    ExtractOpenGraph = true,
    ExtractTwitterCard = true,
    ExtractJsonLd = true,
    ExtractLinkTags = true,
    SingleRowResult = true  // One row with all metadata as columns
};

var connector = new PageMetadataConnector(configuration);
var metadata = await connector.GetDataAsync();
```

### Extract Standard Meta Tags Only

```csharp
var configuration = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractStandardMeta = true,
    ExtractOpenGraph = false,
    ExtractTwitterCard = false,
    ExtractJsonLd = false,
    ExtractLinkTags = false
};
```

### Extract Open Graph Tags

```csharp
var configuration = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractOpenGraph = true
    // Open Graph columns are prefixed with "OG_"
    // e.g., OG_title, OG_image, OG_description
};
```

### Name/Value Pair Format

```csharp
var configuration = new PageMetadataConnectorConfiguration
{
    Source = url,
    SingleRowResult = false  // Returns rows with Name/Value columns
};

// Result schema: Name, Value
// Each metadata field becomes a row
```

### Custom Meta Tags

```csharp
var configuration = new PageMetadataConnectorConfiguration
{
    Source = url,
    CustomMetaTags = new Dictionary<string, string>
    {
        { "MyViewport", "viewport" },
        { "MyRobots", "robots" },
        { "AppId", "fb:app_id" }
    }
};
```

## How to Run

```bash
cd WebMetadataExtraction
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.WebConnector Metadata Extraction Sample ===

Target URL: https://quotes.toscrape.com/

1. Extracting all metadata (default settings)...
   Extracted ~20 metadata fields

2. Metadata fields extracted:
   - Url
   - Title
   - Description
   - Keywords
   - Author
   - Charset
   - Language
   - OG_title
   - OG_image
   ...

3. Key metadata values:
   URL: https://quotes.toscrape.com/
   Title: Quotes to Scrape
   Description: ...
   Charset: utf-8
   Language: en

4. Extracting standard meta tags only...
   Standard meta fields: X
   Fields: Title, Description, Keywords, etc.

5. Extracting Open Graph metadata...
   Open Graph fields found or (none)

6. Extracting Twitter Card metadata...
   Twitter Card tags found or (none)

7. Extracting JSON-LD structured data...
   JSON-LD content or (none)

8. Extracting link tags...
   Link tag fields: Canonical, Favicon, FeedUrl, etc.

9. Using name/value pair format...
   Extracted N name/value pairs

10. Extracting custom meta tags...
    Custom values displayed

11. Extracting ALL meta tags (exploration mode)...
    Total metadata fields discovered

=== Sample Complete ===
```

## Target Website

This sample uses [quotes.toscrape.com](https://quotes.toscrape.com/) which provides a clean HTML structure with standard meta tags suitable for metadata extraction demos.

## Configuration Options

| Property | Default | Description |
|----------|---------|-------------|
| `ExtractStandardMeta` | `true` | Extract description, keywords, author, etc. |
| `ExtractOpenGraph` | `true` | Extract og:title, og:image, og:description |
| `ExtractTwitterCard` | `true` | Extract twitter:card, twitter:title, etc. |
| `ExtractJsonLd` | `true` | Extract JSON-LD structured data scripts |
| `ExtractLinkTags` | `true` | Extract canonical, favicon, feed URLs |
| `CustomMetaTags` | `{}` | Custom column name to meta name mapping |
| `ExtractAllMetaTags` | `false` | Extract all meta tags as columns |
| `SingleRowResult` | `true` | Single row vs name/value pairs |

## Output Columns (SingleRowResult = true)

**Standard Meta:**
- `Url` - Page URL
- `Title` - Page title
- `Description` - Meta description
- `Keywords` - Meta keywords
- `Author` - Meta author
- `Robots` - Robots directive
- `Viewport` - Viewport settings
- `Generator` - CMS/generator
- `Theme-Color` - Theme color
- `Charset` - Character encoding
- `Language` - Page language

**Open Graph (OG_ prefix):**
- `OG_title`, `OG_type`, `OG_url`
- `OG_image`, `OG_image_width`, `OG_image_height`
- `OG_description`, `OG_site_name`, `OG_locale`

**Twitter Card (Twitter_ prefix):**
- `Twitter_card`, `Twitter_site`, `Twitter_creator`
- `Twitter_title`, `Twitter_description`, `Twitter_image`

**Link Tags:**
- `Canonical` - Canonical URL
- `Favicon` - Favicon URL
- `AppleTouchIcon` - Apple touch icon URL
- `FeedUrl`, `FeedType` - RSS/Atom feed
- `AlternateLanguages` - Alternate language versions
- `PreconnectDomains` - Preconnect hints

**JSON-LD:**
- `JsonLd` - Raw JSON-LD content as string

## Use Cases

- **SEO Auditing**: Analyze meta tags for SEO compliance
- **Social Sharing Analysis**: Verify Open Graph and Twitter Cards
- **Structured Data Validation**: Extract and validate JSON-LD
- **Content Indexing**: Extract titles and descriptions
- **Competitive Analysis**: Compare metadata across sites

## Related Samples

- **WebProductScraping** - Extract product data with CSS selectors
- **WebLinkExtraction** - Extract and analyze links
- **WebImageExtraction** - Extract images for SEO analysis
