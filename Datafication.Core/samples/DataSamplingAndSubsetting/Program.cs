using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core Data Sampling and Subsetting Sample ===\n");

// Create sample data with many rows
var employees = new DataBlock();
employees.AddColumn(new DataColumn("EmployeeId", typeof(int)));
employees.AddColumn(new DataColumn("Name", typeof(string)));
employees.AddColumn(new DataColumn("Department", typeof(string)));
employees.AddColumn(new DataColumn("Salary", typeof(decimal)));

// Add 20 employees
for (int i = 1; i <= 20; i++)
{
    var dept = i % 3 == 0 ? "Engineering" : i % 3 == 1 ? "Marketing" : "Sales";
    var salary = 50000m + (i * 2000m);
    employees.AddRow(new object[] { i, $"Employee {i}", dept, salary });
}

Console.WriteLine($"Created DataBlock with {employees.RowCount} employees\n");

// 1. Get first N rows (Head)
var first5 = employees.Head(5);
Console.WriteLine("1. Head(5) - First 5 rows:");
PrintDataBlock(first5);

// 2. Get last N rows (Tail)
var last3 = employees.Tail(3);
Console.WriteLine("\n2. Tail(3) - Last 3 rows:");
PrintDataBlock(last3);

// 3. Get random sample of rows
var randomSample = employees.Sample(5);
Console.WriteLine("\n3. Sample(5) - Random sample of 5 rows:");
PrintDataBlock(randomSample);

// 4. Get random sample with seed for reproducibility
var reproducibleSample1 = employees.Sample(5, seed: 42);
var reproducibleSample2 = employees.Sample(5, seed: 42);
Console.WriteLine("\n4. Sample(5, seed: 42) - Reproducible random sample:");
Console.WriteLine("   First sample:");
PrintDataBlock(reproducibleSample1);
Console.WriteLine("   Second sample (same seed, should match):");
PrintDataBlock(reproducibleSample2);
Console.WriteLine($"   Samples match: {DataBlocksEqual(reproducibleSample1, reproducibleSample2)}");

// 5. Efficiently copy a range of rows
var middleRows = employees.CopyRowRange(startRow: 5, rowCount: 10);
Console.WriteLine("\n5. CopyRowRange(startRow: 5, rowCount: 10) - Middle rows:");
PrintDataBlock(middleRows);

// 6. Practical use cases
Console.WriteLine("\n6. Practical Use Cases:");

// Use case 1: Preview data
var preview = employees.Head(3);
Console.WriteLine("   Preview first 3 rows:");
PrintDataBlock(preview);

// Use case 2: Get latest records
var latest = employees.Tail(5).Sort(SortDirection.Descending, "EmployeeId");
Console.WriteLine("\n   Latest 5 records (sorted by ID descending):");
PrintDataBlock(latest);

// Use case 3: Random sampling for testing
var testSample = employees.Sample(10, seed: 123);
Console.WriteLine("\n   Random sample for testing (seed: 123):");
PrintDataBlock(testSample);

// Use case 4: Extract specific range
var range = employees.CopyRowRange(startRow: 10, rowCount: 5);
Console.WriteLine("\n   Extract rows 10-14:");
PrintDataBlock(range);

// Use case 5: Chain with other operations
var topEarners = employees
    .Sort(SortDirection.Descending, "Salary")
    .Head(5);
Console.WriteLine("\n7. Chained operations - Top 5 earners:");
Console.WriteLine("   Sort(Descending, 'Salary') then Head(5):");
PrintDataBlock(topEarners);

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
    while (cursor.MoveNext())
    {
        var values = columnNames.Select(col => 
        {
            var val = cursor.GetValue(col);
            if (val is decimal d) return d.ToString("C");
            return val?.ToString() ?? "null";
        });
        Console.WriteLine($"   {string.Join(" | ", values)}");
    }
}

static bool DataBlocksEqual(DataBlock db1, DataBlock db2)
{
    if (db1.RowCount != db2.RowCount) return false;
    if (db1.Schema.Count != db2.Schema.Count) return false;
    
    var cols = db1.Schema.GetColumnNames().ToArray();
    for (int i = 0; i < db1.RowCount; i++)
    {
        foreach (var col in cols)
        {
            if (!Equals(db1[i, col], db2[i, col])) return false;
        }
    }
    return true;
}

