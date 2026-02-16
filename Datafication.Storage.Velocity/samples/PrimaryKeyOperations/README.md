# PrimaryKeyOperations Sample

This sample demonstrates O(1) primary key operations in Datafication.Storage.Velocity, including fast lookups, updates, and deletes using business keys.

## Overview

The PrimaryKeyOperations sample shows how to:
- Configure a primary key column for O(1) lookups
- Find row IDs by primary key with `FindRowIdAsync`
- Update and delete rows using primary keys
- Perform batch operations with multiple primary keys
- Access values using VelocityRowId for maximum performance
- Check row deletion status and index statistics

## Key Features Demonstrated

### Primary Key Configuration
- `VelocityOptions.PrimaryKeyColumn` - Set the primary key column
- Automatic index building for O(1) lookups

### O(1) Lookups
- `FindRowIdAsync()` - Get VelocityRowId from primary key
- `GetValue()` - Access values by VelocityRowId

### Single Row Operations
- `UpdateRowAsync(primaryKey, values)` - Update by primary key
- `UpdateRowAsync(rowId, values)` - Update by VelocityRowId
- `DeleteRowAsync(primaryKey)` - Delete by primary key

### Batch Operations
- `UpdateRowsAsync(Dictionary<string, object[]>)` - Batch updates
- `DeleteRowsAsync(IEnumerable<string>)` - Batch deletes

### Diagnostics
- `IsRowDeleted()` - Check if a row is marked as deleted
- `GetPrimaryKeyIndexStats()` - Get index statistics

## How to Run

```bash
cd PrimaryKeyOperations
dotnet restore
dotnet run
```

## Expected Output

The sample will output:
1. Initial data with primary keys
2. O(1) row lookup by primary key
3. Single row update by primary key
4. Update using VelocityRowId
5. Delete by primary key
6. Batch updates for multiple keys
7. Batch deletes for multiple keys
8. Row deletion status check
9. Primary key index statistics
10. Final data state

## Related Samples

- **CRUDOperations** - Learn about basic index-based CRUD operations
- **StorageManagement** - Learn about compaction and storage optimization
- **QueryOperations** - Learn about filtering and querying data
