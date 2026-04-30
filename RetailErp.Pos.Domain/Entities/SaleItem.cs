namespace RetailErp.Pos.Domain.Entities;

public sealed class SaleItem
{
	public Guid SaleItemId { get; set; }
	public Guid SaleId { get; set; }
	public Guid ProductId { get; set; }
	public string Barcode { get; set; } = string.Empty;
	public int Quantity { get; set; }
	public decimal UnitPrice { get; set; }
	public decimal LineTotal { get; set; }
}