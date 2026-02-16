using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core Basic Operations Sample ===\n");

// Create a new DataBlock
var employees = new DataBlock();

// Add columns with type information
employees.AddColumn(new DataColumn("EmployeeId", typeof(int)));
employees.AddColumn(new DataColumn("Name", typeof(string)));
employees.AddColumn(new DataColumn("Department", typeof(string)));
employees.AddColumn(new DataColumn("Salary", typeof(decimal)));
employees.AddColumn(new DataColumn("HireDate", typeof(DateTime)));

Console.WriteLine("1. Created DataBlock and added columns:");
Console.WriteLine($"   Columns: {string.Join(", ", employees.Schema.GetColumnNames())}\n");

// Add rows
employees.AddRow(new object[] { 1, "Alice Johnson", "Engineering", 95000m, new DateTime(2020, 3, 15) });
employees.AddRow(new object[] { 2, "Bob Smith", "Marketing", 72000m, new DateTime(2021, 6, 1) });
employees.AddRow(new object[] { 3, "Carol White", "Engineering", 88000m, new DateTime(2019, 11, 20) });
employees.AddRow(new object[] { 4, "David Brown", "Sales", 65000m, new DateTime(2022, 1, 10) });
employees.AddRow(new object[] { 5, "Eve Davis", "Engineering", 102000m, new DateTime(2018, 5, 3) });

Console.WriteLine($"2. Added {employees.RowCount} rows to the DataBlock\n");

// Column Operations
Console.WriteLine("3. Column Operations:");
Console.WriteLine($"   HasColumn('Salary'): {employees.HasColumn("Salary")}");
Console.WriteLine($"   HasColumn('Bonus'): {employees.HasColumn("Bonus")}");

var nameColumn = employees.GetColumn("Name");
Console.WriteLine($"   GetColumn('Name'): {nameColumn.Name}, Type: {nameColumn.DataType.GetClrType().Name}");

// Access column using indexer
var salaryColumn = employees["Salary"];
Console.WriteLine($"   Access via indexer employees[\"Salary\"]: Column exists\n");

// Select specific columns (projection)
var nameAndSalary = employees.Select("Name", "Salary");
Console.WriteLine($"4. Select('Name', 'Salary'): Created new DataBlock with {nameAndSalary.Schema.Count} columns");
Console.WriteLine($"   Columns: {string.Join(", ", nameAndSalary.Schema.GetColumnNames())}\n");

// Row Operations
Console.WriteLine("5. Row Operations:");
Console.WriteLine($"   Total rows: {employees.RowCount}");

// Access individual cell values using indexer
var firstEmployeeName = employees[0, "Name"];
Console.WriteLine($"   employees[0, \"Name\"]: {firstEmployeeName}");

var firstEmployeeSalary = employees[0, "Salary"];
Console.WriteLine($"   employees[0, \"Salary\"]: {firstEmployeeSalary:C}\n");

// Update a cell value
employees[0, "Salary"] = 98000m;
Console.WriteLine($"6. Updated employees[0, \"Salary\"] to: {employees[0, "Salary"]:C}\n");

// Insert a row at specific position
employees.InsertRow(2, new object[] { 6, "Frank Miller", "HR", 70000m, new DateTime(2021, 9, 15) });
Console.WriteLine($"7. Inserted row at index 2. New row count: {employees.RowCount}");
Console.WriteLine($"   Row at index 2: {employees[2, "Name"]} - {employees[2, "Department"]}\n");

// Update an entire row
employees.UpdateRow(0, new object[] { 1, "Alice Johnson-Smith", "Engineering", 98000m, new DateTime(2020, 3, 15) });
Console.WriteLine($"8. Updated entire row at index 0");
Console.WriteLine($"   New name: {employees[0, "Name"]}\n");

// Remove a row by index
var rowCountBefore = employees.RowCount;
employees.RemoveRow(5);
Console.WriteLine($"9. Removed row at index 5");
Console.WriteLine($"   Row count before: {rowCountBefore}, after: {employees.RowCount}\n");

// Iterate over rows using cursor
Console.WriteLine("10. Iterating over rows using GetRowCursor:");
var cursor = employees.GetRowCursor("Name", "Department", "Salary");
int rowIndex = 0;
while (cursor.MoveNext())
{
    var name = cursor.GetValue("Name");
    var dept = cursor.GetValue("Department");
    var salary = cursor.GetValue("Salary");
    Console.WriteLine($"   Row {rowIndex}: {name} - {dept}: {salary:C}");
    rowIndex++;
}

Console.WriteLine("\n=== Sample Complete ===");

