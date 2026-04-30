using RetailErp.Pos.Application.Common.Exceptions;
using RetailErp.Pos.Application.DTOs.Sales;
using RetailErp.Pos.Application.Interfaces.IRepositories.IProduct;
using RetailErp.Pos.Application.Interfaces.IRepositories.ISale;
using RetailErp.Pos.Application.Interfaces.IServices;
using RetailErp.Pos.Domain.Entities;
using RetailErp.Pos.Domain.Enums;

namespace RetailErp.Pos.Application.Services;

public sealed class SalesService(
		ISaleCommand saleCommand,
		ISaleQuery saleQuery,
		IProductQuery productQuery,
		IProductCommand productCommand) : ISalesService
{
	private readonly ISaleCommand _saleCommand = saleCommand;
	private readonly ISaleQuery _saleQuery = saleQuery;
	private readonly IProductQuery _productQuery = productQuery;
	private readonly IProductCommand _productCommand = productCommand;

	public async Task CreateAsync(CreateSaleRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		if (request.Items == null || request.Items.Count == 0)
			throw new BadRequestException("Sale must contain at least one item.", "sale_items_required");

		var saleId = Guid.NewGuid();
		var saleDate = DateTime.UtcNow;
		var saleItems = new List<SaleItem>();

		foreach (var item in request.Items)
		{
			if (item.Quantity <= 0)
				throw new BadRequestException("Quantity must be greater than zero.", "sale_item_quantity_invalid");

			if (item.UnitPrice <= 0)
				throw new BadRequestException("Unit price must be greater than zero.", "sale_item_price_invalid");

			var product = await _productQuery.GetByIdAsync(item.ProductId)
				?? throw new NotFoundException($"Product '{item.ProductId}' was not found.", "product_not_found");

			if (product.StockQuantity < item.Quantity)
				throw new ConflictException($"Insufficient stock for product '{product.Name}'.", "product_stock_insufficient");

			saleItems.Add(new SaleItem
			{
				SaleItemId = Guid.NewGuid(),
				SaleId = saleId,
				ProductId = item.ProductId,
				Barcode = item.Barcode,
				Quantity = item.Quantity,
				UnitPrice = item.UnitPrice
			});
		}

		var sale = new Sale
		{
			SaleId = saleId,
			OutletId = request.OutletId,
			SaleDate = saleDate,
			TotalAmount = saleItems.Sum(x => x.Quantity * x.UnitPrice),
			SyncStatus = SyncStatus.Pending,
			RetryCount = 0,
			LastSyncAttemptAt = null,
			Items = saleItems
		};

		await _saleCommand.CreateAsync(sale);

		//foreach (var item in saleItems)
		//{
		//	await _productCommand.ReduceStockAsync(item.ProductId, item.Quantity);
		//}
	}

	public async Task<IReadOnlyList<SaleResponse>> GetAllAsync()
	{
		var sales = await _saleQuery.GetAllAsync();

		return sales.Select(MapToResponse).ToList();
	}

	public async Task<SaleResponse?> GetByIdAsync(Guid saleId)
	{
		var sale = await _saleQuery.GetByIdAsync(saleId);

		return sale is null ? null : MapToResponse(sale);
	}

	private static SaleResponse MapToResponse(Sale sale)
	{
		return new SaleResponse
		{
			SaleId = sale.SaleId,
			OutletId = sale.OutletId,
			SaleDate = sale.SaleDate,
			TotalAmount = sale.TotalAmount,
			SyncStatus = sale.SyncStatus.ToString()
		};
	}
}
