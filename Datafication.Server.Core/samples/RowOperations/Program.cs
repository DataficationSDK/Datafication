using Datafication.Core.Data;
using Datafication.Server.Core.Models;
using Datafication.Server.Core.Registry;
using Datafication.Server.Core.Extensions;
using RowOperations;

var builder = WebApplication.CreateBuilder(args);

// 1. Add anonymous authentication
builder.AddAnonymousAuthentication();

// 2. Add Datafication Server with admin access enabled for row operations
builder.Services.AddDataficationServer(
    options =>
    {
        options.AllowAnonymousAccess = true;
        options.RoutePrefix = "api/data";
    },
    authOptions =>
    {
        // Read access for queries
        authOptions.AddPolicy("DataBlockAccess", policy => policy.RequireAssertion(_ => true));
        // Write access for row operations (add, update, delete)
        authOptions.AddPolicy("DataBlockAdmin", policy => policy.RequireAssertion(_ => true));
    });

var app = builder.Build();

// 3. Register a sample DataBlock for demonstrating row operations
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<IDataBlockRegistry>();

    var tasks = new DataBlock();
    tasks.AddColumn(new DataColumn("Id", typeof(int)));
    tasks.AddColumn(new DataColumn("Title", typeof(string)));
    tasks.AddColumn(new DataColumn("Status", typeof(string)));
    tasks.AddColumn(new DataColumn("Priority", typeof(int)));

    // Initial sample data
    tasks.AddRow(new object[] { 1, "Setup project", "Completed", 1 });
    tasks.AddRow(new object[] { 2, "Write documentation", "In Progress", 2 });
    tasks.AddRow(new object[] { 3, "Add unit tests", "Pending", 3 });

    var metadata = new DataBlockMetadata
    {
        Name = "Task List",
        Description = "Sample task list for demonstrating row CRUD operations",
        Tags = new[] { "sample", "tasks", "crud" },
        RegisteredAt = DateTime.UtcNow
    };

    registry.RegisterDataBlock("tasks", tasks, metadata);

    Console.WriteLine("Registered 'tasks' DataBlock with 3 rows");
}

// 4. Configure the HTTP pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine();
Console.WriteLine("RowOperations Server is running at http://localhost:5000");
Console.WriteLine();
Console.WriteLine("Row Operation Endpoints:");
Console.WriteLine("  POST   /api/data/datablocks/tasks/rows     - Add row (append)");
Console.WriteLine("  POST   /api/data/datablocks/tasks/rows/{i} - Insert row at index");
Console.WriteLine("  PUT    /api/data/datablocks/tasks/rows/{i} - Update row at index");
Console.WriteLine("  DELETE /api/data/datablocks/tasks/rows/{i} - Delete row at index");
Console.WriteLine("  GET    /api/data/datablocks/tasks          - View current data");
Console.WriteLine();

app.Run();
