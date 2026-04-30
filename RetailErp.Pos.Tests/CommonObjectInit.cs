using RetailErp.Pos.Application.DTOs.Products;
using RetailErp.Pos.Application.DTOs.Sales;
using RetailErp.Pos.Domain.Entities;
using RetailErp.Pos.Domain.Enums;

namespace RetailErp.Pos.Tests;

public static class CommonObjectInit
{
	public static Product Product()
	{
		var unique = Guid.NewGuid().ToString("N")[..8];

		return new Product
		{
			ProductId = Guid.NewGuid(),
			Name = $"Test Product {unique}",
			Barcode = $"BAR-{unique}",
			StockQuantity = 100,
			Price = 120,
			CreatedAt = DateTime.UtcNow
		};
	}

	public static CreateProductRequest CreateProductRequest()
	{
		var unique = Guid.NewGuid().ToString("N")[..8];

		return new CreateProductRequest
		{
			Name = $"Test Product {unique}",
			Barcode = $"BAR-{unique}",
			StockQuantity = 100,
			Price = 120
		};
	}

	public static Sale Sale(Guid? outletId = null)
	{
		return new Sale
		{
			SaleId = Guid.NewGuid(),
			OutletId = outletId ?? Guid.NewGuid(),
			SaleDate = DateTime.UtcNow,
			TotalAmount = 240,
			SyncStatus = SyncStatus.Pending,
			RetryCount = 0,
			LastSyncAttemptAt = null,
			Items = new List<SaleItem>()
		};
	}

	public static CreateSaleRequest CreateSaleRequest(Product product)
	{
		return new CreateSaleRequest
		{
			OutletId = Guid.NewGuid(),
			Items = new List<CreateSaleItemRequest>
						{
								new()
								{
										ProductId = product.ProductId,
										Barcode = product.Barcode,
										Quantity = 2,
										UnitPrice = product.Price
								}
						}
		};
	}
}