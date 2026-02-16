using Datafication.Core.Data;
using Datafication.Server.Core.Models;
using Datafication.Server.Core.Registry;
using Datafication.Server.Core.Extensions;
using BasicServer;

var builder = WebApplication.CreateBuilder(args);

// 1. Add anonymous authentication (simplest setup for getting started)
builder.AddAnonymousAuthentication();

// 2. Add Datafication Server with anonymous access enabled
builder.Services.AddDataficationServer(
    options =>
    {
        options.AllowAnonymousAccess = true;
        options.RoutePrefix = "api/data";
    },
    authOptions =>
    {
        // Allow all requests through for this basic sample
        authOptions.AddPolicy("DataBlockAccess", policy => policy.RequireAssertion(_ => true));
    });

var app = builder.Build();

// 3. Register a sample DataBlock during app startup
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<IDataBlockRegistry>();

    // Create a simple employee DataBlock
    var employees = new DataBlock();
    employees.AddColumn(new DataColumn("Id", typeof(int)));
    employees.AddColumn(new DataColumn("Name", typeof(string)));
    employees.AddColumn(new DataColumn("Department", typeof(string)));
    employees.AddColumn(new DataColumn("Salary", typeof(decimal)));

    // Add sample data
    employees.AddRow(new object[] { 1, "Alice Johnson", "Engineering", 95000m });
    employees.AddRow(new object[] { 2, "Bob Smith", "Marketing", 75000m });
    employees.AddRow(new object[] { 3, "Carol Williams", "Engineering", 105000m });
    employees.AddRow(new object[] { 4, "David Brown", "Sales", 65000m });
    employees.AddRow(new object[] { 5, "Eva Martinez", "Engineering", 88000m });

    var metadata = new DataBlockMetadata
    {
        Name = "Employees",
        Description = "Sample employee directory",
        Tags = new[] { "sample", "employees", "hr" },
        RegisteredAt = DateTime.UtcNow
    };

    registry.RegisterDataBlock("employees", employees, metadata);

    Console.WriteLine("Registered 'employees' DataBlock with 5 rows");
}

// 4. Configure the HTTP pipeline
app.UseAuthentication();
app.UseAuthorization();

// 5. Map controller endpoints (Datafication.Server.Core provides the REST API)
app.MapControllers();

Console.WriteLine("BasicServer is running at http://localhost:5000");
Console.WriteLine("Try these endpoints:");
Console.WriteLine("  GET  http://localhost:5000/api/data/datablocks           - List all DataBlocks");
Console.WriteLine("  GET  http://localhost:5000/api/data/datablocks/employees - Get employee data");
Console.WriteLine();

app.Run();
