using FluentAssertions;
using RetailErp.Pos.Application.Services;
using RetailErp.Pos.Infrastructure.Data.Command;
using RetailErp.Pos.Infrastructure.Data.Query;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Services;

public sealed class SalesServiceTests
{
	private readonly SalesService _salesService;
	private readonly ProductCommand _productCommand;
	private readonly ProductQuery _productQuery;
	private readonly SaleQuery _saleQuery;

	public SalesServiceTests()
	{
		var config = new ConfigTest();

		_productCommand = new ProductCommand(config.DbConnectionFactory);
		_productQuery = new ProductQuery(config.DbConnectionFactory);

		var saleCommand = new SaleCommand(config.DbConnectionFactory);
		_saleQuery = new SaleQuery(config.DbConnectionFactory);

		_salesService = new SalesService(
				saleCommand,
				_saleQuery,
				_productQuery,
				_productCommand);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task CreateAsync_ShouldCreateSale_AndReduceStock()
	{
		// Arrange
		var product = CommonObjectInit.Product();
		product.StockQuantity = 10;

		await _productCommand.CreateAsync(product);

		var request = CommonObjectInit.CreateSaleRequest(product);
		var expectedStock = product.StockQuantity - request.Items.Sum(x => x.Quantity);
		var expectedTotal = request.Items.Sum(x => x.Quantity * x.UnitPrice);

		// Act
		await _salesService.CreateAsync(request);

		var updatedProduct = await _productQuery.GetByIdAsync(product.ProductId);
		var sales = await _salesService.GetAllAsync();

		// Assert
		updatedProduct.Should().NotBeNull();
		updatedProduct!.StockQuantity.Should().Be(expectedStock);

		sales.Should().Contain(x =>
				x.OutletId == request.OutletId &&
				x.TotalAmount == expectedTotal &&
				x.SyncStatus == "Pending");
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task CreateAsync_ShouldThrow_WhenProductNotFound()
	{
		// Arrange
		var product = CommonObjectInit.Product();
		var request = CommonObjectInit.CreateSaleRequest(product);

		// Act
		var action = async () => await _salesService.CreateAsync(request);

		// Assert
		await action.Should()
				.ThrowAsync<InvalidOperationException>()
				.WithMessage("*Product not found*");
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task CreateAsync_ShouldThrow_WhenStockIsInsufficient()
	{
		// Arrange
		var product = CommonObjectInit.Product();
		product.StockQuantity = 1;

		await _productCommand.CreateAsync(product);

		var request = CommonObjectInit.CreateSaleRequest(product);
		request.Items[0].Quantity = 5;

		// Act
		var action = async () => await _salesService.CreateAsync(request);

		// Assert
		await action.Should()
				.ThrowAsync<InvalidOperationException>()
				.WithMessage("*Insufficient stock*");
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task CreateAsync_ShouldThrow_WhenSaleItemsAreEmpty()
	{
		// Arrange
		var request = CommonObjectInit.CreateSaleRequest(CommonObjectInit.Product());
		request.Items.Clear();

		// Act
		var action = async () => await _salesService.CreateAsync(request);

		// Assert
		await action.Should()
				.ThrowAsync<ArgumentException>()
				.WithMessage("*at least one item*");
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task CreateAsync_ShouldThrow_WhenQuantityIsZero()
	{
		// Arrange
		var product = CommonObjectInit.Product();

		await _productCommand.CreateAsync(product);

		var request = CommonObjectInit.CreateSaleRequest(product);
		request.Items[0].Quantity = 0;

		// Act
		var action = async () => await _salesService.CreateAsync(request);

		// Assert
		await action.Should()
				.ThrowAsync<ArgumentException>()
				.WithMessage("*Quantity*");
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task CreateAsync_ShouldThrow_WhenUnitPriceIsZero()
	{
		// Arrange
		var product = CommonObjectInit.Product();

		await _productCommand.CreateAsync(product);

		var request = CommonObjectInit.CreateSaleRequest(product);
		request.Items[0].UnitPrice = 0;

		// Act
		var action = async () => await _salesService.CreateAsync(request);

		// Assert
		await action.Should()
				.ThrowAsync<ArgumentException>()
				.WithMessage("*Unit price*");
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByIdAsync_ShouldReturnSale_WhenSaleExists()
	{
		// Arrange
		var product = CommonObjectInit.Product();

		await _productCommand.CreateAsync(product);

		var request = CommonObjectInit.CreateSaleRequest(product);

		await _salesService.CreateAsync(request);

		var createdSale = (await _saleQuery.GetAllAsync())
				.FirstOrDefault(x => x.OutletId == request.OutletId);

		// Act
		var result = await _salesService.GetByIdAsync(createdSale!.SaleId);

		// Assert
		result.Should().NotBeNull();
		result!.SaleId.Should().Be(createdSale.SaleId);
		result.OutletId.Should().Be(request.OutletId);
		result.TotalAmount.Should().Be(request.Items.Sum(x => x.Quantity * x.UnitPrice));
		result.SyncStatus.Should().Be("Pending");
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByIdAsync_ShouldReturnNull_WhenSaleDoesNotExist()
	{
		// Arrange
		var saleId = Guid.NewGuid();

		// Act
		var result = await _salesService.GetByIdAsync(saleId);

		// Assert
		result.Should().BeNull();
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetAllAsync_ShouldReturnSales()
	{
		// Arrange
		var product = CommonObjectInit.Product();

		await _productCommand.CreateAsync(product);

		var request = CommonObjectInit.CreateSaleRequest(product);

		await _salesService.CreateAsync(request);

		// Act
		var result = await _salesService.GetAllAsync();

		// Assert
		result.Should().NotBeNull();
		result.Should().Contain(x =>
				x.OutletId == request.OutletId &&
				x.TotalAmount == request.Items.Sum(i => i.Quantity * i.UnitPrice));
	}
}