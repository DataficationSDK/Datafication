# Datafication.Server.Core

[![NuGet](https://img.shields.io/nuget/v/Datafication.Server.Core.svg)](https://www.nuget.org/packages/Datafication.Server.Core)

A plug-and-play REST API library for .NET that enables instant HTTP access to DataBlock instances with features including authentication, caching, analytics, and row-level operations.

## Description

Datafication.Server.Core transforms your ASP.NET Core application into a powerful DataBlock server with minimal configuration. The library provides a complete REST API layer for exposing DataBlock data, managing registries, and performing real-time data operations. Built on ASP.NET Core 8, it features a meta-architecture where DataBlocks manage DataBlock registrations, showcasing the flexibility of the Datafication platform while delivering features like JWT authentication, memory caching, background maintenance, and comprehensive analytics.

### Key Features

- **Instant REST API**: Add DataBlock HTTP endpoints with one line of code
- **Comprehensive Query Engine**: Full pandas-like query capabilities via REST - filtering, sorting, aggregation, grouping, window functions, joins, computed columns, and data transformations
- **Meta Registry Architecture**: DataBlocks managing DataBlock registrations - eating our own dog food
- **JWT Authentication**: Built-in support for secure Bearer token authentication with customizable policies
- **Caching**: Integrated memory cache support for high-performance data access
- **Row-Level Operations**: Full CRUD operations at the row level within DataBlocks
- **Registry Analytics**: Comprehensive insights, usage patterns, and optimization recommendations
- **Multiple Output Formats**: JSON (built-in), plus HTML, CSV, Markdown via registered sinks
- **Flexible Configuration**: Development, production, and enterprise presets
- **Connector Registry**: Dynamic registration and management of data connectors
- **Sink Registry**: Support for multiple output sink types (PDF, Excel, etc.)
- **Background Maintenance**: Automated cleanup, optimization, and health monitoring
- **Export/Import**: Serialize and restore complete registry state

## Table of Contents

- [Description](#description)
  - [Key Features](#key-features)
- [Installation](#installation)
- [Usage Examples](#usage-examples)
  - [Quick Start](#quick-start)
  - [Development Setup](#development-setup)
  - [Production Setup](#production-setup)
  - [Custom Configuration](#custom-configuration)
  - [Registering DataBlocks](#registering-datablocks)
  - [Authentication Configuration](#authentication-configuration)
  - [Using the REST API](#using-the-rest-api)
  - [Row Operations](#row-operations)
  - [Comprehensive DataBlock Querying](#comprehensive-datablock-querying)
  - [Registry Analytics](#registry-analytics)
  - [Advanced Querying](#advanced-querying)
  - [Connector and Sink Registration](#connector-and-sink-registration)
  - [Export and Import Registry](#export-and-import-registry)
  - [Cache Management](#cache-management)
- [Configuration Reference](#configuration-reference)
  - [DataBlockServerOptions](#datablockerserveroptions)
  - [MetaRegistryOptions](#metaregistryoptions)
  - [Configuration Presets](#configuration-presets)
- [API Reference](#api-reference)
  - [Core Classes](#core-classes)
  - [Extension Methods](#extension-methods)
  - [REST API Endpoints](#rest-api-endpoints)
- [Common Patterns](#common-patterns)
  - [Simple Read-Only Data API](#simple-read-only-data-api)
  - [Full CRUD API with Authentication](#full-crud-api-with-authentication)
  - [Enterprise Data Lake API](#enterprise-data-lake-api)
- [Performance Tips](#performance-tips)
- [License](#license)

## Installation

> **Note**: Datafication.Server.Core is currently in pre-release. The packages are now available on nuget.org.

```bash
dotnet add package Datafication.Server.Core
```

**Running the Samples:**

```bash
cd samples/DataBlockServer
dotnet run
```

**Dependencies:**
- Datafication.Core (included)
- Datafication.MemoryCache (included)
- ASP.NET Core 8.0 or higher

## Usage Examples

### Quick Start

The simplest way to add DataBlock server capabilities to your ASP.NET Core application:

```csharp
using Datafication.Server.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add DataBlock server with default configuration
builder.Services.AddDataficationServer();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

This provides REST API endpoints at `/api/data/*` for all DataBlock operations.

### Development Setup

For development environments with detailed logging and frequent maintenance:

```csharp
using Datafication.Server.Core.Extensions;
using Datafication.Server.Core.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Development-optimized with debugging features
builder.Services.AddDataBlockRegistry(RegistryPreset.Development);
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Production Setup

For production environments with enterprise features and optimized settings:

```csharp
using Datafication.Server.Core.Extensions;
using Datafication.Server.Core.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Enterprise-ready with all features and optimized caching
builder.Services.AddDataBlockRegistry(RegistryPreset.Enterprise);
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Custom Configuration

Full control over server behavior with custom options:

```csharp
using Datafication.Server.Core.Extensions;
using Datafication.Core.Sinks;
using Datafication.Sinks.Connectors.CsvConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataficationServer(options =>
{
    // API Configuration
    options.RoutePrefix = "myapp/data";  // Custom API prefix
    options.MaxRowsPerRequest = 5000;    // Row limit per request
    options.MaxRegisteredDataBlocks = 500;
    
    // Security
    options.AllowAnonymousAccess = false;
    options.DefaultAccessPolicy = "DataBlockAccess";
    options.IncludeDetailedErrors = false;  // Hide details in production
    
    // Caching
    options.EnableCaching = true;
    options.CacheDurationSeconds = 600;  // 10 minutes

    // Register sinks for output formats
    // Note: "json" format has a built-in fallback, so a JSON sink is optional
    options.RegisterSink<HtmlTableSink>("html");
    options.RegisterSink<CsvStringSink>("csv");
    options.RegisterSink<MarkdownTableSink>("markdown");

    // Access Policies
    options.AccessPolicies = new Dictionary<string, string[]>
    {
        { "DataBlockAccess", new[] { "datablock.read" } },
        { "DataBlockAdmin", new[] { "datablock.read", "datablock.write", "datablock.admin" } }
    };
});

// Configure registry with custom options
builder.Services.AddDataBlockRegistry(registryOptions =>
{
    // Performance settings
    registryOptions.EnableLazyLoading = true;
    registryOptions.EnableCaching = true;
    registryOptions.CompressSerializedData = true;

    // Cache configuration
    registryOptions.MaxCacheSize = 5000;
    registryOptions.CacheEvictionTime = TimeSpan.FromHours(2);

    // Maintenance settings
    registryOptions.MaintenanceInterval = TimeSpan.FromMinutes(30);
    registryOptions.BackgroundMaintenanceEnabled = true;
    registryOptions.MaxDataBlockAge = TimeSpan.FromDays(90);
});

builder.Services.AddControllers();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Registering DataBlocks

Programmatically register DataBlocks when your application starts:

```csharp
using Datafication.Core.Data;
using Datafication.Server.Core.Registry;

// In your startup or seeding code
public class DataSeeder
{
    private readonly IDataBlockRegistry _registry;
    
    public DataSeeder(IDataBlockRegistry registry)
    {
        _registry = registry;
    }
    
    public async Task SeedAsync()
    {
        // Create a DataBlock
        var employees = new DataBlock();
        
        // Add columns
        employees.AddColumn(new DataColumn("Id", typeof(int)));
        employees.AddColumn(new DataColumn("Name", typeof(string)));
        employees.AddColumn(new DataColumn("Department", typeof(string)));
        employees.AddColumn(new DataColumn("Salary", typeof(decimal)));
        employees.AddColumn(new DataColumn("StartDate", typeof(DateTime)));
        
        // Add data
        employees.AddRow(new object[] { 1, "Alice Johnson", "Engineering", 95000m, new DateTime(2020, 3, 15) });
        employees.AddRow(new object[] { 2, "Bob Smith", "Marketing", 72000m, new DateTime(2021, 6, 1) });
        employees.AddRow(new object[] { 3, "Carol White", "Engineering", 88000m, new DateTime(2019, 11, 20) });
        
        // Create metadata for the DataBlock
        var metadata = new DataBlockMetadata
        {
            Name = "Employee Directory",
            Description = "Active employee information",
            Tags = new[] { "hr", "core", "employees" }
        };
        
        // Register with the server
        _registry.RegisterDataBlock("employees", employees, metadata);
        
        Console.WriteLine($"Registered DataBlock 'employees' with {employees.RowCount} rows");
    }
}

// Register seeder in Program.cs
builder.Services.AddSingleton<DataSeeder>();

var app = builder.Build();

// Seed data at startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}
```

### Authentication Configuration

Configure JWT Bearer token authentication:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DataBlockAccess", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "datablock.read"));
    
    options.AddPolicy("DataBlockAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "datablock.admin"));
});

builder.Services.AddDataficationServer();
builder.Services.AddControllers();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

**appsettings.json:**

```json
{
  "Jwt": {
    "Key": "your-secret-key-here-must-be-at-least-32-characters-long",
    "Issuer": "YourAppName",
    "Audience": "YourAppName",
    "ExpirationMinutes": 60
  }
}
```

### Using the REST API

Once configured, access your DataBlocks via HTTP:

```bash
# Set your authentication token
TOKEN="your-jwt-token-here"

# List all available DataBlocks
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/data/datablocks

# Get schema for a specific DataBlock
curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/data/datablocks/employees/schema

# Fetch data from a DataBlock (JSON format)
curl -H "Authorization: Bearer $TOKEN" \
     "http://localhost:5000/api/data/datablocks/employees?limit=10&format=json"

# Fetch with column filtering
curl -H "Authorization: Bearer $TOKEN" \
     "http://localhost:5000/api/data/datablocks/employees?columns=Name,Department,Salary"

# Fetch as CSV
curl -H "Authorization: Bearer $TOKEN" \
     "http://localhost:5000/api/data/datablocks/employees?format=csv"

# Fetch with pagination
curl -H "Authorization: Bearer $TOKEN" \
     "http://localhost:5000/api/data/datablocks/employees?offset=10&limit=5"
```

### Row Operations

Perform CRUD operations on individual rows:

```csharp
// Using HttpClient in C#
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);

// Add a new row
var addRequest = new { values = new object[] { 10, "David Lee", "Sales", 78000m, DateTime.Now } };
var addResponse = await client.PostAsJsonAsync(
    "http://localhost:5000/api/data/datablocks/employees/rows",
    addRequest);
var addResult = await addResponse.Content.ReadFromJsonAsync<RowOperationResponse>();
Console.WriteLine($"Added row at index {addResult.AffectedRowIndex}");

// Insert row at specific position
var insertRequest = new { values = new object[] { 11, "Eve Martinez", "HR", 72000m, DateTime.Now } };
var insertResponse = await client.PostAsJsonAsync(
    "http://localhost:5000/api/data/datablocks/employees/rows/3",
    insertRequest);

// Update existing row
var updateRequest = new { values = new object[] { 1, "Alice Johnson-Smith", "Engineering", 98000m, new DateTime(2020, 3, 15) } };
var updateResponse = await client.PutAsJsonAsync(
    "http://localhost:5000/api/data/datablocks/employees/rows/0",
    updateRequest);

// Delete row
var deleteResponse = await client.DeleteAsync(
    "http://localhost:5000/api/data/datablocks/employees/rows/5");
var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<RowOperationResponse>();
Console.WriteLine($"Current row count: {deleteResult.CurrentRowCount}");
```

**Using cURL:**

```bash
# Add row
curl -X POST \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"values": [10, "David Lee", "Sales", 78000, "2024-01-15"]}' \
     http://localhost:5000/api/data/datablocks/employees/rows

# Insert row at index 3
curl -X POST \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"values": [11, "Eve Martinez", "HR", 72000, "2024-01-15"]}' \
     http://localhost:5000/api/data/datablocks/employees/rows/3

# Update row at index 0
curl -X PUT \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"values": [1, "Alice Updated", "Engineering", 98000, "2020-03-15"]}' \
     http://localhost:5000/api/data/datablocks/employees/rows/0

# Delete row at index 5
curl -X DELETE \
     -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/data/datablocks/employees/rows/5
```

### Comprehensive DataBlock Querying

The `/datablocks/{id}/query` endpoint provides full pandas-like query capabilities via REST. Execute complex operations including filtering, sorting, aggregation, grouping, window functions, joins, and data transformations in a single request.

**Basic Query Example:**

```bash
curl -X POST \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "where": [
         {"column": "Department", "operator": "eq", "value": "Engineering"},
         {"column": "Salary", "operator": "gte", "value": 80000}
       ],
       "sort": {"column": "Salary", "direction": "desc"},
       "select": ["Name", "Department", "Salary"],
       "take": 10
     }' \
     http://localhost:5000/api/data/datablocks/employees/query
```

**Supported Query Operations:**

| Operation | Description | Example |
|-----------|-------------|---------|
| `where` | Filter rows by conditions | `{"column": "status", "operator": "eq", "value": "active"}` |
| `whereIn` | Filter by value collection | `{"column": "category", "values": ["A", "B", "C"]}` |
| `whereNot` | Exclude specific value | `{"column": "type", "value": "deleted"}` |
| `sort` | Sort results | `{"column": "created_at", "direction": "desc"}` |
| `select` | Project columns | `["Name", "Email", "Salary"]` |
| `skip` / `take` | Pagination | `"skip": 100, "take": 50` |
| `sample` | Random sampling | `"sample": 100, "sampleSeed": 42` |
| `aggregate` | Simple aggregation | `{"type": "sum", "columns": ["amount"]}` |
| `groupBy` | Group with aggregation | `{"column": "category", "aggregations": {"price": "mean"}}` |
| `dropDuplicates` | Remove duplicates | `{"columns": ["email"], "keep": "first"}` |
| `dropNulls` | Remove null rows | `{"mode": "any"}` |
| `fillNulls` | Fill null values | `{"method": "mean", "columns": ["price"]}` |
| `window` | Window functions | `{"column": "sales", "function": "cumsum"}` |
| `compute` | Computed columns | `{"resultColumn": "total", "expression": "[price] * [qty]"}` |
| `merge` | Join DataBlocks | `{"otherDataBlockId": "orders", "keyColumn": "id"}` |
| `melt` | Unpivot (wide to long) | `{"fixedColumns": ["id"], "meltedColumnName": "metric"}` |
| `transpose` | Transpose rows/columns | `{"headerColumnName": "name"}` |

**GroupBy with Aggregations:**

```bash
curl -X POST \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "groupBy": {
         "column": "Department",
         "aggregations": {
           "Salary": "mean",
           "Id": "count"
         },
         "resultColumnNames": {
           "Salary": "avg_salary",
           "Id": "employee_count"
         }
       },
       "sort": {"column": "avg_salary", "direction": "desc"}
     }' \
     http://localhost:5000/api/data/datablocks/employees/query
```

**Window Functions:**

```bash
curl -X POST \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "window": [
         {
           "column": "sales",
           "function": "movingavg",
           "windowSize": 7,
           "resultColumn": "sales_7day_avg"
         },
         {
           "column": "sales",
           "function": "cumsum",
           "resultColumn": "running_total"
         },
         {
           "column": "sales",
           "function": "rank",
           "orderByColumn": "sales",
           "partitionByColumns": ["region"],
           "resultColumn": "rank_in_region"
         }
       ],
       "sort": {"column": "date", "direction": "asc"}
     }' \
     http://localhost:5000/api/data/datablocks/sales/query
```

**Supported Window Functions:**
- **Moving**: `movingavg`, `movingsum`, `movingmin`, `movingmax`, `movingstddev`, `movingvariance`, `movingcount`, `movingmedian`, `movingpercentile`, `ema`
- **Cumulative**: `cumsum`, `cumavg`, `cummin`, `cummax`
- **Lag/Lead**: `lag`, `lead`
- **Ranking**: `rownumber`, `rank`, `denserank`
- **Value**: `firstvalue`, `lastvalue`, `nthvalue`

**Computed Columns:**

```bash
curl -X POST \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "compute": [
         {"resultColumn": "total_value", "expression": "[Price] * [Quantity]"},
         {"resultColumn": "discounted", "expression": "[Price] * 0.9"}
       ],
       "select": ["ProductName", "Price", "Quantity", "total_value", "discounted"]
     }' \
     http://localhost:5000/api/data/datablocks/products/query
```

**Join DataBlocks:**

```bash
curl -X POST \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "merge": {
         "otherDataBlockId": "customers",
         "keyColumn": "customer_id",
         "otherKeyColumn": "id",
         "mode": "left"
       },
       "select": ["order_id", "customer_name", "total"]
     }' \
     http://localhost:5000/api/data/datablocks/orders/query
```

Join modes: `inner` (default), `left`, `right`, `full`

**Data Cleaning:**

```bash
curl -X POST \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "dropNulls": {"mode": "any"},
       "fillNulls": {
         "method": "mean",
         "columns": ["price", "quantity"]
       },
       "dropDuplicates": {
         "columns": ["email"],
         "keep": "first"
       }
     }' \
     http://localhost:5000/api/data/datablocks/raw_data/query
```

**Query Response:**

```json
{
  "data": [
    {"Department": "Engineering", "avg_salary": 91500, "employee_count": 2},
    {"Department": "Marketing", "avg_salary": 72000, "employee_count": 1}
  ],
  "totalRows": 2,
  "returnedRows": 2,
  "columns": ["Department", "avg_salary", "employee_count"],
  "metadata": {
    "executionTimeMs": 12,
    "operationsApplied": ["GroupBy: Department", "Sort: avg_salary desc"],
    "executedAt": "2024-01-15T10:30:00Z"
  }
}
```

**C# Client Example:**

```csharp
using Datafication.Server.Core.Models;

var queryRequest = new DataBlockQueryRequest
{
    Where = new List<QueryFilterCondition>
    {
        new() { Column = "Status", Operator = "eq", Value = "Active" },
        new() { Column = "Amount", Operator = "gte", Value = 1000 }
    },
    GroupBy = new QueryGroupByRequest
    {
        Column = "Category",
        Aggregations = new Dictionary<string, string>
        {
            { "Amount", "sum" },
            { "Id", "count" }
        },
        ResultColumnNames = new Dictionary<string, string>
        {
            { "Amount", "total_amount" },
            { "Id", "order_count" }
        }
    },
    Sort = new QuerySortCondition { Column = "total_amount", Direction = "desc" },
    Take = 10
};

var response = await client.PostAsJsonAsync(
    "http://localhost:5000/api/data/datablocks/orders/query",
    queryRequest);
var result = await response.Content.ReadFromJsonAsync<DataBlockQueryResponse>();

Console.WriteLine($"Found {result.ReturnedRows} categories");
foreach (var row in result.Data)
{
    Console.WriteLine($"{row["Category"]}: ${row["total_amount"]} ({row["order_count"]} orders)");
}
```

### Registry Analytics

Access comprehensive analytics and insights about your DataBlock registry:

```csharp
// Get basic registry analytics as a DataBlock
var analytics = await client.GetFromJsonAsync<DataBlock>(
    "http://localhost:5000/api/data/registry/analytics");

// Get comprehensive analytics report
var comprehensive = await client.GetFromJsonAsync<DataBlock>(
    "http://localhost:5000/api/data/registry/analytics/comprehensive");

// Analyze usage patterns
var usagePatterns = await client.GetFromJsonAsync<DataBlock>(
    "http://localhost:5000/api/data/registry/analytics/usage-patterns");

// Get optimization recommendations
var optimization = await client.GetFromJsonAsync<DataBlock>(
    "http://localhost:5000/api/data/registry/analytics/optimization");

// Process recommendations
using var cursor = optimization.GetRowCursor();
while (cursor.MoveNext())
{
    var type = cursor.GetValue("OpportunityType")?.ToString();
    var priority = cursor.GetValue("Priority")?.ToString();
    var description = cursor.GetValue("Description")?.ToString();
    
    Console.WriteLine($"[{priority}] {type}: {description}");
}
```

### Advanced Querying

Perform complex queries against the registry:

```csharp
using Datafication.Server.Core.Models;

// Build advanced query
var query = new RegistryQueryRequest
{
    Filters = new List<RegistryFilter>
    {
        new() { Column = "RowCount", Operator = "gt", Value = 1000 },
        new() { Column = "IsReadOnly", Operator = "eq", Value = false },
        new() { Column = "Name", Operator = "contains", Value = "sales" }
    },
    SelectColumns = new List<string> { "Id", "Name", "RowCount", "SerializedSize", "LastAccessedAt" },
    OrderBy = "RowCount",
    OrderDescending = true,
    Skip = 0,
    Take = 10
};

// Execute query
var response = await client.PostAsJsonAsync(
    "http://localhost:5000/api/data/registry/query", 
    query);
var results = await response.Content.ReadFromJsonAsync<DataBlock>();

Console.WriteLine($"Found {results.RowCount} matching DataBlocks");
```

**Available Query Operators:**
- `eq` - Equals
- `ne` - Not equals
- `gt` - Greater than
- `gte` - Greater than or equal
- `lt` - Less than
- `lte` - Less than or equal
- `contains` - String contains
- `startswith` - String starts with
- `endswith` - String ends with

### Connector and Sink Registration

Register custom connectors and sinks for dynamic data access:

```csharp
using Datafication.Connectors.CsvConnector;
using Datafication.Sinks.PdfSink;

builder.Services.AddDataficationServer(options =>
{
    // Register CSV connector
    options.RegisterConnector<CsvDataConnector, CsvConnectorConfiguration>("csv");
    
    // Register Excel connector
    options.RegisterConnector<ExcelDataConnector, ExcelConnectorConfiguration>("excel");
    
    // Register JSON connector
    options.RegisterConnector<JsonDataConnector, JsonConnectorConfiguration>("json");
    
    // Register PDF sink
    options.RegisterSink<PdfDataSink>("pdf");
    
    // Register Excel sink
    options.RegisterSink<ExcelDataSink>("excel");
});

// Access registered connectors at runtime
var connectorRegistry = app.Services.GetRequiredService<IConnectorRegistry>();
var connectorFactory = connectorRegistry.GetConnectorFactory("csv");

// Access registered sinks
var sinkRegistry = app.Services.GetRequiredService<ISinkRegistry>();
var pdfSink = sinkRegistry.CreateSink("pdf");
```

### Export and Import Registry

Backup and restore your complete registry state:

```csharp
// Export registry state
var exportResponse = await client.GetAsync(
    "http://localhost:5000/api/data/registry/export");
var snapshot = await exportResponse.Content.ReadFromJsonAsync<DataBlockSnapshot>();

// Save to file
await File.WriteAllTextAsync(
    "registry-backup.json", 
    JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true }));

// Import to another instance
var importContent = await File.ReadAllTextAsync("registry-backup.json");
var importResponse = await client.PostAsync(
    "http://staging:5000/api/data/registry/import",
    new StringContent(importContent, Encoding.UTF8, "application/json"));

var importResult = await importResponse.Content.ReadFromJsonAsync<dynamic>();
Console.WriteLine($"Imported {importResult.importedCount} DataBlocks");
```

### Cache Management

Manage the registry cache for optimal performance:

```csharp
// Clear registry cache
var clearResponse = await client.PostAsync(
    "http://localhost:5000/api/data/registry/clear-cache", 
    null);
var clearResult = await clearResponse.Content.ReadFromJsonAsync<dynamic>();
Console.WriteLine($"Cleared {clearResult.clearedEntries} cache entries");

// Force maintenance
var maintenanceResponse = await client.PostAsync(
    "http://localhost:5000/api/data/registry/maintenance", 
    null);
var maintenanceResult = await maintenanceResponse.Content.ReadFromJsonAsync<dynamic>();
Console.WriteLine($"Maintenance completed in {maintenanceResult.totalDuration}");
```

## Configuration Reference

### DataBlockServerOptions

Main configuration class for DataBlock server behavior.

**Properties:**

- **`RoutePrefix`** (string, default: `"api/data"`): API route prefix for all endpoints
  - Custom prefix: `"myapp/data"` → endpoints at `/myapp/data/*`

- **`DefaultAccessPolicy`** (string, default: `"DataBlockAccess"`): Default authorization policy

- **`AllowAnonymousAccess`** (bool, default: `false`): Allow unauthenticated requests

- **`MaxRowsPerRequest`** (int, default: `10000`): Maximum rows returned per request

- **`MaxRegisteredDataBlocks`** (int, default: `1000`): Maximum DataBlocks in registry

- **`EnableCaching`** (bool, default: `true`): Enable response caching

- **`CacheDurationSeconds`** (int, default: `300`): Cache expiration time in seconds

- **`IncludeDetailedErrors`** (bool, default: `false`): Include detailed error messages (disable in production)

- **`AccessPolicies`** (IDictionary<string, string[]>): Custom policy-to-claims mapping

- **`RegistryImplementationType`** (Type?): Custom registry implementation type

- **`RegisteredConnectors`** (IDictionary): Registered connector factories

- **`RegisteredSinks`** (IDictionary): Registered sink types

**Methods:**

```csharp
// Register connector
DataBlockServerOptions RegisterConnector<TFactory, TConfiguration>(string connectorType)

// Register sink
DataBlockServerOptions RegisterSink<TSink>(string sinkKey)
```

### MetaRegistryOptions

Configuration for the meta DataBlock registry system.

**Properties:**

- **`EnableLazyLoading`** (bool, default: `true`): Lazy load DataBlocks on first access

- **`EnableCaching`** (bool, default: `true`): Enable registry caching

- **`CompressSerializedData`** (bool, default: `true`): Compress DataBlocks in storage

- **`CompressionLevel`** (CompressionLevel, default: `Optimal`): Compression level when enabled

- **`MaxCacheSize`** (int, default: `1000`): Maximum cached DataBlocks

- **`CacheEvictionTime`** (TimeSpan, default: `1 hour`): Cache entry lifetime

- **`CacheKeyPrefix`** (string, default: `"registry:"`): Cache key prefix

- **`MaintenanceInterval`** (TimeSpan, default: `30 minutes`): Maintenance task frequency

- **`BackgroundMaintenanceEnabled`** (bool, default: `false`): Run maintenance in background

- **`MaxDataBlockAge`** (TimeSpan, default: `null`): Maximum age before cleanup (null = no limit)

- **`EnableDetailedLogging`** (bool, default: `false`): Detailed operation logging

### Configuration Presets

Pre-configured options for common scenarios using `RegistryPreset`:

| Preset | Use Case | Cache Size | Maintenance | Logging | Analytics |
|--------|----------|------------|-------------|---------|-----------|
| `RegistryPreset.Development` | Development/Debug | 200 | 2 min | Detailed | Yes |
| `RegistryPreset.Production` | Production | 1000 | 30 min | Minimal | No |
| `RegistryPreset.Enterprise` | Enterprise | 5000 | 1 hour | Minimal | Yes |

**Preset Details:**

- **Development**: Optimized for debugging with detailed logging, short cache duration (5 min), no compression, and quick data expiration (1 day).
- **Production**: Balanced performance with compression enabled, longer cache (2 hours), and background maintenance.
- **Enterprise**: Full-featured with largest cache (4 hours), analytics service, optimal compression, and extended data retention (180 days).

**Usage:**

```csharp
using Datafication.Server.Core.Configuration;

// Basic (default settings)
builder.Services.AddDataBlockRegistry();

// Development
builder.Services.AddDataBlockRegistry(RegistryPreset.Development);

// Production
builder.Services.AddDataBlockRegistry(RegistryPreset.Production);

// Enterprise
builder.Services.AddDataBlockRegistry(RegistryPreset.Enterprise);

// Custom configuration
builder.Services.AddDataBlockRegistry(options =>
{
    options.EnableCaching = true;
    options.MaxCacheSize = 2000;
    options.CacheEvictionTime = TimeSpan.FromHours(1);
});
```

## API Reference

For complete API documentation, see the [Datafication.Server.Core API Reference](https://datafication.co/help/api/reference/Datafication.Server.Core.html).

### Core Classes

**IDataBlockRegistry**
- Main interface for DataBlock registration and management
- **Methods:**
  - `void RegisterDataBlock(string id, DataBlock dataBlock, DataBlockMetadata? metadata = null)` - Register DataBlock with ID and optional metadata
  - `DataBlock? GetDataBlock(string id)` - Get DataBlock by ID
  - `DataBlockMetadata? GetDataBlockMetadata(string id)` - Get DataBlock metadata by ID
  - `bool UnregisterDataBlock(string id)` - Remove DataBlock from registry
  - `bool Contains(string id)` - Check if DataBlock exists
  - `IEnumerable<string> GetAllDataBlockIds()` - Get all registered DataBlock IDs
- **Properties:**
  - `int Count` - Total number of registered DataBlocks

**MetaDataBlockRegistry**
- Primary implementation using DataBlock to manage DataBlocks
- Implements `IDataBlockRegistry`, `ISerializableDataBlockRegistry`
- **Properties:**
  - `DataBlock Registry` - Underlying registry DataBlock
  - `IDataCache Cache` - Cache implementation
- **Methods:**
  - All `IDataBlockRegistry` methods
  - `Task ClearCacheAsync()` - Clear registry cache
  - `Task CompactAsync()` - Compact registry storage
  - `DataBlock GetAnalytics()` - Get analytics as DataBlock

**DataBlockController**
- ASP.NET Core controller providing REST endpoints
- **Endpoints:**
  - List, Schema, Fetch, Register, Remove (DataBlocks)
  - Add, Insert, Update, Remove (Rows)
  - Export, Import, Analytics, Query (Registry)
  - Clear Cache, Maintenance (Management)

**IConnectorRegistry**
- Manages registered data connectors
- **Methods:**
  - `void RegisterConnector<TFactory, TConfiguration>(string connectorType)` - Register connector factory
  - `IDataConnectorFactory? GetConnectorFactory(string connectorType)` - Get connector factory
  - `IDataConnector CreateConnector(string connectorType, IDataConnectorConfiguration configuration)` - Create connector instance
  - `IEnumerable<string> GetRegisteredConnectorTypes()` - List registered connector types
  - `bool IsConnectorRegistered(string connectorType)` - Check if connector is registered

**ISinkRegistry**
- Manages registered data sinks
- **Methods:**
  - `void RegisterSink<TSink>(string sinkKey)` - Register sink type
  - `object? CreateSink(string sinkKey, Dictionary<string, object>? sinkOptions = null)` - Create sink instance
  - `Task<object> TransformAsync(string sinkKey, DataBlock dataBlock, Dictionary<string, object>? sinkOptions = null)` - Transform DataBlock using sink
  - `IEnumerable<string> GetRegisteredSinkKeys()` - List registered sink keys
  - `bool IsSinkRegistered(string sinkKey)` - Check if sink is registered
  - `Type? GetSinkReturnType(string sinkKey)` - Get sink return type
  - `string GetContentType(string sinkKey)` - Get HTTP content type for sink

**RegistryAnalytics**
- Provides analytics and optimization insights
- **Methods:**
  - `DataBlock GetComprehensiveAnalytics(DataBlock registry)` - Detailed metrics
  - `DataBlock GetUsagePatterns(DataBlock registry)` - Usage analysis
  - `DataBlock GetOptimizationRecommendations(DataBlock registry)` - Optimization tips

**RegistryMaintenanceService**
- Background service for registry maintenance
- **Methods:**
  - `Task ExecuteAsync(CancellationToken token)` - Run maintenance loop
  - `Task PerformMaintenanceAsync()` - Execute maintenance tasks

### Extension Methods

**ServiceCollectionExtensions** (namespace: `Datafication.Server.Core.Extensions`)

```csharp
// Basic setup
IServiceCollection AddDataficationServer(this IServiceCollection services)
IServiceCollection AddDataficationServer(this IServiceCollection services, Action<DataBlockServerOptions> configure)

// Registry with presets (recommended)
IServiceCollection AddDataBlockRegistry(this IServiceCollection services)
IServiceCollection AddDataBlockRegistry(this IServiceCollection services, RegistryPreset preset)
IServiceCollection AddDataBlockRegistry(this IServiceCollection services, Action<MetaRegistryOptions> configure)
IServiceCollection AddDataBlockRegistry(this IServiceCollection services, Action<MetaRegistryOptions> configure, bool includeMaintenanceService, bool includeAnalytics = false)

// Registry with custom cache
IServiceCollection AddDataBlockRegistry<TCache>(this IServiceCollection services, Action<MetaRegistryOptions>? configure = null)
    where TCache : class, IDataCache
```

### REST API Endpoints

**Base URL**: `/api/data` (or custom `RoutePrefix`)

**DataBlock Operations:**

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/datablocks` | GET | List all DataBlocks |
| `/datablocks/{id}` | GET | Fetch DataBlock data |
| `/datablocks/{id}/schema` | GET | Get DataBlock schema |
| `/datablocks/{id}/info` | GET | Get DataBlock summary info |
| `/datablocks/{id}/query` | POST | Comprehensive query with filtering, sorting, aggregation, window functions, joins, and more |
| `/datablocks` | POST | Register new DataBlock |
| `/datablocks/{id}/metadata` | PATCH | Update DataBlock metadata |
| `/datablocks/{id}` | DELETE | Unregister DataBlock |

**Row Operations:**

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/datablocks/{id}/rows` | POST | Add row to DataBlock |
| `/datablocks/{id}/rows/{index}` | POST | Insert row at index |
| `/datablocks/{id}/rows/{index}` | PUT | Update row |
| `/datablocks/{id}/rows/{index}` | DELETE | Delete row |

**Sink/Transform Operations:**

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/sinks` | GET | List registered sinks |
| `/datablocks/{dataBlockId}/transform/{sinkId}` | POST | Transform DataBlock using sink |

**Registry Management:**

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/registry/export` | GET | Export registry state |
| `/registry/import` | POST | Import registry state |
| `/registry/clear-cache` | POST | Clear registry cache |
| `/registry/maintenance` | POST | Force maintenance |

**Analytics:**

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/registry/analytics` | GET | Basic analytics |
| `/registry/analytics/comprehensive` | GET | Detailed analytics |
| `/registry/analytics/usage-patterns` | GET | Usage patterns |
| `/registry/analytics/optimization` | GET | Optimization tips |
| `/registry/query` | POST | Advanced query |

## Common Patterns

### Simple Read-Only Data API

Expose DataBlocks as read-only REST API with minimal configuration:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Simple setup with anonymous access for public data
builder.Services.AddDataficationServer(options =>
{
    options.AllowAnonymousAccess = true;  // Public API
    options.MaxRowsPerRequest = 1000;

    // Register sinks for output formats (json has built-in fallback)
    options.RegisterSink<CsvStringSink>("csv");
});

builder.Services.AddControllers();

var app = builder.Build();

// Seed read-only data
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<IDataBlockRegistry>();
    
    // Load public datasets
    var countries = await DataBlock.Connector.LoadCsvAsync("data/countries.csv");
    registry.RegisterDataBlock("countries", countries, new DataBlockMetadata
    {
        Name = "Countries",
        Description = "Public country data",
        Tags = new[] { "reference", "geography" }
    });
    
    var cities = await DataBlock.Connector.LoadCsvAsync("data/cities.csv");
    registry.RegisterDataBlock("cities", cities, new DataBlockMetadata
    {
        Name = "Cities",
        Description = "Public city data",
        Tags = new[] { "reference", "geography" }
    });
}

app.MapControllers();
app.Run();

// Access: GET http://localhost:5000/api/data/fetch/countries?format=json
```

### Full CRUD API with Authentication

Complete DataBlock API with JWT authentication and full CRUD capabilities:

```csharp
var builder = WebApplication.CreateBuilder(args);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DataBlockAccess", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "datablock.read"));
    
    options.AddPolicy("DataBlockAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "datablock.admin"));
});

// DataBlock Server
builder.Services.AddDataficationServer(options =>
{
    options.RoutePrefix = "api/v1/data";
    options.AllowAnonymousAccess = false;
    options.MaxRowsPerRequest = 10000;
    options.EnableCaching = true;
    options.CacheDurationSeconds = 300;
});

builder.Services.AddDataBlockRegistry(RegistryPreset.Production);
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Supports:
// - Read operations (with datablock.read scope)
// - Row-level CRUD (with datablock.admin scope)
// - Registry management (with datablock.admin scope)
```

### Enterprise Data Lake API

High-performance API for large-scale data operations with analytics:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Enterprise-grade authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* ... JWT config ... */);

builder.Services.AddAuthorization(/* ... policies ... */);

// Enterprise DataBlock Server
builder.Services.AddDataficationServer(options =>
{
    options.RoutePrefix = "api/datablock";
    options.MaxRowsPerRequest = 50000;  // Large result sets
    options.MaxRegisteredDataBlocks = 10000;  // Many datasets
    options.EnableCaching = true;
    options.CacheDurationSeconds = 3600;  // 1 hour cache

    // Register connectors for various data sources
    options.RegisterConnector<CsvDataConnector, CsvConnectorConfiguration>("csv");
    options.RegisterConnector<ParquetDataConnector, ParquetConnectorConfiguration>("parquet");
    options.RegisterConnector<S3Connector, S3ConnectorConfiguration>("s3");

    // Register sinks for output formats and export
    options.RegisterSink<HtmlTableSink>("html");
    options.RegisterSink<CsvStringSink>("csv");
    options.RegisterSink<ExcelDataSink>("excel");
    options.RegisterSink<PdfDataSink>("pdf");
});

// Enterprise registry with all features (includes maintenance and analytics)
builder.Services.AddDataBlockRegistry(RegistryPreset.Enterprise);

builder.Services.AddControllers();

var app = builder.Build();

// Load data from multiple sources
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<IDataBlockRegistry>();
    
    // Load from various sources
    var sales = await DataBlock.Connector.LoadParquetAsync(new Uri("s3://bucket/sales.parquet"));
    registry.RegisterDataBlock("sales", sales, new DataBlockMetadata
    {
        Name = "Sales Data",
        Description = "Historical sales transactions",
        Tags = new[] { "sales", "transactions", "revenue" }
    });
    
    var customers = await DataBlock.Connector.LoadCsvAsync("data/customers.csv");
    registry.RegisterDataBlock("customers", customers, new DataBlockMetadata
    {
        Name = "Customer Directory",
        Description = "Active customer information",
        Tags = new[] { "customers", "crm" }
    });
    
    var inventory = await VelocityDataBlock.OpenAsync("data/inventory.dfc");
    registry.RegisterDataBlock("inventory", inventory, new DataBlockMetadata
    {
        Name = "Inventory",
        Description = "Current product inventory levels",
        Tags = new[] { "inventory", "stock", "warehouse" }
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Features:
// - High-volume data access
// - Multiple data source types
// - Analytics and optimization
// - Background maintenance
// - Export to multiple formats
```

## Performance Tips

1. **Enable Caching**: Use both DataBlock server caching and meta registry caching for optimal performance
   ```csharp
   options.EnableCaching = true;
   options.CacheDurationSeconds = 600;
   registryOptions.EnableCaching = true;
   registryOptions.MaxCacheSize = 5000;
   ```

2. **Adjust Row Limits**: Set `MaxRowsPerRequest` based on your data size and network capacity
   ```csharp
   options.MaxRowsPerRequest = 10000;  // Balance between payload size and round trips
   ```

3. **Use Column Filtering**: Request only needed columns to reduce payload size
   ```bash
   GET /api/data/fetch/employees?columns=Id,Name,Salary
   ```

4. **Pagination**: Always use pagination for large datasets
   ```bash
   GET /api/data/fetch/large-dataset?offset=0&limit=100
   ```

5. **Lazy Loading**: Enable lazy loading in the meta registry to reduce memory footprint
   ```csharp
   registryOptions.EnableLazyLoading = true;
   ```

6. **Compression**: Enable compression for large DataBlocks to save storage space
   ```csharp
   registryOptions.CompressSerializedData = true;
   registryOptions.CompressionLevel = CompressionLevel.Optimal;
   ```

7. **Background Maintenance**: Enable background maintenance to avoid blocking operations
   ```csharp
   registryOptions.BackgroundMaintenanceEnabled = true;
   registryOptions.MaintenanceInterval = TimeSpan.FromHours(1);
   ```

8. **Registry Analytics**: Regularly check optimization recommendations
   ```bash
   GET /api/data/registry/analytics/optimization
   ```

9. **Choose Output Format Wisely**: JSON for APIs, CSV for exports, HTML for debugging
   ```bash
   GET /api/data/fetch/data?format=csv  # Smaller payload than JSON for large datasets
   ```

10. **Environment-Specific Presets**: Use appropriate preset for your environment
    ```csharp
    // Development: Frequent maintenance, detailed logging
    services.AddDataBlockRegistry(RegistryPreset.Development);

    // Production: Optimized settings, minimal logging
    services.AddDataBlockRegistry(RegistryPreset.Production);

    // Enterprise: Maximum cache, background maintenance, analytics
    services.AddDataBlockRegistry(RegistryPreset.Enterprise);
    ```

11. **Monitor Cache Hit Rates**: Check analytics to tune cache settings
    ```bash
    GET /api/data/registry/analytics/comprehensive
    ```

12. **Clear Cache After Bulk Updates**: Manually clear cache after major data changes
    ```bash
    POST /api/data/registry/clear-cache
    ```

## License

This library is licensed under the **Datafication SDK License Agreement**. See the [LICENSE](./LICENSE) file for details.

**Summary:**
- **Free Use**: Organizations with fewer than 5 developers AND annual revenue under $500,000 USD may use the SDK without a commercial license
- **Commercial License Required**: Organizations with 5+ developers OR annual revenue exceeding $500,000 USD must obtain a commercial license
- **Open Source Exemption**: Open source projects meeting specific criteria may be exempt from developer count limits

For commercial licensing inquiries, contact [support@datafication.co](mailto:support@datafication.co).

---

**Datafication.Server.Core** - Turn your DataBlocks into a production-ready REST API in minutes.

For more examples and documentation, visit our [samples directory](../../samples/DataBlockServer/).

