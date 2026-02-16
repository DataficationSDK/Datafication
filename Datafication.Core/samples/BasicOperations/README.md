# BasicOperations Sample

This sample demonstrates the fundamental operations of Datafication.Core, including creating DataBlocks, adding columns and rows, and performing basic data access operations.

## Overview

The BasicOperations sample shows how to:
- Create a new DataBlock
- Add columns with type information
- Add rows with data
- Perform column operations (check existence, get column, select columns)
- Perform row operations (access cells, update cells, insert/remove rows)
- Iterate over rows using a cursor

## Key Features Demonstrated

### Creating DataBlocks
- Creating a new `DataBlock` instance
- Adding typed columns using `DataColumn`
- Adding rows with `AddRow()`

### Column Operations
- `HasColumn()` - Check if a column exists
- `GetColumn()` - Get a column by name
- Indexer access `dataBlock["ColumnName"]`
- `Select()` - Project specific columns to a new DataBlock

### Row Operations
- `RowCount` - Get the number of rows
- Indexer access `dataBlock[rowIndex, "ColumnName"]` - Get cell value
- Indexer assignment `dataBlock[rowIndex, "ColumnName"] = value` - Update cell value
- `InsertRow()` - Insert a row at a specific position
- `UpdateRow()` - Update an entire row
- `RemoveRow()` - Remove a row by index

### Row Cursor
- `GetRowCursor()` - Get a cursor for iterating over rows
- `MoveNext()` - Move to the next row
- `GetValue()` - Get a value from the current row

## How to Run

```bash
cd BasicOperations
dotnet restore
dotnet run
```

## Expected Output

The sample will output:
1. Column creation confirmation
2. Row addition confirmation
3. Column operation results
4. Row operation results
5. Row iteration results

## Related Samples

- **FilteringAndSorting** - Learn how to filter and sort data
- **GroupingAndAggregation** - Learn how to group and aggregate data
- **DataQuality** - Learn about schema inspection and data quality operations

