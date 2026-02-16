# QueryPerformance Sample

This sample demonstrates query performance benchmarking in Datafication.Storage.Velocity using large-scale datasets ranging from 5K to 100M+ rows.

## Overview

The QueryPerformance sample shows how to:
- Benchmark complex queries on large datasets
- Measure query throughput in rows/ms
- Compare performance across different dataset sizes
- Auto-download test datasets from the Datafication server

## Key Features Demonstrated

### Large-Scale Dataset Support
- **sf005** - Small dataset (~5K rows) for quick tests
- **sf1** - Medium dataset (~1M rows) - default
- **sf20** - Large dataset (~20M rows)
- **sf50** - Very large dataset (~50M rows)
- **sf100** - Massive dataset (~100M rows)

### Performance Metrics
- Per-iteration timing with rows/ms throughput
- Statistical analysis (average, median, min, max, std dev)
- Overall throughput calculation
- Dataset load time measurement

### Automatic Dataset Management
- Downloads `.dfc` files from Datafication server if not cached
- Progress indicator during download
- Caches datasets locally for subsequent runs

### Complex Query Benchmarking
The benchmark executes a multi-operation query:
- Column projection (SELECT)
- Numeric filtering (WHERE with comparison)
- String filtering (WHERE with equality)
- Sorting (ORDER BY)

## How to Run

```bash
cd QueryPerformance
dotnet restore
dotnet run [scale_factor]
```

### Scale Factor Options

| Option | Description | Approximate Rows |
|--------|-------------|------------------|
| sf005  | Small dataset | ~5,000 |
| sf1    | Medium dataset (default) | ~1,000,000 |
| sf20   | Large dataset | ~20,000,000 |
| sf50   | Very large dataset | ~50,000,000 |
| sf100  | Massive dataset | ~100,000,000 |

### Examples

```bash
# Run with default 1M row dataset
dotnet run

# Run with 20M row dataset
dotnet run sf20

# Run with small dataset for quick testing
dotnet run sf005
```

## Expected Output

```
=== Datafication.Storage.Velocity Query Performance Sample ===

Scale Factor: SF1 (~1M rows)
Using cached dataset: .../data/test_SF1.dfc

Loading dataset...
Dataset loaded in 245 ms
Total rows: 1,000,000
Total columns: 14

--- Query Definition ---
  SELECT "Total Profit", "Total Cost", "Country"
  WHERE "Total Profit" > 4,000,000
  AND "Country" = 'United States'
  ORDER BY "Total Profit" DESC

--- Warm-up ---
Warm-up complete. Result rows: 1,234

--- Benchmark (10 iterations) ---
  Run  1:    12.34 ms (81,037 rows/ms)
  Run  2:    11.87 ms (84,246 rows/ms)
  ...

--- Results ---
  Dataset size:      1,000,000 rows
  Iterations:        10

  Timing Statistics:
    Average:         12.15 ms (±0.42 std dev)
    Median:          12.08 ms
    Min:             11.52 ms
    Max:             13.01 ms

  Throughput:
    Per-run average: 82,304 rows/ms
    Overall:         82,305 rows/ms

=== Benchmark Complete ===
```

## Notes

- First run will download the dataset (may take several minutes for larger scale factors)
- Datasets are cached in the `data/` subdirectory
- JIT warm-up is performed before benchmarking for accurate measurements
- Garbage collection is triggered before benchmarking to reduce noise

## Related Samples

- **QueryOperations** - Learn about query syntax and operations
- **GroupingAndAggregation** - Learn about grouping and aggregate functions
- **WindowFunctions** - Learn about SIMD-accelerated analytics
