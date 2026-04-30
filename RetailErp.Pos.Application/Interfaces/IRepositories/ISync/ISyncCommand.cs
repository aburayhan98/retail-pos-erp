using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Infrastructure.Repositories.Interfaces.ISync;

public interface ISyncCommand
{
	Task InsertCentralSaleAsync(Sale sale);
	Task MarkAsSyncedAsync(Guid saleId);
	Task MarkAsFailedAsync(Guid saleId);
	Task IncreaseRetryCountAsync(Guid saleId);
	Task MarkAsFailedAndIncreaseRetryAsync(Guid saleId);
}