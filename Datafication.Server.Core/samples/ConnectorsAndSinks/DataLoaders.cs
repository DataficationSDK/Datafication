using System.Text.Json;
using Datafication.Core.Data;

namespace ConnectorsAndSinks;

/// <summary>
/// Helper methods for loading sample data from files.
/// </summary>
public static class DataLoaders
{
    /// <summary>
    /// Loads product data from a CSV file.
    /// </summary>
    public static DataBlock LoadProductsFromCsv()
    {
        var block = new DataBlock();
        block.AddColumn(new DataColumn("ProductId", typeof(int)));
        block.AddColumn(new DataColumn("ProductName", typeof(string)));
        block.AddColumn(new DataColumn("Category", typeof(string)));
        block.AddColumn(new DataColumn("Price", typeof(decimal)));
        block.AddColumn(new DataColumn("InStock", typeof(bool)));

        var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "products.csv");
        if (File.Exists(dataPath))
        {
            var lines = File.ReadAllLines(dataPath).Skip(1); // Skip header
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 5)
                {
                    block.AddRow(new object[]
                    {
                        int.Parse(parts[0]),
                        parts[1],
                        parts[2],
                        decimal.Parse(parts[3]),
                        bool.Parse(parts[4])
                    });
                }
            }
        }
        else
        {
            // Fallback data if file not found
            block.AddRow(new object[] { 1, "Laptop Pro", "Electronics", 1299.99m, true });
            block.AddRow(new object[] { 2, "Wireless Mouse", "Electronics", 29.99m, true });
            block.AddRow(new object[] { 3, "Office Chair", "Furniture", 349.99m, true });
        }

        return block;
    }

    /// <summary>
    /// Loads order data from a JSON file.
    /// </summary>
    public static DataBlock LoadOrdersFromJson()
    {
        var block = new DataBlock();
        block.AddColumn(new DataColumn("OrderId", typeof(int)));
        block.AddColumn(new DataColumn("CustomerId", typeof(int)));
        block.AddColumn(new DataColumn("ProductId", typeof(int)));
        block.AddColumn(new DataColumn("Quantity", typeof(int)));
        block.AddColumn(new DataColumn("OrderDate", typeof(string)));

        var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");
        if (File.Exists(dataPath))
        {
            var json = File.ReadAllText(dataPath);
            var orders = JsonSerializer.Deserialize<List<OrderDto>>(json);
            if (orders != null)
            {
                foreach (var order in orders)
                {
                    block.AddRow(new object[]
                    {
                        order.OrderId,
                        order.CustomerId,
                        order.ProductId,
                        order.Quantity,
                        order.OrderDate
                    });
                }
            }
        }
        else
        {
            // Fallback data if file not found
            block.AddRow(new object[] { 1001, 1, 1, 1, "2024-01-15" });
            block.AddRow(new object[] { 1002, 2, 3, 2, "2024-01-16" });
        }

        return block;
    }

    private record OrderDto(int OrderId, int CustomerId, int ProductId, int Quantity, string OrderDate);
}
