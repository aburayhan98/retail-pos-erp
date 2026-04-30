namespace RetailErp.Pos.Application.DTOs.Sales;

public sealed record SaleDto
{
	public Guid SaleId { get; set; }
	public string OutletId { get; set; } = string.Empty;
	public DateTime SaleDate { get; set; }
	public decimal TotalAmount { get; set; }
	public string SyncStatus { get; set; } = string.Empty;
}
