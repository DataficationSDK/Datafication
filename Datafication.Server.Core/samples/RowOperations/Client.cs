using System.Text;
using System.Text.Json;

namespace RowOperations;

/// <summary>
/// Example C# client demonstrating CRUD operations on DataBlock rows.
/// Run this client while the RowOperations server is running.
/// </summary>
public static class RowClient
{
    private static readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("http://localhost:5000")
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Run all CRUD examples.
    /// </summary>
    public static async Task RunExamplesAsync()
    {
        Console.WriteLine("=== RowOperations Client Examples ===\n");

        // Show initial state
        await ShowCurrentDataAsync("Initial Data");

        // 1. Add row (append)
        await AddRowAsync();
        await ShowCurrentDataAsync("After Adding Row");

        // 2. Insert row at specific index
        await InsertRowAtIndexAsync();
        await ShowCurrentDataAsync("After Inserting Row at Index 1");

        // 3. Update existing row
        await UpdateRowAsync();
        await ShowCurrentDataAsync("After Updating Row");

        // 4. Delete row
        await DeleteRowAsync();
        await ShowCurrentDataAsync("After Deleting Row");
    }

    /// <summary>
    /// Add a new row to the end of the DataBlock.
    /// </summary>
    public static async Task AddRowAsync()
    {
        Console.WriteLine("--- Adding New Row ---");

        var request = new
        {
            values = new object[] { 4, "Review PR", "Pending", 2 }
        };

        await PostAsync("/api/data/datablocks/tasks/rows", request);
    }

    /// <summary>
    /// Insert a row at a specific index.
    /// </summary>
    public static async Task InsertRowAtIndexAsync()
    {
        Console.WriteLine("--- Inserting Row at Index 1 ---");

        var request = new
        {
            values = new object[] { 5, "Urgent bug fix", "In Progress", 1 }
        };

        // POST to /rows/{index} to insert at that position
        await PostAsync("/api/data/datablocks/tasks/rows/1", request);
    }

    /// <summary>
    /// Update an existing row by index.
    /// </summary>
    public static async Task UpdateRowAsync()
    {
        Console.WriteLine("--- Updating Row at Index 0 ---");

        var request = new
        {
            values = new object[] { 1, "Setup project", "Completed", 0 }
        };

        // PUT to /rows/{index} to update that row
        await PutAsync("/api/data/datablocks/tasks/rows/0", request);
    }

    /// <summary>
    /// Delete a row by index.
    /// </summary>
    public static async Task DeleteRowAsync()
    {
        Console.WriteLine("--- Deleting Row at Index 2 ---");

        // DELETE /rows/{index} to remove that row
        await DeleteAsync("/api/data/datablocks/tasks/rows/2");
    }

    /// <summary>
    /// Show the current state of the DataBlock.
    /// </summary>
    public static async Task ShowCurrentDataAsync(string label)
    {
        Console.WriteLine($"\n=== {label} ===");
        var response = await _client.GetAsync("/api/data/datablocks/tasks");
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine(result);
        Console.WriteLine();
    }

    private static async Task PostAsync(string url, object body)
    {
        var json = JsonSerializer.Serialize(body, _jsonOptions);
        Console.WriteLine($"POST {url}");
        Console.WriteLine($"Body: {json}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);

        Console.WriteLine($"Status: {response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {error}");
        }
        Console.WriteLine();
    }

    private static async Task PutAsync(string url, object body)
    {
        var json = JsonSerializer.Serialize(body, _jsonOptions);
        Console.WriteLine($"PUT {url}");
        Console.WriteLine($"Body: {json}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Console.WriteLine($"Status: {response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {error}");
        }
        Console.WriteLine();
    }

    private static async Task DeleteAsync(string url)
    {
        Console.WriteLine($"DELETE {url}");

        var response = await _client.DeleteAsync(url);

        Console.WriteLine($"Status: {response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {error}");
        }
        Console.WriteLine();
    }
}
