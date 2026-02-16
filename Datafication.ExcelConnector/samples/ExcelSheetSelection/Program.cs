using Datafication.Core.Data;
using Datafication.Extensions.Connectors.ExcelConnector;
using Datafication.Connectors.ExcelConnector;

Console.WriteLine("=== Datafication.ExcelConnector Sheet Selection Sample ===\n");

var dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data");
var excelPath = Path.Combine(dataPath, "demo_excel_connector_data.xlsx");

// ============================================================================
// 1. DEFAULT BEHAVIOR - Loads the first sheet when no sheet is specified
// ============================================================================
Console.WriteLine("1. Default Behavior (First Sheet)");
Console.WriteLine(new string('-', 50));

var defaultSheet = await DataBlock.Connector.LoadExcelAsync(excelPath);

Console.WriteLine($"   Loaded first sheet by default");
Console.WriteLine($"   Rows: {defaultSheet.RowCount}");
Console.WriteLine($"   First column: {defaultSheet.Schema.GetColumnNames().First()}");
Console.WriteLine();

// ============================================================================
// 2. SHEET BY NAME - Select a specific sheet using SheetName
// ============================================================================
Console.WriteLine("2. Sheet Selection by Name");
Console.WriteLine(new string('-', 50));

var customersConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Customers",
    HasHeader = true
};
var customers = await DataBlock.Connector.LoadExcelAsync(customersConfig);

Console.WriteLine($"   Sheet: Customers");
Console.WriteLine($"   Rows: {customers.RowCount}");
Console.WriteLine($"   Columns: {string.Join(", ", customers.Schema.GetColumnNames())}");
Console.WriteLine();

var productsConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",
    HasHeader = true
};
var products = await DataBlock.Connector.LoadExcelAsync(productsConfig);

Console.WriteLine($"   Sheet: Products");
Console.WriteLine($"   Rows: {products.RowCount}");
Console.WriteLine($"   Columns: {string.Join(", ", products.Schema.GetColumnNames())}");
Console.WriteLine();

// ============================================================================
// 3. SHEET BY INDEX - Select a sheet using zero-based index
// ============================================================================
Console.WriteLine("3. Sheet Selection by Index (0-based)");
Console.WriteLine(new string('-', 50));

// Index 0 = Customers, Index 1 = Products, Index 2 = Orders, etc.
var ordersConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetIndex = 2,  // Orders sheet
    HasHeader = true
};
var orders = await DataBlock.Connector.LoadExcelAsync(ordersConfig);

Console.WriteLine($"   Sheet Index: 2");
Console.WriteLine($"   Rows: {orders.RowCount}");
Console.WriteLine($"   Columns: {string.Join(", ", orders.Schema.GetColumnNames())}");
Console.WriteLine();

var kpisConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetIndex = 6,  // KPIs sheet
    HasHeader = true
};
var kpis = await DataBlock.Connector.LoadExcelAsync(kpisConfig);

Console.WriteLine($"   Sheet Index: 6 (KPIs)");
Console.WriteLine($"   Rows: {kpis.RowCount}");
Console.WriteLine($"   Columns: {string.Join(", ", kpis.Schema.GetColumnNames())}");
Console.WriteLine();

// ============================================================================
// 4. SHEETNAME TAKES PRECEDENCE - When both SheetName and SheetIndex are set
// ============================================================================
Console.WriteLine("4. SheetName Takes Precedence Over SheetIndex");
Console.WriteLine(new string('-', 50));

var precedenceConfig = new ExcelConnectorConfiguration
{
    Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
    SheetName = "Products",  // This will be used
    SheetIndex = 0,          // This will be ignored
    HasHeader = true
};
var precedenceResult = await DataBlock.Connector.LoadExcelAsync(precedenceConfig);

Console.WriteLine($"   Config: SheetName='Products', SheetIndex=0");
Console.WriteLine($"   Result: Loaded '{precedenceResult.Schema.GetColumnNames().First()}' sheet (Products, not Customers)");
Console.WriteLine($"   Rows: {precedenceResult.RowCount}");
Console.WriteLine();

// ============================================================================
// 5. MULTI-SHEET LOADING - Load data from multiple sheets
// ============================================================================
Console.WriteLine("5. Multi-Sheet Loading");
Console.WriteLine(new string('-', 50));

// Load all relevant sheets for analysis
var sheetsToLoad = new[] { "Customers", "Products", "Orders", "OrderItems" };
var dataDictionary = new Dictionary<string, DataBlock>();

foreach (var sheetName in sheetsToLoad)
{
    var config = new ExcelConnectorConfiguration
    {
        Source = new Uri(excelPath, UriKind.RelativeOrAbsolute),
        SheetName = sheetName,
        HasHeader = true
    };
    dataDictionary[sheetName] = await DataBlock.Connector.LoadExcelAsync(config);
    Console.WriteLine($"   Loaded {sheetName}: {dataDictionary[sheetName].RowCount} rows");
}

Console.WriteLine();
Console.WriteLine("   All sheets loaded successfully!");
Console.WriteLine($"   Total datasets: {dataDictionary.Count}");
Console.WriteLine($"   Total rows across all sheets: {dataDictionary.Values.Sum(d => d.RowCount)}");

// ============================================================================
// 6. SHEET DATA PREVIEW - Preview data from different sheets
// ============================================================================
Console.WriteLine();
Console.WriteLine("6. Sheet Data Preview");
Console.WriteLine(new string('-', 50));

void PreviewSheet(string name, DataBlock data, int rows = 3)
{
    Console.WriteLine($"\n   {name} (first {rows} rows):");
    var cursor = data.GetRowCursor();
    int count = 0;
    var columns = data.Schema.GetColumnNames().Take(4).ToArray();
    while (cursor.MoveNext() && count < rows)
    {
        var preview = string.Join(" | ", columns.Select(c => $"{c}={cursor.GetValue(c)}"));
        Console.WriteLine($"     {preview}");
        count++;
    }
}

PreviewSheet("Customers", dataDictionary["Customers"]);
PreviewSheet("Products", dataDictionary["Products"]);
PreviewSheet("Orders", dataDictionary["Orders"]);

Console.WriteLine("\n=== Sample Complete ===");
