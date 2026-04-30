using FluentAssertions;
using RetailErp.Pos.Application.Services;
using RetailErp.Pos.Domain.Enums;
using RetailErp.Pos.Infrastructure.Data.Command;
using RetailErp.Pos.Infrastructure.Data.Query;
using RetailErp.Pos.Infrastructure.Repositories.Commands;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Services;

public sealed class SyncServiceTests
{
	private readonly SaleCommand _saleCommand;
	private readonly SaleQuery _saleQuery;
	private readonly SyncCommand _syncCommand;
	private readonly SyncQuery _syncQuery;
	private readonly SyncService _syncService;

	public SyncServiceTests()
	{
		var config = new ConfigTest();

		_saleCommand = new SaleCommand(config.DbConnectionFactory);
		_saleQuery = new SaleQuery(config.DbConnectionFactory);

		_syncCommand = new SyncCommand(config.DbConnectionFactory);
		_syncQuery = new SyncQuery(config.DbConnectionFactory);

		_syncService = new SyncService(_syncCommand, _syncQuery);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task SyncSalesAsync_ShouldInsertPendingSaleIntoCentralSales_AndMarkAsSynced()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);

		// Act
		var response = await _syncService.SyncSalesAsync();

		var existsInCentral = await _syncQuery.ExistsInCentralAsync(sale.SaleId);
		var updatedSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		response.TotalProcessed.Should().BeGreaterThan(0);
		response.SuccessCount.Should().BeGreaterThan(0);
		response.SuccessfulSaleIds.Should().Contain(sale.SaleId);

		existsInCentral.Should().BeTrue();

		updatedSale.Should().NotBeNull();
		updatedSale!.SyncStatus.Should().Be(SyncStatus.Synced);
		updatedSale.LastSyncAttemptAt.Should().NotBeNull();
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task SyncSalesAsync_ShouldNotInsertDuplicate_WhenSaleAlreadyExistsInCentral()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);
		await _syncCommand.InsertCentralSaleAsync(sale);

		// Act
		var response = await _syncService.SyncSalesAsync();

		var updatedSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		response.TotalProcessed.Should().BeGreaterThan(0);
		response.DuplicateCount.Should().BeGreaterThan(0);
		response.DuplicateSaleIds.Should().Contain(sale.SaleId);

		updatedSale.Should().NotBeNull();
		updatedSale!.SyncStatus.Should().Be(SyncStatus.Synced);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task SyncSalesAsync_ShouldReturnZero_WhenNoPendingOrFailedSalesExist()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);
		await _syncCommand.MarkAsSyncedAsync(sale.SaleId);

		// Act
		var response = await _syncService.SyncSalesAsync();

		// Assert
		response.TotalProcessed.Should().Be(0);
		response.SuccessCount.Should().Be(0);
		response.FailedCount.Should().Be(0);
		response.DuplicateCount.Should().Be(0);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task SyncSalesAsync_ShouldProcessFailedSalesAgain()
	{
		// Arrange
		var sale = CommonObjectInit.Sale();

		await _saleCommand.CreateAsync(sale);
		await _syncCommand.MarkAsFailedAsync(sale.SaleId);

		// Act
		var response = await _syncService.SyncSalesAsync();

		var existsInCentral = await _syncQuery.ExistsInCentralAsync(sale.SaleId);
		var updatedSale = await _saleQuery.GetByIdAsync(sale.SaleId);

		// Assert
		response.TotalProcessed.Should().BeGreaterThan(0);
		response.SuccessfulSaleIds.Should().Contain(sale.SaleId);

		existsInCentral.Should().BeTrue();

		updatedSale.Should().NotBeNull();
		updatedSale!.SyncStatus.Should().Be(SyncStatus.Synced);
	}
}