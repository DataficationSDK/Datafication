# WebImageExtraction Sample

Demonstrates extracting images and their metadata from web pages using the ImageExtractorConnector.

## Overview

This sample shows how to:
- Extract all images from a web page
- Filter by file extension (allowed/excluded)
- Filter by minimum size (width/height)
- Target specific page sections with CSS selectors
- Detect lazy-loaded images (data-src attributes)
- Include srcset for responsive images
- Analyze parent element context
- Perform accessibility audits (missing alt text)
- Create image inventories

## Key Features Demonstrated

### Extract All Images

```csharp
var configuration = new ImageExtractorConnectorConfiguration
{
    Source = new Uri("https://books.toscrape.com/"),
    ImageSelector = "img",
    RemoveDuplicates = true,
    ResolveUrls = true,
    IncludeDataSrc = true,
    IncludeSrcset = true,
    IncludeParentInfo = true
};

var connector = new ImageExtractorConnector(configuration);
var images = await connector.GetDataAsync();
```

### Filter by Extension

```csharp
// Only JPG/JPEG images
var configuration = new ImageExtractorConnectorConfiguration
{
    Source = url,
    AllowedExtensions = new List<string> { ".jpg", ".jpeg" }
};

// Exclude icons and vectors
var configExclude = new ImageExtractorConnectorConfiguration
{
    Source = url,
    ExcludedExtensions = new List<string> { ".gif", ".ico", ".svg" }
};
```

### Filter by Size

```csharp
var configuration = new ImageExtractorConnectorConfiguration
{
    Source = url,
    MinWidth = 100,   // Minimum 100px wide
    MinHeight = 100   // Minimum 100px tall
};
```

### Target Specific Sections

```csharp
var configuration = new ImageExtractorConnectorConfiguration
{
    Source = url,
    ImageSelector = ".product-gallery img"  // Only product gallery images
};
```

### Detect Lazy-Loaded Images

```csharp
var configuration = new ImageExtractorConnectorConfiguration
{
    Source = url,
    IncludeDataSrc = true  // Extract data-src, data-lazy, etc.
};

// Then filter for lazy-loaded images
var lazyLoaded = images.Where("DataSrc", x => !string.IsNullOrEmpty(x?.ToString()));
```

## How to Run

```bash
cd WebImageExtraction
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.WebConnector Image Extraction Sample ===

Target URL: https://books.toscrape.com/

1. Extracting all images from page...
   Found ~25 unique images

2. Schema Information:
   - ElementIndex: Int32
   - Src: String
   - Alt: String
   - Title: String
   - Width: Int32
   - Height: Int32
   - Loading: String
   - IsBackground: Boolean
   - Srcset: String
   - Sizes: String
   - DataSrc: String
   - ParentTag: String
   - ParentClass: String
   - FileExtension: String

3. First 10 images:
   [Table showing images with Alt, Src, Extension]

4. Filtering by extension (JPG only)...
   Found X JPG images

5. Excluding extensions (no GIF, ICO, SVG)...
   Found Y images after exclusions

6. Filtering by size (MinWidth = 100, MinHeight = 100)...
   Found Z images with size >= 100x100

7. Extracting images from specific selector (product images only)...
   Found N product images

8. Analyzing parent elements...
   Parent element distribution by tag

9. Analyzing images by file extension...
   Extension distribution

10. Accessibility audit: Images missing alt text...
    Found N images with missing/empty alt text

11. Checking for lazy-loaded images...
    Found N images with lazy-load attributes

12. Image Inventory Summary:
    - Total unique images: X
    - Product images: Y
    - Images with alt text: Z
    - Images missing alt: W
    - Lazy-loaded images: V

=== Sample Complete ===
```

## Target Website

This sample uses [books.toscrape.com](https://books.toscrape.com/) which provides:
- Book cover images for product listings
- Consistent image structure
- Various image sizes and formats
- Alt text for accessibility testing

## Configuration Options

| Property | Default | Description |
|----------|---------|-------------|
| `ImageSelector` | `"img"` | CSS selector for image elements |
| `IncludeBackgroundImages` | `false` | Extract CSS background-image |
| `BackgroundImageSelector` | `"*"` | Selector for background image search |
| `MinWidth` | `null` | Minimum width filter (pixels) |
| `MinHeight` | `null` | Minimum height filter (pixels) |
| `AllowedExtensions` | `[]` | Only include these extensions |
| `ExcludedExtensions` | `[]` | Exclude these extensions |
| `IncludeDataSrc` | `true` | Extract lazy-load attributes |
| `IncludeSrcset` | `true` | Extract responsive srcset |
| `IncludeParentInfo` | `true` | Include parent element tag/class |
| `RemoveDuplicates` | `true` | Remove duplicate URLs |
| `ResolveUrls` | `true` | Convert relative to absolute URLs |

## Output Columns

- `ElementIndex` - Position on page
- `Src` - Image URL (resolved if ResolveUrls=true)
- `Alt` - Alt text for accessibility
- `Title` - Title attribute
- `Width` / `Height` - Explicit dimensions if specified
- `Loading` - Loading attribute (lazy, eager)
- `IsBackground` - True if CSS background image
- `Srcset` - Responsive image sources
- `Sizes` - Responsive sizes attribute
- `DataSrc` - Lazy-load source URL
- `ParentTag` - Parent element tag name
- `ParentClass` - Parent element class
- `FileExtension` - Extracted file extension

## Use Cases

- **Image Asset Inventory**: Catalog all images on a site
- **Accessibility Audit**: Find images missing alt text
- **Performance Analysis**: Identify large images or missing lazy-loading
- **Content Migration**: Extract all images for migration
- **SEO Audit**: Verify image optimization

## Related Samples

- **WebLinkExtraction** - Extract links instead of images
- **WebProductScraping** - Extract product data including images
- **WebMetadataExtraction** - Extract page metadata for SEO
