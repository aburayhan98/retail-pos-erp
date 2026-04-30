using Microsoft.Extensions.Configuration;
using RetailErp.Pos.Infrastructure.Data;

namespace RetailErp.Pos.Tests;

public sealed class ConfigTest
{
	private readonly IConfiguration _configuration;

	public ConfigTest() => _configuration = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false)
				.AddUserSecrets<ConfigTest>(optional: true)
				.Build();

	public string DbConnection =>
			_configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException("DefaultConnection not found.");

	public IDbConnectionFactory DbConnectionFactory =>
			new DbConnectionFactory(_configuration);
}