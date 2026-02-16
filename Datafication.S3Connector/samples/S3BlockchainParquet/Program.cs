using Datafication.Connectors.S3Connector;
using Datafication.Core.Data;
using Datafication.Storage.Velocity;

Console.WriteLine("=== Datafication.S3Connector Blockchain Parquet Sample ===\n");
Console.WriteLine("This sample demonstrates loading date-partitioned Parquet data");
Console.WriteLine("from the AWS Public Blockchain Data S3 bucket.\n");

// Output paths
var dataPath = Path.Combine(Path.GetTempPath(), "s3_blockchain_samples");
if (Directory.Exists(dataPath))
    Directory.Delete(dataPath, recursive: true);
Directory.CreateDirectory(dataPath);

try
{
    // 1. About AWS Public Blockchain Data
    Console.WriteLine("1. AWS Public Blockchain Data");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   AWS provides free access to blockchain datasets optimized for analytics.");
    Console.WriteLine("   Data is stored in compressed Parquet files, partitioned by date.");
    Console.WriteLine();
    Console.WriteLine("   Bucket: aws-public-blockchain");
    Console.WriteLine("   Region: us-east-2");
    Console.WriteLine();
    Console.WriteLine("   Available blockchains:");
    Console.WriteLine("     - Bitcoin (BTC):     s3://aws-public-blockchain/v1.0/btc/");
    Console.WriteLine("     - Ethereum (ETH):    s3://aws-public-blockchain/v1.0/eth/");
    Console.WriteLine("     - Arbitrum:          s3://aws-public-blockchain/v1.1/sonarx/arbitrum/");
    Console.WriteLine("     - Base:              s3://aws-public-blockchain/v1.1/sonarx/base/");
    Console.WriteLine("     - XRP Ledger:        s3://aws-public-blockchain/v1.1/sonarx/xrp/");
    Console.WriteLine("     - Stellar:           s3://aws-public-blockchain/v1.1/stellar/");
    Console.WriteLine("     - Cronos:            s3://aws-public-blockchain/v1.1/cronos/");
    Console.WriteLine();

    // 2. Dataset Structure
    Console.WriteLine("2. Dataset Structure");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   Bitcoin structure (v1.0/btc/):");
    Console.WriteLine("     - blocks/         Block headers and metadata");
    Console.WriteLine("     - transactions/   Transaction data");
    Console.WriteLine("     - inputs/         Transaction inputs");
    Console.WriteLine("     - outputs/        Transaction outputs");
    Console.WriteLine();
    Console.WriteLine("   Each folder contains date-partitioned Parquet files:");
    Console.WriteLine("     date=YYYY-MM-DD/part-00000-*.snappy.parquet");
    Console.WriteLine();

    // 3. Load Bitcoin Blocks (Single Date Partition)
    Console.WriteLine("3. Loading Bitcoin Block Data (Single Date)");
    Console.WriteLine("   " + new string('-', 60));

    // Load a specific date's block data
    // Note: Loading specific date partitions avoids schema evolution issues
    // (early blockchain data has 17 columns, later data has 18 columns)
    var btcConfig = new S3ConnectorConfiguration
    {
        Region = "us-east-2",
        BucketName = "aws-public-blockchain",
        ObjectKey = "v1.0/btc/blocks/date=2024-01-01/",  // Single date partition
        AllowMultipleSegments = true
    };

    Console.WriteLine($"   Bucket: {btcConfig.BucketName}");
    Console.WriteLine($"   Region: {btcConfig.Region}");
    Console.WriteLine($"   Path:   {btcConfig.ObjectKey}");
    Console.WriteLine();
    Console.WriteLine("   Note: Loading specific date partitions avoids schema evolution");
    Console.WriteLine("   issues (early data has 17 columns, recent data has 18).\n");

    // 4. Streaming Bitcoin Blocks to VelocityDataBlock
    Console.WriteLine("4. Streaming Bitcoin Blocks to VelocityDataBlock");
    Console.WriteLine("   " + new string('-', 60));

    var velocityPath = Path.Combine(dataPath, "btc_blocks.dfc");
    var velocityOptions = new VelocityOptions
    {
        DefaultCompression = VelocityCompressionType.LZ4,
        AutoCompactionEnabled = false
    };

    using (var velocity = new VelocityDataBlock(velocityPath, velocityOptions))
    {
        var connector = new S3DataConnector(btcConfig);
        try
        {
            Console.WriteLine("   Loading blocks from 2024-01-01...\n");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Use error handler to handle any issues gracefully
            btcConfig.ErrorHandler = (ex) =>
            {
                Console.WriteLine($"   Warning: {ex.Message}");
            };

            await connector.GetStorageDataAsync(velocity, batchSize: 50000);
            await velocity.FlushAsync();

            stopwatch.Stop();

            if (velocity.RowCount > 0)
            {
                var fileInfo = new FileInfo(velocityPath);
                Console.WriteLine($"   Rows loaded: {velocity.RowCount:N0}");
                Console.WriteLine($"   Time: {stopwatch.ElapsedMilliseconds:N0}ms");
                Console.WriteLine($"   File size: {fileInfo.Length / 1024.0 / 1024.0:F2} MB");
                Console.WriteLine();

                // Display schema
                Console.WriteLine("   Schema:");
                foreach (var col in velocity.Schema.GetColumnNames().Take(8))
                {
                    Console.WriteLine($"     - {col}");
                }
                if (velocity.Schema.Count > 8)
                    Console.WriteLine($"     ... and {velocity.Schema.Count - 8} more columns");
            }
            else
            {
                Console.WriteLine("   No data loaded. The date partition may not exist.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Note: {ex.Message}");
            Console.WriteLine("   Try a different date partition.");
        }
        finally
        {
            connector.Dispose();
        }
    }
    Console.WriteLine();

    // 5. Schema Evolution in Long-Running Datasets
    Console.WriteLine("5. Schema Evolution Considerations");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   Long-running datasets like blockchain data often have schema changes:");
    Console.WriteLine();
    Console.WriteLine("   - Early Bitcoin data (2009): 17 columns");
    Console.WriteLine("   - Recent Bitcoin data (2024): 18 columns");
    Console.WriteLine();
    Console.WriteLine("   When loading multiple date partitions:");
    Console.WriteLine("   - Load partitions with consistent schemas together");
    Console.WriteLine("   - Or process each partition separately");
    Console.WriteLine();
    Console.WriteLine("   ```csharp");
    Console.WriteLine("   // Load a date range with consistent schema (recent data)");
    Console.WriteLine("   var dates = new[] { \"2024-01-01\", \"2024-01-02\", \"2024-01-03\" };");
    Console.WriteLine("   foreach (var date in dates)");
    Console.WriteLine("   {");
    Console.WriteLine("       var config = new S3ConnectorConfiguration");
    Console.WriteLine("       {");
    Console.WriteLine("           Region = \"us-east-2\",");
    Console.WriteLine("           BucketName = \"aws-public-blockchain\",");
    Console.WriteLine("           ObjectKey = $\"v1.0/btc/blocks/date={date}/\",");
    Console.WriteLine("           AllowMultipleSegments = true");
    Console.WriteLine("       };");
    Console.WriteLine("       using var connector = new S3DataConnector(config);");
    Console.WriteLine("       await connector.GetStorageDataAsync(velocity, batchSize: 50000);");
    Console.WriteLine("   }");
    Console.WriteLine("   ```\n");

    // 6. Ethereum Data Example
    Console.WriteLine("6. Ethereum Data Structure");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   Ethereum (v1.0/eth/) contains similar partitions:");
    Console.WriteLine("     - blocks/         Block data");
    Console.WriteLine("     - transactions/   Transaction details");
    Console.WriteLine("     - logs/           Event logs");
    Console.WriteLine("     - token_transfers/ ERC-20 transfers");
    Console.WriteLine("     - traces/         Internal transactions");
    Console.WriteLine("     - contracts/      Smart contract deployments");
    Console.WriteLine();
    Console.WriteLine("   ```csharp");
    Console.WriteLine("   var ethConfig = new S3ConnectorConfiguration");
    Console.WriteLine("   {");
    Console.WriteLine("       Region = \"us-east-2\",");
    Console.WriteLine("       BucketName = \"aws-public-blockchain\",");
    Console.WriteLine("       ObjectKey = \"v1.0/eth/transactions/date=2024-06-15/\",");
    Console.WriteLine("       AllowMultipleSegments = true");
    Console.WriteLine("   };");
    Console.WriteLine("   ```\n");

    // 7. Performance Considerations
    Console.WriteLine("7. Performance Considerations");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   - Data is updated daily at 00:30 UTC");
    Console.WriteLine("   - Use specific date partitions for faster loading");
    Console.WriteLine("   - Parquet files use Snappy compression");
    Console.WriteLine("   - Dataset is experimental - not for production use");
    Console.WriteLine("   - Cross-region access may be slower (data is in us-east-2)");
    Console.WriteLine();

    // 8. Available Datasets Reference
    Console.WriteLine("8. Available Blockchain Datasets");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine($"   {"Blockchain",-15} {"Path",-40} {"Provider",-10}");
    Console.WriteLine("   " + new string('-', 65));
    Console.WriteLine($"   {"Bitcoin",-15} {"v1.0/btc/",-40} {"AWS",-10}");
    Console.WriteLine($"   {"Ethereum",-15} {"v1.0/eth/",-40} {"AWS",-10}");
    Console.WriteLine($"   {"Arbitrum",-15} {"v1.1/sonarx/arbitrum/",-40} {"SonarX",-10}");
    Console.WriteLine($"   {"Aptos",-15} {"v1.1/sonarx/aptos/",-40} {"SonarX",-10}");
    Console.WriteLine($"   {"Base",-15} {"v1.1/sonarx/base/",-40} {"SonarX",-10}");
    Console.WriteLine($"   {"XRP Ledger",-15} {"v1.1/sonarx/xrp/",-40} {"SonarX",-10}");
    Console.WriteLine($"   {"Stellar",-15} {"v1.1/stellar/",-40} {"Stellar",-10}");
    Console.WriteLine($"   {"Cronos",-15} {"v1.1/cronos/",-40} {"Cronos",-10}");
    Console.WriteLine();

    // 9. Use Cases
    Console.WriteLine("9. Common Use Cases");
    Console.WriteLine("   " + new string('-', 60));
    Console.WriteLine("   - Blockchain analytics and research");
    Console.WriteLine("   - Transaction pattern analysis");
    Console.WriteLine("   - Cross-chain data comparison");
    Console.WriteLine("   - Historical block exploration");
    Console.WriteLine("   - Smart contract analysis (Ethereum)");
    Console.WriteLine();
}
finally
{
    // Cleanup
    if (Directory.Exists(dataPath))
    {
        Directory.Delete(dataPath, recursive: true);
        Console.WriteLine($"Cleaned up: {dataPath}");
    }
}

Console.WriteLine("\n=== Sample Complete ===");
