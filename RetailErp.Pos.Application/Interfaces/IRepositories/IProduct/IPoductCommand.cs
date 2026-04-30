using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Application.Interfaces.IRepositories.IProduct;

public interface IProductCommand
{
	Task CreateAsync(Product product);
	Task ReduceStockAsync(Guid productId, int quantity);
}