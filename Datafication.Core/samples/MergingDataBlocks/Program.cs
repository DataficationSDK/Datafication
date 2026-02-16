using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core Merging DataBlocks Sample ===\n");

// Create employees DataBlock
var employees = new DataBlock();
employees.AddColumn(new DataColumn("EmployeeId", typeof(int)));
employees.AddColumn(new DataColumn("Name", typeof(string)));
employees.AddColumn(new DataColumn("Department", typeof(string)));
employees.AddColumn(new DataColumn("Salary", typeof(decimal)));

employees.AddRow(new object[] { 1, "Alice Johnson", "Engineering", 95000m });
employees.AddRow(new object[] { 2, "Bob Smith", "Marketing", 72000m });
employees.AddRow(new object[] { 3, "Carol White", "Engineering", 88000m });
employees.AddRow(new object[] { 4, "David Brown", "Sales", 65000m });
employees.AddRow(new object[] { 5, "Eve Davis", "Engineering", 102000m });

Console.WriteLine("Employees DataBlock:");
PrintDataBlock(employees);

// Create departments DataBlock
var departments = new DataBlock();
departments.AddColumn(new DataColumn("Department", typeof(string)));
departments.AddColumn(new DataColumn("Location", typeof(string)));
departments.AddColumn(new DataColumn("Budget", typeof(decimal)));

departments.AddRow(new object[] { "Engineering", "Building A", 500000m });
departments.AddRow(new object[] { "Marketing", "Building B", 300000m });
departments.AddRow(new object[] { "Sales", "Building C", 250000m });
departments.AddRow(new object[] { "HR", "Building D", 150000m });

Console.WriteLine("\nDepartments DataBlock:");
PrintDataBlock(departments);

// 1. Inner join (only matching rows)
var innerJoined = employees.Merge(departments, "Department", MergeMode.Inner);
Console.WriteLine("\n1. Inner Join (MergeMode.Inner) - Only matching rows:");
PrintDataBlock(innerJoined);

// 2. Left join (all rows from left DataBlock)
var leftJoined = employees.Merge(departments, "Department", MergeMode.Left);
Console.WriteLine("\n2. Left Join (MergeMode.Left) - All rows from employees:");
PrintDataBlock(leftJoined);

// 3. Right join (all rows from right DataBlock)
var rightJoined = employees.Merge(departments, "Department", MergeMode.Right);
Console.WriteLine("\n3. Right Join (MergeMode.Right) - All rows from departments:");
PrintDataBlock(rightJoined);

// 4. Full outer join (all rows from both)
var fullJoined = employees.Merge(departments, "Department", MergeMode.Full);
Console.WriteLine("\n4. Full Outer Join (MergeMode.Full) - All rows from both:");
PrintDataBlock(fullJoined);

// 5. Merge with different key column names
var employeesWithDeptId = new DataBlock();
employeesWithDeptId.AddColumn(new DataColumn("EmployeeId", typeof(int)));
employeesWithDeptId.AddColumn(new DataColumn("Name", typeof(string)));
employeesWithDeptId.AddColumn(new DataColumn("DeptName", typeof(string))); // Different column name
employeesWithDeptId.AddColumn(new DataColumn("Salary", typeof(decimal)));

employeesWithDeptId.AddRow(new object[] { 1, "Alice Johnson", "Engineering", 95000m });
employeesWithDeptId.AddRow(new object[] { 2, "Bob Smith", "Marketing", 72000m });

var mergedWithDifferentKeys = employeesWithDeptId.Merge(
    departments, 
    "DeptName",      // Key column in left DataBlock
    "Department",    // Key column in right DataBlock
    MergeMode.Inner
);
Console.WriteLine("\n5. Merge with different key column names:");
Console.WriteLine("   Left key: 'DeptName', Right key: 'Department'");
PrintDataBlock(mergedWithDifferentKeys);

Console.WriteLine("\n=== Sample Complete ===");

static void PrintDataBlock(DataBlock dataBlock)
{
    if (dataBlock.RowCount == 0)
    {
        Console.WriteLine("   (No rows)");
        return;
    }

    var columnNames = dataBlock.Schema.GetColumnNames().ToArray();
    var cursor = dataBlock.GetRowCursor(columnNames);
    
    // Print header
    Console.WriteLine($"   {string.Join(" | ", columnNames)}");
    Console.WriteLine($"   {new string('-', columnNames.Sum(c => c.Length) + (columnNames.Length - 1) * 3)}");
    
    // Print rows
    int rowCount = 0;
    while (cursor.MoveNext() && rowCount < 10) // Limit to 10 rows for display
    {
        var values = columnNames.Select(col => 
        {
            var val = cursor.GetValue(col);
            if (val is decimal d) return d.ToString("C");
            return val?.ToString() ?? "null";
        });
        Console.WriteLine($"   {string.Join(" | ", values)}");
        rowCount++;
    }
    if (dataBlock.RowCount > 10)
    {
        Console.WriteLine($"   ... ({dataBlock.RowCount - 10} more rows)");
    }
}

