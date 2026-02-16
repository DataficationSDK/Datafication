# BasicServer Sample

This sample demonstrates the simplest way to set up a Datafication.Server.Core REST API server. It's the best starting point for new users.

## Overview

- Minimal server configuration with anonymous access
- Register a single DataBlock with sample employee data
- Expose REST API endpoints for data retrieval

## Key Features Demonstrated

### 1. Server Setup

```csharp
builder.AddAnonymousAuthentication();
builder.Services.AddDataficationServer(
    options => options.AllowAnonymousAccess = true,
    authOptions => authOptions.AddPolicy("DataBlockAccess", policy => policy.RequireAssertion(_ => true)));
```

### 2. DataBlock Registration

```csharp
var registry = scope.ServiceProvider.GetRequiredService<IDataBlockRegistry>();
var employees = new DataBlock();
employees.AddColumn(new DataColumn("Id", typeof(int)));
employees.AddColumn(new DataColumn("Name", typeof(string)));
// ... add rows
registry.RegisterDataBlock("employees", employees, metadata);
```

## How to Run

```bash
cd BasicServer
dotnet restore
dotnet run
```

The server will start at `http://localhost:5000`.

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/data/datablocks` | List all registered DataBlocks |
| GET | `/api/data/datablocks/{id}` | Get data from a specific DataBlock |
| GET | `/api/data/datablocks/{id}/schema` | Get the schema of a DataBlock |

## curl Examples

### List all DataBlocks

```bash
curl http://localhost:5000/api/data/datablocks
```

### Get employee data

```bash
curl http://localhost:5000/api/data/datablocks/employees
```

### Get schema

```bash
curl http://localhost:5000/api/data/datablocks/employees/schema
```

## C# Client Example

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// List all DataBlocks
var datablocks = await client.GetFromJsonAsync<string[]>("/api/data/datablocks");
Console.WriteLine($"Available DataBlocks: {string.Join(", ", datablocks)}");

// Get employee data
var response = await client.GetAsync("/api/data/datablocks/employees");
var json = await response.Content.ReadAsStringAsync();
Console.WriteLine(json);
```

## Expected Output

When you run the server and call `GET /api/data/employees`, you'll receive:

```json
{
  "columns": ["Id", "Name", "Department", "Salary"],
  "rows": [
    [1, "Alice Johnson", "Engineering", 95000],
    [2, "Bob Smith", "Marketing", 75000],
    [3, "Carol Williams", "Engineering", 105000],
    [4, "David Brown", "Sales", 65000],
    [5, "Eva Martinez", "Engineering", 88000]
  ]
}
```

## Related Samples

- **QueryOperations** - Learn how to filter, sort, and aggregate data
- **RowOperations** - Learn how to add, update, and delete rows
- **AuthenticatedServer** - Learn how to add JWT authentication
