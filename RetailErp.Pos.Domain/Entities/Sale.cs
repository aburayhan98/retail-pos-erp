using RetailErp.Pos.Domain.Enums;

namespace RetailErp.Pos.Domain.Entities;

public sealed class Sale
{
	public Guid SaleId { get; set; }
	public string OutletId { get; set; } = string.Empty;
	public DateTime SaleDate { get; set; }
	public decimal TotalAmount { get; set; }
	public SyncStatus SyncStatus { get; set; }
	public int RetryCount { get; set; }
	public DateTime? LastSyncAttemptAt { get; set; }
	public List<SaleItem> Items { get; set; } = new();
}