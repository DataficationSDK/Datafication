# Datafication

A high-performance columnar data engine for .NET that brings pandas-style data manipulation to the .NET ecosystem.

## The Problem

.NET developers have long faced a gap when working with data at scale. While Python offers pandas, Polars, and a rich ecosystem for data manipulation, .NET developers are often left cobbling together solutions or context-switching to Python for analytical workloads.

Common pain points:
- No native columnar data structures optimized for analytical queries
- Limited options for filtering, grouping, and aggregating large datasets in memory
- Performance bottlenecks when working with millions of rows
- Forced to choose between staying in .NET or switching to Python for data work

## The Solution

Datafication is a complete data platform built from the ground up for .NET. At its core is the **DataBlock**—a column-oriented, in-memory analytical database that makes working with data intuitive and fast.

Key design principles:
- **Column-oriented storage** for cache-efficient analytical operations
- **Immutable operations** that return new DataBlocks, enabling clean method chaining
- **Schema-aware** with rich metadata support (labels, descriptions, formats, constraints)
- **Performance-first** with optimized code paths for common operations

## Core Capabilities

### DataBlock: In-Memory Analytical Database

The DataBlock provides a complete API for data manipulation:

- **Filtering**: `Where()`, `WhereIn()`, `Filter()` with support for all comparison operators
- **Projection**: `Select()` for column selection
- **Sorting**: `Sort()` with ascending/descending support
- **Grouping**: `GroupBy()` and `GroupByAggregate()` for SQL-style aggregations
- **Merging**: `Merge()` with inner, left, right, and full outer join modes
- **Aggregations**: `Sum()`, `Mean()`, `Min()`, `Max()`, `Variance()`, `StandardDeviation()`, `Percentile()`
- **Computed Columns**: `Compute()` with a rich expression language supporting 100+ functions
- **Window Functions**: Moving averages, lag/lead, row numbers, and more

### Velocity: High-Performance Columnar Storage

The Velocity storage engine implements the DFC (Datafication Columnar Format)—a binary columnar format optimized for analytical workloads.

Performance characteristics:
- **60M+ rows/second** query throughput (benchmarked on a 2020 iMac with NVMe Gen 3 SSD)
- **O(1) CRUD operations** via tombstone management and write-ahead logging
- **Automatic compaction** with configurable strategies (BinPack, Sort, ZOrder)
- **LZ4 and Deflate compression** with intelligent codec selection
- **MVCC support** for concurrent read/write access

### Data Connectors

Import and export data from multiple sources:

| Format | Capabilities |
|--------|--------------|
| CSV | Configurable delimiters, header detection, type inference |
| Parquet | Row group support, schema preservation, streaming I/O |
| JSON | Array/object support, nested data flattening |
| Excel | Multi-sheet support, cell type preservation |
| ADO.NET | SQL Server, PostgreSQL, MySQL via standard .NET data access |
| Web | HTML table extraction, CSS selectors, link parsing |
| S3 | AWS and S3-compatible storage, automatic format detection, multi-segment streaming |

## Quick Example

```csharp
// Load data from CSV
var connector = new CsvDataConnector(new CsvConnectorConfiguration
{
    FilePath = "sales.csv"
});
var sales = await connector.GetDataAsync();

// Analyze: filter, compute, group, and aggregate
var result = sales
    .Where("region", "North America", ComparisonOperator.Equals)
    .Compute("profit_margin", "profit / revenue")
    .Where("profit_margin", 0.25, ComparisonOperator.GreaterThan)
    .GroupByAggregate("product_category", "revenue", AggregationType.Sum, "total_revenue")
    .Sort(SortDirection.Descending, "total_revenue");

// Output
Console.WriteLine(result.ToTextTable());
```

## Public Repositories

The following components have public repositories with documentation and samples:

| Repository | Description |
|------------|-------------|
| [Datafication.Core](https://github.com/DataficationSDK/Datafication.Core) | Core runtime with DataBlock, schemas, and streaming I/O |
| [Datafication.Storage.Velocity](https://github.com/DataficationSDK/Datafication.Storage.Velocity) | High-performance columnar storage engine |
| [Datafication.Server.Core](https://github.com/DataficationSDK/Datafication.Server.Core) | Server API for exposing DataBlocks as RESTful endpoints |
| [Datafication.ParquetConnector](https://github.com/DataficationSDK/Datafication.ParquetConnector) | Apache Parquet reading and writing |
| [Datafication.CsvConnector](https://github.com/DataficationSDK/Datafication.CsvConnector) | CSV import/export with type inference |
| [Datafication.ExcelConnector](https://github.com/DataficationSDK/Datafication.ExcelConnector) | Excel (.xlsx/.xls) reading and writing |
| [Datafication.JsonConnector](https://github.com/DataficationSDK/Datafication.JsonConnector) | JSON file and API data loading |
| [Datafication.WebConnector](https://github.com/DataficationSDK/Datafication.WebConnector) | Web scraping with HTML tables, CSS selectors, and browser rendering |
| [Datafication.S3Connector](https://github.com/DataficationSDK/Datafication.S3Connector) | Amazon S3 and S3-compatible storage access |

## Beyond the Public Repos

Datafication is a complete data platform. The SDK is currently in pre-release, and we are actively creating repositories for each layer of the platform. Each repository includes technical documentation and samples demonstrating how to use the library.

Additional capabilities beyond the current public repositories include:

- **Vector database** for semantic search and similarity operations
- **ML/AI integration** with ONNX Runtime support for embeddings and inference
- **Clustering algorithms** (K-Means, DBSCAN) for pattern discovery

These components will be released as separate repositories over time.

## Getting Started

> **Note**: Datafication is currently in pre-release. The packages are now available on nuget.org.

For detailed class and method documentation, see the [API Reference](https://datafication.co/help/api/index.html).

**Installation:**

```bash
dotnet add package Datafication.Core
dotnet add package Datafication.Storage.Velocity
dotnet add package Datafication.Server.Core
dotnet add package Datafication.CsvConnector
dotnet add package Datafication.ExcelConnector
dotnet add package Datafication.JsonConnector
dotnet add package Datafication.ParquetConnector
dotnet add package Datafication.WebConnector
dotnet add package Datafication.S3Connector
```

**Running the Samples:**

Each repository includes a `samples` directory with runnable sample projects:

```bash
cd samples/BasicOperations
dotnet run
```

## Requirements

- .NET 8.0 or later
- Windows, macOS, or Linux

## License

See individual repository licenses for details.
