using RetailErp.Pos.Application.DTOs.Sync;
using RetailErp.Pos.Application.Interfaces.IServices;
using RetailErp.Pos.Infrastructure.Repositories.Interfaces.ISync;

namespace RetailErp.Pos.Application.Services;

public sealed class SyncService(
		ISyncCommand syncCommand,
		ISyncQuery syncQuery) : ISyncService
{
	private readonly ISyncCommand _syncCommand = syncCommand;
	private readonly ISyncQuery _syncQuery = syncQuery;

	public async Task<SyncResponse> SyncSalesAsync()
	{
		var sales = await _syncQuery.GetSyncedSalesAsync();

		var response = new SyncResponse
		{
			SyncedAt = DateTime.UtcNow
		};

		foreach (var sale in sales)
		{
			try
			{
				var exists = await _syncQuery.ExistsInCentralAsync(sale.SaleId);

				if (!exists)
				{
					await _syncCommand.InsertCentralSaleAsync(sale);

					response.SuccessfulSaleIds.Add(sale.SaleId);
					response.SuccessCount++;
				}
				else
				{
					response.DuplicateSaleIds.Add(sale.SaleId);
					response.DuplicateCount++;
				}

				await _syncCommand.MarkAsSyncedAsync(sale.SaleId);
			}
			catch
			{
				response.FailedSaleIds.Add(sale.SaleId);
				response.FailedCount++;

				await _syncCommand.IncreaseRetryCountAsync(sale.SaleId);
				await _syncCommand.MarkAsFailedAsync(sale.SaleId);
			}
		}

		response.TotalProcessed = sales.Count;
		response.Message = response.FailedCount > 0
				? "Sync completed with some failed records."
				: "Sync completed successfully.";

		return response;
	}
}