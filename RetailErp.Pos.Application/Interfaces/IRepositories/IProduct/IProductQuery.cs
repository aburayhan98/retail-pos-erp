using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Application.Interfaces.IRepositories.IProduct;

public interface IProductQuery
{
	Task<IReadOnlyList<Product>> GetAllAsync();
	Task<Product?> GetByIdAsync(Guid productId);
	Task<Product?> GetByBarcodeAsync(string barcode);
}