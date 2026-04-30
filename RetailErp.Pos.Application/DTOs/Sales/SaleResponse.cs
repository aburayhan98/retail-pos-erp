namespace RetailErp.Pos.Application.DTOs.Sales;

public class SaleResponse
{
	public Guid SaleId { get; set; }
	public Guid OutletId { get; set; }
	public DateTime SaleDate { get; set; }
	public decimal TotalAmount { get; set; }
	public string SyncStatus { get; set; } = string.Empty;
	public int RetryCount { get; set; }
	public DateTime? LastSyncAttemptAt { get; set; }
}
