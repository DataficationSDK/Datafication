using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core Data Quality Sample ===\n");

// Create sample data with quality issues
var rawEmployees = new DataBlock();
rawEmployees.AddColumn(new DataColumn("EmployeeId", typeof(int?)));
rawEmployees.AddColumn(new DataColumn("Name", typeof(string)));
rawEmployees.AddColumn(new DataColumn("Email", typeof(string)));
rawEmployees.AddColumn(new DataColumn("Department", typeof(string)));
rawEmployees.AddColumn(new DataColumn("Salary", typeof(decimal?)));
rawEmployees.AddColumn(new DataColumn("Status", typeof(string)));

// Add rows with various quality issues
rawEmployees.AddRow(new object[] { 1, "Alice Johnson", "alice@example.com", "Engineering", 95000m, "Active" });
rawEmployees.AddRow(new object[] { null, "Bob Smith", "bob@example.com", "Marketing", 72000m, "Active" }); // Missing ID
rawEmployees.AddRow(new object[] { 2, null, "carol@example.com", "Engineering", 88000m, "Active" }); // Missing name
rawEmployees.AddRow(new object[] { 3, "David Brown", "david@example.com", "Sales", null, "Active" }); // Missing salary
rawEmployees.AddRow(new object[] { 1, "Alice Johnson", "alice@example.com", "Engineering", 95000m, "Active" }); // Duplicate
rawEmployees.AddRow(new object[] { 4, "Eve Davis", "eve@example.com", "Engineering", 102000m, "Inactive" });
rawEmployees.AddRow(new object[] { 5, "Frank Miller", "frank@example.com", null, 70000m, "Active" }); // Missing department
rawEmployees.AddRow(new object[] { 6, "Grace Lee", "grace@example.com", "Sales", 65000m, "Active" });

Console.WriteLine("Raw Data with Quality Issues:");
Console.WriteLine($"   Total rows: {rawEmployees.RowCount}\n");
PrintDataBlock(rawEmployees);

// 1. Schema inspection
var schema = rawEmployees.Schema;
Console.WriteLine("\n1. Schema Inspection:");
Console.WriteLine($"   Number of columns: {schema.Count}");
Console.WriteLine($"   Column names: {string.Join(", ", schema.GetColumnNames())}");

foreach (var colName in schema.GetColumnNames())
{
    var col = rawEmployees.GetColumn(colName);
    Console.WriteLine($"   - {colName}: {col.DataType.GetClrType().Name}");
}

// 2. Info() method for detailed statistics
var info = rawEmployees.Info();
Console.WriteLine("\n2. Info() - Detailed Statistics:");
PrintDataBlock(info);

// 3. Data quality checks
Console.WriteLine("\n3. Data Quality Checks:");

// Check for nulls in critical columns using Info()
info = rawEmployees.Info();
var employeeIdInfo = info.Where("Column", "EmployeeId");
var nameInfo = info.Where("Column", "Name");
var salaryInfo = info.Where("Column", "Salary");

if (employeeIdInfo.RowCount > 0)
{
    var nullCount = (int)employeeIdInfo[0, "Null Count"];
    Console.WriteLine($"   Rows with null EmployeeId: {nullCount}");
}

if (nameInfo.RowCount > 0)
{
    var nullCount = (int)nameInfo[0, "Null Count"];
    Console.WriteLine($"   Rows with null Name: {nullCount}");
}

if (salaryInfo.RowCount > 0)
{
    var nullCount = (int)salaryInfo[0, "Null Count"];
    Console.WriteLine($"   Rows with null Salary: {nullCount}");
}

// Check for duplicates
var duplicates = rawEmployees.DropDuplicates(KeepDuplicateMode.None);
var duplicateCount = rawEmployees.RowCount - duplicates.RowCount;
Console.WriteLine($"   Duplicate rows: {duplicateCount}");

// 4. Combining quality operations
var cleaned = rawEmployees
    .DropNulls(DropNullMode.Any)  // Remove rows with missing data
    .DropDuplicates(KeepDuplicateMode.First, "EmployeeId")  // Remove duplicate IDs
    .Where("Status", "Active")  // Filter active employees
    .Sort(SortDirection.Ascending, "Name");  // Sort by name

Console.WriteLine("\n4. Combined Quality Operations:");
Console.WriteLine("   DropNulls -> DropDuplicates -> Where -> Sort");
Console.WriteLine($"   Original rows: {rawEmployees.RowCount}");
Console.WriteLine($"   Cleaned rows: {cleaned.RowCount}");
PrintDataBlock(cleaned);

// 5. Quality report generation
Console.WriteLine("\n5. Quality Report:");
var qualityReport = new DataBlock();
qualityReport.AddColumn(new DataColumn("Metric", typeof(string)));
qualityReport.AddColumn(new DataColumn("Value", typeof(object)));

var employeeIdNullCount = employeeIdInfo.RowCount > 0 ? (int)employeeIdInfo[0, "Null Count"] : 0;
var nameNullCount = nameInfo.RowCount > 0 ? (int)nameInfo[0, "Null Count"] : 0;
var salaryNullCount = salaryInfo.RowCount > 0 ? (int)salaryInfo[0, "Null Count"] : 0;

qualityReport.AddRow(new object[] { "Total Rows", rawEmployees.RowCount });
qualityReport.AddRow(new object[] { "Rows with Null EmployeeId", employeeIdNullCount });
qualityReport.AddRow(new object[] { "Rows with Null Name", nameNullCount });
qualityReport.AddRow(new object[] { "Rows with Null Salary", salaryNullCount });
qualityReport.AddRow(new object[] { "Duplicate Rows", duplicateCount });
qualityReport.AddRow(new object[] { "Cleaned Rows", cleaned.RowCount });
qualityReport.AddRow(new object[] { "Data Quality Score", $"{((double)cleaned.RowCount / rawEmployees.RowCount * 100):F1}%" });

PrintDataBlock(qualityReport);

// 6. Column-specific quality checks
Console.WriteLine("\n6. Column-Specific Quality Checks:");
salaryInfo = rawEmployees.Info();
var salaryRow = salaryInfo.Where("Column", "Salary").GetRowCursor().MoveNext() ? salaryInfo.Where("Column", "Salary") : null;
if (salaryRow != null && salaryRow.RowCount > 0)
{
    var cursor = salaryRow.GetRowCursor();
    if (cursor.MoveNext())
    {
        var nonNullCount = cursor.GetValue("Non-Null Count");
        var nullCount = cursor.GetValue("Null Count");
        Console.WriteLine($"   Salary column:");
        Console.WriteLine($"     Non-null values: {nonNullCount}");
        Console.WriteLine($"     Null values: {nullCount}");
    }
}

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
    Console.WriteLine($"   {new string('-', Math.Min(100, columnNames.Sum(c => c.Length) + (columnNames.Length - 1) * 3))}");
    
    // Print rows (limit to 10 for display)
    int rowCount = 0;
    while (cursor.MoveNext() && rowCount < 10)
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

