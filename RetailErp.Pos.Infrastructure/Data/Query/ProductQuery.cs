using Dapper;
using RetailErp.Pos.Application.Interfaces.IRepositories.IProduct;
using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Infrastructure.Data.Query;

public sealed class ProductQuery(IDbConnectionFactory dbConnectionFactory) : IProductQuery
{
	private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

	public async Task<IReadOnlyList<Product>> GetAllAsync()
	{
		const string sql = """
            SELECT
                ProductId,
                Name,
                Barcode,
                StockQuantity,
                Price,
                CreatedAt
            FROM Products
            ORDER BY CreatedAt DESC;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		var products = await connection.QueryAsync<Product>(sql);

		return products.ToList();
	}

	public async Task<Product?> GetByIdAsync(Guid productId)
	{
		const string sql = """
            SELECT
                ProductId,
                Name,
                Barcode,
                StockQuantity,
                Price,
                CreatedAt
            FROM Products
            WHERE ProductId = @ProductId;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		return await connection.QueryFirstOrDefaultAsync<Product>(
				sql,
				new { ProductId = productId });
	}

	public async Task<Product?> GetByBarcodeAsync(string barcode)
	{
		const string sql = """
            SELECT
                ProductId,
                Name,
                Barcode,
                StockQuantity,
                Price,
                CreatedAt
            FROM Products
            WHERE Barcode = @Barcode;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		return await connection.QueryFirstOrDefaultAsync<Product>(
				sql,
				new { Barcode = barcode });
	}
}