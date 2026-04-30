using Dapper;
using RetailErp.Pos.Domain.Entities;
using RetailErp.Pos.Infrastructure.Data;
using RetailErp.Pos.Infrastructure.Repositories.Interfaces.ISync;

namespace RetailErp.Pos.Infrastructure.Repositories.Commands;

public sealed class SyncCommand : ISyncCommand
{
	private readonly IDbConnectionFactory _dbConnectionFactory;

	public SyncCommand(IDbConnectionFactory dbConnectionFactory)
	{
		_dbConnectionFactory = dbConnectionFactory;
	}

	public async Task InsertCentralSaleAsync(Sale sale)
	{
		const string sql = """
            INSERT INTO CentralSales
            (
                SaleId,
                SaleDate,
                TotalAmount,
                SyncStatus,
                RetryCount,
                SyncedAt
            )
            VALUES
            (
                @SaleId,
                @SaleDate,
                @TotalAmount,
                'Synced',
                @RetryCount,
                @SyncedAt
            );
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		await connection.ExecuteAsync(sql, new
		{
			sale.SaleId,
			sale.SaleDate,
			sale.TotalAmount,
			sale.RetryCount,
			SyncedAt = DateTime.UtcNow
		});
	}

	public async Task MarkAsSyncedAsync(Guid saleId)
	{
		const string sql = """
            UPDATE Sales
            SET
                SyncStatus = 'Synced',
                LastSyncAttemptAt = @LastSyncAttemptAt
            WHERE SaleId = @SaleId;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		await connection.ExecuteAsync(sql, new
		{
			SaleId = saleId,
			LastSyncAttemptAt = DateTime.UtcNow
		});
	}

	public async Task MarkAsFailedAsync(Guid saleId)
	{
		const string sql = """
            UPDATE Sales
            SET
                SyncStatus = 'Failed',
                LastSyncAttemptAt = @LastSyncAttemptAt
            WHERE SaleId = @SaleId;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		await connection.ExecuteAsync(sql, new
		{
			SaleId = saleId,
			LastSyncAttemptAt = DateTime.UtcNow
		});
	}

	public async Task IncreaseRetryCountAsync(Guid saleId)
	{
		const string sql = """
            UPDATE Sales
            SET
                RetryCount = RetryCount + 1,
                LastSyncAttemptAt = @LastSyncAttemptAt
            WHERE SaleId = @SaleId;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		await connection.ExecuteAsync(sql, new
		{
			SaleId = saleId,
			LastSyncAttemptAt = DateTime.UtcNow
		});
	}
}