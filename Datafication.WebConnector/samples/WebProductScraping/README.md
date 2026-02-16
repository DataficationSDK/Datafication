# WebProductScraping Sample

Demonstrates CSS selector-based scraping for extracting product listings from e-commerce websites.

## Overview

This sample shows how to:
- Use CssSelectorConnector to extract product data
- Extract text content with SubSelectors
- Extract attribute values with AttributeSubSelectors
- Limit results with MaxElements
- Include raw HTML content for analysis
- Filter and process scraped data

## Key Features Demonstrated

### Basic Product Extraction with SubSelectors

```csharp
var configuration = new CssSelectorConnectorConfiguration
{
    Source = new Uri("https://books.toscrape.com/"),
    Selector = "article.product_pod",  // Each matched element becomes a row

    SubSelectors = new Dictionary<string, string>
    {
        { "Title", "h3 a" },           // Book title
        { "Price", ".price_color" },   // Price
        { "Availability", ".availability" }
    },

    TrimValues = true
};
```

### Extracting Attributes with AttributeSubSelectors

```csharp
var configuration = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = "article.product_pod",

    // Format: "selector|attribute"
    AttributeSubSelectors = new Dictionary<string, string>
    {
        { "ProductUrl", "h3 a|href" },          // href from link
        { "ImageUrl", "img|src" },               // src from image
        { "DataId", ".|data-product-id" }        // "." = current element
    }
};
```

### Limiting Results

```csharp
var configuration = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = ".product_pod",
    MaxElements = 10  // Only extract first 10 products
};
```

### Including HTML Content

```csharp
var configuration = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = ".product_pod",
    IncludeInnerText = true,   // All text content
    IncludeInnerHtml = true,   // HTML inside element
    IncludeOuterHtml = true    // Full element HTML
};
```

## How to Run

```bash
cd WebProductScraping
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.WebConnector Product Scraping Sample ===

Target URL: https://books.toscrape.com/

1. Extracting products with SubSelectors...
   Extracted 20 products

2. Schema Information:
   - ElementIndex: Int32
   - Title: String
   - Price: String
   - Availability: String

3. First 10 products:
   -------------------------------------------------------------------------------
   Title                                    Price           Availability
   -------------------------------------------------------------------------------
   A Light in the Attic                     £51.77          In stock
   Tipping the Velvet                       £53.74          In stock
   Soumission                               £50.10          In stock
   ...

4. Extracting with AttributeSubSelectors (links and images)...
   Extracted 20 products with URLs and images

5. Products with links and ratings:
   [Table showing products with Rating and ImageUrl columns]

6. Limiting results with MaxElements...
   Limited to 5 products (MaxElements = 5)

7. Including HTML content for analysis...
   Schema with HTML columns:
   - ElementIndex
   - TagName
   - InnerText
   - InnerHtml
   - Title

8. Filtering products (price contains '51')...
   Found N products with '51' in price

=== Sample Complete ===
```

## Target Website

This sample uses [books.toscrape.com](https://books.toscrape.com/), a sandbox e-commerce website specifically designed for web scraping practice. It provides:
- Consistent HTML structure
- Product listings with titles, prices, ratings
- Images and links
- No rate limiting or blocking

## Configuration Options

| Property | Default | Description |
|----------|---------|-------------|
| `Selector` | `"*"` | Primary CSS selector for matching elements |
| `SubSelectors` | `{}` | Dictionary of column name to CSS selector for text content |
| `AttributeSubSelectors` | `{}` | Dictionary of column name to "selector\|attribute" for attribute values |
| `Attributes` | `[]` | List of attribute names to extract from matched elements |
| `IncludeInnerText` | `true` | Include text content column |
| `IncludeInnerHtml` | `false` | Include inner HTML column |
| `IncludeOuterHtml` | `false` | Include outer HTML column |
| `IncludeTagName` | `true` | Include tag name column |
| `IncludeElementIndex` | `true` | Include element index column |
| `MaxElements` | `null` | Limit number of extracted elements |
| `TrimValues` | `true` | Trim whitespace from values |

## Related Samples

- **WebQuoteScraping** - More advanced CSS selector patterns
- **WebTableExtraction** - HTML table extraction
- **WebLinkExtraction** - Extracting and filtering links
- **WebImageExtraction** - Extracting images with metadata
