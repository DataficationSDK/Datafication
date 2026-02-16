# WindowFunctions Sample

This sample demonstrates SIMD-accelerated window functions in Datafication.Storage.Velocity for high-performance time series and analytical operations.

## Overview

The WindowFunctions sample shows how to:
- Calculate moving averages (simple and exponential)
- Compute volatility with moving standard deviation
- Create cumulative sums and running totals
- Use Lag/Lead for period-over-period comparisons
- Chain multiple window functions
- Apply partitioned windows for per-category analysis

## Key Features Demonstrated

### Moving Calculations
- `MovingAverage` - Simple moving average
- `ExponentialMovingAverage` - EMA with decay weighting
- `MovingStandardDeviation` - Rolling volatility (Welford's algorithm)
- `MovingSum`, `MovingMin`, `MovingMax` - Rolling aggregates

### Cumulative Operations
- `CumulativeSum` - Running total (SIMD prefix sum)
- `CumulativeAverage`, `CumulativeMin`, `CumulativeMax`

### Comparison Functions
- `Lag` - Access previous row values
- `Lead` - Access future row values
- Custom default values for edge cases

### Ranking Functions
- `Rank` - Ranking with gaps
- `DenseRank` - Ranking without gaps
- `RowNumber` - Sequential row numbering

### Advanced Features
- **Partitioned Windows** - Calculate per category/group
- **Chained Functions** - Multiple windows in single query
- **Expression Integration** - Combine with Compute()

## How to Run

```bash
cd WindowFunctions
dotnet restore
dotnet run
```

## Expected Output

The sample will output:
1. 3-day moving average
2. 5-day exponential moving average
3. Moving standard deviation (volatility)
4. Cumulative volume sum
5. Lag with daily change calculation
6. Moving min and max (support/resistance)
7. Bollinger Bands (SMA + StdDev)
8. Ranking results
9. Partitioned windows (multi-symbol)
10. Rolling sales analysis

## Performance

VelocityDataBlock window functions use SIMD vectorization:
- **10-30x faster** than DataBlock equivalents
- **50-150M values/sec** on modern CPUs with AVX2/AVX-512
- Optimized for large datasets (100K+ rows)

## Related Samples

- **GroupingAndAggregation** - Learn about grouped aggregations
- **ComputedColumns** - Learn about expression calculations
- **QueryOperations** - Learn about filtering and sorting
