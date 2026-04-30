namespace RetailErp.Pos.Domain.Entities;

public sealed class Product
{
	public Guid ProductId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Barcode { get; set; } = string.Empty;
	public int StockQuantity { get; set; }
	public decimal Price { get; set; }
	public DateTime CreatedAt { get; set; }
}