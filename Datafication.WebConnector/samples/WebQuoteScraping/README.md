# WebQuoteScraping Sample

Demonstrates advanced CSS selector patterns for extracting structured text content like quotes.

## Overview

This sample shows advanced CSS selector techniques:
- Complex nested SubSelectors
- AttributeSubSelectors for link extraction
- Pseudo-selectors like `:first-child`
- Including raw HTML for analysis
- GroupBy operations on scraped data
- Filtering and sorting extracted content
- Extracting direct element attributes

## Key Features Demonstrated

### Nested SubSelectors for Text Content

```csharp
var configuration = new CssSelectorConnectorConfiguration
{
    Source = new Uri("https://quotes.toscrape.com/"),
    Selector = ".quote",  // Parent container

    SubSelectors = new Dictionary<string, string>
    {
        { "QuoteText", ".text" },
        { "Author", ".author" },
        { "Tags", ".tags" }
    }
};
```

### AttributeSubSelectors for Links

```csharp
var configuration = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = ".quote",
    AttributeSubSelectors = new Dictionary<string, string>
    {
        { "AuthorUrl", "a[href*='author']|href" }  // href from author link
    }
};
```

### Pseudo-selectors

```csharp
SubSelectors = new Dictionary<string, string>
{
    { "FirstTag", ".tag:first-child" },    // First tag only
    { "LastTag", ".tag:last-child" }       // Last tag only
}
```

### GroupBy Analysis

```csharp
var connector = new CssSelectorConnector(configuration);
var quotes = await connector.GetDataAsync();

// Group by author to count quotes per author
var authorGroups = quotes.GroupBy("Author");
// Returns: Author, Count columns
```

### Filtering Content

```csharp
var worldQuotes = quotes.Where("QuoteText", x =>
    x?.ToString()?.ToLowerInvariant().Contains("world") == true);
```

### Including Raw HTML

```csharp
var configuration = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = ".quote",
    IncludeInnerHtml = true,  // Raw HTML inside element
    IncludeOuterHtml = true   // Full element HTML
};
```

## How to Run

```bash
cd WebQuoteScraping
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.WebConnector Quote Scraping Sample ===

Target URL: https://quotes.toscrape.com/

1. Extracting quotes with nested SubSelectors...
   Extracted 10 quotes

2. Schema Information:
   - ElementIndex: Int32
   - QuoteText: String
   - Author: String
   - Tags: String
   - AuthorUrl: String

3. Sample quotes:
   "The world as we have created it is a process of our thinking..."
   - Albert Einstein
   Tags: change, deep-thoughts, thinking, world

   "It is our choices, Harry, that show what we truly are..."
   - J.K. Rowling
   Tags: abilities, choices
   ...

4. Extracting first tag from each quote...
   - Albert Einstein: #change
   - J.K. Rowling: #abilities
   ...

5. Grouping quotes by author...
   Found quotes from X different authors:
   - Albert Einstein: 2 quote(s)
   - J.K. Rowling: 1 quote(s)
   ...

6. Extracting author profile links...
   Author profile links:
   - Albert Einstein: /author/Albert-Einstein
   ...

7. Filtering quotes containing 'world'...
   Found N quotes about 'world'

8. Including InnerHtml for HTML structure analysis...
   [HTML structure preview]

9. Sorting quotes by author...
   First 5 quotes sorted alphabetically

10. Extracting attributes from main selector element...
    Schema with direct attributes

=== Sample Complete ===
```

## Target Website

This sample uses [quotes.toscrape.com](https://quotes.toscrape.com/) which provides:
- Structured quote containers (`.quote`)
- Nested content (text, author, tags)
- Links to author profiles
- Multiple tags per quote
- Clean, consistent HTML structure

## CSS Selector Patterns

| Pattern | Description |
|---------|-------------|
| `.quote` | Class selector |
| `.quote .text` | Nested element |
| `.tag:first-child` | First child pseudo-selector |
| `a[href*='author']` | Attribute contains selector |
| `.|data-id` | Current element attribute |
| `span a[href]` | Descendant with attribute |

## Configuration Options

| Property | Default | Description |
|----------|---------|-------------|
| `Selector` | `"*"` | Primary CSS selector |
| `SubSelectors` | `{}` | Nested text extraction |
| `AttributeSubSelectors` | `{}` | Nested attribute extraction |
| `Attributes` | `[]` | Direct element attributes |
| `IncludeInnerText` | `true` | All text content |
| `IncludeInnerHtml` | `false` | HTML inside element |
| `IncludeOuterHtml` | `false` | Full element HTML |
| `IncludeTagName` | `true` | Element tag name |
| `IncludeElementIndex` | `true` | Position on page |
| `MaxElements` | `null` | Limit results |
| `TrimValues` | `true` | Trim whitespace |

## Advanced Patterns

### Multiple Attribute Extraction

```csharp
AttributeSubSelectors = new Dictionary<string, string>
{
    { "Link", "a|href" },
    { "LinkTitle", "a|title" },
    { "ImageSrc", "img|src" },
    { "ImageAlt", "img|alt" },
    { "DataId", ".|data-id" }  // Current element
}
```

### Combining with DataBlock Operations

```csharp
var quotes = await connector.GetDataAsync();

// Filter
var filtered = quotes.Where("Author", x => x?.ToString() == "Albert Einstein");

// Sort
var sorted = quotes.Sort(SortDirection.Ascending, "Author");

// Group and count
var groups = quotes.GroupBy("Author");

// Take first N
var limited = quotes.Head(5);
```

## Related Samples

- **WebProductScraping** - Basic CSS selector patterns
- **WebTableExtraction** - HTML table extraction
- **WebLinkExtraction** - Specialized link extraction
- **WebFactoryPattern** - Quick connector creation
