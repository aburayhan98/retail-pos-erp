using FluentAssertions;
using RetailErp.Pos.Application.Common.Exceptions;
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
		var request = CommonObjectInit.CreateProductRequest();

		await _service.CreateAsync(request);

		var product = await _productQuery.GetByBarcodeAsync(request.Barcode);

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
		var request = CommonObjectInit.CreateProductRequest();

		await _service.CreateAsync(request);

		var action = async () => await _service.CreateAsync(request);

		await action.Should()
				.ThrowAsync<ConflictException>()
				.WithMessage("*barcode*");
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetAllAsync_ShouldReturnProducts()
	{
		var request = CommonObjectInit.CreateProductRequest();

		await _service.CreateAsync(request);

		var products = await _service.GetAllAsync();

		products.Should().Contain(x => x.Barcode == request.Barcode);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
	{
		var request = CommonObjectInit.CreateProductRequest();

		await _service.CreateAsync(request);

		var createdProduct = await _productQuery.GetByBarcodeAsync(request.Barcode);

		var product = await _service.GetByIdAsync(createdProduct!.ProductId);

		product.Should().NotBeNull();
		product!.ProductId.Should().Be(createdProduct.ProductId);
		product.Name.Should().Be(request.Name);
		product.Barcode.Should().Be(request.Barcode);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
	{
		var productId = Guid.NewGuid();

		var product = await _service.GetByIdAsync(productId);

		product.Should().BeNull();
	}
}
