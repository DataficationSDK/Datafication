# RemovingDuplicates Sample

This sample demonstrates how to remove duplicate rows from DataBlocks in Datafication.Core.

## Overview

The RemovingDuplicates sample shows how to:
- Remove duplicates based on all columns
- Remove duplicates based on specific columns
- Control which duplicates to keep (first, last, or none)
- Combine duplicate removal with other data quality operations

## Key Features Demonstrated

### DropDuplicates Operations

#### Drop All Duplicates (Default)
Keep the first occurrence of each duplicate set:
```csharp
var uniqueRows = employees.DropDuplicates();
```

#### Keep Last Occurrence
Keep the last occurrence of each duplicate set:
```csharp
var lastOccurrences = employees.DropDuplicates(KeepDuplicateMode.Last);
```

#### Remove All Duplicates
Remove all duplicates, keep only unique rows:
```csharp
var onlyUnique = employees.DropDuplicates(KeepDuplicateMode.None);
```

#### Drop Duplicates Based on Specific Columns
Remove duplicates based on one or more columns:
```csharp
// Single column
var uniqueById = employees.DropDuplicates(KeepDuplicateMode.First, "EmployeeId");

// Multiple columns
var uniqueByNameDept = employees.DropDuplicates(KeepDuplicateMode.First, "Name", "Department");
```

### KeepDuplicateMode Enum
- `First` - Keep the first occurrence of each duplicate set (default)
- `Last` - Keep the last occurrence of each duplicate set
- `None` - Remove all duplicates, keep only unique rows

### Real-World Examples

#### Clean Customer Records
```csharp
var uniqueCustomers = customers
    .DropDuplicates(KeepDuplicateMode.Last, "Email")  // Keep latest record per email
    .Where("Status", "Active")
    .Sort(SortDirection.Ascending, "LastName");
```

#### Chain with Data Quality Operations
```csharp
var cleanedEmployees = rawEmployees
    .DropNulls(DropNullMode.Any, "EmployeeId", "Name")  // Remove rows with missing required fields
    .DropDuplicates(KeepDuplicateMode.First, "EmployeeId")  // Remove duplicate IDs
    .Where("Status", "Active")
    .Sort(SortDirection.Ascending, "Name");
```

## How to Run

```bash
cd RemovingDuplicates
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates:
- Different duplicate removal modes
- Removing duplicates based on specific columns
- Combining duplicate removal with filtering and sorting
- Real-world data cleaning scenarios

## Related Samples

- **DataQuality** - Learn about comprehensive data quality operations
- **DataTransformation** - Learn about other data cleaning operations
- **ETLPipeline** - See duplicate removal in a complete pipeline

