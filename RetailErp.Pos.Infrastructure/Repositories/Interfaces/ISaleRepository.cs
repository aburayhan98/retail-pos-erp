using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Infrastructure.Repositories.Interfaces;

public interface ISaleRepository
{
	Task CreateAsync(Sale sale);
	Task<IReadOnlyList<Sale>> GetAllAsync();
	Task MarkSyncAsync(Guid saleId, bool isSuccess);
	Task MarkFailedAsync(Guid saleId);
	Task IncreaseRetryAsync (Guid saleId);
	Task<bool> ExistsInCentralAsync(Guid saleId);
	Task InsertCentralSaleAsync(Sale sale);
}
