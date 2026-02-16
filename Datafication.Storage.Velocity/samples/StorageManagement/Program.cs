using Datafication.Storage.Velocity;
using Datafication.Core.Data;

Console.WriteLine("=== Datafication.Storage.Velocity Storage Management Sample ===\n");

await RunSampleAsync();

async Task RunSampleAsync()
{
    var dataPath = Path.Combine(Path.GetTempPath(), "velocity_storage_sample");

    // Clean up any stale files from previous runs
    if (Directory.Exists(dataPath))
        Directory.Delete(dataPath, recursive: true);
    Directory.CreateDirectory(dataPath);

    // 1. Create with VelocityOptions for storage configuration
    Console.WriteLine("1. Creating VelocityDataBlock with storage options:");
    var filePath = Path.Combine(dataPath, "managed_data.dfc");
    var options = new VelocityOptions
    {
        PrimaryKeyColumn = "Id",
        TargetSegmentSizeBytes = 64 * 1024 * 1024, // 64MB
        DefaultCompression = VelocityCompressionType.LZ4,
        EnableAutoCompression = true,
        AutoCompactionEnabled = true,
        AutoCompactionTrigger = VelocityCompactionTrigger.SegmentCount,
        MaxSegmentsBeforeCompaction = 5
    };

    using var velocityBlock = new VelocityDataBlock(filePath, options);

    velocityBlock.AddColumn(new DataColumn("Id", typeof(int)));
    velocityBlock.AddColumn(new DataColumn("Name", typeof(string)));
    velocityBlock.AddColumn(new DataColumn("Value", typeof(decimal)));

    Console.WriteLine($"   File path: {filePath}");
    Console.WriteLine($"   Primary key: {options.PrimaryKeyColumn}");
    Console.WriteLine($"   Target segment size: {options.TargetSegmentSizeBytes / 1024 / 1024} MB");
    Console.WriteLine($"   Compression: {options.DefaultCompression}");
    Console.WriteLine($"   Auto-compaction: {options.AutoCompactionEnabled}");
    Console.WriteLine($"   Compaction trigger: {options.AutoCompactionTrigger}");
    Console.WriteLine($"   Max segments before compaction: {options.MaxSegmentsBeforeCompaction}\n");

    // Add initial data
    Console.WriteLine("2. Adding initial data (500 rows):");
    for (int i = 0; i < 500; i++)
    {
        velocityBlock.AddRow(new object[] { i, $"Item-{i:D4}", (decimal)(i * 10.50) });
    }
    await velocityBlock.FlushAsync();
    Console.WriteLine($"   Rows added: {velocityBlock.RowCount}");

    // 3. Get storage statistics
    Console.WriteLine("\n3. Initial storage statistics:");
    var stats = await velocityBlock.GetStorageStatsAsync();
    PrintStorageStats(stats);

    // 4. Perform some deletions
    Console.WriteLine("\n4. Deleting 100 rows (20% of data):");
    for (int i = 0; i < 100; i++)
    {
        await velocityBlock.DeleteRowAsync($"{i * 5}"); // Delete every 5th row
    }
    await velocityBlock.FlushAsync();

    Console.WriteLine($"   Active rows: {velocityBlock.RowCount}");

    // 5. Storage stats after deletions
    Console.WriteLine("\n5. Storage statistics after deletions:");
    stats = await velocityBlock.GetStorageStatsAsync();
    PrintStorageStats(stats);

    // 6. Manual compaction
    Console.WriteLine("\n6. Manual compaction:");
    if (stats.CanCompact)
    {
        Console.WriteLine("   Compaction recommended - running CompactAsync()...");
        await velocityBlock.CompactAsync();
        Console.WriteLine("   Compaction completed!");

        stats = await velocityBlock.GetStorageStatsAsync();
        Console.WriteLine("\n   Statistics after compaction:");
        PrintStorageStats(stats);
    }
    else
    {
        Console.WriteLine("   Compaction not currently needed");
    }

    // 7. Compaction strategies
    Console.WriteLine("\n7. Compaction strategies:");
    Console.WriteLine("   VelocityCompactionStrategy.Quick    - Fast, less optimization");
    Console.WriteLine("   VelocityCompactionStrategy.Standard - Balanced (default)");
    Console.WriteLine("   VelocityCompactionStrategy.Aggressive - Maximum optimization");

    // 8. Configure auto-compaction
    Console.WriteLine("\n8. Configuring auto-compaction:");
    velocityBlock.ConfigureAutoCompaction(
        enabled: true,
        trigger: VelocityCompactionTrigger.SegmentCount,
        threshold: 5
    );
    Console.WriteLine("   Trigger: SegmentCount");
    Console.WriteLine("   Threshold: 5 segments");

    // Reconfigure to percentage-based
    velocityBlock.ConfigureAutoCompaction(
        enabled: true,
        trigger: VelocityCompactionTrigger.DeletedRowPercentage,
        threshold: 15
    );
    Console.WriteLine("\n   Reconfigured to:");
    Console.WriteLine("   Trigger: DeletedRowPercentage");
    Console.WriteLine("   Threshold: 15%");

    // 9. Enable background compaction
    Console.WriteLine("\n9. Background compaction:");
    velocityBlock.EnableBackgroundCompaction(enabled: true);
    Console.WriteLine("   Background compaction enabled");
    Console.WriteLine("   Compaction runs automatically when thresholds are met");

    // 10. Primary key index statistics
    Console.WriteLine("\n10. Primary key index statistics:");
    var (indexedKeys, indexBuilt, segments) = velocityBlock.GetPrimaryKeyIndexStats();
    Console.WriteLine($"    Indexed keys: {indexedKeys}");
    Console.WriteLine($"    Index built: {indexBuilt}");
    Console.WriteLine($"    Segments: {segments}");

    // 11. Flush to ensure persistence
    Console.WriteLine("\n11. Flushing changes to disk:");
    await velocityBlock.FlushAsync();
    Console.WriteLine("    All pending changes written to disk");

    // 12. Schema information
    Console.WriteLine("\n12. Schema information (Info method):");
    var info = velocityBlock.Info();
    for (int i = 0; i < info.RowCount; i++)
    {
        Console.WriteLine($"    {info[i, "Column"],-15} {info[i, "Type"],-20} Nulls: {info[i, "Null Count"]}");
    }

    // 13. Final statistics
    Console.WriteLine("\n13. Final storage statistics:");
    stats = await velocityBlock.GetStorageStatsAsync();
    PrintStorageStats(stats);

    // Disable background compaction and dispose before cleanup
    velocityBlock.EnableBackgroundCompaction(enabled: false);
    velocityBlock.Dispose();

    // Cleanup
    Directory.Delete(dataPath, recursive: true);

    Console.WriteLine("\n=== Sample Complete ===");
    Console.WriteLine("\nStorage Management Best Practices:");
    Console.WriteLine("  - Enable auto-compaction for automatic optimization");
    Console.WriteLine("  - Use background compaction for non-blocking maintenance");
    Console.WriteLine("  - Call FlushAsync() to ensure data persistence");
    Console.WriteLine("  - Monitor storage stats for efficiency insights");
    Console.WriteLine("  - Compact when deleted percentage exceeds 15-20%");
}

static void PrintStorageStats(StorageStats stats)
{
    Console.WriteLine($"    Total rows: {stats.TotalRows:N0}");
    Console.WriteLine($"    Active rows: {stats.ActiveRows:N0}");
    Console.WriteLine($"    Deleted rows: {stats.DeletedRows:N0}");
    Console.WriteLine($"    Deleted %: {stats.DeletedPercentage:F1}%");
    Console.WriteLine($"    Storage files: {stats.StorageFiles}");
    Console.WriteLine($"    Estimated size: {stats.EstimatedSizeBytes / 1024.0:F1} KB");
    Console.WriteLine($"    Can compact: {stats.CanCompact}");
}
