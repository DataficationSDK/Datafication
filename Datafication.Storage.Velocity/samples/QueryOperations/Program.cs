using Datafication.Storage.Velocity;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Storage.Velocity Query Operations Sample ===\n");

await RunSampleAsync();

async Task RunSampleAsync()
{
    var dataPath = Path.Combine(Path.GetTempPath(), "velocity_query_sample");

    // Clean up any stale files from previous runs
    if (Directory.Exists(dataPath))
        Directory.Delete(dataPath, recursive: true);
    Directory.CreateDirectory(dataPath);

    var filePath = Path.Combine(dataPath, "employees.dfc");

    using var velocityBlock = new VelocityDataBlock(filePath);

    velocityBlock.AddColumn(new DataColumn("EmployeeId", typeof(int)));
    velocityBlock.AddColumn(new DataColumn("Name", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Email", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Department", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Salary", typeof(decimal)));
    velocityBlock.AddColumn(new DataColumn("YearsExperience", typeof(int)));

    // Add sample data
    velocityBlock.AddRow(new object[] { 1, "Alice Johnson", "alice@example.com", "Engineering", 95000m, 5 });
    velocityBlock.AddRow(new object[] { 2, "Bob Smith", "bob@company.org", "Marketing", 72000m, 3 });
    velocityBlock.AddRow(new object[] { 3, "Carol White", "carol@example.com", "Engineering", 88000m, 4 });
    velocityBlock.AddRow(new object[] { 4, "David Brown", "david@company.org", "Sales", 65000m, 2 });
    velocityBlock.AddRow(new object[] { 5, "Eve Davis", "eve@example.com", "Engineering", 102000m, 7 });
    velocityBlock.AddRow(new object[] { 6, "Frank Miller", "frank@example.com", "HR", 58000m, 1 });
    velocityBlock.AddRow(new object[] { 7, "Grace Lee", "grace@company.org", "Marketing", 78000m, 4 });
    velocityBlock.AddRow(new object[] { 8, "Henry Wilson", "henry@example.com", "Engineering", 92000m, 6 });
    await velocityBlock.FlushAsync();

    Console.WriteLine("Sample data (8 employees):\n");

    // 1. Simple Where filter
    Console.WriteLine("1. Where filter - Engineering department:");
    var engineers = velocityBlock
        .Where("Department", "Engineering")
        .Execute();
    PrintDataBlock(engineers, "Name", "Department", "Salary");

    // 2. Multiple Where conditions
    Console.WriteLine("\n2. Multiple Where conditions - Engineering with Salary > 90000:");
    var seniorEngineers = velocityBlock
        .Where("Department", "Engineering")
        .Where("Salary", 90000m, ComparisonOperator.GreaterThan)
        .Execute();
    PrintDataBlock(seniorEngineers, "Name", "Salary", "YearsExperience");

    // 3. String pattern matching with WhereContains
    Console.WriteLine("\n3. WhereContains - Email containing '@example.com':");
    var exampleEmails = velocityBlock
        .WhereContains("Email", "@example.com")
        .Execute();
    PrintDataBlock(exampleEmails, "Name", "Email");

    // 4. WhereStartsWith
    Console.WriteLine("\n4. WhereStartsWith - Names starting with 'E':");
    var eNames = velocityBlock
        .WhereStartsWith("Name", "E")
        .Execute();
    PrintDataBlock(eNames, "Name", "Department");

    // 5. WhereEndsWith
    Console.WriteLine("\n5. WhereEndsWith - Email ending with '.org':");
    var orgEmails = velocityBlock
        .WhereEndsWith("Email", ".org")
        .Execute();
    PrintDataBlock(orgEmails, "Name", "Email");

    // 6. Select specific columns
    Console.WriteLine("\n6. Select - Only Name and Salary columns:");
    var nameAndSalary = velocityBlock
        .Select("Name", "Salary")
        .Execute();
    PrintDataBlock(nameAndSalary, "Name", "Salary");

    // 7. Sort ascending
    Console.WriteLine("\n7. Sort ascending by Salary:");
    var sortedAsc = velocityBlock
        .Sort(SortDirection.Ascending, "Salary")
        .Execute();
    PrintDataBlock(sortedAsc, "Name", "Salary");

    // 8. Sort descending
    Console.WriteLine("\n8. Sort descending by YearsExperience:");
    var sortedDesc = velocityBlock
        .Sort(SortDirection.Descending, "YearsExperience")
        .Execute();
    PrintDataBlock(sortedDesc, "Name", "YearsExperience");

    // 9. Head - First N rows
    Console.WriteLine("\n9. Head(3) - Top 3 rows:");
    var top3 = velocityBlock
        .Head(3)
        .Execute();
    PrintDataBlock(top3, "EmployeeId", "Name");

    // 10. Tail - Last N rows
    Console.WriteLine("\n10. Tail(3) - Bottom 3 rows:");
    var bottom3 = velocityBlock
        .Tail(3)
        .Execute();
    PrintDataBlock(bottom3, "EmployeeId", "Name");

    // 11. Sample - Random rows
    Console.WriteLine("\n11. Sample(3, seed: 42) - Random sample of 3:");
    var sample = velocityBlock
        .Sample(3, seed: 42)
        .Execute();
    PrintDataBlock(sample, "Name", "Department");

    // 12. Combined query with deferred execution
    Console.WriteLine("\n12. Combined query - Top 3 highest paid in Engineering:");
    var topEngineers = velocityBlock
        .Where("Department", "Engineering")
        .Sort(SortDirection.Descending, "Salary")
        .Head(3)
        .Select("Name", "Salary", "YearsExperience")
        .Execute();
    PrintDataBlock(topEngineers, "Name", "Salary", "YearsExperience");

    // 13. Complex query chain
    Console.WriteLine("\n13. Complex query - Experienced employees with high salary:");
    var experienced = velocityBlock
        .Where("YearsExperience", 3, ComparisonOperator.GreaterThanOrEqual)
        .Where("Salary", 75000m, ComparisonOperator.GreaterThan)
        .Sort(SortDirection.Descending, "Salary")
        .Select("Name", "Department", "Salary", "YearsExperience")
        .Execute();
    PrintDataBlock(experienced, "Name", "Department", "Salary", "YearsExperience");

    // Dispose the VelocityDataBlock before cleanup
    velocityBlock.Dispose();

    // Cleanup
    Directory.Delete(dataPath, recursive: true);

    Console.WriteLine("\n=== Sample Complete ===");
    Console.WriteLine("\nKey Takeaways:");
    Console.WriteLine("  - Deferred execution optimizes query chains");
    Console.WriteLine("  - Multiple Where clauses combine for single-pass filtering");
    Console.WriteLine("  - String matching uses SIMD acceleration");
    Console.WriteLine("  - Call Execute() to run the query plan");
}

static void PrintDataBlock(DataBlock data, params string[] columns)
{
    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{col,-18} ");
    }
    Console.WriteLine();

    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{new string('-', 18)} ");
    }
    Console.WriteLine();

    for (int i = 0; i < data.RowCount; i++)
    {
        Console.Write("   ");
        foreach (var col in columns)
        {
            var value = data[i, col];
            string formatted = value switch
            {
                decimal d => $"{d:C}",
                _ => value?.ToString() ?? "null"
            };
            Console.Write($"{formatted,-18} ");
        }
        Console.WriteLine();
    }
}
