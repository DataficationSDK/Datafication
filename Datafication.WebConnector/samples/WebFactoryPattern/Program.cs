using Datafication.Core.Data;
using Datafication.Core.Connectors;
using Datafication.Core.Factories;
using Datafication.Connectors.WebConnector.Connectors;
using Datafication.Connectors.WebConnector.Factories;

Console.WriteLine("=== Datafication.WebConnector Factory Pattern Sample ===\n");

var booksUrl = new Uri("https://books.toscrape.com/");
var quotesUrl = new Uri("https://quotes.toscrape.com/");
var wikiUrl = new Uri("https://en.wikipedia.org/wiki/List_of_countries_by_population_(United_Nations)");

Console.WriteLine("Target URLs:");
Console.WriteLine($"  - Books: {booksUrl}");
Console.WriteLine($"  - Quotes: {quotesUrl}");
Console.WriteLine($"  - Wikipedia: {wikiUrl}\n");

// 1. Quick HTML table extraction via factory
Console.WriteLine("1. Quick table extraction via WebConnectorFactory...");
var tableConnector = WebConnectorFactory.CreateHtmlTableConnector(wikiUrl);
var tableData = await tableConnector.GetDataAsync();
Console.WriteLine($"   Extracted {tableData.RowCount} rows from table\n");

// 2. Quick link extraction via factory
Console.WriteLine("2. Quick link extraction via WebConnectorFactory...");
var linkConnector = WebConnectorFactory.CreateLinkExtractor(booksUrl);
var linkData = await linkConnector.GetDataAsync();
Console.WriteLine($"   Extracted {linkData.RowCount} unique links\n");

// 3. Quick CSS selector extraction via factory
Console.WriteLine("3. Quick CSS selector extraction via WebConnectorFactory...");
var cssConnector = WebConnectorFactory.CreateCssSelectorConnector(booksUrl, ".product_pod");
var cssData = await cssConnector.GetDataAsync();
Console.WriteLine($"   Extracted {cssData.RowCount} product elements\n");

// 4. Quick metadata extraction via factory
Console.WriteLine("4. Quick metadata extraction via WebConnectorFactory...");
var metaConnector = WebConnectorFactory.CreatePageMetadataConnector(quotesUrl);
var metaData = await metaConnector.GetDataAsync();
Console.WriteLine($"   Extracted {metaData.Schema.Count} metadata fields\n");

// 5. Quick image extraction via factory
Console.WriteLine("5. Quick image extraction via WebConnectorFactory...");
var imageConnector = WebConnectorFactory.CreateImageExtractor(booksUrl);
var imageData = await imageConnector.GetDataAsync();
Console.WriteLine($"   Extracted {imageData.RowCount} images\n");

// 6. Using factory with custom configuration
Console.WriteLine("6. Factory instance with custom configuration...");
var factory = new WebConnectorFactory();

// Create table connector with custom config
var customTableConfig = new HtmlTableConnectorConfiguration
{
    Source = wikiUrl,
    TableSelector = "table.wikitable",
    FirstRowIsHeader = true,
    IncludeTableMetadata = true,
    MergeTables = false
};

// Use factory to create connector from configuration
IDataConnector customConnector = factory.CreateDataConnector(customTableConfig);
var customData = await customConnector.GetDataAsync();
Console.WriteLine($"   Custom table connector: {customData.RowCount} rows\n");

// 7. Using IDataConnectorFactory interface
Console.WriteLine("7. Using IDataConnectorFactory interface pattern...");

// Factory implements IDataConnectorFactory for dependency injection
IDataConnectorFactory connectorFactory = new WebConnectorFactory();

// Create different connectors through the same interface
var configs = new IDataConnectorConfiguration[]
{
    new LinkExtractorConnectorConfiguration { Source = booksUrl },
    new ImageExtractorConnectorConfiguration { Source = booksUrl },
    new PageMetadataConnectorConfiguration { Source = quotesUrl }
};

foreach (var config in configs)
{
    var connector = connectorFactory.CreateDataConnector(config);
    var data = await connector.GetDataAsync();
    Console.WriteLine($"   {config.GetType().Name}: {data.RowCount} rows");
}
Console.WriteLine();

// 8. Comparison: Factory vs Direct instantiation
Console.WriteLine("8. Comparison: Factory vs Direct instantiation...\n");

Console.WriteLine("   Factory approach (one-liner):");
Console.WriteLine("   var connector = WebConnectorFactory.CreateLinkExtractor(url);");
Console.WriteLine();

Console.WriteLine("   Direct instantiation (verbose):");
Console.WriteLine("   var config = new LinkExtractorConnectorConfiguration { Source = url };");
Console.WriteLine("   var connector = new LinkExtractorConnector(config);");
Console.WriteLine();

// 9. Extract from multiple sources using factory
Console.WriteLine("9. Processing multiple sources with factory...");
var urls = new[]
{
    new Uri("https://books.toscrape.com/"),
    new Uri("https://quotes.toscrape.com/")
};

Console.WriteLine("   Link counts per site:");
foreach (var sourceUrl in urls)
{
    var links = WebConnectorFactory.CreateLinkExtractor(sourceUrl);
    var linksData = await links.GetDataAsync();
    Console.WriteLine($"   - {sourceUrl.Host}: {linksData.RowCount} links");
}
Console.WriteLine();

// 10. Factory pattern benefits summary
Console.WriteLine("10. Factory Pattern Benefits:");
Console.WriteLine("    - Simplified connector creation with sensible defaults");
Console.WriteLine("    - Consistent interface (IDataConnectorFactory)");
Console.WriteLine("    - Easy dependency injection support");
Console.WriteLine("    - Reduced boilerplate code");
Console.WriteLine("    - Type-safe configuration selection");

Console.WriteLine("\n=== Sample Complete ===");
