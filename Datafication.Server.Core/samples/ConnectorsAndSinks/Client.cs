using System.Text;
using System.Text.Json;

namespace ConnectorsAndSinks;

/// <summary>
/// Example C# client demonstrating different output formats and data export.
/// Run this client while the ConnectorsAndSinks server is running.
/// </summary>
public static class FormatClient
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("http://localhost:5000")
    };

    /// <summary>
    /// Run all format examples.
    /// </summary>
    public static async Task RunExamplesAsync()
    {
        Console.WriteLine("=== ConnectorsAndSinks Client Examples ===\n");

        // Different output formats
        await GetAsJsonAsync();
        await GetAsCsvAsync();
        await GetAsHtmlAsync();

        // Query with format
        await QueryWithFormatAsync();
    }

    /// <summary>
    /// Get data as JSON (default format).
    /// </summary>
    public static async Task GetAsJsonAsync()
    {
        Console.WriteLine("--- JSON Format (Default) ---");

        var response = await _client.GetAsync("/api/data/datablocks/products");
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
        Console.WriteLine($"Response:\n{content[..Math.Min(500, content.Length)]}...");
        Console.WriteLine();
    }

    /// <summary>
    /// Get data as CSV.
    /// </summary>
    public static async Task GetAsCsvAsync()
    {
        Console.WriteLine("--- CSV Format ---");

        var response = await _client.GetAsync("/api/data/datablocks/products?format=csv");
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
        Console.WriteLine($"Response:\n{content[..Math.Min(500, content.Length)]}...");
        Console.WriteLine();
    }

    /// <summary>
    /// Get data as HTML table.
    /// </summary>
    public static async Task GetAsHtmlAsync()
    {
        Console.WriteLine("--- HTML Format ---");

        var response = await _client.GetAsync("/api/data/datablocks/summary?format=html");
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
        Console.WriteLine($"Response:\n{content[..Math.Min(500, content.Length)]}...");
        Console.WriteLine();
    }

    /// <summary>
    /// Query data and get result in CSV format.
    /// </summary>
    public static async Task QueryWithFormatAsync()
    {
        Console.WriteLine("--- Query with CSV Output ---");

        var query = new
        {
            where = new[]
            {
                new { column = "Category", @operator = "eq", value = "Electronics" }
            },
            select = new[] { "ProductName", "Price" }
        };

        var json = JsonSerializer.Serialize(query);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Note: format parameter in query string
        var response = await _client.PostAsync("/api/data/datablocks/products/query?format=csv", content);
        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
        Console.WriteLine($"Response:\n{result}");
        Console.WriteLine();
    }
}
