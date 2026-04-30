using FluentAssertions;
using RetailErp.Pos.Application.Services;
using RetailErp.Pos.Infrastructure.Data.Command;
using RetailErp.Pos.Infrastructure.Data.Query;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Services;

public sealed class ProductsServiceTests
{
	private readonly ProductsService _service;
	private readonly ProductQuery _productQuery;

	public ProductsServiceTests()
	{
		var config = new ConfigTest();

		var productCommand = new ProductCommand(config.DbConnectionFactory);
		_productQuery = new ProductQuery(config.DbConnectionFactory);

		_service = new ProductsService(productCommand, _productQuery);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task CreateAsync_ShouldCreateProduct()
	{
		// Arrange
		var request = CommonObjectInit.CreateProductRequest();

		// Act
		await _service.CreateAsync(request);

		var product = await _productQuery.GetByBarcodeAsync(request.Barcode);

		// Assert
		product.Should().NotBeNull();
		product!.Name.Should().Be(request.Name);
		product.Barcode.Should().Be(request.Barcode);
		product.StockQuantity.Should().Be(request.StockQuantity);
		product.Price.Should().Be(request.Price);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task CreateAsync_ShouldThrow_WhenBarcodeAlreadyExists()
	{
		// Arrange
		var request = CommonObjectInit.CreateProductRequest();

		await _service.CreateAsync(request);

		// Act
		var action = async () => await _service.CreateAsync(request);

		// Assert
		await action.Should()
				.ThrowAsync<InvalidOperationException>()
				.WithMessage("*barcode*");
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetAllAsync_ShouldReturnProducts()
	{
		// Arrange
		var request = CommonObjectInit.CreateProductRequest();

		await _service.CreateAsync(request);

		// Act
		var products = await _service.GetAllAsync();

		// Assert
		products.Should().Contain(x => x.Barcode == request.Barcode);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
	{
		// Arrange
		var request = CommonObjectInit.CreateProductRequest();

		await _service.CreateAsync(request);

		var createdProduct = await _productQuery.GetByBarcodeAsync(request.Barcode);

		// Act
		var product = await _service.GetByIdAsync(createdProduct!.ProductId);

		// Assert
		product.Should().NotBeNull();
		product!.ProductId.Should().Be(createdProduct.ProductId);
		product.Name.Should().Be(request.Name);
		product.Barcode.Should().Be(request.Barcode);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
	{
		// Arrange
		var productId = Guid.NewGuid();

		// Act
		var product = await _service.GetByIdAsync(productId);

		// Assert
		product.Should().BeNull();
	}
}