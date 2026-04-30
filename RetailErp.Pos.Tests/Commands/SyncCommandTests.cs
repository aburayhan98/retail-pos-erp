using FluentAssertions;
using RetailErp.Pos.Domain.Enums;
using RetailErp.Pos.Infrastructure.Data.Command;
using RetailErp.Pos.Infrastructure.Data.Query;
using RetailErp.Pos.Infrastructure.Repositories.Commands;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Commands;

public sealed class SyncCommandTests
{
	private readonly SaleCommand _saleCommand;
	private readonly SaleQuery _saleQuery;
	private readonly SyncCommand _syncCommand;
	private readonly SyncQuery _syncQuery;

	public SyncCommandTests()
	{
		var config = new ConfigTest();

		_saleCommand = new SaleCommand(config.DbConnectionFactory);
		_saleQuery = new SaleQuery(config.DbConnectionFactory);

		_syncCommand = new SyncCommand(config.DbConnectionFactory);
		_syncQuery = new SyncQuery(config.DbConnectionFactory);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task InsertCentralSaleAsync_ShouldInsertSaleIntoCentralSales()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		// Act
		await _syncCommand.InsertCentralSaleAsync(sale);

		var exists = await _syncQuery.ExistsInCentralAsync(sale.SaleId);

		// Assert
		exists.Should().BeTrue();
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task MarkAsSyncedAsync_ShouldUpdateSaleStatusToSynced()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);

		// Act
		await _syncCommand.MarkAsSyncedAsync(sale.SaleId);

		var updatedSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		updatedSale.Should().NotBeNull();
		updatedSale!.SyncStatus.Should().Be(SyncStatus.Synced);
		updatedSale.LastSyncAttemptAt.Should().NotBeNull();
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task MarkAsFailedAsync_ShouldUpdateSaleStatusToFailed()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);

		// Act
		await _syncCommand.MarkAsFailedAsync(sale.SaleId);

		var updatedSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		updatedSale.Should().NotBeNull();
		updatedSale!.SyncStatus.Should().Be(SyncStatus.Failed);
		updatedSale.LastSyncAttemptAt.Should().NotBeNull();
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task IncreaseRetryCountAsync_ShouldIncreaseRetryCount()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);

		// Act
		await _syncCommand.IncreaseRetryCountAsync(sale.SaleId);

		var updatedSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		updatedSale.Should().NotBeNull();
		updatedSale!.RetryCount.Should().Be(sale.RetryCount + 1);
		updatedSale.LastSyncAttemptAt.Should().NotBeNull();
	}
}