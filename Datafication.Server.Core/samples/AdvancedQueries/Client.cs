using System.Text;
using System.Text.Json;

namespace AdvancedQueries;

/// <summary>
/// Example C# client demonstrating advanced query features.
/// Run this client while the AdvancedQueries server is running.
/// </summary>
public static class AdvancedQueryClient
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
    /// Run all advanced query examples.
    /// </summary>
    public static async Task RunExamplesAsync()
    {
        Console.WriteLine("=== AdvancedQueries Client Examples ===\n");

        // Window Functions
        await MovingAverageAsync();
        await CumulativeSumAsync();
        await RankingAsync();
        await LagLeadAsync();

        // Computed Columns
        await ComputedColumnAsync();

        // Joins
        await LeftJoinAsync();
        await InnerJoinAsync();
    }

    /// <summary>
    /// Calculate moving average using window function.
    /// </summary>
    public static async Task MovingAverageAsync()
    {
        Console.WriteLine("--- Window Function: Moving Average ---");

        var query = new
        {
            window = new[]
            {
                new
                {
                    column = "Price",
                    function = "movingavg",
                    windowSize = 3,
                    resultColumn = "MovingAvg3Day"
                }
            }
        };

        await ExecuteQueryAsync("stocks", query);
    }

    /// <summary>
    /// Calculate cumulative sum using window function.
    /// </summary>
    public static async Task CumulativeSumAsync()
    {
        Console.WriteLine("--- Window Function: Cumulative Sum ---");

        var query = new
        {
            window = new[]
            {
                new
                {
                    column = "Volume",
                    function = "cumsum",
                    resultColumn = "CumulativeVolume"
                }
            }
        };

        await ExecuteQueryAsync("stocks", query);
    }

    /// <summary>
    /// Rank rows using window function.
    /// </summary>
    public static async Task RankingAsync()
    {
        Console.WriteLine("--- Window Function: Ranking ---");

        var query = new
        {
            window = new[]
            {
                new
                {
                    column = "Price",
                    function = "rank",
                    orderByColumn = "Price",
                    partitionByColumns = new[] { "Symbol" },
                    resultColumn = "PriceRank"
                }
            }
        };

        await ExecuteQueryAsync("stocks", query);
    }

    /// <summary>
    /// Use LAG to compare with previous row.
    /// </summary>
    public static async Task LagLeadAsync()
    {
        Console.WriteLine("--- Window Function: LAG (Previous Value) ---");

        var query = new
        {
            window = new[]
            {
                new
                {
                    column = "Price",
                    function = "lag",
                    offset = 1,
                    resultColumn = "PreviousPrice"
                }
            }
        };

        await ExecuteQueryAsync("stocks", query);
    }

    /// <summary>
    /// Create computed columns using expressions.
    /// </summary>
    public static async Task ComputedColumnAsync()
    {
        Console.WriteLine("--- Computed Column ---");

        var query = new
        {
            compute = new[]
            {
                new
                {
                    resultColumn = "TotalValue",
                    expression = "[Price] * [Volume]"
                }
            }
        };

        await ExecuteQueryAsync("stocks", query);
    }

    /// <summary>
    /// Left join orders with customers.
    /// </summary>
    public static async Task LeftJoinAsync()
    {
        Console.WriteLine("--- Left Join: Orders with Customers ---");

        var query = new
        {
            merge = new
            {
                otherDataBlockId = "customers",
                keyColumn = "CustomerId",
                otherKeyColumn = "CustomerId",
                mode = "left"
            }
        };

        await ExecuteQueryAsync("orders", query);
    }

    /// <summary>
    /// Inner join orders with products.
    /// </summary>
    public static async Task InnerJoinAsync()
    {
        Console.WriteLine("--- Inner Join: Orders with Products ---");

        var query = new
        {
            merge = new
            {
                otherDataBlockId = "products",
                keyColumn = "ProductId",
                otherKeyColumn = "ProductId",
                mode = "inner"
            }
        };

        await ExecuteQueryAsync("orders", query);
    }

    private static async Task ExecuteQueryAsync(string dataBlockId, object query)
    {
        var json = JsonSerializer.Serialize(query, _jsonOptions);
        Console.WriteLine($"Query: {json}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"/api/data/datablocks/{dataBlockId}/query", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Result: {result}");
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine(error);
        }

        Console.WriteLine();
    }
}
