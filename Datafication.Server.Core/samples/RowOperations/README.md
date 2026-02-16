# RowOperations Sample

This sample demonstrates CRUD (Create, Read, Update, Delete) operations on DataBlock rows via the REST API.

## Overview

- Add new rows (append to end)
- Insert rows at a specific index
- Update existing rows
- Delete rows by index

## How to Run

```bash
cd RowOperations
dotnet restore
dotnet run
```

The server will start at `http://localhost:5000`.

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/data/datablocks/tasks` | Get all rows |
| POST | `/api/data/datablocks/tasks/rows` | Add row (append) |
| POST | `/api/data/datablocks/tasks/rows/{index}` | Insert row at index |
| PUT | `/api/data/datablocks/tasks/rows/{index}` | Update row at index |
| DELETE | `/api/data/datablocks/tasks/rows/{index}` | Delete row at index |

## Add Row (Append)

Add a new row to the end of the DataBlock:

```bash
curl -X POST http://localhost:5000/api/data/datablocks/tasks/rows \
  -H "Content-Type: application/json" \
  -d '{"values": [4, "Review PR", "Pending", 2]}'
```

## Insert Row at Index

Insert a row at a specific position (0-based index):

```bash
curl -X POST http://localhost:5000/api/data/datablocks/tasks/rows/1 \
  -H "Content-Type: application/json" \
  -d '{"values": [5, "Urgent bug fix", "In Progress", 1]}'
```

## Update Row

Update a row at a specific index (replaces entire row):

```bash
curl -X PUT http://localhost:5000/api/data/datablocks/tasks/rows/0 \
  -H "Content-Type: application/json" \
  -d '{"values": [1, "Setup project", "Completed", 0]}'
```

## Delete Row

Delete a row by index:

```bash
curl -X DELETE http://localhost:5000/api/data/datablocks/tasks/rows/2
```

## View Current Data

```bash
curl http://localhost:5000/api/data/datablocks/tasks
```

## C# Client Example

```csharp
using System.Text;
using System.Text.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// Add new row
var addRequest = new { values = new object[] { 4, "New task", "Pending", 2 } };
var addJson = JsonSerializer.Serialize(addRequest);
await client.PostAsync("/api/data/datablocks/tasks/rows",
    new StringContent(addJson, Encoding.UTF8, "application/json"));

// Insert row at index 1
var insertRequest = new { values = new object[] { 5, "Urgent task", "In Progress", 1 } };
var insertJson = JsonSerializer.Serialize(insertRequest);
await client.PostAsync("/api/data/datablocks/tasks/rows/1",
    new StringContent(insertJson, Encoding.UTF8, "application/json"));

// Update row at index 0
var updateRequest = new { values = new object[] { 1, "Setup project", "Completed", 0 } };
var updateJson = JsonSerializer.Serialize(updateRequest);
await client.PutAsync("/api/data/datablocks/tasks/rows/0",
    new StringContent(updateJson, Encoding.UTF8, "application/json"));

// Delete row at index 2
await client.DeleteAsync("/api/data/datablocks/tasks/rows/2");

// View result
var result = await client.GetStringAsync("/api/data/datablocks/tasks");
Console.WriteLine(result);
```

## Request Format

### Add/Insert/Update Row

```json
{
  "values": [value1, value2, value3, ...]
}
```

The values array must match the column order of the DataBlock schema.

## Authorization

Row operations require the `DataBlockAdmin` policy. In this sample, anonymous access is allowed for demonstration purposes. In production, you should require proper authentication.

See the **AuthenticatedServer** sample for JWT-based access control.

## Related Samples

- **BasicServer** - Start here if you're new to Datafication.Server.Core
- **QueryOperations** - Learn how to query and filter data
- **AuthenticatedServer** - Learn how to secure row operations with JWT
