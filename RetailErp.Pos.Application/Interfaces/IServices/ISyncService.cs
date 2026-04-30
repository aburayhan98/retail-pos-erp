using RetailErp.Pos.Application.DTOs.Sync;

namespace RetailErp.Pos.Application.Interfaces.IServices;

public interface ISyncService
{
	Task<SyncResponse> SyncSalesAsync();
}