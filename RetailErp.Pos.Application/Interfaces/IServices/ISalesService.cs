using RetailErp.Pos.Application.DTOs.Sales;


namespace RetailErp.Pos.Application.Interfaces.IServices;

public interface ISalesService
{
	Task CreateAsync(CreateSaleRequest request);
	Task<IReadOnlyList<SaleResponse>> GetAllAsync();
	Task<SaleResponse?> GetByIdAsync(Guid saleId);
}