using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace QueryOperations;

/// <summary>
/// Example C# client demonstrating how to execute queries against the DataBlock API.
/// Run this client while the QueryOperations server is running.
/// </summary>
public static class QueryClient
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
    /// Run all query examples.
    /// </summary>
    public static async Task RunExamplesAsync()
    {
        Console.WriteLine("=== QueryOperations Client Examples ===\n");

        // 1. Basic filter - get only Electronics
        await FilterByCategoryAsync("Electronics");

        // 2. Filter with comparison - get sales over $3000
        await FilterByTotalAsync(3000);

        // 3. Sort by total descending
        await SortByTotalDescendingAsync();

        // 4. GroupBy category with sum aggregation
        await GroupByCategoryAsync();

        // 5. Combined: filter, sort, and select specific columns
        await CombinedQueryAsync();
    }

    /// <summary>
    /// Filter sales by category.
    /// </summary>
    public static async Task FilterByCategoryAsync(string category)
    {
        Console.WriteLine($"--- Filter by Category = '{category}' ---");

        var query = new
        {
            where = new[]
            {
                new { column = "Category", @operator = "eq", value = category }
            }
        };

        await ExecuteQueryAsync(query);
    }

    /// <summary>
    /// Filter sales where total is greater than a value.
    /// </summary>
    public static async Task FilterByTotalAsync(decimal minTotal)
    {
        Console.WriteLine($"--- Filter by Total > {minTotal} ---");

        var query = new
        {
            where = new[]
            {
                new { column = "Total", @operator = "gt", value = minTotal.ToString() }
            }
        };

        await ExecuteQueryAsync(query);
    }

    /// <summary>
    /// Sort sales by total in descending order.
    /// </summary>
    public static async Task SortByTotalDescendingAsync()
    {
        Console.WriteLine("--- Sort by Total (Descending) ---");

        var query = new
        {
            sort = new { column = "Total", direction = "desc" }
        };

        await ExecuteQueryAsync(query);
    }

    /// <summary>
    /// GroupBy category with sum and count aggregations.
    /// </summary>
    public static async Task GroupByCategoryAsync()
    {
        Console.WriteLine("--- GroupBy Category with Aggregations ---");

        var query = new
        {
            groupBy = new
            {
                column = "Category",
                aggregations = new Dictionary<string, string>
                {
                    { "Total", "sum" },
                    { "Quantity", "sum" },
                    { "Id", "count" }
                }
            }
        };

        await ExecuteQueryAsync(query);
    }

    /// <summary>
    /// Combined query: filter Electronics, sort by Total desc, select specific columns.
    /// </summary>
    public static async Task CombinedQueryAsync()
    {
        Console.WriteLine("--- Combined Query: Filter + Sort + Select ---");

        var query = new
        {
            where = new[]
            {
                new { column = "Category", @operator = "eq", value = "Electronics" }
            },
            sort = new { column = "Total", direction = "desc" },
            select = new[] { "Product", "Quantity", "Total" }
        };

        await ExecuteQueryAsync(query);
    }

    private static async Task ExecuteQueryAsync(object query)
    {
        var json = JsonSerializer.Serialize(query, _jsonOptions);
        Console.WriteLine($"Query: {json}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/data/datablocks/sales/query", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Result: {result}");
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }

        Console.WriteLine();
    }
}
