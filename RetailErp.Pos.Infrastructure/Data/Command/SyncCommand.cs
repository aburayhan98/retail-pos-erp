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
