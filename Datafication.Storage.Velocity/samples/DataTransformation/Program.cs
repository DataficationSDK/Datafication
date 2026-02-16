using Datafication.Storage.Velocity;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Storage.Velocity Data Transformation Sample ===\n");

await RunSampleAsync();

async Task RunSampleAsync()
{
    var dataPath = Path.Combine(Path.GetTempPath(), "velocity_transform_sample");

    // Clean up any stale files from previous runs
    if (Directory.Exists(dataPath))
        Directory.Delete(dataPath, recursive: true);
    Directory.CreateDirectory(dataPath);

    // 1. DropNulls - Remove rows with null values
    Console.WriteLine("1. DropNulls - Remove rows with null values:");
    var nullsPath = Path.Combine(dataPath, "with_nulls.dfc");
    CleanupDfcFiles(nullsPath);
    using (var nullsBlock = new VelocityDataBlock(nullsPath))
    {
        nullsBlock.AddColumn(new DataColumn("Id", typeof(int)));
        nullsBlock.AddColumn(new DataColumn("Name", typeof(string)));
        nullsBlock.AddColumn(new DataColumn("Email", typeof(string)));
        nullsBlock.AddColumn(new DataColumn("Phone", typeof(string)));

        nullsBlock.AddRow(new object[] { 1, "Alice", "alice@example.com", "555-0101" });
        nullsBlock.AddRow(new object[] { 2, "Bob", null!, "555-0102" });
        nullsBlock.AddRow(new object[] { 3, "Carol", "carol@example.com", null! });
        nullsBlock.AddRow(new object[] { 4, null!, "david@example.com", "555-0104" });
        nullsBlock.AddRow(new object[] { 5, "Eve", "eve@example.com", "555-0105" });
        await nullsBlock.FlushAsync();

        Console.WriteLine("   Original data (5 rows with some nulls):");
        PrintDataBlock(nullsBlock.Clone(), "Id", "Name", "Email", "Phone");

        var noEmailNulls = nullsBlock
            .DropNulls("Email")
            .Execute();
        Console.WriteLine($"\n   After DropNulls('Email'): {noEmailNulls.RowCount} rows");

        var noAnyNulls = nullsBlock
            .DropNulls(DropNullMode.Any);
        Console.WriteLine($"   After DropNulls(Any): {noAnyNulls.RowCount} rows");
    }

    // 2. DropDuplicates - Remove duplicate rows
    Console.WriteLine("\n2. DropDuplicates - Remove duplicate rows:");
    var dupsPath = Path.Combine(dataPath, "with_dups.dfc");
    CleanupDfcFiles(dupsPath);
    using (var dupsBlock = new VelocityDataBlock(dupsPath))
    {
        dupsBlock.AddColumn(new DataColumn("Email", typeof(string)));
        dupsBlock.AddColumn(new DataColumn("Name", typeof(string)));
        dupsBlock.AddColumn(new DataColumn("LastLogin", typeof(DateTime)));

        dupsBlock.AddRow(new object[] { "alice@example.com", "Alice Johnson", new DateTime(2025, 1, 1) });
        dupsBlock.AddRow(new object[] { "bob@example.com", "Bob Smith", new DateTime(2025, 1, 2) });
        dupsBlock.AddRow(new object[] { "alice@example.com", "Alice J.", new DateTime(2025, 1, 3) });
        dupsBlock.AddRow(new object[] { "carol@example.com", "Carol White", new DateTime(2025, 1, 4) });
        dupsBlock.AddRow(new object[] { "bob@example.com", "Bob S.", new DateTime(2025, 1, 5) });
        await dupsBlock.FlushAsync();

        Console.WriteLine("   Original data (5 rows with duplicate emails):");
        PrintDataBlock(dupsBlock.Clone(), "Email", "Name", "LastLogin");

        var keepFirst = dupsBlock
            .DropDuplicates(KeepDuplicateMode.First, "Email")
            .Execute();
        Console.WriteLine($"\n   DropDuplicates(First, 'Email'): {keepFirst.RowCount} unique emails");
        PrintDataBlock(keepFirst, "Email", "Name", "LastLogin");

        var keepLast = dupsBlock
            .DropDuplicates(KeepDuplicateMode.Last, "Email")
            .Execute();
        Console.WriteLine($"\n   DropDuplicates(Last, 'Email'): {keepLast.RowCount} unique (latest records)");
        PrintDataBlock(keepLast, "Email", "Name", "LastLogin");
    }

    // 3. FillNulls - Fill missing values
    Console.WriteLine("\n3. FillNulls - Fill missing values:");
    var fillPath = Path.Combine(dataPath, "fill_nulls.dfc");
    CleanupDfcFiles(fillPath);
    using (var fillBlock = new VelocityDataBlock(fillPath))
    {
        fillBlock.AddColumn(new DataColumn("Date", typeof(DateTime)));
        fillBlock.AddColumn(new DataColumn("Temperature", typeof(double)));
        fillBlock.AddColumn(new DataColumn("Humidity", typeof(double)));

        fillBlock.AddRow(new object[] { new DateTime(2025, 1, 1), 72.5, 45.0 });
        fillBlock.AddRow(new object[] { new DateTime(2025, 1, 2), DBNull.Value, 48.0 });
        fillBlock.AddRow(new object[] { new DateTime(2025, 1, 3), 68.0, DBNull.Value });
        fillBlock.AddRow(new object[] { new DateTime(2025, 1, 4), DBNull.Value, 52.0 });
        fillBlock.AddRow(new object[] { new DateTime(2025, 1, 5), 75.0, 50.0 });
        await fillBlock.FlushAsync();

        Console.WriteLine("   Original data with nulls:");
        PrintDataBlock(fillBlock.Clone(), "Date", "Temperature", "Humidity");

        var forwardFilled = fillBlock
            .FillNulls(FillMethod.ForwardFill, "Temperature")
            .Execute();
        Console.WriteLine("\n   After ForwardFill on Temperature:");
        PrintDataBlock(forwardFilled, "Date", "Temperature", "Humidity");

        var meanFilled = fillBlock
            .FillNulls(FillMethod.Mean, "Temperature", "Humidity")
            .Execute();
        Console.WriteLine("\n   After Mean fill on both columns:");
        PrintDataBlock(meanFilled, "Date", "Temperature", "Humidity");
    }

    // 4. Melt - Unpivot from wide to long format
    Console.WriteLine("\n4. Melt - Unpivot wide to long format:");
    var meltPath = Path.Combine(dataPath, "wide_data.dfc");
    CleanupDfcFiles(meltPath);
    using (var wideBlock = new VelocityDataBlock(meltPath))
    {
        wideBlock.AddColumn(new DataColumn("Product", typeof(string)));
        wideBlock.AddColumn(new DataColumn("Q1_Sales", typeof(double)));
        wideBlock.AddColumn(new DataColumn("Q2_Sales", typeof(double)));
        wideBlock.AddColumn(new DataColumn("Q3_Sales", typeof(double)));

        wideBlock.AddRow(new object[] { "Widget", 100.0, 150.0, 120.0 });
        wideBlock.AddRow(new object[] { "Gadget", 200.0, 180.0, 220.0 });
        await wideBlock.FlushAsync();

        Console.WriteLine("   Wide format data:");
        PrintDataBlock(wideBlock.Clone(), "Product", "Q1_Sales", "Q2_Sales", "Q3_Sales");

        var melted = wideBlock
            .Melt(
                fixedColumns: new[] { "Product" },
                meltedColumnName: "Quarter",
                meltedValueName: "Sales"
            )
            .Execute();
        Console.WriteLine("\n   After Melt (long format):");
        // Get actual column names from the melted result
        var meltedColumns = melted.Schema.GetColumnNames().ToArray();
        PrintDataBlock(melted, meltedColumns);
    }

    // 5. Merge - Join two datasets
    Console.WriteLine("\n5. Merge - Join two datasets:");
    var empPath = Path.Combine(dataPath, "employees.dfc");
    CleanupDfcFiles(empPath);
    using (var empBlock = new VelocityDataBlock(empPath))
    {
        empBlock.AddColumn(new DataColumn("EmpId", typeof(int)));
        empBlock.AddColumn(new DataColumn("Name", typeof(string)));
        empBlock.AddColumn(new DataColumn("DeptId", typeof(int)));

        empBlock.AddRow(new object[] { 1, "Alice", 10 });
        empBlock.AddRow(new object[] { 2, "Bob", 20 });
        empBlock.AddRow(new object[] { 3, "Carol", 10 });
        empBlock.AddRow(new object[] { 4, "David", 30 });
        await empBlock.FlushAsync();

        var deptBlock = new DataBlock();
        deptBlock.AddColumn(new DataColumn("DeptId", typeof(int)));
        deptBlock.AddColumn(new DataColumn("DeptName", typeof(string)));
        deptBlock.AddRow(new object[] { 10, "Engineering" });
        deptBlock.AddRow(new object[] { 20, "Marketing" });
        deptBlock.AddRow(new object[] { 40, "Finance" });

        Console.WriteLine("   Employees:");
        PrintDataBlock(empBlock.Clone(), "EmpId", "Name", "DeptId");

        Console.WriteLine("\n   Departments:");
        PrintDataBlock(deptBlock, "DeptId", "DeptName");

        var innerJoin = empBlock
            .Merge(deptBlock, "DeptId", "DeptId", MergeMode.Inner)
            .Execute();
        Console.WriteLine($"\n   Inner Join result ({innerJoin.RowCount} rows):");
        // Get actual column names from the merge result
        var innerJoinColumns = innerJoin.Schema.GetColumnNames().ToArray();
        PrintDataBlock(innerJoin, innerJoinColumns);

        var leftJoin = empBlock
            .Merge(deptBlock, "DeptId", "DeptId", MergeMode.Left)
            .Execute();
        Console.WriteLine($"\n   Left Join result ({leftJoin.RowCount} rows):");
        // Get actual column names from the merge result
        var leftJoinColumns = leftJoin.Schema.GetColumnNames().ToArray();
        PrintDataBlock(leftJoin, leftJoinColumns);
    }

    // 6. Data Quality Pipeline
    Console.WriteLine("\n6. Data Quality Pipeline (chained transformations):");
    var pipelinePath = Path.Combine(dataPath, "raw_data.dfc");
    CleanupDfcFiles(pipelinePath);
    using (var rawBlock = new VelocityDataBlock(pipelinePath))
    {
        rawBlock.AddColumn(new DataColumn("UserId", typeof(string)));
        rawBlock.AddColumn(new DataColumn("Email", typeof(string)));
        rawBlock.AddColumn(new DataColumn("Score", typeof(double)));
        rawBlock.AddColumn(new DataColumn("Status", typeof(string)));

        rawBlock.AddRow(new object[] { "U001", "alice@example.com", 85.0, "Active" });
        rawBlock.AddRow(new object[] { "U002", null!, 72.0, "Active" });
        rawBlock.AddRow(new object[] { "U003", "carol@example.com", DBNull.Value, "Inactive" });
        rawBlock.AddRow(new object[] { "U001", "alice.new@example.com", 90.0, "Active" });
        rawBlock.AddRow(new object[] { "U004", "david@example.com", 65.0, "Active" });
        await rawBlock.FlushAsync();

        Console.WriteLine("   Raw data (with nulls and duplicates):");
        PrintDataBlock(rawBlock.Clone(), "UserId", "Email", "Score", "Status");

        var cleanedData = rawBlock
            .Where("Status", "Active")
            .DropNulls("Email")
            .FillNulls(FillMethod.Mean, "Score")
            .DropDuplicates(KeepDuplicateMode.Last, "UserId")
            .Sort(SortDirection.Ascending, "UserId")
            .Execute();

        Console.WriteLine("\n   After pipeline (Active, no null emails, filled scores, deduped):");
        PrintDataBlock(cleanedData, "UserId", "Email", "Score", "Status");
    }

    // Cleanup
    Directory.Delete(dataPath, recursive: true);

    Console.WriteLine("\n=== Sample Complete ===");
    Console.WriteLine("\nTransformation Operations:");
    Console.WriteLine("  - DropNulls: Remove rows with null values");
    Console.WriteLine("  - DropDuplicates: Remove duplicate rows");
    Console.WriteLine("  - FillNulls: Fill missing values");
    Console.WriteLine("  - Melt: Unpivot wide to long format");
    Console.WriteLine("  - Merge: Join datasets (Inner, Left, Right, Full)");
}

static void PrintDataBlock(DataBlock data, params string[] columns)
{
    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{col,-20} ");
    }
    Console.WriteLine();

    Console.Write("   ");
    foreach (var col in columns)
    {
        Console.Write($"{new string('-', 20)} ");
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
                null => "(null)",
                DBNull _ => "(null)",
                DateTime dt => dt.ToString("yyyy-MM-dd"),
                double d => $"{d:F1}",
                _ => value.ToString() ?? "(null)"
            };
            Console.Write($"{formatted,-20} ");
        }
        Console.WriteLine();
    }
}

/// <summary>
/// Cleans up DFC files including the main file and any segment files.
/// This ensures samples start fresh on each run.
/// </summary>
static void CleanupDfcFiles(string basePath)
{
    // Delete main file
    if (File.Exists(basePath))
        File.Delete(basePath);

    // Delete segment files (e.g., basePath_segment_0001.dfc)
    var directory = Path.GetDirectoryName(basePath) ?? ".";
    var baseFileName = Path.GetFileNameWithoutExtension(basePath);
    var extension = Path.GetExtension(basePath);

    foreach (var segmentFile in Directory.GetFiles(directory, $"{baseFileName}_segment_*{extension}"))
    {
        File.Delete(segmentFile);
    }

    // Delete any related metadata files (tombstones, indexes, etc.)
    foreach (var metaFile in Directory.GetFiles(directory, $"{baseFileName}*"))
    {
        if (metaFile != basePath) // Don't try to delete the main file again
        {
            try { File.Delete(metaFile); } catch { /* ignore cleanup errors */ }
        }
    }
}
