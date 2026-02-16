# JsonRemoteApi Sample

Demonstrates loading JSON data from HTTP/HTTPS URLs (REST APIs) using the Datafication.JsonConnector library.

## Overview

This sample shows how to:
- Load JSON data directly from REST API endpoints
- Use `JsonConnectorConfiguration` with HTTPS URLs
- Process and filter remote API responses
- Handle HTTP errors gracefully
- Transform API data using DataBlock operations

## Key Features Demonstrated

### Simple URL Loading

```csharp
var users = await DataBlock.Connector.LoadJsonAsync("https://api.example.com/users");
Console.WriteLine($"Loaded {users.RowCount} users");
```

### Configuration with HTTPS URL

```csharp
var config = new JsonConnectorConfiguration
{
    Id = "api-connector",
    Source = new Uri("https://api.example.com/data"),
    ErrorHandler = (ex) => Logger.Error($"API error: {ex.Message}")
};

var connector = new JsonDataConnector(config);
var data = await connector.GetDataAsync();
```

### Processing API Responses

```csharp
// Load from API
var posts = await DataBlock.Connector.LoadJsonAsync("https://api.example.com/posts");

// Filter by field
var userPosts = posts.Where("userId", 1);

// Sort results
var sortedPosts = posts.Sort(SortDirection.Descending, "createdAt");

// Select specific columns
var summary = posts.Select("id", "title", "author");
```

### HTTP Error Handling

```csharp
try
{
    var data = await DataBlock.Connector.LoadJsonAsync("https://api.example.com/data");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP error: {ex.Message}");
    // Handle 404, 500, network errors, etc.
}
```

## Public APIs Used

This sample uses [JSONPlaceholder](https://jsonplaceholder.typicode.com/), a free fake API for testing:

| Endpoint | Description | Records |
|----------|-------------|---------|
| `/users` | User profiles | 10 |
| `/posts` | Blog posts | 100 |
| `/comments` | Post comments | 500 |
| `/todos` | Todo items | 200 |
| `/albums` | Photo albums | 100 |

## How to Run

```bash
cd JsonRemoteApi
dotnet restore
dotnet run
```

**Note**: This sample requires an internet connection to access the public API.

## Expected Output

```
=== Datafication.JsonConnector Remote API Sample ===

1. Loading from JSONPlaceholder API (users)...
   Loaded 10 users
   Columns: id, name, username, email, address...
   First 3 users:
   - Leanne Graham (Sincere@april.biz)
   - Ervin Howell (Shanna@melissa.tv)
   - Clementine Bauch (Nathan@yesenia.net)

2. Loading posts and filtering by userId...
   Loaded 100 posts
   User 1 has 10 posts
   First post: "sunt aut facere repellat provident..."

3. Using Configuration with HTTPS URL...
   Loaded 200 todos
   Connector ID: remote-api-demo
   Completed: 90, Incomplete: 110

4. Loading comments and analyzing...
   Loaded 500 comments
   Comments span 100 unique posts
   Sample comment by: [email]

5. Loading albums and sorting...
   Loaded 100 albums
   First 5 albums (alphabetically):
   [sorted album list]

6. HTTP Error Handling...
   HttpRequestException caught: [error message]

=== Sample Complete ===
```

## Supported URL Schemes

| Scheme | Example |
|--------|---------|
| `https://` | `https://api.example.com/data.json` |
| `http://` | `http://localhost:8080/data.json` |
| `file://` | `file:///path/to/local/file.json` |

## Related Samples

- **JsonBasicLoad** - Loading from local files
- **JsonErrorHandling** - Comprehensive error handling patterns
- **JsonETLPipeline** - Combining remote and local data sources
