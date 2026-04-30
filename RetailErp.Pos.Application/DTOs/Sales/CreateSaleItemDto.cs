namespace RetailErp.Pos.Application.DTOs.Sales;

public sealed record CreateSaleItemDto
{
	public Guid ProductId { get; set; }
	public string Barcode { get; set; } = string.Empty;
	public int Quantity { get; set; }
	public decimal UnitPrice { get; set; }
	public decimal LineTotal { get; set; }
}
