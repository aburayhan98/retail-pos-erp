using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Infrastructure.Repositories.Interfaces
{
	public interface IProductRepository
	{
		Task CreateAsync(Product product);
		Task <IReadOnlyList<Product>> GetAllAsync();
		Task<Product?> GetByIdAsync(Guid productId);
		Task ReduceStockAsync(Guid productId, int quantity);
	}
}
