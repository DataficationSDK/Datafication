# WindowFunctions Sample

This sample demonstrates how to use window functions in Datafication.Core for time series analysis, statistical calculations, and analytical operations.

## Overview

Window functions (also known as analytic functions) enable calculations across sets of rows that are related to the current row. Unlike aggregate functions that collapse rows, window functions preserve all row details while computing values based on a sliding, cumulative, or partitioned window of data.

This sample shows how to:
- Calculate moving averages and sums
- Compute exponential moving averages (EMA)
- Calculate moving standard deviation and variance
- Use moving median and percentiles
- Perform cumulative calculations (running totals)
- Compare rows using Lag/Lead functions
- Rank and number rows
- Partition windows by category columns
- Chain multiple window functions for complex analytics

## Key Features Demonstrated

### Moving Aggregations

#### MovingAverage
Calculates the arithmetic mean over a sliding window.

```csharp
var result = stockData.Window(
    "Price",
    WindowFunctionType.MovingAverage,
    20,  // 20-day window
    "MA_20"
);
```

#### MovingSum, MovingMin, MovingMax, MovingCount
Rolling calculations over a window of rows.

```csharp
var result = salesData
    .Window("Amount", WindowFunctionType.MovingSum, 7, "WeeklyTotal")
    .Window("Amount", WindowFunctionType.MovingMin, 7, "WeeklyMin")
    .Window("Amount", WindowFunctionType.MovingMax, 7, "WeeklyMax")
    .Window("Amount", WindowFunctionType.MovingCount, 7, "ValidDays");
```

### Statistical Functions

#### MovingStandardDeviation / MovingVariance
Calculate rolling statistics using Welford's algorithm for numerical stability.

```csharp
// 20-day volatility (standard deviation)
var result = stockData.Window(
    "Returns",
    WindowFunctionType.MovingStandardDeviation,
    20,
    "Volatility_20"
);

// Moving variance
var result = stockData.Window(
    "Returns",
    WindowFunctionType.MovingVariance,
    20,
    "Variance_20"
);
```

#### ExponentialMovingAverage (EMA)
Exponentially weighted moving average that gives more weight to recent values.

**Formula:** `EMA[t] = value[t] × α + EMA[t-1] × (1-α)` where `α = 2/(windowSize+1)`

```csharp
// 12-day EMA for MACD indicator
var result = stockData.Window(
    "Price",
    WindowFunctionType.ExponentialMovingAverage,
    12,
    "EMA_12"
);
```

### Percentile Functions

#### MovingMedian
Calculates the median (50th percentile) over a sliding window.

```csharp
// 5-day median for robust central tendency
var result = priceData.Window(
    "Price",
    WindowFunctionType.MovingMedian,
    5,
    "Median_5"
);
```

#### MovingPercentile
Calculates any percentile (0.0 to 1.0) using linear interpolation.

```csharp
// Quartile analysis
var result = data
    .Window("Value", WindowFunctionType.MovingPercentile, 20, "P25", percentile: 0.25)
    .Window("Value", WindowFunctionType.MovingPercentile, 20, "P50", percentile: 0.50)
    .Window("Value", WindowFunctionType.MovingPercentile, 20, "P75", percentile: 0.75);

// Calculate interquartile range (IQR)
var iqr = result.SelectWith(
    SelectExpression.AllColumns(),
    SelectExpression.Computed("IQR", "P75 - P25")
);
```

### Cumulative Functions

Cumulative functions compute running totals, averages, and extremes from the start of the partition/dataset.

```csharp
// Running total
var result = salesData.Window(
    "Amount",
    WindowFunctionType.CumulativeSum,
    null,  // No window size needed for cumulative
    "RunningTotal"
);

// Running average
var result = scoreData.Window(
    "Score",
    WindowFunctionType.CumulativeAverage,
    null,
    "AvgToDate"
);

// All-time high and low
var result = stockData
    .Window("Price", WindowFunctionType.CumulativeMin, null, "AllTimeLow")
    .Window("Price", WindowFunctionType.CumulativeMax, null, "AllTimeHigh");
```

### Offset Functions (Lag/Lead)

Access values from previous or following rows.

#### Lag
Retrieve value from N rows before the current row.

```csharp
// Compare with previous day
var result = stockData.Window(
    "Price",
    WindowFunctionType.Lag,
    1,  // 1 row back
    "PrevPrice",
    defaultValue: 0.0  // Use 0.0 for first row (no previous value)
);

// Calculate day-over-day change
var change = result
    .Compute("Change", "Price - PrevPrice")
    .Compute("ChangePct", "(Price - PrevPrice) / PrevPrice * 100");
```

**Note:** The `defaultValue` parameter specifies what value to use when Lag references a row that doesn't exist (e.g., the first row). If not specified, `null` is used.

#### Lead
Retrieve value from N rows ahead of the current row.

```csharp
// Look ahead to next value
var result = data.Window(
    "Value",
    WindowFunctionType.Lead,
    1,
    "NextValue",
    defaultValue: 0.0  // Use 0.0 for last row (no next value)
);
```

### Ranking Functions

Assign ranks and row numbers.

```csharp
// Sequential numbering
var result = data.Window(
    null,  // No column needed for RowNumber
    WindowFunctionType.RowNumber,
    null,
    "RowNum"
);

// Rank with gaps for ties
var result = studentData.Window(
    "Score",
    WindowFunctionType.Rank,
    null,
    "Rank"
);

// Dense rank without gaps
var result = studentData.Window(
    "Score",
    WindowFunctionType.DenseRank,
    null,
    "DenseRank"
);
```

### Partitioned Windows

Partition data into independent groups where window functions reset for each partition.

```csharp
// Moving average per stock symbol
var result = stockData.Window(
    "Price",
    WindowFunctionType.MovingAverage,
    50,
    "MA_50",
    partitionBy: new[] { "Symbol" }
);

// Each symbol gets its own independent moving average
```

### Chaining Window Functions

Combine multiple window functions for rich analytics.

```csharp
// Technical analysis with multiple indicators
var analysis = stockData
    .Window("Close", WindowFunctionType.MovingAverage, 20, "SMA_20")
    .Window("Close", WindowFunctionType.MovingAverage, 50, "SMA_50")
    .Window("Close", WindowFunctionType.ExponentialMovingAverage, 12, "EMA_12")
    .Window("Close", WindowFunctionType.ExponentialMovingAverage, 26, "EMA_26")
    .Window("Close", WindowFunctionType.MovingStandardDeviation, 20, "StdDev_20")
    .Window("Volume", WindowFunctionType.MovingAverage, 20, "AvgVolume")
    .SelectWith(
        SelectExpression.AllColumns(),
        SelectExpression.Computed("MACD", "EMA_12 - EMA_26"),
        SelectExpression.Computed("BB_Upper", "SMA_20 + (StdDev_20 * 2)"),
        SelectExpression.Computed("BB_Lower", "SMA_20 - (StdDev_20 * 2)")
    );
```

## Available Window Functions

### Moving Aggregations
- `MovingAverage` - Rolling average
- `MovingSum` - Rolling sum
- `MovingMin` - Rolling minimum
- `MovingMax` - Rolling maximum
- `MovingCount` - Count non-null values in window

### Statistical Functions
- `MovingStandardDeviation` - Rolling standard deviation (Welford's algorithm)
- `MovingVariance` - Rolling variance (Welford's algorithm)
- `ExponentialMovingAverage` - Exponential weighted average

### Percentile Functions
- `MovingMedian` - Rolling median (50th percentile)
- `MovingPercentile` - Rolling percentile with linear interpolation

### Cumulative Functions
- `CumulativeSum` - Running total
- `CumulativeAverage` - Running average
- `CumulativeMin` - Running minimum
- `CumulativeMax` - Running maximum

### Offset Functions
- `Lag` - Value from previous row(s)
- `Lead` - Value from following row(s)

### Ranking Functions
- `RowNumber` - Sequential numbering
- `Rank` - Rank with gaps for ties
- `DenseRank` - Rank without gaps for ties

### Window Boundary Functions
- `FirstValue` - First value in window/partition
- `LastValue` - Last value in window/partition
- `NthValue` - Nth value in window/partition

## How to Run

```bash
cd WindowFunctions
dotnet restore
dotnet run
```

## Expected Output

The sample demonstrates:
1. **Stock Price Analysis**
   - 20-day and 50-day moving averages
   - Exponential moving averages (12-day, 26-day)
   - MACD indicator calculation
   - Bollinger Bands (mean ± 2σ)
   - Price change analysis using Lag

2. **Sales Analytics**
   - 7-day rolling totals
   - Cumulative revenue
   - Week-over-week comparisons

3. **Quality Control**
   - Moving statistics for anomaly detection
   - Control limits (mean ± 3σ)

4. **Ranking and Percentiles**
   - Student score rankings
   - Quartile analysis

## Performance Tips

1. **Use VelocityDataBlock for large datasets** (100K+ rows)
   - 10-30x faster than DataBlock
   - SIMD-accelerated operations
   - See Datafication.Storage.Velocity package

2. **Chain window functions** for efficiency
   ```csharp
   // Good: Single pass over data
   var result = data
       .Window("Price", WindowFunctionType.MovingAverage, 20, "MA_20")
       .Window("Price", WindowFunctionType.MovingStdDev, 20, "StdDev_20");
   ```

3. **Choose appropriate window sizes**
   - Short-term trends: 5-20 periods
   - Medium-term trends: 20-50 periods
   - Long-term trends: 100-200 periods

4. **Order data before window functions**
   ```csharp
   var ordered = data
       .OrderBy("Date")
       .Window("Price", WindowFunctionType.MovingAverage, 20, "MA_20");
   ```

## Common Use Cases

### Financial Analysis
- Moving averages for trend identification
- MACD and other technical indicators
- Volatility calculation (standard deviation)
- Price momentum and rate of change

### Sales Analytics
- Rolling sales totals
- Week-over-week/month-over-month comparisons
- Cumulative revenue tracking
- Sales rankings

### Quality Control
- Control charts (mean ± 3σ)
- Trend detection
- Anomaly identification
- Process capability analysis

### Time Series Analysis
- Smoothing noisy data
- Trend extraction
- Seasonal decomposition
- Gap detection

## Related Samples

- **GroupingAndAggregation** - Learn how to group and aggregate data (without preserving rows)
- **FilteringAndSorting** - Prepare data for window function analysis
- **ExpressionsAndComputedColumns** - Combine window functions with expressions
- **DataTransformation** - Transform data after applying window functions

## Additional Resources

- [Window Functions Guide](../../../../agent-docs/docs/window-functions-guide.md) - Comprehensive documentation
- [VelocityDataBlock](../../../../website/github/Datafication.Storage.Velocity/README.md) - High-performance implementation
- API Documentation - Full API reference

## Notes

- Window functions preserve all rows (unlike GROUP BY which collapses rows)
- Null values are automatically skipped in calculations
- First N-1 rows will be null for N-sized moving windows
- Partitioning creates independent windows per partition
- Ordering matters for time series data
