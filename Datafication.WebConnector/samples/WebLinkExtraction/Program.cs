using Datafication.Core.Data;
using Datafication.Connectors.WebConnector.Connectors;

Console.WriteLine("=== Datafication.WebConnector Link Extraction Sample ===\n");

// Books to Scrape - has various link types for demonstration
var url = new Uri("https://books.toscrape.com/");
Console.WriteLine($"Target URL: {url}\n");

// 1. Extract all links from page
Console.WriteLine("1. Extracting all links from page...");
var allLinksConfig = new LinkExtractorConnectorConfiguration
{
    Source = url,
    LinkSelector = "a[href]",
    RemoveDuplicates = true,
    IncludeLinkText = true,
    IncludeTitle = true,
    IncludeRel = true,
    IncludeTarget = true
};

var allLinksConnector = new LinkExtractorConnector(allLinksConfig);
var allLinks = await allLinksConnector.GetDataAsync();
Console.WriteLine($"   Found {allLinks.RowCount} unique links\n");

// 2. Display schema
Console.WriteLine("2. Schema Information:");
foreach (var colName in allLinks.Schema.GetColumnNames())
{
    var column = allLinks.GetColumn(colName);
    Console.WriteLine($"   - {colName}: {column.DataType.GetClrType().Name}");
}
Console.WriteLine();

// 3. Display sample links
Console.WriteLine("3. First 10 links:");
Console.WriteLine("   " + new string('-', 90));
Console.WriteLine($"   {"Text",-30} {"Href",-55}");
Console.WriteLine("   " + new string('-', 90));

var cursor = allLinks.GetRowCursor("Text", "Href");
int rowCount = 0;
while (cursor.MoveNext() && rowCount < 10)
{
    var text = cursor.GetValue("Text")?.ToString() ?? "";
    var href = cursor.GetValue("Href")?.ToString() ?? "";

    if (text.Length > 27) text = text.Substring(0, 27) + "...";
    if (href.Length > 52) href = href.Substring(0, 52) + "...";

    Console.WriteLine($"   {text,-30} {href,-55}");
    rowCount++;
}
Console.WriteLine("   " + new string('-', 90));
Console.WriteLine($"   ... and {allLinks.RowCount - 10} more links\n");

// 4. Extract internal links only
Console.WriteLine("4. Extracting internal links only...");
var internalConfig = new LinkExtractorConnectorConfiguration
{
    Source = url,
    InternalLinksOnly = true,  // Only same-domain links
    RemoveDuplicates = true,
    IncludeLinkText = true
};

var internalConnector = new LinkExtractorConnector(internalConfig);
var internalLinks = await internalConnector.GetDataAsync();
Console.WriteLine($"   Found {internalLinks.RowCount} internal links\n");

// 5. Extract external links only
Console.WriteLine("5. Extracting external links only...");
var externalConfig = new LinkExtractorConnectorConfiguration
{
    Source = url,
    ExternalLinksOnly = true,  // Only different-domain links
    RemoveDuplicates = true,
    IncludeLinkText = true
};

var externalConnector = new LinkExtractorConnector(externalConfig);
var externalLinks = await externalConnector.GetDataAsync();
Console.WriteLine($"   Found {externalLinks.RowCount} external links\n");

// 6. Filter by URL pattern (category links)
Console.WriteLine("6. Filtering by URL pattern (category links)...");
var categoryConfig = new LinkExtractorConnectorConfiguration
{
    Source = url,
    UrlPattern = @"/category/",  // Regex pattern to match
    RemoveDuplicates = true,
    IncludeLinkText = true
};

var categoryConnector = new LinkExtractorConnector(categoryConfig);
var categoryLinks = await categoryConnector.GetDataAsync();
Console.WriteLine($"   Found {categoryLinks.RowCount} category links:\n");

var catCursor = categoryLinks.GetRowCursor("Text", "Href");
rowCount = 0;
while (catCursor.MoveNext() && rowCount < 8)
{
    var text = catCursor.GetValue("Text")?.ToString() ?? "";
    var href = catCursor.GetValue("Href")?.ToString() ?? "";

    if (href.Length > 60) href = href.Substring(0, 60) + "...";

    Console.WriteLine($"   {text,-25} {href}");
    rowCount++;
}
if (categoryLinks.RowCount > 8)
{
    Console.WriteLine($"   ... and {categoryLinks.RowCount - 8} more category links");
}
Console.WriteLine();

// 7. Custom exclusion patterns
Console.WriteLine("7. Using custom exclusion patterns...");
var excludeConfig = new LinkExtractorConnectorConfiguration
{
    Source = url,
    RemoveDuplicates = true,
    ExcludePatterns = new List<string>
    {
        @"^javascript:",    // JavaScript links
        @"^mailto:",        // Email links
        @"^tel:",           // Phone links
        @"^#$",             // Empty anchor
        @"/catalogue/",     // Exclude product pages
        @"/category/"       // Exclude category pages
    },
    IncludeLinkText = true
};

var excludeConnector = new LinkExtractorConnector(excludeConfig);
var excludeLinks = await excludeConnector.GetDataAsync();
Console.WriteLine($"   After exclusions: {excludeLinks.RowCount} links remain\n");

// 8. Extract navigation links only
Console.WriteLine("8. Extracting navigation links using specific selector...");
var navConfig = new LinkExtractorConnectorConfiguration
{
    Source = url,
    LinkSelector = ".side_categories a",  // Only sidebar category links
    RemoveDuplicates = true,
    IncludeLinkText = true,
    IncludeAnchorClass = true  // Include class attribute
};

var navConnector = new LinkExtractorConnector(navConfig);
var navLinks = await navConnector.GetDataAsync();
Console.WriteLine($"   Found {navLinks.RowCount} navigation links\n");

// 9. Analyze link types
Console.WriteLine("9. Analyzing links by URL pattern...");
Console.WriteLine("   Link distribution:");

// Product links (contain /catalogue/ but not /category/)
var productLinks = allLinks.Filter(row =>
    row["Href"]?.ToString()?.Contains("/catalogue/") == true &&
    row["Href"]?.ToString()?.Contains("/category/") == false);
Console.WriteLine($"   - Product pages: {productLinks.RowCount}");

// Category links
Console.WriteLine($"   - Category pages: {categoryLinks.RowCount}");

// Homepage link
var homeLinks = allLinks.Filter(row =>
    row["Href"]?.ToString()?.EndsWith("books.toscrape.com/") == true ||
    row["Href"]?.ToString()?.EndsWith("/index.html") == true);
Console.WriteLine($"   - Homepage links: {homeLinks.RowCount}");

Console.WriteLine();

// 10. Include anchor id and class
Console.WriteLine("10. Extracting anchor metadata (id, class)...");
var metaConfig = new LinkExtractorConnectorConfiguration
{
    Source = url,
    LinkSelector = "a[href]",
    RemoveDuplicates = false,  // Keep all occurrences
    IncludeLinkText = true,
    IncludeAnchorId = true,
    IncludeAnchorClass = true
};

var metaConnector = new LinkExtractorConnector(metaConfig);
var metaLinks = await metaConnector.GetDataAsync();
Console.WriteLine($"   Extracted {metaLinks.RowCount} links with anchor metadata");
Console.WriteLine("   Schema includes:");
foreach (var colName in metaLinks.Schema.GetColumnNames())
{
    Console.WriteLine($"   - {colName}");
}

Console.WriteLine("\n=== Sample Complete ===");
