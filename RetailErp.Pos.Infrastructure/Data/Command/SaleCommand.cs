using Dapper;
using RetailErp.Pos.Application.Interfaces.IRepositories.ISale;
using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Infrastructure.Data.Command;

public sealed class SaleCommand(IDbConnectionFactory dbConnectionFactory) : ISaleCommand
{
	private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

	public async Task CreateAsync(Sale sale)
	{
		const string sql = """
            INSERT INTO Sales
            (
                SaleId,
                OutletId,
                SaleDate,
                TotalAmount,
                SyncStatus,
                RetryCount
            )
            VALUES
            (
                @SaleId,
                @OutletId,
                @SaleDate,
                @TotalAmount,
                @SyncStatus,
                @RetryCount
            );
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();
		await connection.ExecuteAsync(sql, new
		{
			sale.SaleId,
			sale.OutletId,
			sale.SaleDate,
			sale.TotalAmount,
			SyncStatus = sale.SyncStatus.ToString(),
			sale.RetryCount
		});
	}

	public async Task MarkSyncAsync(Guid saleId, bool isSuccess)
	{
		const string sql = """
            UPDATE Sales
            SET SyncStatus = @Status
            WHERE SaleId = @SaleId;
            """;

		var status = isSuccess ? "Synced" : "Failed";

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		await connection.ExecuteAsync(sql, new
		{
			SaleId = saleId,
			Status = status
		});
	}

	public async Task MarkFailedAsync(Guid saleId)
	{
		const string sql = """
            UPDATE Sales
            SET SyncStatus = 'Failed'
            WHERE SaleId = @SaleId;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		await connection.ExecuteAsync(sql, new { SaleId = saleId });
	}

	public async Task IncreaseRetryAsync(Guid saleId)
	{
		const string sql = """
            UPDATE Sales
            SET RetryCount = RetryCount + 1
            WHERE SaleId = @SaleId;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		await connection.ExecuteAsync(sql, new { SaleId = saleId });
	}

	public async Task InsertCentralSaleAsync(Sale sale)
	{
		const string sql = """
            INSERT INTO CentralSales
            (
                SaleId,
                OutletId,
                SaleDate,
                TotalAmount
            )
            VALUES
            (
                @SaleId,
                @OutletId,
                @SaleDate,
                @TotalAmount
            );
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		await connection.ExecuteAsync(sql, sale);
	}
}
