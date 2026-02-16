using Datafication.Core.Data;
using Datafication.Server.Core.Models;
using Datafication.Server.Core.Registry;
using Datafication.Server.Core.Extensions;
using QueryOperations;

var builder = WebApplication.CreateBuilder(args);

// 1. Add anonymous authentication
builder.AddAnonymousAuthentication();

// 2. Add Datafication Server
builder.Services.AddDataficationServer(
    options =>
    {
        options.AllowAnonymousAccess = true;
        options.RoutePrefix = "api/data";
    },
    authOptions =>
    {
        authOptions.AddPolicy("DataBlockAccess", policy => policy.RequireAssertion(_ => true));
    });

var app = builder.Build();

// 3. Register a sample DataBlock with sales data (good for demonstrating queries)
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<IDataBlockRegistry>();

    var sales = new DataBlock();
    sales.AddColumn(new DataColumn("Id", typeof(int)));
    sales.AddColumn(new DataColumn("Product", typeof(string)));
    sales.AddColumn(new DataColumn("Category", typeof(string)));
    sales.AddColumn(new DataColumn("Region", typeof(string)));
    sales.AddColumn(new DataColumn("Quantity", typeof(int)));
    sales.AddColumn(new DataColumn("UnitPrice", typeof(decimal)));
    sales.AddColumn(new DataColumn("Total", typeof(decimal)));

    // Sample sales data
    sales.AddRow(new object[] { 1, "Laptop", "Electronics", "North", 5, 999.99m, 4999.95m });
    sales.AddRow(new object[] { 2, "Mouse", "Electronics", "South", 50, 29.99m, 1499.50m });
    sales.AddRow(new object[] { 3, "Desk", "Furniture", "North", 10, 299.99m, 2999.90m });
    sales.AddRow(new object[] { 4, "Chair", "Furniture", "East", 25, 199.99m, 4999.75m });
    sales.AddRow(new object[] { 5, "Monitor", "Electronics", "West", 15, 449.99m, 6749.85m });
    sales.AddRow(new object[] { 6, "Keyboard", "Electronics", "North", 30, 79.99m, 2399.70m });
    sales.AddRow(new object[] { 7, "Bookshelf", "Furniture", "South", 8, 149.99m, 1199.92m });
    sales.AddRow(new object[] { 8, "Headphones", "Electronics", "East", 40, 149.99m, 5999.60m });
    sales.AddRow(new object[] { 9, "Lamp", "Furniture", "West", 20, 49.99m, 999.80m });
    sales.AddRow(new object[] { 10, "Tablet", "Electronics", "North", 12, 599.99m, 7199.88m });

    var metadata = new DataBlockMetadata
    {
        Name = "Sales Data",
        Description = "Sample sales transactions for demonstrating query operations",
        Tags = new[] { "sample", "sales", "queries" },
        RegisteredAt = DateTime.UtcNow
    };

    registry.RegisterDataBlock("sales", sales, metadata);

    Console.WriteLine("Registered 'sales' DataBlock with 10 rows");
}

// 4. Configure the HTTP pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine();
Console.WriteLine("QueryOperations Server is running at http://localhost:5000");
Console.WriteLine();
Console.WriteLine("Query Examples (use POST /api/data/datablocks/sales/query with JSON body):");
Console.WriteLine();
Console.WriteLine("1. Filter by category:");
Console.WriteLine(@"   { ""where"": [{ ""column"": ""Category"", ""operator"": ""eq"", ""value"": ""Electronics"" }] }");
Console.WriteLine();
Console.WriteLine("2. Sort by total descending:");
Console.WriteLine(@"   { ""sort"": { ""column"": ""Total"", ""direction"": ""desc"" } }");
Console.WriteLine();
Console.WriteLine("3. GroupBy category with aggregations:");
Console.WriteLine(@"   { ""groupBy"": { ""column"": ""Category"", ""aggregations"": { ""Total"": ""sum"" } } }");
Console.WriteLine();

app.Run();
