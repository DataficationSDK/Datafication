# S3PublicDataset Sample

Demonstrates working with well-known public AWS S3 datasets that support anonymous access.

## Overview

This sample shows how to:
- Access multiple public S3 buckets
- Load different types of data files (CSV, text)
- Work with real-world datasets
- Discover and explore public data sources

## Key Features Demonstrated

### Loading from NOAA Climate Data

```csharp
var config = new S3ConnectorConfiguration
{
    Region = "us-east-1",
    BucketName = "noaa-ghcn-pds",
    ObjectKey = "ghcnd-countries.txt"
};

using var connector = new S3DataConnector(config);
var data = await connector.GetDataAsync();
```

### Using Different Regions

```csharp
// COVID-19 Data Lake (us-east-2)
var config = new S3ConnectorConfiguration
{
    Region = "us-east-2",
    BucketName = "covid19-lake",
    ObjectKey = "path/to/data.csv"
};
```

### Proper Resource Management

```csharp
using (var connector = new S3DataConnector(config))
{
    var data = await connector.GetDataAsync();
    // Process data...
} // Connector automatically disposed
```

## How to Run

```bash
cd S3PublicDataset
dotnet restore
dotnet run
```

## Expected Output

```
=== Datafication.S3Connector Public Dataset Sample ===

1. Loading NOAA Climate Data (Country List)...
   Loaded 218 countries

   Country Codes in GHCN Network:
   AC | Antigua and Barbuda
   AE | United Arab Emirates
   ...

2. Loading NOAA Inventory Data...
   Loaded 750,000+ inventory records
   Load time: ~3-8 seconds

3. COVID-19 Data Lake...
   Demonstrating bucket configuration

4. Popular AWS Open Data Buckets:
   [Table of public buckets]

=== Sample Complete ===
```

## Public Datasets Used

### NOAA GHCN-D (Global Historical Climatology Network - Daily)

| File | Description | Size |
|------|-------------|------|
| `ghcnd-countries.txt` | Country codes | ~5KB |
| `ghcnd-stations.txt` | Weather station metadata | ~12MB |
| `ghcnd-inventory.txt` | Station inventory | ~35MB |
| `csv/by_year/YYYY.csv` | Daily observations by year | ~30MB/year |

### COVID-19 Data Lake

| Path | Description |
|------|-------------|
| `rearc-covid-19-nyt-data-in-usa/` | NY Times COVID data |
| `rearc-covid-19-world-cases-deaths-testing/` | Global statistics |
| `static-datasets/` | Reference data |

## Popular Public Buckets

| Bucket | Region | Description |
|--------|--------|-------------|
| `noaa-ghcn-pds` | us-east-1 | Climate data (CSV) |
| `noaa-goes16` | us-east-1 | Weather satellite |
| `covid19-lake` | us-east-2 | COVID-19 datasets |
| `spacenet-dataset` | us-east-1 | Satellite imagery |
| `amazon-reviews-pds` | us-east-1 | Product reviews (Parquet) |

## Discovery Resources

- [AWS Registry of Open Data](https://registry.opendata.aws/)
- [NOAA GHCN-D Documentation](https://www.ncei.noaa.gov/products/land-based-station/global-historical-climatology-network-daily)
- [AWS COVID-19 Data Lake](https://aws.amazon.com/covid-19-data-lake/)

## Related Samples

- **S3BasicLoad** - Simplest loading pattern
- **S3Configuration** - Full configuration options
- **S3ParquetLoad** - Loading Parquet files
- **S3MultiSegment** - Loading multiple files with prefix patterns
