using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core Removing Duplicates Sample ===\n");

// Create sample data with duplicates
var employees = new DataBlock();
employees.AddColumn(new DataColumn("EmployeeId", typeof(int)));
employees.AddColumn(new DataColumn("Name", typeof(string)));
employees.AddColumn(new DataColumn("Email", typeof(string)));
employees.AddColumn(new DataColumn("Department", typeof(string)));
employees.AddColumn(new DataColumn("Salary", typeof(decimal)));

// Add rows with some duplicates
employees.AddRow(new object[] { 1, "Alice Johnson", "alice@example.com", "Engineering", 95000m });
employees.AddRow(new object[] { 2, "Bob Smith", "bob@example.com", "Marketing", 72000m });
employees.AddRow(new object[] { 1, "Alice Johnson", "alice@example.com", "Engineering", 95000m }); // Duplicate
employees.AddRow(new object[] { 3, "Carol White", "carol@example.com", "Engineering", 88000m });
employees.AddRow(new object[] { 2, "Bob Smith", "bob@example.com", "Marketing", 75000m }); // Duplicate with different salary
employees.AddRow(new object[] { 4, "David Brown", "david@example.com", "Sales", 65000m });
employees.AddRow(new object[] { 5, "Alice Johnson", "alice.new@example.com", "Engineering", 98000m }); // Same name, different email

Console.WriteLine("Original DataBlock with duplicates:");
PrintDataBlock(employees);

// 1. Drop duplicates based on all columns (keep first occurrence)
var uniqueRows = employees.DropDuplicates();
Console.WriteLine("\n1. DropDuplicates() - Keep first occurrence (default):");
PrintDataBlock(uniqueRows);

// 2. Keep last occurrence of each duplicate
var lastOccurrences = employees.DropDuplicates(KeepDuplicateMode.Last);
Console.WriteLine("\n2. DropDuplicates(KeepDuplicateMode.Last) - Keep last occurrence:");
PrintDataBlock(lastOccurrences);

// 3. Remove all duplicates (keep only unique rows)
var onlyUnique = employees.DropDuplicates(KeepDuplicateMode.None);
Console.WriteLine("\n3. DropDuplicates(KeepDuplicateMode.None) - Remove all duplicates:");
PrintDataBlock(onlyUnique);

// 4. Drop duplicates based on specific columns - EmployeeId
var uniqueById = employees.DropDuplicates(KeepDuplicateMode.First, "EmployeeId");
Console.WriteLine("\n4. DropDuplicates(KeepDuplicateMode.First, 'EmployeeId'):");
PrintDataBlock(uniqueById);

// 5. Drop duplicates based on multiple columns
var uniqueByNameDept = employees.DropDuplicates(KeepDuplicateMode.First, "Name", "Department");
Console.WriteLine("\n5. DropDuplicates(KeepDuplicateMode.First, 'Name', 'Department'):");
PrintDataBlock(uniqueByNameDept);

// 6. Real-world example: Clean up duplicate customer records
var customers = new DataBlock();
customers.AddColumn(new DataColumn("CustomerId", typeof(int)));
customers.AddColumn(new DataColumn("Email", typeof(string)));
customers.AddColumn(new DataColumn("Name", typeof(string)));
customers.AddColumn(new DataColumn("Status", typeof(string)));
customers.AddColumn(new DataColumn("LastUpdated", typeof(DateTime)));

customers.AddRow(new object[] { 1, "john@example.com", "John Doe", "Active", new DateTime(2023, 1, 1) });
customers.AddRow(new object[] { 2, "jane@example.com", "Jane Smith", "Active", new DateTime(2023, 2, 1) });
customers.AddRow(new object[] { 1, "john@example.com", "John Doe", "Inactive", new DateTime(2023, 3, 1) }); // Duplicate email, newer
customers.AddRow(new object[] { 3, "bob@example.com", "Bob Johnson", "Active", new DateTime(2023, 1, 15) });

var uniqueCustomers = customers
    .DropDuplicates(KeepDuplicateMode.Last, "Email")  // Keep latest record per email
    .Where("Status", "Active")
    .Sort(SortDirection.Ascending, "Name");
Console.WriteLine("\n6. Real-world example - Clean customer records:");
Console.WriteLine("   DropDuplicates(KeepDuplicateMode.Last, 'Email') then filter Active:");
PrintDataBlock(uniqueCustomers);

// 7. Chain with other data quality operations
var rawEmployees = new DataBlock();
rawEmployees.AddColumn(new DataColumn("EmployeeId", typeof(int?)));
rawEmployees.AddColumn(new DataColumn("Name", typeof(string)));
rawEmployees.AddColumn(new DataColumn("Status", typeof(string)));

rawEmployees.AddRow(new object[] { 1, "Alice", "Active" });
rawEmployees.AddRow(new object[] { null, "Bob", "Active" }); // Missing ID
rawEmployees.AddRow(new object[] { 1, "Alice", "Active" }); // Duplicate
rawEmployees.AddRow(new object[] { 2, null, "Active" }); // Missing name
rawEmployees.AddRow(new object[] { 3, "Carol", "Active" });

var cleanedEmployees = rawEmployees
    .DropNulls(DropNullMode.Any)  // Remove rows with missing data
    .DropDuplicates(KeepDuplicateMode.First, "EmployeeId")  // Remove duplicate IDs
    .Where("Status", "Active")
    .Sort(SortDirection.Ascending, "Name");
Console.WriteLine("\n7. Chained data quality operations:");
Console.WriteLine("   DropNulls -> DropDuplicates -> Where -> Sort:");
PrintDataBlock(cleanedEmployees);

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
    
    // Print rows
    while (cursor.MoveNext())
    {
        var values = columnNames.Select(col => 
        {
            var val = cursor.GetValue(col);
            if (val is decimal d) return d.ToString("C");
            if (val is DateTime dt) return dt.ToString("yyyy-MM-dd");
            return val?.ToString() ?? "null";
        });
        Console.WriteLine($"   {string.Join(" | ", values)}");
    }
}

