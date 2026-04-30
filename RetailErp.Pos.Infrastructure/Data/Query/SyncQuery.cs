using Dapper;
using RetailErp.Pos.Domain.Entities;
using RetailErp.Pos.Infrastructure.Repositories.Interfaces.ISync;

namespace RetailErp.Pos.Infrastructure.Data.Query;

public sealed class SyncQuery(IDbConnectionFactory dbConnectionFactory) : ISyncQuery
{
	private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

	public async Task<IReadOnlyList<Sale>> GetSyncedSalesAsync()
	{
		const string sql = """
            SELECT
                SaleId,
                SaleDate,
                TotalAmount,
                SyncStatus,
                RetryCount,
                LastSyncAttemptAt
            FROM Sales
            WHERE SyncStatus IN ('Pending', 'Failed')
            ORDER BY SaleDate ASC;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		var sales = await connection.QueryAsync<Sale>(sql);

		return sales.ToList();
	}

	public async Task<bool> ExistsInCentralAsync(Guid saleId)
	{
		const string sql = """
            SELECT COUNT(1)
            FROM CentralSales
            WHERE SaleId = @SaleId;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		var count = await connection.ExecuteScalarAsync<int>(
				sql,
				new { SaleId = saleId });

		return count > 0;
	}


}