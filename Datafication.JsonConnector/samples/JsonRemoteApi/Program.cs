using Datafication.Core.Data;
using Datafication.Connectors.JsonConnector;
using Datafication.Extensions.Connectors.JsonConnector;

Console.WriteLine("=== Datafication.JsonConnector Remote API Sample ===\n");

// 1. Load from public REST API using simple path
Console.WriteLine("1. Loading from JSONPlaceholder API (users)...");
try
{
    var users = await DataBlock.Connector.LoadJsonAsync("https://jsonplaceholder.typicode.com/users");
    Console.WriteLine($"   Loaded {users.RowCount} users");

    // Display schema
    Console.WriteLine("   Columns: " + string.Join(", ", users.Schema.GetColumnNames().Take(5)) + "...\n");

    // Display first 3 users
    Console.WriteLine("   First 3 users:");
    var cursor = users.GetRowCursor("id", "name", "email");
    int count = 0;
    while (cursor.MoveNext() && count < 3)
    {
        Console.WriteLine($"   - {cursor.GetValue("name")} ({cursor.GetValue("email")})");
        count++;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}
Console.WriteLine();

// 2. Load posts and filter
Console.WriteLine("2. Loading posts and filtering by userId...");
try
{
    var posts = await DataBlock.Connector.LoadJsonAsync("https://jsonplaceholder.typicode.com/posts");
    Console.WriteLine($"   Loaded {posts.RowCount} posts");

    // Filter posts by user
    var user1Posts = posts.Where("userId", (long)1);
    Console.WriteLine($"   User 1 has {user1Posts.RowCount} posts");

    // Show first post title
    var postCursor = user1Posts.GetRowCursor("title");
    if (postCursor.MoveNext())
    {
        var title = postCursor.GetValue("title")?.ToString() ?? "";
        Console.WriteLine($"   First post: \"{(title.Length > 50 ? title.Substring(0, 50) + "..." : title)}\"");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}
Console.WriteLine();

// 3. Using JsonConnectorConfiguration with HTTPS URL
Console.WriteLine("3. Using Configuration with HTTPS URL...");
try
{
    var config = new JsonConnectorConfiguration
    {
        Id = "remote-api-demo",
        Source = new Uri("https://jsonplaceholder.typicode.com/todos"),
        ErrorHandler = (ex) => Console.WriteLine($"   [API Error] {ex.Message}")
    };

    var connector = new JsonDataConnector(config);
    var todos = await connector.GetDataAsync();

    Console.WriteLine($"   Loaded {todos.RowCount} todos");
    Console.WriteLine($"   Connector ID: {connector.GetConnectorId()}");

    // Count completed vs incomplete
    var completed = todos.Where("completed", true);
    Console.WriteLine($"   Completed: {completed.RowCount}, Incomplete: {todos.RowCount - completed.RowCount}");
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}
Console.WriteLine();

// 4. Load comments and aggregate by post
Console.WriteLine("4. Loading comments and analyzing...");
try
{
    var comments = await DataBlock.Connector.LoadJsonAsync("https://jsonplaceholder.typicode.com/comments");
    Console.WriteLine($"   Loaded {comments.RowCount} comments");

    // Count comments per post using GroupByAggregate
    var commentsPerPost = comments.GroupByAggregate("postId", "id", AggregationType.Count, "CommentCount");
    Console.WriteLine($"   Comments span {commentsPerPost.RowCount} unique posts");

    // Show sample comment
    var commentCursor = comments.GetRowCursor("name", "email");
    if (commentCursor.MoveNext())
    {
        Console.WriteLine($"   Sample comment by: {commentCursor.GetValue("email")}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}
Console.WriteLine();

// 5. Load albums and sort
Console.WriteLine("5. Loading albums and sorting...");
try
{
    var albums = await DataBlock.Connector.LoadJsonAsync("https://jsonplaceholder.typicode.com/albums");
    Console.WriteLine($"   Loaded {albums.RowCount} albums");

    // Sort by title and get first 5
    var sortedAlbums = albums
        .Sort(SortDirection.Ascending, "title")
        .Head(5);

    Console.WriteLine("   First 5 albums (alphabetically):");
    var albumCursor = sortedAlbums.GetRowCursor("id", "title");
    while (albumCursor.MoveNext())
    {
        var title = albumCursor.GetValue("title")?.ToString() ?? "";
        Console.WriteLine($"   - [{albumCursor.GetValue("id")}] {(title.Length > 40 ? title.Substring(0, 40) + "..." : title)}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"   Error: {ex.Message}");
}
Console.WriteLine();

// 6. HTTP error handling demonstration
Console.WriteLine("6. HTTP Error Handling...");
try
{
    // This endpoint returns 404
    var data = await DataBlock.Connector.LoadJsonAsync("https://jsonplaceholder.typicode.com/nonexistent");
    Console.WriteLine($"   Loaded: {data.RowCount} rows");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"   HttpRequestException caught: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"   Exception: {ex.GetType().Name} - {ex.Message}");
}

Console.WriteLine("\n=== Sample Complete ===");
