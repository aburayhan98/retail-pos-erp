using FluentAssertions;
using RetailErp.Pos.Domain.Enums;
using RetailErp.Pos.Infrastructure.Data.Command;
using RetailErp.Pos.Infrastructure.Data.Query;
using RetailErp.Pos.Infrastructure.Repositories.Commands;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Query;

public sealed class SyncQueryTests
{
	private readonly SaleCommand _saleCommand;
	private readonly SyncCommand _syncCommand;
	private readonly SyncQuery _query;

	public SyncQueryTests()
	{
		var config = new ConfigTest();

		_saleCommand = new SaleCommand(config.DbConnectionFactory);
		_syncCommand = new SyncCommand(config.DbConnectionFactory);
		_query = new SyncQuery(config.DbConnectionFactory);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetSyncedSalesAsync_ShouldReturnOnlyPendingAndFailedSales()
	{
		// Arrange
		var pendingSale = CommonObjectInit.Sale();
		var failedSale = CommonObjectInit.Sale();
		var syncedSale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(pendingSale);
		await _saleCommand.CreateAsync(failedSale);
		await _saleCommand.CreateAsync(syncedSale);

		await _syncCommand.MarkAsFailedAsync(failedSale.SaleId);
		await _syncCommand.MarkAsSyncedAsync(syncedSale.SaleId);

		// Act
		var result = await _query.GetSyncedSalesAsync();

		// Assert
		result.Should().Contain(x => x.SaleId == pendingSale.SaleId && x.SyncStatus == SyncStatus.Pending);
		result.Should().Contain(x => x.SaleId == failedSale.SaleId && x.SyncStatus == SyncStatus.Failed);
		result.Should().NotContain(x => x.SaleId == syncedSale.SaleId);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task ExistsInCentralAsync_ShouldReturnTrue_WhenSaleExistsInCentral()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _syncCommand.InsertCentralSaleAsync(sale);

		// Act
		var result = await _query.ExistsInCentralAsync(sale.SaleId);

		// Assert
		result.Should().BeTrue();
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task ExistsInCentralAsync_ShouldReturnFalse_WhenSaleNotExistsInCentral()
	{
		// Arrange
		var saleId = Guid.NewGuid();

		// Act
		var result = await _query.ExistsInCentralAsync(saleId);

		// Assert
		result.Should().BeFalse();
	}
}
