using FluentAssertions;
using RetailErp.Pos.Domain.Enums;
using RetailErp.Pos.Infrastructure.Data.Command;
using RetailErp.Pos.Infrastructure.Data.Query;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Commands;

public sealed class SalesCommandTests
{
	private readonly SaleCommand _saleCommand;
	private readonly SaleQuery _saleQuery;

	public SalesCommandTests()
	{
		var config = new ConfigTest();

		_saleCommand = new SaleCommand(config.DbConnectionFactory);
		_saleQuery = new SaleQuery(config.DbConnectionFactory);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task CreateAsync_ShouldCreateSale()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		// Act
		await _saleCommand.CreateAsync(sale);

		var createdSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		createdSale.Should().NotBeNull();
		createdSale!.SaleId.Should().Be(sale.SaleId);
		createdSale.OutletId.Should().Be(sale.OutletId);
		createdSale.TotalAmount.Should().Be(sale.TotalAmount);
		createdSale.SyncStatus.Should().Be(SyncStatus.Pending);
		createdSale.RetryCount.Should().Be(0);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task MarkAsSyncedAsync_ShouldUpdateSaleStatusToSynced()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);

		// Act
		await _saleCommand.MarkSyncAsync(sale.SaleId, true);

		var updatedSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		updatedSale.Should().NotBeNull();
		updatedSale!.SyncStatus.Should().Be(SyncStatus.Synced);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task MarkSyncAsync_ShouldUpdateSaleStatusToFailed_WhenIsSuccessFalse()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);

		// Act
		await _saleCommand.MarkSyncAsync(sale.SaleId, false);

		var updatedSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		updatedSale.Should().NotBeNull();
		updatedSale!.SyncStatus.Should().Be(SyncStatus.Failed);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task MarkFailedAsync_ShouldUpdateSaleStatusToFailed()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);

		// Act
		await _saleCommand.MarkFailedAsync(sale.SaleId);

		var updatedSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		updatedSale.Should().NotBeNull();
		updatedSale!.SyncStatus.Should().Be(SyncStatus.Failed);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task IncreaseRetryAsync_ShouldIncreaseRetryCount()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);

		// Act
		await _saleCommand.IncreaseRetryAsync(sale.SaleId);

		var updatedSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		updatedSale.Should().NotBeNull();
		updatedSale!.RetryCount.Should().Be(sale.RetryCount + 1);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task InsertCentralSaleAsync_ShouldInsertSaleIntoCentralSales()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		// Act
		await _saleCommand.InsertCentralSaleAsync(sale);

		var exists = await _saleQuery.ExistsInCentralAsync(sale.SaleId);

		// Assert
		exists.Should().BeTrue();
	}
}