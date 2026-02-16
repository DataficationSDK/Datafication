using Datafication.Core.Data;
using Datafication.Server.Core.Models;
using Datafication.Server.Core.Registry;
using Datafication.Server.Core.Extensions;
using AdvancedQueries;

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

// 3. Register multiple DataBlocks for demonstrating advanced queries
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<IDataBlockRegistry>();

    // Stock prices - good for window functions
    // Note: Using string for Date and double for Price to avoid package bug with DateTime+decimal combo
    var stocks = new DataBlock();
    stocks.AddColumn(new DataColumn("Symbol", typeof(string)));
    stocks.AddColumn(new DataColumn("Date", typeof(string)));
    stocks.AddColumn(new DataColumn("Price", typeof(double)));
    stocks.AddColumn(new DataColumn("Volume", typeof(int)));

    stocks.AddRow(new object[] { "TECH", "2024-01-01", 150.00, 10000 });
    stocks.AddRow(new object[] { "TECH", "2024-01-02", 152.50, 12000 });
    stocks.AddRow(new object[] { "TECH", "2024-01-03", 148.00, 15000 });
    stocks.AddRow(new object[] { "TECH", "2024-01-04", 155.00, 11000 });
    stocks.AddRow(new object[] { "TECH", "2024-01-05", 160.00, 18000 });
    stocks.AddRow(new object[] { "BANK", "2024-01-01", 45.00, 8000 });
    stocks.AddRow(new object[] { "BANK", "2024-01-02", 46.50, 9000 });
    stocks.AddRow(new object[] { "BANK", "2024-01-03", 44.00, 7500 });
    stocks.AddRow(new object[] { "BANK", "2024-01-04", 47.00, 10000 });
    stocks.AddRow(new object[] { "BANK", "2024-01-05", 48.50, 12000 });

    registry.RegisterDataBlock("stocks", stocks, new DataBlockMetadata
    {
        Name = "Stock Prices",
        Description = "Daily stock prices for window function demos",
        Tags = new[] { "stocks", "timeseries", "window" },
        RegisteredAt = DateTime.UtcNow
    });

    // Orders - for join demos
    var orders = new DataBlock();
    orders.AddColumn(new DataColumn("OrderId", typeof(int)));
    orders.AddColumn(new DataColumn("CustomerId", typeof(int)));
    orders.AddColumn(new DataColumn("ProductId", typeof(int)));
    orders.AddColumn(new DataColumn("Quantity", typeof(int)));
    orders.AddColumn(new DataColumn("OrderDate", typeof(DateTime)));

    orders.AddRow(new object[] { 1, 101, 1, 2, new DateTime(2024, 1, 15) });
    orders.AddRow(new object[] { 2, 102, 2, 1, new DateTime(2024, 1, 16) });
    orders.AddRow(new object[] { 3, 101, 3, 3, new DateTime(2024, 1, 17) });
    orders.AddRow(new object[] { 4, 103, 1, 1, new DateTime(2024, 1, 18) });
    orders.AddRow(new object[] { 5, 102, 2, 2, new DateTime(2024, 1, 19) });

    registry.RegisterDataBlock("orders", orders, new DataBlockMetadata
    {
        Name = "Orders",
        Description = "Customer orders for join demos",
        Tags = new[] { "orders", "join" },
        RegisteredAt = DateTime.UtcNow
    });

    // Customers - for join demos
    var customers = new DataBlock();
    customers.AddColumn(new DataColumn("CustomerId", typeof(int)));
    customers.AddColumn(new DataColumn("Name", typeof(string)));
    customers.AddColumn(new DataColumn("City", typeof(string)));

    customers.AddRow(new object[] { 101, "Alice Johnson", "New York" });
    customers.AddRow(new object[] { 102, "Bob Smith", "Los Angeles" });
    customers.AddRow(new object[] { 103, "Carol Williams", "Chicago" });
    customers.AddRow(new object[] { 104, "David Brown", "Houston" });

    registry.RegisterDataBlock("customers", customers, new DataBlockMetadata
    {
        Name = "Customers",
        Description = "Customer information for join demos",
        Tags = new[] { "customers", "join" },
        RegisteredAt = DateTime.UtcNow
    });

    // Products - for join demos
    var products = new DataBlock();
    products.AddColumn(new DataColumn("ProductId", typeof(int)));
    products.AddColumn(new DataColumn("ProductName", typeof(string)));
    products.AddColumn(new DataColumn("Price", typeof(decimal)));

    products.AddRow(new object[] { 1, "Laptop", 999.99m });
    products.AddRow(new object[] { 2, "Mouse", 29.99m });
    products.AddRow(new object[] { 3, "Keyboard", 79.99m });

    registry.RegisterDataBlock("products", products, new DataBlockMetadata
    {
        Name = "Products",
        Description = "Product catalog for join demos",
        Tags = new[] { "products", "join" },
        RegisteredAt = DateTime.UtcNow
    });

    Console.WriteLine("Registered DataBlocks: stocks, orders, customers, products");
}

// 4. Configure the HTTP pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine();
Console.WriteLine("AdvancedQueries Server is running at http://localhost:5000");
Console.WriteLine();
Console.WriteLine("Advanced Query Examples:");
Console.WriteLine();
Console.WriteLine("1. Window Functions - Moving average:");
Console.WriteLine(@"   POST /api/data/datablocks/stocks/query");
Console.WriteLine(@"   { ""window"": [{ ""column"": ""Price"", ""function"": ""movingavg"", ""windowSize"": 3, ""resultColumn"": ""MovingAvg"" }] }");
Console.WriteLine();
Console.WriteLine("2. Computed Columns:");
Console.WriteLine(@"   POST /api/data/datablocks/stocks/query");
Console.WriteLine(@"   { ""compute"": [{ ""resultColumn"": ""TotalValue"", ""expression"": ""[Price] * [Volume]"" }] }");
Console.WriteLine();
Console.WriteLine("3. Join/Merge:");
Console.WriteLine(@"   POST /api/data/datablocks/orders/query");
Console.WriteLine(@"   { ""merge"": { ""otherDataBlockId"": ""customers"", ""keyColumn"": ""CustomerId"", ""otherKeyColumn"": ""CustomerId"", ""mode"": ""left"" } }");
Console.WriteLine();

app.Run();
