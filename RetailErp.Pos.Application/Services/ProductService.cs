namespace RetailErp.Pos.Application.Services;

using RetailErp.Pos.Application.DTOs.Products;
using RetailErp.Pos.Application.Interfaces.IRepositories.IProduct;
using RetailErp.Pos.Application.Interfaces.IServices;
using RetailErp.Pos.Domain.Entities;

public sealed class ProductsService(
		IProductCommand productCommand,
		IProductQuery productQuery) : IProductsService
{
	private readonly IProductCommand _productCommand = productCommand;
	private readonly IProductQuery _productQuery = productQuery;

	public async Task<Guid> CreateAsync(CreateProductRequest request)
	{
		if (request == null)
			throw new ArgumentNullException(nameof(request));

		if (string.IsNullOrWhiteSpace(request.Name))
			throw new ArgumentException("Product name is required.");

		if (string.IsNullOrWhiteSpace(request.Barcode))
			throw new ArgumentException("Barcode is required.");

		if (request.StockQuantity < 0)
			throw new ArgumentException("Stock quantity cannot be negative.");

		if (request.Price <= 0)
			throw new ArgumentException("Price must be greater than zero.");

		var existingProduct = await _productQuery.GetByBarcodeAsync(request.Barcode);

		if (existingProduct != null)
			throw new InvalidOperationException("Product with same barcode already exists.");

		var product = new Product
		{
			ProductId = Guid.NewGuid(),
			Name = request.Name.Trim(),
			Barcode = request.Barcode.Trim(),
			StockQuantity = request.StockQuantity,
			Price = request.Price,
			CreatedAt = DateTime.UtcNow
		};

		await _productCommand.CreateAsync(product);

		return product.ProductId;
	}

	public async Task<IReadOnlyList<ProductResponse>> GetAllAsync()
	{
		var products = await _productQuery.GetAllAsync();

		return products.Select(MapToResponse).ToList();
	}

	public async Task<ProductResponse?> GetByIdAsync(Guid productId)
	{
		var product = await _productQuery.GetByIdAsync(productId);

		return product is null ? null : MapToResponse(product);
	}

	private static ProductResponse MapToResponse(Product product)
	{
		return new ProductResponse
		{
			ProductId = product.ProductId,
			Name = product.Name,
			Barcode = product.Barcode,
			StockQuantity = product.StockQuantity,
			Price = product.Price
		};
	}
}
