namespace RetailErp.Pos.Application.DTOs.Sales;

public sealed record CreateSaleDto
{
	public string OutletId { get; set; } = string.Empty;
	public DateTime SaleDate { get; set; }
	public decimal TotalAmount { get; set; }
	public List<CreateSaleItemDto> Items { get; set; } = new();
}
