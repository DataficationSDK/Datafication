using Datafication.Core.Data;
using CustomConnectorAndSink;

Console.WriteLine("=== Datafication.Core Custom Connector and Sink Sample ===\n");

// 1. Create sample XML file for the custom connector
var xmlFilePath = Path.Combine(Path.GetTempPath(), "sample_data.xml");
var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <item>
    <ProductId>1</ProductId>
    <ProductName>Widget A</ProductName>
    <Price>10.50</Price>
    <Quantity>100</Quantity>
  </item>
  <item>
    <ProductId>2</ProductId>
    <ProductName>Widget B</ProductName>
    <Price>15.75</Price>
    <Quantity>50</Quantity>
  </item>
  <item>
    <ProductId>3</ProductId>
    <ProductName>Widget C</ProductName>
    <Price>20.00</Price>
    <Quantity>75</Quantity>
  </item>
</root>";

File.WriteAllText(xmlFilePath, xmlContent);
Console.WriteLine($"1. Created sample XML file: {xmlFilePath}\n");

// 2. Use custom XML connector to load data
Console.WriteLine("2. Using Custom XML Connector:");
var xmlConnector = new CustomXmlConnector(xmlFilePath);
var xmlData = await xmlConnector.GetDataAsync();
Console.WriteLine($"   Loaded {xmlData.RowCount} rows from XML");
Console.WriteLine($"   Columns: {string.Join(", ", xmlData.Schema.GetColumnNames())}");
PrintDataBlock(xmlData);

// 3. Use custom HTML sink to transform data
Console.WriteLine("\n3. Using Custom HTML Sink:");
var htmlSink = new CustomHtmlSink();
var htmlOutput = await htmlSink.Transform(xmlData);
Console.WriteLine("   Generated HTML table:");
Console.WriteLine(htmlOutput);

// 4. Save HTML to file
var htmlFilePath = Path.Combine(Path.GetTempPath(), "output.html");
var fullHtml = $@"<!DOCTYPE html>
<html>
<head>
    <title>DataBlock HTML Output</title>
    <style>
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #4CAF50; color: white; }}
        tr:nth-child(even) {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <h1>DataBlock Output</h1>
    {htmlOutput}
</body>
</html>";

File.WriteAllText(htmlFilePath, fullHtml);
Console.WriteLine($"\n4. Saved HTML output to: {htmlFilePath}");

// 5. Demonstrate using custom connector and sink in a pipeline
Console.WriteLine("\n5. Using Custom Connector and Sink in Pipeline:");
var processedData = xmlData
    .Compute("Revenue", "Price * Quantity")
    .Where("Revenue", 1000.0, ComparisonOperator.GreaterThan)
    .Sort(SortDirection.Descending, "Revenue");

var processedHtml = await htmlSink.Transform(processedData);
Console.WriteLine("   Processed data (Revenue > 1000, sorted):");
Console.WriteLine(processedHtml);

// Cleanup
File.Delete(xmlFilePath);
Console.WriteLine($"\n6. Cleaned up temporary files");

Console.WriteLine("\n=== Sample Complete ===");

static void PrintDataBlock(DataBlock dataBlock)
{
    if (dataBlock.RowCount == 0)
    {
        Console.WriteLine("   (No rows)");
        return;
    }

    var columnNames = dataBlock.Schema.GetColumnNames().ToArray();
    var cursor = dataBlock.GetRowCursor(columnNames);
    
    // Print header
    Console.WriteLine($"   {string.Join(" | ", columnNames)}");
    Console.WriteLine($"   {new string('-', columnNames.Sum(c => c.Length) + (columnNames.Length - 1) * 3)}");
    
    // Print rows
    while (cursor.MoveNext())
    {
        var values = columnNames.Select(col => 
        {
            var val = cursor.GetValue(col);
            if (val is decimal d) return d.ToString("F2");
            return val?.ToString() ?? "null";
        });
        Console.WriteLine($"   {string.Join(" | ", values)}");
    }
}

