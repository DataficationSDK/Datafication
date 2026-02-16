using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core Grouping and Aggregation Sample ===\n");

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

// 1. Group by a column
var groupedByDepartment = employees.GroupBy("Department");
Console.WriteLine("1. GroupBy('Department'):");
var groupInfo = groupedByDepartment.Info();
Console.WriteLine($"   Number of groups: {groupInfo.RowCount}");
PrintDataBlock(groupInfo, "Group", "Count");

// Access individual groups
Console.WriteLine("\n   Individual groups:");
for (int i = 0; i < groupedByDepartment.Count; i++)
{
    var groupKey = groupedByDepartment.GetGroupKey(i);
    var group = groupedByDepartment.GetGroup(i);
    Console.WriteLine($"   Group '{groupKey}': {group.RowCount} employees");
}

// 2. Aggregate operations on individual columns
Console.WriteLine("\n2. Aggregate Operations on Salary column:");
var minSalary = employees.Min("Salary");
Console.WriteLine($"   Min('Salary'): {minSalary[0, "Salary"]:C}");

var maxSalary = employees.Max("Salary");
Console.WriteLine($"   Max('Salary'): {maxSalary[0, "Salary"]:C}");

var avgSalary = employees.Mean("Salary");
Console.WriteLine($"   Mean('Salary'): {avgSalary[0, "Salary"]:C}");

var totalSalary = employees.Sum("Salary");
Console.WriteLine($"   Sum('Salary'): {totalSalary[0, "Salary"]:C}");

var salaryStdDev = employees.StandardDeviation("Salary");
Console.WriteLine($"   StandardDeviation('Salary'): {salaryStdDev[0, "Salary"]:C}");

var salaryVariance = employees.Variance("Salary");
Console.WriteLine($"   Variance('Salary'): {salaryVariance[0, "Salary"]:C}");

// 3. Calculate percentiles
var medianSalary = employees.Percentile(0.5, "Salary");
Console.WriteLine($"\n3. Percentile(0.5, 'Salary') - Median: {medianSalary[0, "Salary"]:C}");

var p95Salary = employees.Percentile(0.95, "Salary");
Console.WriteLine($"   Percentile(0.95, 'Salary') - 95th percentile: {p95Salary[0, "Salary"]:C}");

// 4. Get row count for a specific column (or all columns)
var sizes = employees.Size("Department");
Console.WriteLine($"\n4. Size('Department') - Row count: {sizes[0, "Department"]}");

// 5. Aggregate on all numeric columns (when no column specified)
var allAverages = employees.Mean();
Console.WriteLine("\n5. Mean() - Computes mean for all numeric columns:");
PrintDataBlock(allAverages);

// 6. Group by and aggregate using GroupByAggregate
var departmentStats = employees.GroupByAggregate(
    "Department",
    "Salary",
    AggregationType.Mean,
    "AvgSalary"
);
Console.WriteLine("\n6. GroupByAggregate('Department', 'Salary', AggregationType.Mean):");
PrintDataBlock(departmentStats);

// 7. Multiple aggregations - aggregate each group individually
Console.WriteLine("\n7. Multiple aggregations per department:");
var grouped = employees.GroupBy("Department");
var result = new DataBlock();
result.AddColumn(new DataColumn("Department", typeof(string)));
result.AddColumn(new DataColumn("Count", typeof(int)));
result.AddColumn(new DataColumn("AvgSalary", typeof(decimal)));

for (int i = 0; i < grouped.Count; i++)
{
    var dept = grouped.GetGroupKey(i).ToString() ?? "Unknown";
    var group = grouped.GetGroup(i);
    var count = group.RowCount;
    var averageSalary = group.Mean("Salary");
    var avg = averageSalary.RowCount > 0 ? (decimal)Convert.ToDouble(averageSalary[0, "Salary"]) : 0m;
    result.AddRow(new object[] { dept, count, avg });
}
PrintDataBlock(result);

Console.WriteLine("\n=== Sample Complete ===");

static void PrintDataBlock(DataBlock dataBlock, params string[]? columns)
{
    if (dataBlock.RowCount == 0)
    {
        Console.WriteLine("   (No data)");
        return;
    }

    // Handle params array: when called with no columns argument, params creates an empty array, not null
    var colsToPrint = (columns == null || columns.Length == 0) 
        ? dataBlock.Schema.GetColumnNames().ToArray() 
        : columns;
    
    if (colsToPrint.Length == 0)
    {
        Console.WriteLine("   (No columns to display)");
        return;
    }
    
    var cursor = dataBlock.GetRowCursor(colsToPrint);
    
    // Print header
    Console.WriteLine($"   {string.Join(" | ", colsToPrint)}");
    var separatorLength = colsToPrint.Sum(c => c.Length) + (colsToPrint.Length - 1) * 3;
    Console.WriteLine($"   {new string('-', Math.Max(0, separatorLength))}");
    
    // Print rows
    while (cursor.MoveNext())
    {
        var values = colsToPrint.Select(col => 
        {
            var val = cursor.GetValue(col);
            if (val is decimal d) return d.ToString("C");
            if (val is double db) return db.ToString("F2");
            return val?.ToString() ?? "null";
        });
        Console.WriteLine($"   {string.Join(" | ", values)}");
    }
}

