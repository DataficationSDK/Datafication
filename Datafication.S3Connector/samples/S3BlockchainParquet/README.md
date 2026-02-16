# S3BlockchainParquet Sample

Demonstrates loading date-partitioned Parquet data from the AWS Public Blockchain Data S3 bucket.

## Overview

This sample shows how to:
- Access AWS public blockchain datasets
- Work with date-partitioned Parquet files (date=YYYY-MM-DD)
- Use multi-segment loading with error handling
- Stream large blockchain datasets to VelocityDataBlock

## Key Features Demonstrated

### Loading Single Date Partition (Recommended)

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-2",
    BucketName = "aws-public-blockchain",
    ObjectKey = "v1.0/btc/blocks/date=2024-01-01/",  // Single date partition
    AllowMultipleSegments = true
};

using var velocity = new VelocityDataBlock("btc_blocks.dfc");
using var connector = new S3DataConnector(config);
await connector.GetStorageDataAsync(velocity, batchSize: 50000);
```

### Loading Multiple Dates with Consistent Schema

```csharp
// Load recent dates that share the same schema
var dates = new[] { "2024-01-01", "2024-01-02", "2024-01-03" };
foreach (var date in dates)
{
    var config = new S3ConnectorConfiguration
    {
        Region = "us-east-2",
        BucketName = "aws-public-blockchain",
        ObjectKey = $"v1.0/btc/blocks/date={date}/",
        AllowMultipleSegments = true
    };
    using var connector = new S3DataConnector(config);
    await connector.GetStorageDataAsync(velocity, batchSize: 50000);
}
```

## AWS Public Blockchain Data Structure

### Bitcoin (v1.0/btc/)

| Folder | Description |
|--------|-------------|
| `blocks/` | Block headers and metadata |
| `transactions/` | Transaction data |
| `inputs/` | Transaction inputs |
| `outputs/` | Transaction outputs |

### Ethereum (v1.0/eth/)

| Folder | Description |
|--------|-------------|
| `blocks/` | Block data |
| `transactions/` | Transaction details |
| `logs/` | Event logs |
| `token_transfers/` | ERC-20 transfers |
| `traces/` | Internal transactions |
| `contracts/` | Smart contract deployments |

### Available Blockchains

| Blockchain | Path | Provider |
|------------|------|----------|
| Bitcoin | `v1.0/btc/` | AWS |
| Ethereum | `v1.0/eth/` | AWS |
| Arbitrum | `v1.1/sonarx/arbitrum/` | SonarX |
| Base | `v1.1/sonarx/base/` | SonarX |
| XRP Ledger | `v1.1/sonarx/xrp/` | SonarX |
| Stellar | `v1.1/stellar/` | Stellar |
| Cronos | `v1.1/cronos/` | Cronos |

## How to Run

```bash
cd S3BlockchainParquet
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.S3Connector Blockchain Parquet Sample ===

1. AWS Public Blockchain Data
   [Dataset description and available blockchains]

2. Dataset Structure
   [Bitcoin/Ethereum folder structure]

3. Loading Bitcoin Block Data (Single Date)
   Bucket: aws-public-blockchain
   Region: us-east-2
   Path: v1.0/btc/blocks/date=2024-01-01/

   Note: Loading specific date partitions avoids schema evolution
   issues (early data has 17 columns, recent data has 18).

4. Streaming Bitcoin Blocks to VelocityDataBlock
   Loading blocks from 2024-01-01...
   Rows loaded: ~150
   Time: ~2-5 seconds
   File size: ~10 KB

   Schema:
     - hash
     - size
     - stripped_size
     ...

5. Schema Evolution Considerations
   [Column count changes over time]

6-9. [Ethereum structure, performance tips, reference tables]

=== Sample Complete ===
```

## Schema Evolution

Long-running datasets like blockchain data often experience schema changes:

| Time Period | Columns | Notes |
|-------------|---------|-------|
| Early data (2009) | 17 | Original schema |
| Recent data (2024) | 18 | Added new column |

**Best Practices:**
- Load partitions with consistent schemas together
- Use specific date partitions rather than loading entire prefix
- Process different schema versions separately if needed

## Performance Considerations

- Data is updated daily at 00:30 UTC
- Use specific date partitions for faster loading
- Parquet files use Snappy compression
- Dataset is experimental - not for production use
- Cross-region access may be slower (data is in us-east-2)

## Common Use Cases

- Blockchain analytics and research
- Transaction pattern analysis
- Cross-chain data comparison
- Historical block exploration
- Smart contract analysis (Ethereum)

## Related Samples

- **S3OoklaParquet** - Hive-style partitioned Parquet
- **S3ParquetLoad** - Basic Parquet loading
- **S3MultiSegment** - Multi-file loading patterns
- **S3ErrorHandling** - Error handling strategies
