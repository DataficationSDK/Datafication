using Datafication.Core.Data;
using Datafication.Core.Connectors;
using System.Xml.Linq;

namespace CustomConnectorAndSink;

/// <summary>
/// Custom XML connector that implements IDataConnector interface.
/// This demonstrates how to create a custom connector for loading data from XML files.
/// </summary>
public class CustomXmlConnector : IDataConnector
{
    private readonly string _xmlFilePath;

    public CustomXmlConnector(string xmlFilePath)
    {
        _xmlFilePath = xmlFilePath ?? throw new ArgumentNullException(nameof(xmlFilePath));
    }

    public string GetConnectorId()
    {
        return $"xml-connector-{_xmlFilePath.GetHashCode()}";
    }

    public async Task<DataBlock> GetDataAsync()
    {
        // In a real implementation, this would be async file I/O
        // For this sample, we'll use synchronous operations
        await Task.CompletedTask;

        if (!File.Exists(_xmlFilePath))
        {
            throw new FileNotFoundException($"XML file not found: {_xmlFilePath}");
        }

        var doc = XDocument.Load(_xmlFilePath);
        var dataBlock = new DataBlock();

        // Assume XML structure: <root><item><field1>value1</field1><field2>value2</field2></item>...</root>
        var items = doc.Root?.Elements("item") ?? Enumerable.Empty<XElement>();

        if (!items.Any())
        {
            return dataBlock; // Return empty DataBlock if no items
        }

        // Get all unique field names from first item
        var firstItem = items.First();
        var fieldNames = firstItem.Elements().Select(e => e.Name.LocalName).ToList();

        // Add columns
        foreach (var fieldName in fieldNames)
        {
            // Determine type from first value (simplified - in production, you'd want better type inference)
            var firstValue = firstItem.Element(fieldName)?.Value;
            var columnType = InferType(firstValue);
            dataBlock.AddColumn(new DataColumn(fieldName, columnType));
        }

        // Add rows
        foreach (var item in items)
        {
            var rowValues = new object[fieldNames.Count];
            for (int i = 0; i < fieldNames.Count; i++)
            {
                var element = item.Element(fieldNames[i]);
                var value = element?.Value;
                rowValues[i] = ConvertValue(value, dataBlock.GetColumn(fieldNames[i]).DataType.GetClrType());
            }
            dataBlock.AddRow(rowValues);
        }

        return dataBlock;
    }

    public async Task<IStorageDataBlock> GetStorageDataAsync(IStorageDataBlock target, int batchSize = 10000)
    {
        // Get the data
        var dataBlock = await GetDataAsync();
        
        // In a real implementation, you would stream data in batches to the IStorageDataBlock
        // This is a simplified example - in production, you would:
        // 1. Process data in batches of batchSize
        // 2. Use target.AppendBatchAsync() to append each batch
        // 3. Handle schema matching between source and target
        // 
        // Note: IStorageDataBlock is typically implemented by VelocityDataBlock
        // For this sample, we demonstrate the interface pattern
        
        if (dataBlock.RowCount > 0 && target.SupportsBatchAppend)
        {
            // Ensure target has the same schema
            if (target.Schema.Count == 0)
            {
                foreach (var columnName in dataBlock.Schema.GetColumnNames())
                {
                    var sourceColumn = dataBlock.GetColumn(columnName);
                    target.AddColumn(new DataColumn(sourceColumn.Name, sourceColumn.DataType.GetClrType()));
                }
            }
            
            // Append data in batches
            var totalRows = dataBlock.RowCount;
            for (int startRow = 0; startRow < totalRows; startRow += batchSize)
            {
                var batch = dataBlock.CopyRowRange(startRow, Math.Min(batchSize, totalRows - startRow));
                await target.AppendBatchAsync(batch);
            }
            
            await target.FlushAsync();
        }
        
        return target;
    }

    private static Type InferType(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return typeof(string);

        if (int.TryParse(value, out _))
            return typeof(int);
        if (decimal.TryParse(value, out _))
            return typeof(decimal);
        if (DateTime.TryParse(value, out _))
            return typeof(DateTime);
        if (bool.TryParse(value, out _))
            return typeof(bool);

        return typeof(string);
    }

    private static object? ConvertValue(string? value, Type targetType)
    {
        if (value == null)
            return null;

        if (targetType == typeof(string))
            return value;
        if (targetType == typeof(int) && int.TryParse(value, out int intVal))
            return intVal;
        if (targetType == typeof(decimal) && decimal.TryParse(value, out decimal decVal))
            return decVal;
        if (targetType == typeof(DateTime) && DateTime.TryParse(value, out DateTime dtVal))
            return dtVal;
        if (targetType == typeof(bool) && bool.TryParse(value, out bool boolVal))
            return boolVal;

        return value;
    }
}

