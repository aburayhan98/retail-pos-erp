namespace RetailErp.Pos.Application.DTOs.Products;

 public sealed record ProductDto
{
	public Guid ProductId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Barcode { get; set; } = string.Empty;
	public int StockQuantity { get; set; }
	public decimal Price { get; set; }
}
