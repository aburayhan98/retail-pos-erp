using RetailErp.Pos.Domain.Entities;

namespace RetailErp.Pos.Application.Interfaces.IRepositories.ISale;

public interface ISaleQuery
{
	Task<IReadOnlyList<Sale>> GetAllAsync();
	Task<IReadOnlyList<Sale>> GetPendingSalesAsync();
	Task<Sale?> GetByIdAsync(Guid saleId);
	Task<bool> ExistsInCentralAsync(Guid saleId);
}