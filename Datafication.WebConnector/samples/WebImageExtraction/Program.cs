using Datafication.Core.Data;
using Datafication.Connectors.WebConnector.Connectors;

Console.WriteLine("=== Datafication.WebConnector Image Extraction Sample ===\n");

// Books to Scrape - has book cover images for demonstration
var url = new Uri("https://books.toscrape.com/");
Console.WriteLine($"Target URL: {url}\n");

// 1. Extract all images from page
Console.WriteLine("1. Extracting all images from page...");
var allImagesConfig = new ImageExtractorConnectorConfiguration
{
    Source = url,
    ImageSelector = "img",
    RemoveDuplicates = true,
    ResolveUrls = true,
    IncludeDataSrc = true,
    IncludeSrcset = true,
    IncludeParentInfo = true
};

var allImagesConnector = new ImageExtractorConnector(allImagesConfig);
var allImages = await allImagesConnector.GetDataAsync();
Console.WriteLine($"   Found {allImages.RowCount} unique images\n");

// 2. Display schema
Console.WriteLine("2. Schema Information:");
foreach (var colName in allImages.Schema.GetColumnNames())
{
    var column = allImages.GetColumn(colName);
    Console.WriteLine($"   - {colName}: {column.DataType.GetClrType().Name}");
}
Console.WriteLine();

// 3. Display sample images
Console.WriteLine("3. First 10 images:");
Console.WriteLine("   " + new string('-', 95));
Console.WriteLine($"   {"Alt",-25} {"Src",-45} {"Extension",-15}");
Console.WriteLine("   " + new string('-', 95));

var cursor = allImages.GetRowCursor("Alt", "Src", "FileExtension");
int rowCount = 0;
while (cursor.MoveNext() && rowCount < 10)
{
    var alt = cursor.GetValue("Alt")?.ToString() ?? "(no alt)";
    var src = cursor.GetValue("Src")?.ToString() ?? "";
    var ext = cursor.GetValue("FileExtension")?.ToString() ?? "";

    if (alt.Length > 22) alt = alt.Substring(0, 22) + "...";
    if (src.Length > 42) src = src.Substring(0, 42) + "...";

    Console.WriteLine($"   {alt,-25} {src,-45} {ext,-15}");
    rowCount++;
}
Console.WriteLine("   " + new string('-', 95));
Console.WriteLine($"   ... and {allImages.RowCount - 10} more images\n");

// 4. Filter by extension - only JPG images
Console.WriteLine("4. Filtering by extension (JPG only)...");
var jpgConfig = new ImageExtractorConnectorConfiguration
{
    Source = url,
    AllowedExtensions = new List<string> { ".jpg", ".jpeg" },
    RemoveDuplicates = true,
    ResolveUrls = true
};

var jpgConnector = new ImageExtractorConnector(jpgConfig);
var jpgImages = await jpgConnector.GetDataAsync();
Console.WriteLine($"   Found {jpgImages.RowCount} JPG images\n");

// 5. Exclude specific extensions
Console.WriteLine("5. Excluding extensions (no GIF, ICO, SVG)...");
var excludeConfig = new ImageExtractorConnectorConfiguration
{
    Source = url,
    ExcludedExtensions = new List<string> { ".gif", ".ico", ".svg" },
    RemoveDuplicates = true,
    ResolveUrls = true
};

var excludeConnector = new ImageExtractorConnector(excludeConfig);
var filteredImages = await excludeConnector.GetDataAsync();
Console.WriteLine($"   Found {filteredImages.RowCount} images after exclusions\n");

// 6. Filter by minimum size (requires explicit width/height attributes)
Console.WriteLine("6. Filtering by size (MinWidth = 100, MinHeight = 100)...");
var sizeConfig = new ImageExtractorConnectorConfiguration
{
    Source = url,
    MinWidth = 100,
    MinHeight = 100,
    RemoveDuplicates = true,
    ResolveUrls = true
};

var sizeConnector = new ImageExtractorConnector(sizeConfig);
var sizedImages = await sizeConnector.GetDataAsync();
Console.WriteLine($"   Found {sizedImages.RowCount} images with size >= 100x100\n");

// 7. Extract images from specific section
Console.WriteLine("7. Extracting images from specific selector (product images only)...");
var productImgConfig = new ImageExtractorConnectorConfiguration
{
    Source = url,
    ImageSelector = ".product_pod img",  // Only product images
    RemoveDuplicates = true,
    ResolveUrls = true,
    IncludeParentInfo = true
};

var productImgConnector = new ImageExtractorConnector(productImgConfig);
var productImages = await productImgConnector.GetDataAsync();
Console.WriteLine($"   Found {productImages.RowCount} product images\n");

// 8. Include parent element information
Console.WriteLine("8. Analyzing parent elements...");
Console.WriteLine("   Parent element distribution:");
var parentGroups = productImages.GroupBy("ParentTag").Info();
var parentCursor = parentGroups.GetRowCursor("Group", "Count");
while (parentCursor.MoveNext())
{
    var tag = parentCursor.GetValue("Group");
    var count = parentCursor.GetValue("Count");
    Console.WriteLine($"   - {tag}: {count} images");
}
Console.WriteLine();

// 9. Analyze images by file extension
Console.WriteLine("9. Analyzing images by file extension...");
var extGroups = allImages.GroupBy("FileExtension").Info();
Console.WriteLine("   Extension distribution:");
var extCursor = extGroups.GetRowCursor("Group", "Count");
while (extCursor.MoveNext())
{
    var ext = extCursor.GetValue("Group")?.ToString();
    var count = extCursor.GetValue("Count");
    if (string.IsNullOrEmpty(ext)) ext = "(no extension)";
    Console.WriteLine($"   - {ext}: {count} images");
}
Console.WriteLine();

// 10. Find images missing alt text (accessibility audit)
Console.WriteLine("10. Accessibility audit: Images missing alt text...");
var missingAlt = allImages.Filter(row =>
    string.IsNullOrWhiteSpace(row["Alt"]?.ToString()));
Console.WriteLine($"    Found {missingAlt.RowCount} images with missing/empty alt text\n");

if (missingAlt.RowCount > 0)
{
    Console.WriteLine("    Sample images missing alt:");
    var altCursor = missingAlt.GetRowCursor("Src");
    int altCount = 0;
    while (altCursor.MoveNext() && altCount < 3)
    {
        var src = altCursor.GetValue("Src")?.ToString() ?? "";
        if (src.Length > 60) src = src.Substring(0, 60) + "...";
        Console.WriteLine($"    - {src}");
        altCount++;
    }
    Console.WriteLine();
}

// 11. Check for lazy-loaded images
Console.WriteLine("11. Checking for lazy-loaded images (data-src attributes)...");
var lazyConfig = new ImageExtractorConnectorConfiguration
{
    Source = url,
    IncludeDataSrc = true,
    RemoveDuplicates = true
};

var lazyConnector = new ImageExtractorConnector(lazyConfig);
var lazyImages = await lazyConnector.GetDataAsync();

var withDataSrc = lazyImages.Filter(row =>
    !string.IsNullOrEmpty(row["DataSrc"]?.ToString()));
Console.WriteLine($"    Found {withDataSrc.RowCount} images with lazy-load attributes\n");

// 12. Create image inventory summary
Console.WriteLine("12. Image Inventory Summary:");
Console.WriteLine($"    - Total unique images: {allImages.RowCount}");
Console.WriteLine($"    - Product images: {productImages.RowCount}");
Console.WriteLine($"    - Images with alt text: {allImages.RowCount - missingAlt.RowCount}");
Console.WriteLine($"    - Images missing alt: {missingAlt.RowCount}");
Console.WriteLine($"    - Lazy-loaded images: {withDataSrc.RowCount}");

Console.WriteLine("\n=== Sample Complete ===");
