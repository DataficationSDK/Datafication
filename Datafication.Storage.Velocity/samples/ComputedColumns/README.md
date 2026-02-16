# ComputedColumns Sample

This sample demonstrates computed columns with expressions in Datafication.Storage.Velocity, including arithmetic, logical operators, and math functions.

## Overview

The ComputedColumns sample shows how to:
- Create computed columns with arithmetic expressions
- Chain multiple computed columns
- Use math functions (ROUND, SQRT, ABS, etc.)
- Apply logical operators for boolean expressions
- Validate expressions before execution
- Filter by computed column values
- Combine Compute() with window functions

## Key Features Demonstrated

### Arithmetic Expressions
- Basic operators: `+`, `-`, `*`, `/`, `%`
- Column references in expressions
- Chained calculations

### Math Functions
- `ABS()` - Absolute value
- `ROUND(value, decimals)` - Rounding
- `FLOOR()`, `CEIL()` - Floor and ceiling
- `SQRT()` - Square root
- `POWER(base, exp)` - Exponentiation
- `EXP()`, `LOG()`, `LOG10()` - Exponentials and logarithms

### Date Functions
Extract date components (SIMD-optimized):
- `YEAR()`, `MONTH()`, `DAY()` - Date parts
- `HOUR()`, `MINUTE()`, `SECOND()` - Time parts
- `DAYOFWEEK()`, `DAYOFYEAR()`, `QUARTER()` - Additional components

Date arithmetic:
- `DATEADD(unit, amount, date)` - Add interval to date
- `DATEDIFF(unit, start, end)` - Difference between dates
- `NOW()`, `TODAY()` - Current date/time

SQL-compatible units: `year/yy`, `quarter/qq`, `month/mm`, `day/dd`, `hour/hh`, `minute/mi`, `second/ss`

### String Functions
Manipulate string columns:
- `UPPER()`, `LOWER()` - Case conversion
- `TRIM()`, `LTRIM()`, `RTRIM()` - Whitespace removal
- `LEN()`, `LENGTH()` - String length
- `SUBSTRING(s, start, len)` - Extract substring (1-based)
- `LEFT()`, `RIGHT()` - First/last N characters
- `REPLACE(s, old, new)` - Replace all occurrences
- `CHARINDEX(search, s)` - Find position (1-based, 0 if not found)
- `CONCAT(a, b, ...)` - Concatenate strings
- `LPAD()`, `RPAD()` - Pad with spaces or custom character
- `REVERSE()` - Reverse string

### Logical Operators
- `&&` (AND) - Both conditions true
- `||` (OR) - Either condition true
- `!` (NOT) - Negation
- Comparison operators: `==`, `!=`, `<`, `<=`, `>`, `>=`
- Returns 1 for true, 0 for false

### Expression Validation
- `ValidateExpression(expression, out error)` - Check syntax before execution
- Provides error messages for invalid expressions

### Integration
- Filter by computed values with `Where()`
- Combine with `Window()` for rolling calculations

## How to Run

```bash
cd ComputedColumns
dotnet restore
dotnet run
```

## Expected Output

The sample will output:
1. Basic subtotal calculation
2. Multiple columns (discount, total)
3. Profit calculations with margin
4. Math functions (ROUND, SQRT, ABS)
5. Logical operators (boolean flags)
6. Complex VIP order logic
7. Filtering by computed column
8. Expression validation examples
9. Compute with window function

## Performance Note

- Arithmetic and comparison expressions use SIMD vectorization (100-150M values/sec)
- Date extraction functions use SIMD-optimized extraction (80-120M values/sec)
- String functions use loop-unrolled batch processing (comparable to date functions)
- Logical operators use standard evaluator (10-50M values/sec)

## Related Samples

- **WindowFunctions** - Learn about rolling calculations
- **QueryOperations** - Learn about filtering and Where clauses
- **GroupingAndAggregation** - Learn about aggregation functions
