using Microsoft.AspNetCore.Mvc;
using RetailErp.Pos.Application.DTOs.Sales;
using RetailErp.Pos.Application.Interfaces.IServices;

namespace RetailErp.Pos.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SalesController(ISalesService salesService) : ControllerBase
	{
		private readonly ISalesService _salesService = salesService;

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateSaleRequest request)
		{
			await _salesService.CreateAsync(request);

			return NoContent();
		}

		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<SaleResponse>>> GetAll()
		{
			var sales = await _salesService.GetAllAsync();

			return Ok(sales);
		}

		[HttpGet("{saleId:guid}")]
		public async Task<ActionResult<SaleResponse>> GetById(Guid saleId)
		{
			var sale = await _salesService.GetByIdAsync(saleId);

			if (sale is null)
			{
				return NotFound();
			}

			return Ok(sale);
		}
	}
}
