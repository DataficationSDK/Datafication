using Datafication.Core.Data;
using Datafication.Connectors.WebConnector.Connectors;

Console.WriteLine("=== Datafication.WebConnector Quote Scraping Sample ===\n");

// Quotes to Scrape - structured quote content for advanced CSS selector patterns
var url = new Uri("https://quotes.toscrape.com/");
Console.WriteLine($"Target URL: {url}\n");

// 1. Extract quotes with nested SubSelectors
Console.WriteLine("1. Extracting quotes with nested SubSelectors...");
var quotesConfig = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = ".quote",  // Each quote container

    // Extract text from nested elements
    SubSelectors = new Dictionary<string, string>
    {
        { "QuoteText", ".text" },           // The quote text
        { "Author", ".author" },            // Author name
        { "Tags", ".tags" }                 // Tags as comma-separated text
    },

    // Extract attributes from nested elements
    AttributeSubSelectors = new Dictionary<string, string>
    {
        { "AuthorUrl", "a[href*='author']|href" }  // Link to author page
    },

    IncludeInnerText = false,
    IncludeTagName = false,
    TrimValues = true
};

var quotesConnector = new CssSelectorConnector(quotesConfig);
var quotes = await quotesConnector.GetDataAsync();
Console.WriteLine($"   Extracted {quotes.RowCount} quotes\n");

// 2. Display schema
Console.WriteLine("2. Schema Information:");
foreach (var colName in quotes.Schema.GetColumnNames())
{
    var column = quotes.GetColumn(colName);
    Console.WriteLine($"   - {colName}: {column.DataType.GetClrType().Name}");
}
Console.WriteLine();

// 3. Display sample quotes
Console.WriteLine("3. Sample quotes:");
Console.WriteLine("   " + new string('-', 95));

var cursor = quotes.GetRowCursor("QuoteText", "Author", "Tags");
int rowCount = 0;
while (cursor.MoveNext() && rowCount < 5)
{
    var text = cursor.GetValue("QuoteText")?.ToString() ?? "";
    var author = cursor.GetValue("Author")?.ToString() ?? "";
    var tags = cursor.GetValue("Tags")?.ToString() ?? "";

    // Clean up and truncate
    text = text.Replace("\u201c", "\"").Replace("\u201d", "\"");  // Smart quotes
    if (text.Length > 60) text = text.Substring(0, 60) + "...";
    if (tags.Length > 30) tags = tags.Substring(0, 30) + "...";

    Console.WriteLine($"\n   \"{text}\"");
    Console.WriteLine($"   - {author}");
    Console.WriteLine($"   Tags: {tags}");
    rowCount++;
}
Console.WriteLine("\n   " + new string('-', 95));
Console.WriteLine();

// 4. Extract individual tags (one tag per quote for analysis)
Console.WriteLine("4. Extracting first tag from each quote...");
var tagConfig = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = ".quote",
    SubSelectors = new Dictionary<string, string>
    {
        { "QuoteText", ".text" },
        { "Author", ".author" },
        { "FirstTag", ".tag:first-child" }  // Just the first tag
    },
    IncludeInnerText = false,
    IncludeTagName = false,
    TrimValues = true
};

var tagConnector = new CssSelectorConnector(tagConfig);
var tagData = await tagConnector.GetDataAsync();

Console.WriteLine("   Quotes with first tag:");
var tagCursor = tagData.GetRowCursor("Author", "FirstTag");
int tagCount = 0;
while (tagCursor.MoveNext() && tagCount < 5)
{
    var author = tagCursor.GetValue("Author")?.ToString() ?? "";
    var firstTag = tagCursor.GetValue("FirstTag")?.ToString() ?? "";
    Console.WriteLine($"   - {author}: #{firstTag}");
    tagCount++;
}
Console.WriteLine();

// 5. Group quotes by author
Console.WriteLine("5. Grouping quotes by author...");
var authorGroups = quotes.GroupBy("Author").Info();
Console.WriteLine($"   Found quotes from {authorGroups.RowCount} different authors:\n");

var groupCursor = authorGroups.GetRowCursor("Group", "Count");
while (groupCursor.MoveNext())
{
    var author = groupCursor.GetValue("Group")?.ToString() ?? "";
    var count = groupCursor.GetValue("Count");
    Console.WriteLine($"   - {author}: {count} quote(s)");
}
Console.WriteLine();

// 6. Extract author profile links
Console.WriteLine("6. Extracting author profile links...");
var linkConfig = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = ".quote",
    SubSelectors = new Dictionary<string, string>
    {
        { "Author", ".author" }
    },
    AttributeSubSelectors = new Dictionary<string, string>
    {
        { "AboutLink", "span a[href*='author']|href" }
    },
    IncludeInnerText = false,
    IncludeTagName = false,
    TrimValues = true
};

var linkConnector = new CssSelectorConnector(linkConfig);
var linkData = await linkConnector.GetDataAsync();

// Remove duplicates manually using GroupBy
var uniqueAuthors = linkData.GroupBy("Author");
Console.WriteLine($"   Author profile links:");
var linkCursor = linkData.GetRowCursor("Author", "AboutLink");
var seenAuthors = new HashSet<string>();
while (linkCursor.MoveNext())
{
    var author = linkCursor.GetValue("Author")?.ToString() ?? "";
    var aboutLink = linkCursor.GetValue("AboutLink")?.ToString() ?? "";
    if (!seenAuthors.Contains(author) && !string.IsNullOrEmpty(aboutLink))
    {
        Console.WriteLine($"   - {author}: {aboutLink}");
        seenAuthors.Add(author);
    }
}
Console.WriteLine();

// 7. Filter quotes by keyword
Console.WriteLine("7. Filtering quotes containing 'world'...");
var worldQuotes = quotes.Filter(row =>
    row["QuoteText"]?.ToString()?.ToLowerInvariant().Contains("world") == true);
Console.WriteLine($"   Found {worldQuotes.RowCount} quotes about 'world':\n");

var worldCursor = worldQuotes.GetRowCursor("QuoteText", "Author");
while (worldCursor.MoveNext())
{
    var text = worldCursor.GetValue("QuoteText")?.ToString() ?? "";
    var author = worldCursor.GetValue("Author")?.ToString() ?? "";
    text = text.Replace("\u201c", "\"").Replace("\u201d", "\"");
    if (text.Length > 70) text = text.Substring(0, 70) + "...";
    Console.WriteLine($"   \"{text}\"");
    Console.WriteLine($"   - {author}\n");
}

// 8. Include raw HTML for analysis
Console.WriteLine("8. Including InnerHtml for HTML structure analysis...");
var htmlConfig = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = ".quote",
    SubSelectors = new Dictionary<string, string>
    {
        { "Author", ".author" }
    },
    IncludeInnerText = false,
    IncludeInnerHtml = true,  // Include raw HTML
    IncludeOuterHtml = false,
    IncludeTagName = true,
    MaxElements = 1,
    TrimValues = true
};

var htmlConnector = new CssSelectorConnector(htmlConfig);
var htmlData = await htmlConnector.GetDataAsync();

var htmlCursor = htmlData.GetRowCursor("InnerHtml", "TagName");
if (htmlCursor.MoveNext())
{
    var html = htmlCursor.GetValue("InnerHtml")?.ToString() ?? "";
    var tag = htmlCursor.GetValue("TagName")?.ToString() ?? "";
    Console.WriteLine($"   TagName: {tag}");
    Console.WriteLine($"   InnerHtml preview (first 300 chars):");
    var preview = html.Length > 300 ? html.Substring(0, 300) + "..." : html;
    Console.WriteLine($"   {preview}");
}
Console.WriteLine();

// 9. Sort quotes by author alphabetically
Console.WriteLine("9. Sorting quotes by author...");
var sortedQuotes = quotes.Sort(SortDirection.Ascending, "Author");
Console.WriteLine("   First 5 quotes after sorting:");
var sortCursor = sortedQuotes.GetRowCursor("Author", "QuoteText");
int sortCount = 0;
while (sortCursor.MoveNext() && sortCount < 5)
{
    var author = sortCursor.GetValue("Author")?.ToString() ?? "";
    var text = sortCursor.GetValue("QuoteText")?.ToString() ?? "";
    text = text.Replace("\u201c", "\"").Replace("\u201d", "\"");
    if (text.Length > 50) text = text.Substring(0, 50) + "...";
    Console.WriteLine($"   - {author}: \"{text}\"");
    sortCount++;
}
Console.WriteLine();

// 10. Extract with direct attributes from main selector
Console.WriteLine("10. Extracting attributes from main selector element...");
var attrConfig = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = ".quote",
    Attributes = new List<string> { "itemscope", "itemtype" },  // Schema.org attributes if present
    SubSelectors = new Dictionary<string, string>
    {
        { "Author", ".author" }
    },
    IncludeInnerText = false,
    IncludeTagName = true,
    IncludeElementIndex = true,
    TrimValues = true
};

var attrConnector = new CssSelectorConnector(attrConfig);
var attrData = await attrConnector.GetDataAsync();
Console.WriteLine("   Schema with direct attributes:");
foreach (var col in attrData.Schema.GetColumnNames())
{
    Console.WriteLine($"   - {col}");
}

Console.WriteLine("\n=== Sample Complete ===");
