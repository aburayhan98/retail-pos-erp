using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RetailErp.Pos.Application.DTOs.Sync;
using RetailErp.Pos.Application.Interfaces.IServices;
using RetailErp.Pos.Infrastructure.Repositories.Interfaces.ISync;

namespace RetailErp.Pos.Application.Services;

public sealed class SyncService(
		ISyncCommand syncCommand,
		ISyncQuery syncQuery,
		ILogger<SyncService>? logger = null) : ISyncService
{
	private readonly ISyncCommand _syncCommand = syncCommand;
	private readonly ISyncQuery _syncQuery = syncQuery;
	private readonly ILogger<SyncService> _logger = logger ?? NullLogger<SyncService>.Instance;

	public async Task<SyncResponse> SyncSalesAsync()
	{
		var sales = await _syncQuery.GetUnSyncedSalesAsync();

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
			catch (Exception exception)
			{
				_logger.LogWarning(
					exception,
					"Failed to sync sale {SaleId}. The sale will be retried later.",
					sale.SaleId);

				response.FailedSaleIds.Add(sale.SaleId);
				response.FailedCount++;

				await _syncCommand.MarkAsFailedAndIncreaseRetryAsync(sale.SaleId);
			}
		}

		response.TotalProcessed = sales.Count;
		response.Message = response.FailedCount > 0
				? "Sync completed with some failed records."
				: "Sync completed successfully.";

		return response;
	}
}
