using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AuthenticatedServer;

/// <summary>
/// Example C# client demonstrating JWT authentication.
/// Run this client while the AuthenticatedServer is running.
/// </summary>
public static class AuthClient
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
    /// Run authentication examples.
    /// </summary>
    public static async Task RunExamplesAsync()
    {
        Console.WriteLine("=== AuthenticatedServer Client Examples ===\n");

        // 1. Try without authentication (should fail)
        await TryUnauthenticatedAccessAsync();

        // 2. Get token as reader
        var readerToken = await GetTokenAsync("reader", "reader123");
        if (readerToken != null)
        {
            Console.WriteLine("--- Reader Access ---");
            await AccessWithTokenAsync(readerToken, "read");
            await TryWriteWithTokenAsync(readerToken); // Should fail
        }

        // 3. Get token as admin
        var adminToken = await GetTokenAsync("admin", "admin123");
        if (adminToken != null)
        {
            Console.WriteLine("--- Admin Access ---");
            await AccessWithTokenAsync(adminToken, "admin");
            await TryWriteWithTokenAsync(adminToken); // Should succeed
        }
    }

    /// <summary>
    /// Try to access API without authentication.
    /// </summary>
    public static async Task TryUnauthenticatedAccessAsync()
    {
        Console.WriteLine("--- Unauthenticated Access ---");

        var response = await _client.GetAsync("/api/data/datablocks/employees");
        Console.WriteLine($"GET /api/data/datablocks/employees (no token)");
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine();
    }

    /// <summary>
    /// Get a JWT token from the auth endpoint.
    /// </summary>
    public static async Task<string?> GetTokenAsync(string username, string password)
    {
        Console.WriteLine($"--- Getting Token for '{username}' ---");

        var request = new { username, password };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/auth/token", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Console.WriteLine($"Token received. Role: {result?.Role}, Expires in: {result?.ExpiresIn}s");
            Console.WriteLine($"Token: {result?.Token?[..50]}...");
            Console.WriteLine();
            return result?.Token;
        }
        else
        {
            Console.WriteLine($"Failed to get token: {response.StatusCode}");
            Console.WriteLine();
            return null;
        }
    }

    /// <summary>
    /// Access the API with a token.
    /// </summary>
    public static async Task AccessWithTokenAsync(string token, string role)
    {
        Console.WriteLine($"Accessing API with {role} token...");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/data/datablocks/employees");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Console.WriteLine($"GET /api/data/datablocks/employees");
        Console.WriteLine($"Status: {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {content[..Math.Min(200, content.Length)]}...");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Try to write data (requires admin role).
    /// </summary>
    public static async Task TryWriteWithTokenAsync(string token)
    {
        Console.WriteLine("Attempting write operation...");

        var addRequest = new
        {
            values = new object[] { 4, "New Employee", "Sales", 60000 }
        };

        var json = JsonSerializer.Serialize(addRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/data/datablocks/employees/rows")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Console.WriteLine($"POST /api/data/datablocks/employees/rows");
        Console.WriteLine($"Status: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {error}");
        }
        Console.WriteLine();
    }

    private record TokenResponse(string Token, int ExpiresIn, string Role);
}
