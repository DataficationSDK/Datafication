# BasicOperations Sample

This sample demonstrates the fundamental operations of Datafication.Storage.Velocity, including creating VelocityDataBlocks, configuring options, and basic data access.

## Overview

The BasicOperations sample shows how to:
- Create a new VelocityDataBlock with a file path
- Configure VelocityOptions for storage behavior
- Use factory methods (CreateEnterprise, CreateHighThroughput)
- Save an in-memory DataBlock to DFC format
- Open existing DFC files
- Access rows and columns

## Key Features Demonstrated

### Creating VelocityDataBlocks
- Basic constructor with file path
- VelocityOptions for primary keys, compression, and compaction
- Factory methods for common use cases

### Storage Options
- `PrimaryKeyColumn` - Enable O(1) lookups by business key
- `TargetSegmentSizeBytes` - Control segment size in bytes
- `DefaultCompression` - Set compression type (LZ4, Deflate, None)
- `EnableAutoCompression` - Automatic compression selection
- `AutoCompactionEnabled` - Automatic storage optimization

### Factory Methods
- `CreateEnterprise()` - Optimized for frequent updates
- `CreateHighThroughput()` - Optimized for high-speed ingestion
- `SaveAsync()` - Convert DataBlock to VelocityDataBlock
- `OpenAsync()` - Open existing DFC files

### Data Access
- Row count and column checks
- Indexer access for cell values
- Row cursor for iteration

## How to Run

```bash
cd BasicOperations
dotnet restore
dotnet run
```

## Expected Output

The sample will output:
1. VelocityDataBlock creation details
2. VelocityOptions configuration
3. Factory method usage
4. DataBlock to DFC conversion
5. File reopening confirmation
6. Row and column access examples
7. Row iteration output

## Related Samples

- **CRUDOperations** - Learn about add, update, and delete operations
- **PrimaryKeyOperations** - Learn about O(1) primary key lookups
- **StorageManagement** - Learn about compaction and storage statistics
