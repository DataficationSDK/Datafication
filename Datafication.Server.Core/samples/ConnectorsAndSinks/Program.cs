using Datafication.Core.Data;
using Datafication.Core.Sinks;
using Datafication.Sinks.Connectors.CsvConnector;
using Datafication.Server.Core.Models;
using Datafication.Server.Core.Registry;
using Datafication.Server.Core.Extensions;
using ConnectorsAndSinks;

var builder = WebApplication.CreateBuilder(args);

// 1. Add anonymous authentication
builder.AddAnonymousAuthentication();

// 2. Add Datafication Server with sinks registered
builder.Services.AddDataficationServer(
    options =>
    {
        options.AllowAnonymousAccess = true;
        options.RoutePrefix = "api/data";

        // Register sinks for output formats
        // Note: "json" format has a built-in fallback, but you can register a custom JSON sink
        options.RegisterSink<HtmlTableSink>("html");
        options.RegisterSink<CsvStringSink>("csv");
        options.RegisterSink<MarkdownTableSink>("markdown");
    },
    authOptions =>
    {
        authOptions.AddPolicy("DataBlockAccess", policy => policy.RequireAssertion(_ => true));
        authOptions.AddPolicy("DataBlockAdmin", policy => policy.RequireAssertion(_ => true));
    });

var app = builder.Build();

// 3. Register connectors and sinks, then load data
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<IDataBlockRegistry>();
    var connectorRegistry = scope.ServiceProvider.GetService<IConnectorRegistry>();
    var sinkRegistry = scope.ServiceProvider.GetService<ISinkRegistry>();

    // Register connectors (if connector registry is available)
    if (connectorRegistry != null)
    {
        Console.WriteLine("Connector Registry available");

        // Check for registered connector types
        var connectorTypes = connectorRegistry.GetRegisteredConnectorTypes();
        Console.WriteLine($"Available connectors: {string.Join(", ", connectorTypes)}");
    }

    // Register sinks (if sink registry is available)
    if (sinkRegistry != null)
    {
        Console.WriteLine("Sink Registry available");

        // Check for registered sink types
        var sinkKeys = sinkRegistry.GetRegisteredSinkKeys();
        Console.WriteLine($"Available sinks: {string.Join(", ", sinkKeys)}");
    }

    // Load CSV data manually (demonstrating how data would be loaded)
    var products = DataLoaders.LoadProductsFromCsv();
    registry.RegisterDataBlock("products", products, new DataBlockMetadata
    {
        Name = "Products",
        Description = "Product catalog loaded from CSV",
        Tags = new[] { "products", "csv", "catalog" },
        RegisteredAt = DateTime.UtcNow
    });
    Console.WriteLine($"Registered 'products' with {products.RowCount} rows from CSV");

    // Load JSON data manually (demonstrating how data would be loaded)
    var orders = DataLoaders.LoadOrdersFromJson();
    registry.RegisterDataBlock("orders", orders, new DataBlockMetadata
    {
        Name = "Orders",
        Description = "Order data loaded from JSON",
        Tags = new[] { "orders", "json" },
        RegisteredAt = DateTime.UtcNow
    });
    Console.WriteLine($"Registered 'orders' with {orders.RowCount} rows from JSON");

    // Create a sample DataBlock for export demonstrations
    var summary = new DataBlock();
    summary.AddColumn(new DataColumn("Category", typeof(string)));
    summary.AddColumn(new DataColumn("ProductCount", typeof(int)));
    summary.AddColumn(new DataColumn("TotalValue", typeof(decimal)));

    summary.AddRow(new object[] { "Electronics", 6, 1984.94m });
    summary.AddRow(new object[] { "Furniture", 4, 1199.96m });

    registry.RegisterDataBlock("summary", summary, new DataBlockMetadata
    {
        Name = "Category Summary",
        Description = "Category summary for export demo",
        Tags = new[] { "summary", "export" },
        RegisteredAt = DateTime.UtcNow
    });
    Console.WriteLine("Registered 'summary' for export demonstrations");
}

// 4. Configure the HTTP pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine();
Console.WriteLine("ConnectorsAndSinks Server is running at http://localhost:5000");
Console.WriteLine();
Console.WriteLine("Output Format Examples:");
Console.WriteLine("  GET /api/data/datablocks/products                   - JSON (default)");
Console.WriteLine("  GET /api/data/datablocks/products?format=csv        - CSV format");
Console.WriteLine("  GET /api/data/datablocks/products?format=html       - HTML table");
Console.WriteLine("  GET /api/data/datablocks/products?format=markdown   - Markdown table");
Console.WriteLine();
Console.WriteLine("List registered sinks:");
Console.WriteLine("  GET /api/data/sinks");
Console.WriteLine();

app.Run();
