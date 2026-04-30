using Dapper;
using Microsoft.Data.SqlClient;
using RetailErp.Pos.Application.Common.Exceptions;
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

		try
		{
			using var connection = await _dbConnectionFactory.CreateConnectionAsync();
			await connection.ExecuteAsync(sql, product);
		}
		catch (SqlException exception)
		{
			throw new InfrastructureException("Failed to create product.", exception, "product_create_failed");
		}
	}

	public async Task ReduceStockAsync(Guid productId, int quantity)
	{
		const string sql = """
            UPDATE Products
            SET StockQuantity = StockQuantity - @Quantity
            WHERE ProductId = @ProductId
              AND StockQuantity >= @Quantity;
            """;

		try
		{
			using var connection = await _dbConnectionFactory.CreateConnectionAsync();

			var affectedRows = await connection.ExecuteAsync(sql, new
			{
				ProductId = productId,
				Quantity = quantity
			});

			if (affectedRows == 0)
			{
				throw new ConflictException("Insufficient stock or product was not found.", "product_stock_reduce_failed");
			}
		}
		catch (SqlException exception)
		{
			throw new InfrastructureException("Failed to update product stock.", exception, "product_stock_update_failed");
		}
	}
}
