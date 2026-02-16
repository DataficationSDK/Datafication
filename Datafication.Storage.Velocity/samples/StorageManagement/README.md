# StorageManagement Sample

This sample demonstrates storage management operations in Datafication.Storage.Velocity, including compaction, statistics, and configuration.

## Overview

The StorageManagement sample shows how to:
- Configure VelocityOptions for storage behavior
- Monitor storage statistics
- Perform manual and automatic compaction
- Configure compaction triggers and thresholds
- Enable background compaction
- Access primary key index statistics
- Flush changes to disk

## Key Features Demonstrated

### VelocityOptions Configuration
- `PrimaryKeyColumn` - Set primary key for O(1) lookups
- `TargetSegmentSizeBytes` - Control segment size in bytes
- `DefaultCompression` - Set compression type (LZ4, Deflate, None)
- `EnableAutoCompression` - Enable automatic compression selection
- `AutoCompactionEnabled` - Enable automatic compaction
- `AutoCompactionTrigger` - SegmentCount, TotalSize, DeletedRowPercentage
- `MaxSegmentsBeforeCompaction` - Segment count threshold for compaction

### Storage Statistics
- `GetStorageStatsAsync()` - Get comprehensive storage info
- `TotalRows`, `ActiveRows`, `DeletedRows` - Row counts
- `DeletedPercentage` - Deletion ratio
- `StorageFiles` - Number of storage files
- `EstimatedSizeBytes` - Approximate storage size
- `CanCompact` - Whether compaction is recommended

### Compaction
- `CompactAsync()` - Manual compaction with default strategy
- `CompactAsync(strategy)` - Compaction with specific strategy
- Strategies: Quick, Standard, Aggressive

### Auto-Compaction Configuration
- `ConfigureAutoCompaction(enabled, trigger, threshold)`
- Triggers: SegmentCount, DeletedPercentage, Manual
- Set thresholds appropriate for your workload

### Background Compaction
- `EnableBackgroundCompaction(enabled)` - Non-blocking maintenance
- Compaction runs in background when thresholds are met

### Persistence
- `FlushAsync()` - Ensure all changes are written to disk

### Index Statistics
- `GetPrimaryKeyIndexStats()` - Primary key index info
- Returns: (indexedKeys, indexBuilt, segments)

### Schema Information
- `Info()` - Get column schema as DataBlock
- Shows column names, types, null counts

## How to Run

```bash
cd StorageManagement
dotnet restore
dotnet run
```

## Expected Output

The sample will output:
1. Storage options configuration
2. Initial data loading
3. Initial storage statistics
4. Statistics after deletions
5. Manual compaction results
6. Compaction strategy options
7. Auto-compaction configuration
8. Background compaction setup
9. Primary key index statistics
10. Flush confirmation
11. Schema information
12. Final storage statistics

## Related Samples

- **BasicOperations** - Learn about creating VelocityDataBlocks
- **PrimaryKeyOperations** - Learn about O(1) primary key lookups
- **CRUDOperations** - Learn about data modifications
