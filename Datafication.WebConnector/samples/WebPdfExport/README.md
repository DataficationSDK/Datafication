# WebPdfExport Sample

Demonstrates exporting DataBlocks as PDF documents using the PdfSink.

## Overview

This sample shows how to:
- Create basic PDF exports from scraped data
- Add title pages with author and description
- Configure landscape vs portrait orientation
- Control row limits for large datasets
- Export different data types (quotes, products, tables)
- Work with PDF streams

## Key Features Demonstrated

### Basic PDF Export

```csharp
var sink = new PdfSink
{
    RowLimit = 100,
    LandscapeOrientation = true
};

await using var pdfStream = await sink.Transform(dataBlock);
await using var fileStream = File.Create("output.pdf");
await pdfStream.CopyToAsync(fileStream);
```

### PDF with Title Page

```csharp
var sink = new PdfSink
{
    Title = "My Report",
    Author = "John Doe",
    Description = "A detailed analysis of the scraped data...",
    RowLimit = 100,
    LandscapeOrientation = true
};
```

### Portrait Orientation

```csharp
var sink = new PdfSink
{
    Title = "Vertical Report",
    LandscapeOrientation = false  // Portrait mode
};
```

### Controlling Row Limits

```csharp
// Small preview
var previewSink = new PdfSink { RowLimit = 10 };

// Full report
var fullSink = new PdfSink { RowLimit = 1000 };
```

## How to Run

```bash
cd WebPdfExport
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.WebConnector PDF Export Sample ===

Output directory: [path]/output

1. Extracting quote data from quotes.toscrape.com...
   Extracted 10 quotes

2. Creating basic PDF...
   Saved to: output/quotes_basic.pdf
   File size: ~50,000 bytes

3. Creating PDF with title page...
   Saved to: output/quotes_with_title.pdf
   File size: ~55,000 bytes

4. Creating portrait orientation PDF...
   Saved to: output/quotes_portrait.pdf

5. Extracting product data from books.toscrape.com...
   Extracted 20 products

6. Creating product catalog PDF...
   Saved to: output/book_catalog.pdf

7. Demonstrating row limit options...
   5 rows: X bytes
   10 rows: Y bytes
   20 rows: Z bytes

8. Extracting table data from Wikipedia...
   Extracted ~240 rows

9. Creating Wikipedia table PDF...
   Saved to: output/countries_population.pdf

10. Summary of generated PDF files:
    - book_catalog.pdf: X bytes
    - countries_population.pdf: X bytes
    - quotes_basic.pdf: X bytes
    - quotes_portrait.pdf: X bytes
    - quotes_with_title.pdf: X bytes
    ...

=== Sample Complete ===
```

## Requirements

**Important**: This sample requires Puppeteer/Chromium for PDF generation. On first run, it will automatically download the browser binaries (~200MB).

## PdfSink Properties

| Property | Default | Description |
|----------|---------|-------------|
| `RowLimit` | `1000` | Maximum rows to include in PDF |
| `LandscapeOrientation` | `true` | Use landscape (true) or portrait (false) |
| `Title` | `null` | Title for the title page |
| `Author` | `null` | Author shown on title page |
| `Description` | `null` | Description on title page |

## Title Page Behavior

- If `Title` is null, no title page is created
- If `Title` is set:
  - Title is displayed prominently
  - `Author` is shown below title if set
  - `Description` is shown below author if set

## Orientation Guidelines

**Use Landscape for:**
- Tables with many columns
- Wide datasets
- Product catalogs
- Data-heavy reports

**Use Portrait for:**
- Fewer columns
- Text-heavy content
- Standard documents
- Print-friendly reports

## Working with Streams

```csharp
// Stream to file
await using var pdfStream = await sink.Transform(data);
await using var fileStream = File.Create("output.pdf");
await pdfStream.CopyToAsync(fileStream);

// Stream to memory
await using var pdfStream = await sink.Transform(data);
using var memoryStream = new MemoryStream();
await pdfStream.CopyToAsync(memoryStream);
byte[] pdfBytes = memoryStream.ToArray();

// Stream for web response (ASP.NET)
await using var pdfStream = await sink.Transform(data);
return File(pdfStream, "application/pdf", "report.pdf");
```

## PDF Format Details

- **Paper Size**: A4
- **Margins**: Default browser margins
- **Background**: Prints with background colors
- **Table Styling**: Auto-scaled to fit width

## Use Cases

- **Report Generation**: Create professional PDF reports
- **Data Export**: Allow users to download data as PDF
- **Archiving**: Create permanent records of scraped data
- **Email Attachments**: Generate PDFs for distribution
- **Print-Ready Output**: Create documents for printing

## Performance Notes

- First run downloads browser (~200MB)
- Larger datasets create larger PDFs
- Title page adds minimal overhead
- Landscape uses more horizontal space

## Related Samples

- **WebScreenshotExport** - Export as images instead
- **WebTableExtraction** - Source table data
- **WebProductScraping** - Source product data
- **WebQuoteScraping** - Source text data
