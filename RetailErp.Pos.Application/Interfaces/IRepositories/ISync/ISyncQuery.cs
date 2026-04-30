using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Infrastructure.Repositories.Interfaces.ISync;

public interface ISyncQuery
{
	Task<IReadOnlyList<Sale>> GetUnSyncedSalesAsync();
	Task<bool> ExistsInCentralAsync(Guid saleId);
}