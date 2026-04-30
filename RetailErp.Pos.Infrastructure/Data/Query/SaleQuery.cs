using Dapper;
using RetailErp.Pos.Application.Interfaces.IRepositories.ISale;
using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Infrastructure.Data.Query;

public sealed class SaleQuery(IDbConnectionFactory dbConnectionFactory) : ISaleQuery
{
	private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

	public async Task<IReadOnlyList<Sale>> GetAllAsync()
	{
		const string sql = """
            SELECT
                SaleId,
                SaleDate,
                TotalAmount,
                SyncStatus,
                RetryCount
            FROM Sales
            ORDER BY SaleDate DESC;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		var result = await connection.QueryAsync<Sale>(sql);

		return result.ToList();
	}

	public async Task<IReadOnlyList<Sale>> GetPendingSalesAsync()
	{
		const string sql = """
            SELECT
                SaleId,
                SaleDate,
                TotalAmount,
                SyncStatus,
                RetryCount
            FROM Sales
            WHERE SyncStatus IN ('Pending', 'Failed');
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		var result = await connection.QueryAsync<Sale>(sql);

		return result.ToList();
	}

	public async Task<Sale?> GetByIdAsync(Guid saleId)
	{
		const string sql = """
            SELECT
                SaleId,
                SaleDate,
                TotalAmount,
                SyncStatus,
                RetryCount
            FROM Sales
            WHERE SaleId = @SaleId;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		return await connection.QueryFirstOrDefaultAsync<Sale>(
				sql,
				new { SaleId = saleId });
	}

	public async Task<bool> ExistsInCentralAsync(Guid saleId)
	{
		const string sql = """
            SELECT COUNT(1)
            FROM CentralSales
            WHERE SaleId = @SaleId;
            """;

		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		var count = await connection.ExecuteScalarAsync<int>(sql, new { SaleId = saleId });

		return count > 0;
	}
}