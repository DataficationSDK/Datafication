# Changelog

All notable changes to the Datafication SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.10] - 2026-01-31

### Performance

#### Datafication.Storage.Velocity

- **Repeated queries on compressed DFC files** are up to 2x faster using lazy decompression caching. Compressed columns are decompressed once on first access and cached for the lifetime of the VelocityDataBlock. Subsequent queries reuse the cached data, eliminating redundant decompression overhead.
- **WHERE on large single-segment DFC files** now uses intra-segment parallelism, distributing SIMD bitmask computation across all CPU cores. Previously, single-segment files ran WHERE conditions on a single thread regardless of row count. Files with tens of millions of rows in a single segment see significant throughput improvements.
- **Top-K queries** (`Where().Sort().Head(K)`) are up to 10-17x faster using SIMD-vectorized WHERE evaluation followed by typed heap-based selection. The optimization uses bitmask computation for filtering (~300M+ rows/sec) instead of per-row evaluation (~10M rows/sec), and typed extraction (TryGetDouble/TryGetInt32) for sort keys to avoid boxing. Applies automatically when K is small relative to the dataset.
- **VelocityDataBlock to DataBlock conversion** via `ToDataBlock()` is significantly faster with reduced memory allocations, especially for datasets with string or JSON columns.
- **Head() and Tail()** operations on VelocityDataBlock read only the requested rows directly from DFC storage, skipping full materialization. Provides dramatic speedups when taking small slices of large datasets.
- **Where** operations with multiple string equality conditions are ~40% faster using optimized row-by-row evaluation with short-circuit logic.
- **Sort + Head/Tail** queries (`Sort().Head(K)`, `Sort().Tail(K)`) on VelocityDataBlock are 5-10x faster for small K values. The optimization uses heap-based selection directly on DFC storage, avoiding full sort and materialization. Row group statistics pruning can skip entire data segments that cannot contribute to the result.
- **Query result materialization** is faster across Where and Sort paths using bulk column extraction, reducing per-row overhead.
- **Sort** operations are 2-4x faster with reduced memory allocations.
- **GroupBy** operations are 5-6x faster for SUM/AVG aggregations and 2-3x faster for COUNT using batch key extraction, array-based accumulators for low-cardinality columns, and optimized null handling.
- **Filter** operations have reduced memory overhead.
- **Aggregations** (Min, Max, Sum, Average, Variance, StandardDeviation) are 3-6x faster using single-pass computation.
- **Window functions** (MovingAverage, CumulativeSum, etc.) are ~2x faster using bulk column extraction with MemoryMarshal, parallel processing, and CollectionsMarshal for bulk result construction.
- **VelocityDataBlock.Clone()** is 5-6x faster using bulk column extraction with MemoryMarshal.Cast for fixed-width types and batch string table lookups for dictionary-encoded strings, eliminating per-row method dispatch overhead.

### Breaking Changes

#### Datafication.Storage.Velocity

- **Removed `VelocityResult` and `AsResult()`**: The lazy evaluation API has been removed. Use `Execute()` for all query operations.
  - `VelocityResult`, `VelocityDataRow`, `RowIndexStorage`, `ComputedColumnStorage` classes removed
  - `VelocityDataBlockExtensions` removed (`Count()`, `Any()`, `FirstOrDefault()` extension methods)
  - `AsResult()` method removed from `VelocityDataBlock`

### Changed

#### Datafication.Storage.Velocity

- `Variance()` and `StandardDeviation()` now return `NaN` when fewer than 2 values exist, following standard statistical conventions. Previously returned 0.
