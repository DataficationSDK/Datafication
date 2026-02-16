using Datafication.Core.Data;
using Datafication.Core.Sinks;
using System.Text;

namespace CustomConnectorAndSink;

/// <summary>
/// Custom HTML sink that implements IDataSink<string> interface.
/// This demonstrates how to create a custom sink that transforms a DataBlock into an HTML table.
/// </summary>
public class CustomHtmlSink : IDataSink<string>
{
    public async Task<string> Transform(DataBlock dataBlock)
    {
        await Task.CompletedTask; // For async signature

        if (dataBlock.RowCount == 0)
        {
            return "<table><tr><td>No data</td></tr></table>";
        }

        var sb = new StringBuilder();
        sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0'>");

        // Header row
        var columnNames = dataBlock.Schema.GetColumnNames().ToArray();
        sb.AppendLine("  <thead>");
        sb.AppendLine("    <tr>");
        foreach (var colName in columnNames)
        {
            sb.AppendLine($"      <th>{EscapeHtml(colName)}</th>");
        }
        sb.AppendLine("    </tr>");
        sb.AppendLine("  </thead>");

        // Data rows
        sb.AppendLine("  <tbody>");
        var cursor = dataBlock.GetRowCursor(columnNames);
        while (cursor.MoveNext())
        {
            sb.AppendLine("    <tr>");
            foreach (var colName in columnNames)
            {
                var value = cursor.GetValue(colName);
                var displayValue = FormatValue(value);
                sb.AppendLine($"      <td>{EscapeHtml(displayValue)}</td>");
            }
            sb.AppendLine("    </tr>");
        }
        sb.AppendLine("  </tbody>");

        sb.AppendLine("</table>");

        return sb.ToString();
    }

    private static string FormatValue(object? value)
    {
        if (value == null)
            return "null";
        if (value is decimal d)
            return d.ToString("C");
        if (value is DateTime dt)
            return dt.ToString("yyyy-MM-dd");
        return value.ToString() ?? "null";
    }

    private static string EscapeHtml(string? text)
    {
        if (text == null)
            return string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}

