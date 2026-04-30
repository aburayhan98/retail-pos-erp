using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Application.Interfaces.IRepositories.ISale;

public interface ISaleCommand
{
	Task CreateAsync(Sale sale);
	Task MarkSyncAsync(Guid saleId, bool isSuccess);
	Task MarkFailedAsync(Guid saleId);
	Task IncreaseRetryAsync(Guid saleId);
	Task InsertCentralSaleAsync(Sale sale);
}