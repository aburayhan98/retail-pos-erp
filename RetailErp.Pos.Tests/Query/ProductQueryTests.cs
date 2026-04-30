using FluentAssertions;
using RetailErp.Pos.Infrastructure.Data.Command;
using RetailErp.Pos.Infrastructure.Data.Query;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Query;

public sealed class ProductQueryTests
{
	private readonly ProductCommand _command;
	private readonly ProductQuery _query;

	public ProductQueryTests()
	{
		var config = new ConfigTest();

		_command = new ProductCommand(config.DbConnectionFactory);
		_query = new ProductQuery(config.DbConnectionFactory);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
	{
		// Arrange
		var product = CommonObjectInit.Product();

		await _command.CreateAsync(product);

		// Act
		var result = await _query.GetByIdAsync(product.ProductId);

		// Assert
		result.Should().NotBeNull();
		result!.ProductId.Should().Be(product.ProductId);
		result.Name.Should().Be(product.Name);
		result.Barcode.Should().Be(product.Barcode);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
	{
		// Arrange
		var productId = Guid.NewGuid();

		// Act
		var result = await _query.GetByIdAsync(productId);

		// Assert
		result.Should().BeNull();
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByBarcodeAsync_ShouldReturnProduct_WhenExists()
	{
		// Arrange
		var product = CommonObjectInit.Product();

		await _command.CreateAsync(product);

		// Act
		var result = await _query.GetByBarcodeAsync(product.Barcode);

		// Assert
		result.Should().NotBeNull();
		result!.Barcode.Should().Be(product.Barcode);
		result.ProductId.Should().Be(product.ProductId);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByBarcodeAsync_ShouldReturnNull_WhenNotExists()
	{
		// Arrange
		var barcode = "NON_EXISTING_BARCODE";

		// Act
		var result = await _query.GetByBarcodeAsync(barcode);

		// Assert
		result.Should().BeNull();
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetAllAsync_ShouldReturnProducts()
	{
		// Arrange
		var product1 = CommonObjectInit.Product();
		var product2 = CommonObjectInit.Product();

		await _command.CreateAsync(product1);
		await _command.CreateAsync(product2);

		// Act
		var result = await _query.GetAllAsync();

		// Assert
		result.Should().NotBeNull();
		result.Should().Contain(x => x.ProductId == product1.ProductId);
		result.Should().Contain(x => x.ProductId == product2.ProductId);
	}
}