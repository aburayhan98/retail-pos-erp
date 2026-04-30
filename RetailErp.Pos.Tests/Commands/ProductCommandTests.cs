using FluentAssertions;
using RetailErp.Pos.Infrastructure.Data.Command;
using RetailErp.Pos.Infrastructure.Data.Query;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Commands;

public sealed class ProductCommandTests
{
	private readonly ProductCommand _command;
	private readonly ProductQuery _query;

	public ProductCommandTests()
	{
		var config = new ConfigTest();

		_command = new ProductCommand(config.DbConnectionFactory);
		_query = new ProductQuery(config.DbConnectionFactory);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task CreateAsync_ShouldCreateProduct()
	{
		var product = CommonObjectInit.Product();

		await _command.CreateAsync(product);

		var createdProduct = await _query.GetByIdAsync(product.ProductId);

		createdProduct.Should().NotBeNull();
		createdProduct!.ProductId.Should().Be(product.ProductId);
		createdProduct.Name.Should().Be(product.Name);
		createdProduct.Barcode.Should().Be(product.Barcode);
		createdProduct.StockQuantity.Should().Be(product.StockQuantity);
		createdProduct.Price.Should().Be(product.Price);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task ReduceStockAsync_ShouldReduceProductStock()
	{
		var product = CommonObjectInit.Product();

		await _command.CreateAsync(product);

		await _command.ReduceStockAsync(product.ProductId, 5);

		var updatedProduct = await _query.GetByIdAsync(product.ProductId);

		updatedProduct.Should().NotBeNull();
		updatedProduct!.StockQuantity.Should().Be(95);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByBarcodeAsync_ShouldReturnProduct()
	{
		var product = CommonObjectInit.Product();

		await _command.CreateAsync(product);

		var result = await _query.GetByBarcodeAsync(product.Barcode);

		result.Should().NotBeNull();
		result!.Barcode.Should().Be(product.Barcode);
	}
}