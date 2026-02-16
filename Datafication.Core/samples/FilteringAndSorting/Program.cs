using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core Filtering and Sorting Sample ===\n");

// Create sample employee data
var employees = new DataBlock();
employees.AddColumn(new DataColumn("EmployeeId", typeof(int)));
employees.AddColumn(new DataColumn("Name", typeof(string)));
employees.AddColumn(new DataColumn("Department", typeof(string)));
employees.AddColumn(new DataColumn("Salary", typeof(decimal)));
employees.AddColumn(new DataColumn("HireDate", typeof(DateTime)));

employees.AddRow(new object[] { 1, "Alice Johnson", "Engineering", 95000m, new DateTime(2020, 3, 15) });
employees.AddRow(new object[] { 2, "Bob Smith", "Marketing", 72000m, new DateTime(2021, 6, 1) });
employees.AddRow(new object[] { 3, "Carol White", "Engineering", 88000m, new DateTime(2019, 11, 20) });
employees.AddRow(new object[] { 4, "David Brown", "Sales", 65000m, new DateTime(2022, 1, 10) });
employees.AddRow(new object[] { 5, "Eve Davis", "Engineering", 102000m, new DateTime(2018, 5, 3) });
employees.AddRow(new object[] { 6, "Frank Miller", "Marketing", 75000m, new DateTime(2021, 9, 15) });
employees.AddRow(new object[] { 7, "Grace Lee", "Sales", 70000m, new DateTime(2020, 7, 22) });
employees.AddRow(new object[] { 8, "Henry Wilson", "Engineering", 110000m, new DateTime(2017, 12, 5) });

Console.WriteLine($"Created DataBlock with {employees.RowCount} employees\n");

// 1. Filter using Where with equality
var engineeringEmployees = employees.Where("Department", "Engineering");
Console.WriteLine("1. Where('Department', 'Engineering'):");
PrintDataBlock(engineeringEmployees, "Name", "Department", "Salary");

// 2. Filter with comparison operators
var highEarners = employees.Where("Salary", 80000m, ComparisonOperator.GreaterThan);
Console.WriteLine("\n2. Where('Salary', 80000, ComparisonOperator.GreaterThan):");
PrintDataBlock(highEarners, "Name", "Department", "Salary");

// 3. Filter using multiple conditions (chaining)
var seniorEngineers = employees
    .Where("Department", "Engineering")
    .Where("Salary", 90000m, ComparisonOperator.GreaterThanOrEqual);
Console.WriteLine("\n3. Chained filters - Engineering with Salary >= 90000:");
PrintDataBlock(seniorEngineers, "Name", "Department", "Salary");

// 4. Filter using string operations
var namesStartingWithA = employees.Where("Name", "A", ComparisonOperator.StartsWith);
Console.WriteLine("\n4. Where('Name', 'A', ComparisonOperator.StartsWith):");
PrintDataBlock(namesStartingWithA, "Name", "Department");

var namesContainingSmith = employees.Where("Name", "Smith", ComparisonOperator.Contains);
Console.WriteLine("\n5. Where('Name', 'Smith', ComparisonOperator.Contains):");
PrintDataBlock(namesContainingSmith, "Name", "Department");

// 6. Filter using WhereIn (multiple values)
var selectedDepartments = employees.WhereIn("Department", new[] { "Engineering", "Sales" });
Console.WriteLine("\n6. WhereIn('Department', ['Engineering', 'Sales']):");
PrintDataBlock(selectedDepartments, "Name", "Department", "Salary");

// 7. Filter using WhereNot (exclusion)
var nonMarketingEmployees = employees.WhereNot("Department", "Marketing");
Console.WriteLine("\n7. WhereNot('Department', 'Marketing'):");
PrintDataBlock(nonMarketingEmployees, "Name", "Department");

// 8. Sorting
var sortedBySalaryAsc = employees.Sort(SortDirection.Ascending, "Salary");
Console.WriteLine("\n8. Sort(SortDirection.Ascending, 'Salary'):");
PrintDataBlock(sortedBySalaryAsc, "Name", "Department", "Salary");

var sortedBySalaryDesc = employees.Sort(SortDirection.Descending, "Salary");
Console.WriteLine("\n9. Sort(SortDirection.Descending, 'Salary'):");
PrintDataBlock(sortedBySalaryDesc, "Name", "Department", "Salary");

// 10. Chain sorting with filtering
var topEngineersBySalary = employees
    .Where("Department", "Engineering")
    .Sort(SortDirection.Descending, "Salary");
Console.WriteLine("\n10. Chained: Where('Department', 'Engineering') then Sort(Descending, 'Salary'):");
PrintDataBlock(topEngineersBySalary, "Name", "Department", "Salary");

// 11. Complex filtering with date comparison
var recentHires = employees.Where("HireDate", new DateTime(2020, 1, 1), ComparisonOperator.GreaterThan);
Console.WriteLine("\n11. Where('HireDate', 2020-01-01, ComparisonOperator.GreaterThan):");
PrintDataBlock(recentHires, "Name", "Department", "HireDate");

Console.WriteLine("\n=== Sample Complete ===");

static void PrintDataBlock(DataBlock dataBlock, params string[] columns)
{
    if (dataBlock.RowCount == 0)
    {
        Console.WriteLine("   (No rows match the criteria)");
        return;
    }

    var cursor = dataBlock.GetRowCursor(columns);
    while (cursor.MoveNext())
    {
        var values = columns.Select(col => cursor.GetValue(col)?.ToString() ?? "null");
        Console.WriteLine($"   {string.Join(" | ", values)}");
    }
}

