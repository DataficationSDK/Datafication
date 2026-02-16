using Datafication.Core.Data;
using Datafication.Connectors.WebConnector.Connectors;

Console.WriteLine("=== Datafication.WebConnector Table Extraction Sample ===\n");

// Wikipedia page with multiple well-structured tables
var url = new Uri("https://en.wikipedia.org/wiki/List_of_countries_by_population_(United_Nations)");
Console.WriteLine($"Target URL: {url}\n");

// 1. Basic table extraction
Console.WriteLine("1. Basic table extraction (first table only)...");
var basicConfig = new HtmlTableConnectorConfiguration
{
    Source = url,
    TableSelector = "table.wikitable",
    FirstRowIsHeader = true,
    IncludeTableMetadata = false,
    TrimCellValues = true
};

var basicConnector = new HtmlTableConnector(basicConfig);
var basicData = await basicConnector.GetDataAsync();
Console.WriteLine($"   Extracted {basicData.RowCount} rows with {basicData.Schema.Count} columns\n");

// 2. Display schema information
Console.WriteLine("2. Schema Information:");
foreach (var colName in basicData.Schema.GetColumnNames())
{
    var column = basicData.GetColumn(colName);
    Console.WriteLine($"   - {colName}: {column.DataType.GetClrType().Name}");
}
Console.WriteLine();

// 3. Display sample data using row cursor
Console.WriteLine("3. First 10 rows of data:");
Console.WriteLine("   " + new string('-', 90));
var columnNames = basicData.Schema.GetColumnNames().Take(4).ToArray();
Console.WriteLine($"   {columnNames[0],-10} {columnNames[1],-30} {columnNames[2],-20} {columnNames[3],-20}");
Console.WriteLine("   " + new string('-', 90));

var cursor = basicData.GetRowCursor(columnNames);
int rowCount = 0;
while (cursor.MoveNext() && rowCount < 10)
{
    var col0 = cursor.GetValue(columnNames[0])?.ToString() ?? "";
    var col1 = cursor.GetValue(columnNames[1])?.ToString() ?? "";
    var col2 = cursor.GetValue(columnNames[2])?.ToString() ?? "";
    var col3 = cursor.GetValue(columnNames[3])?.ToString() ?? "";
    Console.WriteLine($"   {col0,-10} {col1,-30} {col2,-20} {col3,-20}");
    rowCount++;
}
Console.WriteLine("   " + new string('-', 90));
Console.WriteLine($"   ... and {basicData.RowCount - 10} more rows\n");

// 4. Table extraction with metadata
Console.WriteLine("4. Extracting with table metadata enabled...");
var metadataConfig = new HtmlTableConnectorConfiguration
{
    Source = url,
    TableSelector = "table.wikitable",
    FirstRowIsHeader = true,
    IncludeTableMetadata = true,  // Adds TableIndex, TableId, TableClass, RowIndex
    TrimCellValues = true
};

var metadataConnector = new HtmlTableConnector(metadataConfig);
var metadataData = await metadataConnector.GetDataAsync();
Console.WriteLine($"   Schema now includes metadata columns:");
foreach (var colName in metadataData.Schema.GetColumnNames().Take(5))
{
    Console.WriteLine($"   - {colName}");
}
Console.WriteLine();

// 5. Merge multiple tables from page
Console.WriteLine("5. Merging multiple tables from page...");
var mergeConfig = new HtmlTableConnectorConfiguration
{
    Source = url,
    TableSelector = "table.wikitable",
    MergeTables = true,  // Combine all matching tables
    IncludeTableMetadata = true,
    FirstRowIsHeader = true,
    TrimCellValues = true
};

var mergeConnector = new HtmlTableConnector(mergeConfig);
var mergedData = await mergeConnector.GetDataAsync();
Console.WriteLine($"   Merged data: {mergedData.RowCount} total rows\n");

// 6. Analyze merged data by table index
Console.WriteLine("6. Analyzing merged data by source table...");
var tableGroups = mergedData.GroupBy("TableIndex").Info();
Console.WriteLine($"   Found data from {tableGroups.RowCount} different tables:");
var groupCursor = tableGroups.GetRowCursor("Group", "Count");
while (groupCursor.MoveNext())
{
    var tableIndex = groupCursor.GetValue("Group");
    var count = groupCursor.GetValue("Count");
    Console.WriteLine($"   - Table {tableIndex}: {count} rows");
}
Console.WriteLine();

// 7. Extract specific table by index
Console.WriteLine("7. Extracting specific table by index (TableIndex = 0)...");
var specificConfig = new HtmlTableConnectorConfiguration
{
    Source = url,
    TableSelector = "table.wikitable",
    TableIndex = 0,  // Only extract the first table
    FirstRowIsHeader = true,
    IncludeTableMetadata = false,
    TrimCellValues = true
};

var specificConnector = new HtmlTableConnector(specificConfig);
var specificData = await specificConnector.GetDataAsync();
Console.WriteLine($"   Table 0 contains {specificData.RowCount} rows\n");

// 8. Working with tables that have no headers
Console.WriteLine("8. Handling tables without headers...");
var noHeaderConfig = new HtmlTableConnectorConfiguration
{
    Source = url,
    TableSelector = "table.wikitable",
    FirstRowIsHeader = false,  // Generate Column_0, Column_1, etc.
    TableIndex = 0,
    TrimCellValues = true
};

var noHeaderConnector = new HtmlTableConnector(noHeaderConfig);
var noHeaderData = await noHeaderConnector.GetDataAsync();
Console.WriteLine("   Generated column names:");
foreach (var colName in noHeaderData.Schema.GetColumnNames().Take(5))
{
    Console.WriteLine($"   - {colName}");
}

Console.WriteLine("\n=== Sample Complete ===");
