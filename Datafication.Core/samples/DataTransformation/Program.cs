using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Core Data Transformation Sample ===\n");

// Create sample data with some nulls
var employees = new DataBlock();
employees.AddColumn(new DataColumn("EmployeeId", typeof(int)));
employees.AddColumn(new DataColumn("Name", typeof(string)));
employees.AddColumn(new DataColumn("Department", typeof(string)));
employees.AddColumn(new DataColumn("Salary", typeof(decimal?)));
employees.AddColumn(new DataColumn("Bonus", typeof(decimal?)));

employees.AddRow(new object[] { 1, "Alice Johnson", "Engineering", 95000m, 5000m });
employees.AddRow(new object[] { 2, "Bob Smith", null, null, null });
employees.AddRow(new object[] { 3, "Carol White", "Engineering", 88000m, null });
employees.AddRow(new object[] { 4, "David Brown", "Sales", null, 2000m });
employees.AddRow(new object[] { 5, "Eve Davis", "Engineering", 102000m, 6000m });
employees.AddRow(new object[] { 6, "Frank Miller", null, 78000m, 3000m });

Console.WriteLine("Original DataBlock:");
PrintDataBlock(employees);

// 1. Transpose operations
var transposed = employees.Transpose();
Console.WriteLine("\n1. Transpose() - Rows and columns swapped:");
PrintDataBlock(transposed.Head(5));

var transposedWithHeaders = employees.Transpose("Name");
Console.WriteLine("\n2. Transpose('Name') - Using Name column as headers:");
PrintDataBlock(transposedWithHeaders.Head(5));

// 2. Melt (unpivot) from wide to long format
var melted = employees.Melt(
    fixedColumns: new[] { "EmployeeId", "Name" },
    meltedColumnName: "Attribute",
    meltedValueName: "Value"
);
Console.WriteLine("\n3. Melt() - Wide to long format:");
Console.WriteLine("   Fixed columns: EmployeeId, Name");
PrintDataBlock(melted);

// 3. DropNulls
var noNulls = employees.DropNulls(DropNullMode.Any);
Console.WriteLine("\n4. DropNulls(DropNullMode.Any) - Drop if any column is null:");
PrintDataBlock(noNulls);

var allNulls = employees.DropNulls(DropNullMode.All);
Console.WriteLine("\n5. DropNulls(DropNullMode.All) - Drop only if all columns are null:");
PrintDataBlock(allNulls);

// 4. FillNulls with various strategies
var forwardFilled = employees.FillNulls(FillMethod.ForwardFill, "Salary");
Console.WriteLine("\n6. FillNulls(FillMethod.ForwardFill, 'Salary'):");
PrintDataBlock(forwardFilled);

var backwardFilled = employees.FillNulls(FillMethod.BackwardFill, "Bonus");
Console.WriteLine("\n7. FillNulls(FillMethod.BackwardFill, 'Bonus'):");
PrintDataBlock(backwardFilled);

var constantFilled = employees.FillNulls(FillMethod.ConstantValue, 0.0, "Bonus");
Console.WriteLine("\n8. FillNulls(FillMethod.ConstantValue, 0.0, 'Bonus'):");
PrintDataBlock(constantFilled);

var meanFilled = employees.FillNulls(FillMethod.Mean, "Salary");
Console.WriteLine("\n9. FillNulls(FillMethod.Mean, 'Salary'):");
Console.WriteLine("   (Mean of 95000, 88000, 102000, 78000 = 90750)");
PrintDataBlock(meanFilled);

var medianFilled = employees.FillNulls(FillMethod.Median, "Salary");
Console.WriteLine("\n10. FillNulls(FillMethod.Median, 'Salary'):");
Console.WriteLine("   (Median of 78000, 88000, 95000, 102000 = 91500)");
PrintDataBlock(medianFilled);

var modeFilled = employees.FillNulls(FillMethod.Mode, "Department");
Console.WriteLine("\n11. FillNulls(FillMethod.Mode, 'Department'):");
Console.WriteLine("   (Fills with most frequent value: 'Engineering')");
PrintDataBlock(modeFilled);

// Create time series data for interpolation
var sensors = new DataBlock();
sensors.AddColumn(new DataColumn("Time", typeof(int)));
sensors.AddColumn(new DataColumn("Temperature", typeof(double?)));
sensors.AddRow(new object[] { 1, 20.5 });
sensors.AddRow(new object[] { 2, null });
sensors.AddRow(new object[] { 3, null });
sensors.AddRow(new object[] { 4, 23.0 });
sensors.AddRow(new object[] { 5, null });
sensors.AddRow(new object[] { 6, 24.5 });

var interpolated = sensors.FillNulls(FillMethod.LinearInterpolation, "Temperature");
Console.WriteLine("\n12. FillNulls(FillMethod.LinearInterpolation, 'Temperature'):");
PrintDataBlock(interpolated);

// 5. Chain multiple fill operations
var cleaned = employees
    .FillNulls(FillMethod.ForwardFill, "Salary")
    .FillNulls(FillMethod.Mean, "Bonus")
    .FillNulls(FillMethod.ConstantValue, (object)"Unknown", "Department");
Console.WriteLine("\n13. Chained FillNulls operations:");
PrintDataBlock(cleaned);

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
    Console.WriteLine($"   {new string('-', Math.Min(80, columnNames.Sum(c => c.Length) + (columnNames.Length - 1) * 3))}");
    
    // Print rows (limit to 10 for display)
    int rowCount = 0;
    while (cursor.MoveNext() && rowCount < 10)
    {
        var values = columnNames.Select(col => 
        {
            var val = cursor.GetValue(col);
            if (val is decimal d) return d.ToString("C");
            if (val is double db) return db.ToString("F2");
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

