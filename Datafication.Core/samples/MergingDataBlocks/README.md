# MergingDataBlocks Sample

This sample demonstrates how to merge (join) DataBlocks using different join types in Datafication.Core.

## Overview

The MergingDataBlocks sample shows how to:
- Perform inner joins
- Perform left joins
- Perform right joins
- Perform full outer joins
- Merge DataBlocks with different key column names

## Key Features Demonstrated

### Merge Operations

The `Merge()` method combines two DataBlocks based on matching key columns, similar to SQL JOIN operations.

#### Inner Join
Returns only rows where the key exists in both DataBlocks:
```csharp
var innerJoined = employees.Merge(departments, "Department", MergeMode.Inner);
```

#### Left Join
Returns all rows from the left DataBlock and matching rows from the right DataBlock:
```csharp
var leftJoined = employees.Merge(departments, "Department", MergeMode.Left);
```

#### Right Join
Returns all rows from the right DataBlock and matching rows from the left DataBlock:
```csharp
var rightJoined = employees.Merge(departments, "Department", MergeMode.Right);
```

#### Full Outer Join
Returns all rows from both DataBlocks, filling in nulls where no match is found:
```csharp
var fullJoined = employees.Merge(departments, "Department", MergeMode.Full);
```

### Merge with Different Key Column Names

When the key columns have different names in each DataBlock:
```csharp
var merged = employees.Merge(
    departments, 
    "DeptName",      // Key column in left DataBlock
    "Department",    // Key column in right DataBlock
    MergeMode.Inner
);
```

### MergeMode Enum
- `Inner` - Only matching rows from both DataBlocks
- `Left` - All rows from left DataBlock, matching from right
- `Right` - All rows from right DataBlock, matching from left
- `Full` - All rows from both DataBlocks

## How to Run

```bash
cd MergingDataBlocks
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates:
- Different join types and their results
- How unmatched rows are handled in each join type
- Merging with different key column names

## Related Samples

- **BasicOperations** - Learn basic DataBlock operations
- **FilteringAndSorting** - Learn how to filter and sort data
- **GroupingAndAggregation** - Learn how to group and aggregate data

