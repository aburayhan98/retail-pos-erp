using RetailErp.Pos.Application.DTOs.Products;

namespace RetailErp.Pos.Application.Interfaces.IServices;



public interface IProductsService
{
	Task<Guid> CreateAsync(CreateProductRequest request);
	Task<IReadOnlyList<ProductResponse>> GetAllAsync();
	Task<ProductResponse?> GetByIdAsync(Guid productId);
}
