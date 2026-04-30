namespace RetailErp.Pos.Application.DTOs.Sync;
public sealed class SyncResponse
{

	public int TotalProcessed { get; set; }
	public int SuccessCount { get; set; }
	public int FailedCount { get; set; }
	public int DuplicateCount { get; set; }

	public List<Guid> SuccessfulSaleIds { get; set; } = [];
	public List<Guid> FailedSaleIds { get; set; } = [];
	public List<Guid> DuplicateSaleIds { get; set; } = [];

	public DateTime SyncedAt { get; set; } = DateTime.UtcNow;

	public string Message { get; set; } = string.Empty;
}