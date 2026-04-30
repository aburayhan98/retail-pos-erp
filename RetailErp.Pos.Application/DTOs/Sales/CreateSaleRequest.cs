using RetailErp.Pos.Application.DTOs.Sales;

public class CreateSaleRequest
{
	public Guid OutletId { get; set; }
	public List<CreateSaleItemRequest> Items { get; set; } = new();
}

