# DataQuality Sample

This sample demonstrates data quality operations and schema inspection in Datafication.Core.

## Overview

The DataQuality sample shows how to:
- Inspect schema information
- Use the `Info()` method for detailed statistics
- Perform data quality checks
- Combine quality operations
- Generate quality reports

## Key Features Demonstrated

### Schema Inspection

#### Access Schema
```csharp
var schema = employees.Schema;
Console.WriteLine($"Number of columns: {schema.Count}");
Console.WriteLine($"Column names: {string.Join(", ", schema.GetColumnNames())}");
```

#### Check Column Existence
```csharp
if (schema.ColumnExists("Salary"))
{
    Console.WriteLine("Salary column exists");
}
```

#### Get Column Information
```csharp
var column = employees.GetColumn("Name");
Console.WriteLine($"Column: {column.Name}, Type: {column.DataType.GetClrType().Name}");
```

### Info() Method

Get detailed information about the DataBlock:
```csharp
var info = employees.Info();
```

The `Info()` method returns a DataBlock with columns:
- **Column**: Column name
- **Label**: Column label
- **Type**: Data type
- **Non-Null Count**: Count of non-null values
- **Null Count**: Count of null values

### Data Quality Checks

#### Check for Nulls
```csharp
var nullIds = employees.Where("EmployeeId", null, ComparisonOperator.Equals);
Console.WriteLine($"Rows with null EmployeeId: {nullIds.RowCount}");
```

#### Check for Duplicates
```csharp
var duplicates = employees.DropDuplicates(KeepDuplicateMode.None);
var duplicateCount = employees.RowCount - duplicates.RowCount;
```

### Combining Quality Operations

Chain multiple quality operations:
```csharp
var cleaned = rawEmployees
    .DropNulls(DropNullMode.Any, "EmployeeId", "Name")  // Remove rows with missing required fields
    .DropDuplicates(KeepDuplicateMode.First, "EmployeeId")  // Remove duplicate IDs
    .Where("Status", "Active")  // Filter active employees
    .Sort(SortDirection.Ascending, "Name");  // Sort by name
```

### Quality Report Generation

Create a quality report DataBlock:
```csharp
var qualityReport = new DataBlock();
qualityReport.AddColumn(new DataColumn("Metric", typeof(string)));
qualityReport.AddColumn(new DataColumn("Value", typeof(object)));

qualityReport.AddRow(new object[] { "Total Rows", employees.RowCount });
qualityReport.AddRow(new object[] { "Rows with Null EmployeeId", nullIds.RowCount });
// ... more metrics
```

## How to Run

```bash
cd DataQuality
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates:
- Schema inspection and column information
- Detailed statistics using Info()
- Various data quality checks
- Combined quality operations
- Quality report generation

## Related Samples

- **DataTransformation** - Learn about data transformation operations
- **RemovingDuplicates** - Learn how to remove duplicates
- **ETLPipeline** - See data quality in a complete pipeline

