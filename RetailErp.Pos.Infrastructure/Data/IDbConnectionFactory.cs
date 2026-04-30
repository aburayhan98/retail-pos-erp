using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RetailErp.Pos.Application.Common.Exceptions;

namespace RetailErp.Pos.Infrastructure.Data;

public interface IDbConnectionFactory
{
	Task<IDbConnection> CreateConnectionAsync();
}

public sealed class DbConnectionFactory : IDbConnectionFactory
{
	private readonly string _connectionString;

	public DbConnectionFactory(IConfiguration configuration)
	{
		_connectionString = configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
	}

	public async Task<IDbConnection> CreateConnectionAsync()
	{
		try
		{
			var connection = new SqlConnection(_connectionString);
			await connection.OpenAsync();
			return connection;
		}
		catch (SqlException exception)
		{
			throw new InfrastructureException(
				"Unable to connect to the database.",
				exception,
				"database_connection_failed");
		}
	}
}
