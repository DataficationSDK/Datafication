using Datafication.Core.Data;
using Datafication.Connectors.WebConnector.Connectors;

Console.WriteLine("=== Datafication.WebConnector Product Scraping Sample ===\n");

// Books to Scrape - a site built for web scraping practice
var url = new Uri("https://books.toscrape.com/");
Console.WriteLine($"Target URL: {url}\n");

// 1. Basic product extraction using SubSelectors
Console.WriteLine("1. Extracting products with SubSelectors...");
var basicConfig = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = "article.product_pod",  // Each book is an article.product_pod

    // Extract text content from nested elements
    SubSelectors = new Dictionary<string, string>
    {
        { "Title", "h3 a" },           // Book title from the h3 link
        { "Price", ".price_color" },   // Price from the price span
        { "Availability", ".availability" }  // Stock status
    },

    IncludeInnerText = false,  // We're using SubSelectors instead
    IncludeTagName = false,
    TrimValues = true
};

var basicConnector = new CssSelectorConnector(basicConfig);
var basicData = await basicConnector.GetDataAsync();
Console.WriteLine($"   Extracted {basicData.RowCount} products\n");

// 2. Display schema information
Console.WriteLine("2. Schema Information:");
foreach (var colName in basicData.Schema.GetColumnNames())
{
    var column = basicData.GetColumn(colName);
    Console.WriteLine($"   - {colName}: {column.DataType.GetClrType().Name}");
}
Console.WriteLine();

// 3. Display sample products
Console.WriteLine("3. First 10 products:");
Console.WriteLine("   " + new string('-', 80));
Console.WriteLine($"   {"Title",-40} {"Price",-15} {"Availability",-20}");
Console.WriteLine("   " + new string('-', 80));

var cursor = basicData.GetRowCursor("Title", "Price", "Availability");
int rowCount = 0;
while (cursor.MoveNext() && rowCount < 10)
{
    var title = cursor.GetValue("Title")?.ToString() ?? "";
    var price = cursor.GetValue("Price")?.ToString() ?? "";
    var availability = cursor.GetValue("Availability")?.ToString() ?? "";

    // Truncate long titles
    if (title.Length > 37) title = title.Substring(0, 37) + "...";

    Console.WriteLine($"   {title,-40} {price,-15} {availability,-20}");
    rowCount++;
}
Console.WriteLine("   " + new string('-', 80));
Console.WriteLine($"   Showing 10 of {basicData.RowCount} products\n");

// 4. Extract attributes using AttributeSubSelectors
Console.WriteLine("4. Extracting with AttributeSubSelectors (links and images)...");
var attrConfig = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = "article.product_pod",

    // Text content from nested elements
    SubSelectors = new Dictionary<string, string>
    {
        { "Title", "h3 a" },
        { "Price", ".price_color" }
    },

    // Attribute values from nested elements (format: "selector|attribute")
    AttributeSubSelectors = new Dictionary<string, string>
    {
        { "ProductUrl", "h3 a|href" },     // Get href from the title link
        { "ImageUrl", ".image_container img|src" },  // Get src from product image
        { "Rating", "p.star-rating|class" }  // Get class attribute (contains rating)
    },

    IncludeInnerText = false,
    IncludeTagName = false,
    IncludeElementIndex = true,
    TrimValues = true
};

var attrConnector = new CssSelectorConnector(attrConfig);
var attrData = await attrConnector.GetDataAsync();
Console.WriteLine($"   Extracted {attrData.RowCount} products with URLs and images\n");

// 5. Display products with links
Console.WriteLine("5. Products with links and ratings:");
Console.WriteLine("   " + new string('-', 90));
Console.WriteLine($"   {"Title",-35} {"Price",-10} {"Rating",-20} {"ImageUrl",-20}");
Console.WriteLine("   " + new string('-', 90));

var attrCursor = attrData.GetRowCursor("Title", "Price", "Rating", "ImageUrl");
rowCount = 0;
while (attrCursor.MoveNext() && rowCount < 5)
{
    var title = attrCursor.GetValue("Title")?.ToString() ?? "";
    var price = attrCursor.GetValue("Price")?.ToString() ?? "";
    var rating = attrCursor.GetValue("Rating")?.ToString() ?? "";
    var imageUrl = attrCursor.GetValue("ImageUrl")?.ToString() ?? "";

    // Clean up rating (extract just the rating word)
    if (rating.Contains("star-rating"))
    {
        rating = rating.Replace("star-rating", "").Trim();
    }

    // Truncate values
    if (title.Length > 32) title = title.Substring(0, 32) + "...";
    if (imageUrl.Length > 17) imageUrl = imageUrl.Substring(0, 17) + "...";

    Console.WriteLine($"   {title,-35} {price,-10} {rating,-20} {imageUrl,-20}");
    rowCount++;
}
Console.WriteLine("   " + new string('-', 90));
Console.WriteLine();

// 6. Limiting results with MaxElements
Console.WriteLine("6. Limiting results with MaxElements...");
var limitConfig = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = "article.product_pod",
    SubSelectors = new Dictionary<string, string>
    {
        { "Title", "h3 a" },
        { "Price", ".price_color" }
    },
    MaxElements = 5,  // Only get first 5 products
    IncludeInnerText = false,
    IncludeTagName = false
};

var limitConnector = new CssSelectorConnector(limitConfig);
var limitData = await limitConnector.GetDataAsync();
Console.WriteLine($"   Limited to {limitData.RowCount} products (MaxElements = 5)\n");

// 7. Including raw HTML content
Console.WriteLine("7. Including HTML content for analysis...");
var htmlConfig = new CssSelectorConnectorConfiguration
{
    Source = url,
    Selector = "article.product_pod",
    SubSelectors = new Dictionary<string, string>
    {
        { "Title", "h3 a" }
    },
    IncludeInnerText = true,    // All text content
    IncludeInnerHtml = true,    // HTML inside the element
    IncludeOuterHtml = false,   // We'll skip OuterHtml to keep output manageable
    IncludeTagName = true,
    MaxElements = 1,
    TrimValues = true
};

var htmlConnector = new CssSelectorConnector(htmlConfig);
var htmlData = await htmlConnector.GetDataAsync();
Console.WriteLine("   Schema with HTML columns:");
foreach (var colName in htmlData.Schema.GetColumnNames())
{
    Console.WriteLine($"   - {colName}");
}

// Show InnerHtml preview
var htmlCursor = htmlData.GetRowCursor("InnerHtml");
if (htmlCursor.MoveNext())
{
    var html = htmlCursor.GetValue("InnerHtml")?.ToString() ?? "";
    if (html.Length > 200) html = html.Substring(0, 200) + "...";
    Console.WriteLine($"\n   InnerHtml preview: {html}");
}
Console.WriteLine();

// 8. Filtering scraped data
Console.WriteLine("8. Filtering products (price contains '51')...");
var filteredData = attrData.Filter(row => row["Price"]?.ToString()?.Contains("51") == true);
Console.WriteLine($"   Found {filteredData.RowCount} products with '51' in price:\n");

var filterCursor = filteredData.GetRowCursor("Title", "Price");
while (filterCursor.MoveNext())
{
    var title = filterCursor.GetValue("Title")?.ToString() ?? "";
    var price = filterCursor.GetValue("Price")?.ToString() ?? "";
    if (title.Length > 40) title = title.Substring(0, 40) + "...";
    Console.WriteLine($"   {title,-45} {price}");
}

Console.WriteLine("\n=== Sample Complete ===");
