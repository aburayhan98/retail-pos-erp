
using Microsoft.AspNetCore.Mvc;
using RetailErp.Pos.Application.DTOs.Products;
using RetailErp.Pos.Application.Interfaces.IServices;

namespace RetailErp.Pos.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductsController(IProductsService productService) : ControllerBase
	{
		private readonly IProductsService _productService = productService;

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
		{
			var productId = await _productService.CreateAsync(request);

			return CreatedAtAction(nameof(GetById), new { productId }, new { productId });
		}

		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll()
		{
			var products = await _productService.GetAllAsync();

			return Ok(products);
		}

		[HttpGet("{productId:guid}")]
		public async Task<ActionResult<ProductResponse>> GetById(Guid productId)
		{
			var product = await _productService.GetByIdAsync(productId);

			if (product is null)
			{
				return NotFound();
			}

			return Ok(product);
		}
	}
}
