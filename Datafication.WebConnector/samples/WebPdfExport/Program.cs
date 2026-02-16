using Datafication.Core.Data;
using Datafication.Connectors.WebConnector.Connectors;
using Datafication.Sinks.Connectors.WebConnector;

Console.WriteLine("=== Datafication.WebConnector PDF Export Sample ===\n");

// Output directory
var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
Directory.CreateDirectory(outputDir);
Console.WriteLine($"Output directory: {outputDir}\n");

// 1. Extract quote data for PDF export
Console.WriteLine("1. Extracting quote data from quotes.toscrape.com...");
var quotesConfig = new CssSelectorConnectorConfiguration
{
    Source = new Uri("https://quotes.toscrape.com/"),
    Selector = ".quote",
    SubSelectors = new Dictionary<string, string>
    {
        { "Quote", ".text" },
        { "Author", ".author" },
        { "Tags", ".tags" }
    },
    IncludeInnerText = false,
    IncludeTagName = false,
    TrimValues = true
};

var quotesConnector = new CssSelectorConnector(quotesConfig);
var quotesData = await quotesConnector.GetDataAsync();
Console.WriteLine($"   Extracted {quotesData.RowCount} quotes\n");

// 2. Basic PDF export
Console.WriteLine("2. Creating basic PDF...");
var basicSink = new PdfSink
{
    RowLimit = 100,
    LandscapeOrientation = true
};

await using (var pdfStream = await basicSink.Transform(quotesData))
{
    var basicPath = Path.Combine(outputDir, "quotes_basic.pdf");
    await using var fileStream = File.Create(basicPath);
    await pdfStream.CopyToAsync(fileStream);
    Console.WriteLine($"   Saved to: {basicPath}");
    Console.WriteLine($"   File size: {new FileInfo(basicPath).Length:N0} bytes\n");
}

// 3. PDF with title page
Console.WriteLine("3. Creating PDF with title page...");
var titleSink = new PdfSink
{
    Title = "Famous Quotes Collection",
    Author = "Datafication.WebConnector",
    Description = "A collection of inspirational quotes extracted from quotes.toscrape.com using " +
                  "the Datafication.WebConnector library. This demonstrates the PDF export capability " +
                  "for generating professional reports from scraped web data.",
    RowLimit = 100,
    LandscapeOrientation = true
};

await using (var pdfStream = await titleSink.Transform(quotesData))
{
    var titlePath = Path.Combine(outputDir, "quotes_with_title.pdf");
    await using var fileStream = File.Create(titlePath);
    await pdfStream.CopyToAsync(fileStream);
    Console.WriteLine($"   Saved to: {titlePath}");
    Console.WriteLine($"   File size: {new FileInfo(titlePath).Length:N0} bytes\n");
}

// 4. Portrait orientation PDF
Console.WriteLine("4. Creating portrait orientation PDF...");
var portraitSink = new PdfSink
{
    Title = "Quotes Report (Portrait)",
    LandscapeOrientation = false,  // Portrait mode
    RowLimit = 10
};

await using (var pdfStream = await portraitSink.Transform(quotesData))
{
    var portraitPath = Path.Combine(outputDir, "quotes_portrait.pdf");
    await using var fileStream = File.Create(portraitPath);
    await pdfStream.CopyToAsync(fileStream);
    Console.WriteLine($"   Saved to: {portraitPath}");
    Console.WriteLine($"   File size: {new FileInfo(portraitPath).Length:N0} bytes\n");
}

// 5. Extract product data for wider table
Console.WriteLine("5. Extracting product data from books.toscrape.com...");
var productConfig = new CssSelectorConnectorConfiguration
{
    Source = new Uri("https://books.toscrape.com/"),
    Selector = "article.product_pod",
    SubSelectors = new Dictionary<string, string>
    {
        { "Title", "h3 a" },
        { "Price", ".price_color" },
        { "Availability", ".availability" }
    },
    AttributeSubSelectors = new Dictionary<string, string>
    {
        { "ProductUrl", "h3 a|href" },
        { "Rating", "p.star-rating|class" }
    },
    IncludeInnerText = false,
    IncludeTagName = false,
    TrimValues = true
};

var productConnector = new CssSelectorConnector(productConfig);
var productData = await productConnector.GetDataAsync();
Console.WriteLine($"   Extracted {productData.RowCount} products\n");

// 6. Product catalog PDF
Console.WriteLine("6. Creating product catalog PDF...");
var catalogSink = new PdfSink
{
    Title = "Book Catalog",
    Author = "Books to Scrape",
    Description = "A catalog of books scraped from books.toscrape.com. " +
                  "This sample demonstrates exporting product data with multiple columns to PDF format.",
    RowLimit = 20,
    LandscapeOrientation = true
};

await using (var pdfStream = await catalogSink.Transform(productData))
{
    var catalogPath = Path.Combine(outputDir, "book_catalog.pdf");
    await using var fileStream = File.Create(catalogPath);
    await pdfStream.CopyToAsync(fileStream);
    Console.WriteLine($"   Saved to: {catalogPath}");
    Console.WriteLine($"   File size: {new FileInfo(catalogPath).Length:N0} bytes\n");
}

// 7. Row limit demonstration
Console.WriteLine("7. Demonstrating row limit options...");
var rowLimits = new[] { 5, 10, 20 };
foreach (var limit in rowLimits)
{
    var limitSink = new PdfSink
    {
        Title = $"Report ({limit} rows)",
        RowLimit = limit,
        LandscapeOrientation = true
    };

    await using var pdfStream = await limitSink.Transform(productData);
    var limitPath = Path.Combine(outputDir, $"products_{limit}_rows.pdf");
    await using var fileStream = File.Create(limitPath);
    await pdfStream.CopyToAsync(fileStream);
    Console.WriteLine($"   {limit} rows: {new FileInfo(limitPath).Length:N0} bytes");
}
Console.WriteLine();

// 8. Extract table data from Wikipedia for comparison
Console.WriteLine("8. Extracting table data from Wikipedia...");
var wikiConfig = new HtmlTableConnectorConfiguration
{
    Source = new Uri("https://en.wikipedia.org/wiki/List_of_countries_by_population_(United_Nations)"),
    TableSelector = "table.wikitable",
    FirstRowIsHeader = true,
    IncludeTableMetadata = false,
    TrimCellValues = true
};

var wikiConnector = new HtmlTableConnector(wikiConfig);
var wikiData = await wikiConnector.GetDataAsync();
Console.WriteLine($"   Extracted {wikiData.RowCount} rows\n");

// 9. Wikipedia data PDF
Console.WriteLine("9. Creating Wikipedia table PDF...");
var wikiSink = new PdfSink
{
    Title = "Countries by Population",
    Author = "Wikipedia",
    Description = "Population data extracted from Wikipedia's List of countries by population " +
                  "using the HtmlTableConnector.",
    RowLimit = 50,
    LandscapeOrientation = true
};

await using (var pdfStream = await wikiSink.Transform(wikiData))
{
    var wikiPath = Path.Combine(outputDir, "countries_population.pdf");
    await using var fileStream = File.Create(wikiPath);
    await pdfStream.CopyToAsync(fileStream);
    Console.WriteLine($"   Saved to: {wikiPath}");
    Console.WriteLine($"   File size: {new FileInfo(wikiPath).Length:N0} bytes\n");
}

// 10. Summary of generated files
Console.WriteLine("10. Summary of generated PDF files:");
Console.WriteLine($"    Output directory: {outputDir}\n");

var files = Directory.GetFiles(outputDir, "*.pdf");
long totalSize = 0;
foreach (var file in files.OrderBy(f => f))
{
    var info = new FileInfo(file);
    totalSize += info.Length;
    Console.WriteLine($"    - {info.Name}: {info.Length:N0} bytes");
}
Console.WriteLine();
Console.WriteLine($"    Total: {files.Length} files, {totalSize:N0} bytes");

Console.WriteLine("\n=== Sample Complete ===");
Console.WriteLine($"\nGenerated {files.Length} PDF files.");
Console.WriteLine($"View them at: {outputDir}");
