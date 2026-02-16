using Datafication.Core.Data;
using Datafication.Connectors.WebConnector.Connectors;

Console.WriteLine("=== Datafication.WebConnector Metadata Extraction Sample ===\n");

// Quotes to Scrape - has meta tags for demonstration
var url = new Uri("https://quotes.toscrape.com/");
Console.WriteLine($"Target URL: {url}\n");

// 1. Extract all metadata with default settings
Console.WriteLine("1. Extracting all metadata (default settings)...");
var defaultConfig = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractStandardMeta = true,
    ExtractOpenGraph = true,
    ExtractTwitterCard = true,
    ExtractJsonLd = true,
    ExtractLinkTags = true,
    SingleRowResult = true  // One row with all metadata as columns
};

var defaultConnector = new PageMetadataConnector(defaultConfig);
var metadata = await defaultConnector.GetDataAsync();
Console.WriteLine($"   Extracted {metadata.Schema.Count} metadata fields\n");

// 2. Display schema (metadata field names)
Console.WriteLine("2. Metadata fields extracted:");
var columnNames = metadata.Schema.GetColumnNames().ToList();
foreach (var colName in columnNames.Take(15))
{
    Console.WriteLine($"   - {colName}");
}
if (columnNames.Count > 15)
{
    Console.WriteLine($"   ... and {columnNames.Count - 15} more fields\n");
}
else
{
    Console.WriteLine();
}

// 3. Display key metadata values
Console.WriteLine("3. Key metadata values:");
var cursor = metadata.GetRowCursor(columnNames.ToArray());
if (cursor.MoveNext())
{
    var url_val = cursor.GetValue("Url")?.ToString() ?? "(none)";
    var title = cursor.GetValue("Title")?.ToString() ?? "(none)";
    Console.WriteLine($"   URL: {url_val}");
    Console.WriteLine($"   Title: {title}");

    // Try to get description if available
    if (columnNames.Contains("Description"))
    {
        var desc = cursor.GetValue("Description")?.ToString() ?? "(none)";
        Console.WriteLine($"   Description: {desc}");
    }

    // Try to get charset
    if (columnNames.Contains("Charset"))
    {
        var charset = cursor.GetValue("Charset")?.ToString() ?? "(none)";
        Console.WriteLine($"   Charset: {charset}");
    }

    // Try to get language
    if (columnNames.Contains("Language"))
    {
        var lang = cursor.GetValue("Language")?.ToString() ?? "(none)";
        Console.WriteLine($"   Language: {lang}");
    }
}
Console.WriteLine();

// 4. Extract standard meta tags only
Console.WriteLine("4. Extracting standard meta tags only...");
var standardConfig = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractStandardMeta = true,
    ExtractOpenGraph = false,
    ExtractTwitterCard = false,
    ExtractJsonLd = false,
    ExtractLinkTags = false
};

var standardConnector = new PageMetadataConnector(standardConfig);
var standardMeta = await standardConnector.GetDataAsync();
Console.WriteLine($"   Standard meta fields: {standardMeta.Schema.Count}");
Console.WriteLine("   Fields:");
foreach (var col in standardMeta.Schema.GetColumnNames())
{
    Console.WriteLine($"   - {col}");
}
Console.WriteLine();

// 5. Extract Open Graph metadata
Console.WriteLine("5. Extracting Open Graph metadata...");
var ogConfig = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractStandardMeta = false,
    ExtractOpenGraph = true,
    ExtractTwitterCard = false,
    ExtractJsonLd = false,
    ExtractLinkTags = false
};

var ogConnector = new PageMetadataConnector(ogConfig);
var ogMeta = await ogConnector.GetDataAsync();
Console.WriteLine($"   Open Graph fields: {ogMeta.Schema.Count}");
var ogColumns = ogMeta.Schema.GetColumnNames().Where(c => c.StartsWith("OG_")).ToList();
if (ogColumns.Any())
{
    Console.WriteLine("   Open Graph tags found:");
    foreach (var col in ogColumns)
    {
        Console.WriteLine($"   - {col}");
    }
}
else
{
    Console.WriteLine("   (No Open Graph tags found on this page)");
}
Console.WriteLine();

// 6. Extract Twitter Card metadata
Console.WriteLine("6. Extracting Twitter Card metadata...");
var twitterConfig = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractStandardMeta = false,
    ExtractOpenGraph = false,
    ExtractTwitterCard = true,
    ExtractJsonLd = false,
    ExtractLinkTags = false
};

var twitterConnector = new PageMetadataConnector(twitterConfig);
var twitterMeta = await twitterConnector.GetDataAsync();
var twitterColumns = twitterMeta.Schema.GetColumnNames().Where(c => c.StartsWith("Twitter_")).ToList();
if (twitterColumns.Any())
{
    Console.WriteLine("   Twitter Card tags found:");
    foreach (var col in twitterColumns)
    {
        Console.WriteLine($"   - {col}");
    }
}
else
{
    Console.WriteLine("   (No Twitter Card tags found on this page)");
}
Console.WriteLine();

// 7. Extract JSON-LD structured data
Console.WriteLine("7. Extracting JSON-LD structured data...");
var jsonLdConfig = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractStandardMeta = false,
    ExtractOpenGraph = false,
    ExtractTwitterCard = false,
    ExtractJsonLd = true,
    ExtractLinkTags = false
};

var jsonLdConnector = new PageMetadataConnector(jsonLdConfig);
var jsonLdMeta = await jsonLdConnector.GetDataAsync();

if (jsonLdMeta.Schema.GetColumnNames().Contains("JsonLd"))
{
    var jsonCursor = jsonLdMeta.GetRowCursor("JsonLd");
    if (jsonCursor.MoveNext())
    {
        var jsonLd = jsonCursor.GetValue("JsonLd")?.ToString();
        if (!string.IsNullOrEmpty(jsonLd))
        {
            Console.WriteLine("   JSON-LD content (preview):");
            var preview = jsonLd.Length > 200 ? jsonLd.Substring(0, 200) + "..." : jsonLd;
            Console.WriteLine($"   {preview}");
        }
        else
        {
            Console.WriteLine("   (No JSON-LD found on this page)");
        }
    }
}
else
{
    Console.WriteLine("   (No JSON-LD column in results)");
}
Console.WriteLine();

// 8. Extract link tags (canonical, favicon, etc.)
Console.WriteLine("8. Extracting link tags...");
var linkConfig = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractStandardMeta = false,
    ExtractOpenGraph = false,
    ExtractTwitterCard = false,
    ExtractJsonLd = false,
    ExtractLinkTags = true
};

var linkConnector = new PageMetadataConnector(linkConfig);
var linkMeta = await linkConnector.GetDataAsync();
Console.WriteLine("   Link tag fields:");
foreach (var col in linkMeta.Schema.GetColumnNames())
{
    Console.WriteLine($"   - {col}");
}
Console.WriteLine();

// 9. Name/Value pair format
Console.WriteLine("9. Using name/value pair format...");
var pairConfig = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractStandardMeta = true,
    ExtractOpenGraph = true,
    ExtractTwitterCard = true,
    ExtractJsonLd = false,  // Skip JSON-LD for cleaner output
    ExtractLinkTags = true,
    SingleRowResult = false  // Multiple rows with Name/Value pairs
};

var pairConnector = new PageMetadataConnector(pairConfig);
var pairMeta = await pairConnector.GetDataAsync();
Console.WriteLine($"   Extracted {pairMeta.RowCount} name/value pairs\n");

Console.WriteLine("   First 10 metadata pairs:");
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine($"   {"Name",-30} {"Value",-30}");
Console.WriteLine("   " + new string('-', 60));

var pairCursor = pairMeta.GetRowCursor("Name", "Value");
int pairCount = 0;
while (pairCursor.MoveNext() && pairCount < 10)
{
    var name = pairCursor.GetValue("Name")?.ToString() ?? "";
    var value = pairCursor.GetValue("Value")?.ToString() ?? "";

    if (name.Length > 27) name = name.Substring(0, 27) + "...";
    if (value.Length > 27) value = value.Substring(0, 27) + "...";

    Console.WriteLine($"   {name,-30} {value,-30}");
    pairCount++;
}
Console.WriteLine("   " + new string('-', 60));
Console.WriteLine();

// 10. Extract custom meta tags
Console.WriteLine("10. Extracting custom meta tags...");
var customConfig = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractStandardMeta = false,
    ExtractOpenGraph = false,
    ExtractTwitterCard = false,
    ExtractJsonLd = false,
    ExtractLinkTags = false,
    CustomMetaTags = new Dictionary<string, string>
    {
        { "CustomViewport", "viewport" },
        { "CustomRobots", "robots" },
        { "CustomGenerator", "generator" }
    }
};

var customConnector = new PageMetadataConnector(customConfig);
var customMeta = await customConnector.GetDataAsync();
Console.WriteLine("   Custom meta tag values:");
var customCursor = customMeta.GetRowCursor(customMeta.Schema.GetColumnNames().ToArray());
if (customCursor.MoveNext())
{
    foreach (var col in customMeta.Schema.GetColumnNames())
    {
        var val = customCursor.GetValue(col)?.ToString() ?? "(not found)";
        Console.WriteLine($"   - {col}: {val}");
    }
}
Console.WriteLine();

// 11. Extract all meta tags for exploration
Console.WriteLine("11. Extracting ALL meta tags (exploration mode)...");
var allTagsConfig = new PageMetadataConnectorConfiguration
{
    Source = url,
    ExtractStandardMeta = true,
    ExtractOpenGraph = true,
    ExtractTwitterCard = true,
    ExtractJsonLd = false,
    ExtractLinkTags = true,
    ExtractAllMetaTags = true  // Include all found meta tags
};

var allTagsConnector = new PageMetadataConnector(allTagsConfig);
var allTagsMeta = await allTagsConnector.GetDataAsync();
Console.WriteLine($"   Total metadata fields discovered: {allTagsMeta.Schema.Count}");

// Show any Meta_ prefixed columns (from ExtractAllMetaTags)
var extraMeta = allTagsMeta.Schema.GetColumnNames().Where(c => c.StartsWith("Meta_")).ToList();
if (extraMeta.Any())
{
    Console.WriteLine($"   Additional meta tags found ({extraMeta.Count}):");
    foreach (var col in extraMeta.Take(5))
    {
        Console.WriteLine($"   - {col}");
    }
    if (extraMeta.Count > 5)
    {
        Console.WriteLine($"   ... and {extraMeta.Count - 5} more");
    }
}

Console.WriteLine("\n=== Sample Complete ===");
