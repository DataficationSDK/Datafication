# CRUDOperations Sample

This sample demonstrates Create, Read, Update, and Delete operations using Datafication.Storage.Velocity's VelocityDataBlock.

## Overview

The CRUDOperations sample shows how to:
- Create (add) new rows to a VelocityDataBlock
- Read values using indexer access
- Update entire rows or individual cells
- Delete rows by index
- Batch append multiple rows efficiently

## Key Features Demonstrated

### Create Operations
- `AddRow()` - Add a single row with values
- `AppendBatchAsync()` - Efficiently append multiple rows

### Read Operations
- `GetValue(rowIndex, columnIndex)` for direct cell access
- `GetRowCursor()` for iterating over rows

### Update Operations
- `UpdateRow()` - Replace an entire row with new values

### Delete Operations
- `RemoveRow()` - Delete a row by index

### Persistence
- `FlushAsync()` - Ensure changes are written to disk

## How to Run

```bash
cd CRUDOperations
dotnet restore
dotnet run
```

## Expected Output

The sample will output:
1. Initial row creation with 5 products
2. Reading specific values
3. Updating an entire row
4. Updating a single cell value
5. Deleting a row
6. Batch appending multiple rows
7. Final state of all data

## Related Samples

- **BasicOperations** - Learn about creating and configuring VelocityDataBlocks
- **PrimaryKeyOperations** - Learn about O(1) updates/deletes by primary key
- **BatchOperations** - Learn about high-throughput batch operations
