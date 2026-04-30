using FluentAssertions;
using RetailErp.Pos.Domain.Enums;
using RetailErp.Pos.Infrastructure.Data.Command;
using RetailErp.Pos.Infrastructure.Data.Query;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Query;

public sealed class SalesQueryTests
{
	private readonly SaleCommand _command;
	private readonly SaleQuery _query;

	public SalesQueryTests()
	{
		var config = new ConfigTest();

		_command = new SaleCommand(config.DbConnectionFactory);
		_query = new SaleQuery(config.DbConnectionFactory);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByIdAsync_ShouldReturnSale_WhenExists()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _command.CreateAsync(sale);

		// Act
		var result = await _query.GetByIdAsync(sale.SaleId);

		// Assert
		result.Should().NotBeNull();
		result!.SaleId.Should().Be(sale.SaleId);
		result.OutletId.Should().Be(sale.OutletId);
		result.TotalAmount.Should().Be(sale.TotalAmount);
		result.SyncStatus.Should().Be(SyncStatus.Pending);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
	{
		// Arrange
		var saleId = Guid.NewGuid();

		// Act
		var result = await _query.GetByIdAsync(saleId);

		// Assert
		result.Should().BeNull();
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetAllAsync_ShouldReturnSales()
	{
		// Arrange
		var sale1 = CommonObjectInit.Sale();
		var sale2 = CommonObjectInit.Sale();

		await _command.CreateAsync(sale1);
		await _command.CreateAsync(sale2);

		// Act
		var result = await _query.GetAllAsync();

		// Assert
		result.Should().Contain(x => x.SaleId == sale1.SaleId);
		result.Should().Contain(x => x.SaleId == sale2.SaleId);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetPendingSalesAsync_ShouldReturnOnlyPendingAndFailedSales()
	{
		// Arrange
		var pendingSale = CommonObjectInit.Sale();
		var failedSale = CommonObjectInit.Sale();
		var syncedSale = CommonObjectInit.Sale();

		await _command.CreateAsync(pendingSale);
		await _command.CreateAsync(failedSale);
		await _command.CreateAsync(syncedSale);

		await _command.MarkFailedAsync(failedSale.SaleId);
		await _command.MarkSyncAsync(syncedSale.SaleId, true);

		// Act
		var result = await _query.GetPendingSalesAsync();

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

		await _command.InsertCentralSaleAsync(sale);

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
