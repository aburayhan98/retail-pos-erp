using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RetailErp.Pos.API.Controllers;
using RetailErp.Pos.Application.DTOs.Products;
using RetailErp.Pos.Application.Interfaces.IServices;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Api;

public sealed class ProductsControllerTests
{
	[Fact]
	public async Task Create_ShouldReturnCreatedAtAction_WithCreatedProductId()
	{
		var productId = Guid.NewGuid();
		var request = CreateProductRequest();
		var service = new StubProductsService
		{
			CreateAsyncResult = productId
		};
		var controller = CreateController(service);

		IActionResult actionResult = await controller.Create(request);

		var createdResult = actionResult.Should().BeOfType<CreatedAtActionResult>().Subject;
		createdResult.ActionName.Should().Be(nameof(ProductsController.GetById));
		createdResult.RouteValues.Should().NotBeNull();
		createdResult.RouteValues!["productId"].Should().Be(productId);
		createdResult.Value.Should().BeEquivalentTo(new { productId });
		service.CreateAsyncRequest.Should().BeSameAs(request);
	}

	[FactInDebugOnly]

	public async Task GetAll_ShouldReturnOk_WithProducts()
	{
		var products = new List<ProductResponse>
		{
			CreateProductResponse()
		};
		var service = new StubProductsService
		{
			GetAllAsyncResult = products
		};
		var controller = CreateController(service);

		ActionResult<IReadOnlyList<ProductResponse>> actionResult = await controller.GetAll();

		var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
		okResult.Value.Should().BeSameAs(products);
	}

	[Fact]
	public async Task GetById_ShouldReturnOk_WhenProductExists()
	{
		var product = CreateProductResponse();
		var service = new StubProductsService
		{
			GetByIdAsyncResult = product
		};
		var controller = CreateController(service);

		ActionResult<ProductResponse> actionResult = await controller.GetById(product.ProductId);

		var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
		okResult.Value.Should().BeSameAs(product);
		service.GetByIdAsyncProductId.Should().Be(product.ProductId);
	}

	[Fact]
	public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
	{
		var productId = Guid.NewGuid();
		var service = new StubProductsService();
		var controller = CreateController(service);

		ActionResult<ProductResponse> actionResult = await controller.GetById(productId);

		actionResult.Result.Should().BeOfType<NotFoundResult>();
		service.GetByIdAsyncProductId.Should().Be(productId);
	}

	private static ProductsController CreateController(IProductsService productsService) =>
		new(productsService);

	private static CreateProductRequest CreateProductRequest() =>
		new()
		{
			Name = "Milk",
			Barcode = "12345",
			StockQuantity = 7,
			Price = 12.5m
		};

	private static ProductResponse CreateProductResponse() =>
		new()
		{
			ProductId = Guid.NewGuid(),
			Name = "Milk",
			Barcode = "12345",
			StockQuantity = 4,
			Price = 12.5m
		};

	private sealed class StubProductsService : IProductsService
	{
		public Guid CreateAsyncResult { get; set; }
		public CreateProductRequest? CreateAsyncRequest { get; private set; }
		public IReadOnlyList<ProductResponse> GetAllAsyncResult { get; set; } = [];
		public Guid GetByIdAsyncProductId { get; private set; }
		public ProductResponse? GetByIdAsyncResult { get; set; }

		public Task<Guid> CreateAsync(CreateProductRequest request)
		{
			CreateAsyncRequest = request;
			return Task.FromResult(CreateAsyncResult);
		}

		public Task<IReadOnlyList<ProductResponse>> GetAllAsync() =>
			Task.FromResult(GetAllAsyncResult);

		public Task<ProductResponse?> GetByIdAsync(Guid productId)
		{
			GetByIdAsyncProductId = productId;
			return Task.FromResult(GetByIdAsyncResult);
		}
	}
}
