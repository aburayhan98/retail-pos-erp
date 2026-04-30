using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Infrastructure.Repositories.Interfaces.ISync;

public interface ISyncQuery
{
	Task<IReadOnlyList<Sale>> GetSyncedSalesAsync();
	Task<bool> ExistsInCentralAsync(Guid saleId);
}