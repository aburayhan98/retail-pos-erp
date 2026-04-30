using Dapper;
using RetailErp.Pos.Application.Interfaces.IRepositories.IProduct;
using RetailErp.Pos.Domain.Entities;
using RetailErp.Pos.Infrastructure.Data;

namespace RetailErp.Pos.Infrastructure.Data.Command;

public sealed class ProductCommand : IProductCommand
{
	private readonly IDbConnectionFactory _dbConnectionFactory;

	public ProductCommand(IDbConnectionFactory dbConnectionFactory)
	{
		_dbConnectionFactory = dbConnectionFactory;
	}

	public async Task CreateAsync(Product product)
	{
		const string sql = """
            INSERT INTO Products
            (
                ProductId,
                Name,
                Barcode,
                StockQuantity,
                Price,
                CreatedAt
            )
            VALUES
            (
                @ProductId,
                @Name,
                @Barcode,
                @StockQuantity,
                @Price,
                @CreatedAt
            );
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();
		await connection.ExecuteAsync(sql, product);
	}

	public async Task ReduceStockAsync(Guid productId, int quantity)
	{
		const string sql = """
            UPDATE Products
            SET StockQuantity = StockQuantity - @Quantity
            WHERE ProductId = @ProductId
              AND StockQuantity >= @Quantity;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		var affectedRows = await connection.ExecuteAsync(sql, new
		{
			ProductId = productId,
			Quantity = quantity
		});

		if (affectedRows == 0)
		{
			throw new InvalidOperationException("Insufficient stock or product not found.");
		}
	}
}