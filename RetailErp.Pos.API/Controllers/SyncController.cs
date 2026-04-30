using Microsoft.AspNetCore.Mvc;
using RetailErp.Pos.Application.DTOs.Sync;
using RetailErp.Pos.Application.Interfaces.IServices;

namespace RetailErp.Pos.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SyncController(ISyncService syncService) : ControllerBase
	{
		private readonly ISyncService _syncService = syncService;

		[HttpPost("sales")]
		public async Task<ActionResult<SyncResponse>> SyncSales()
		{
			var response = await _syncService.SyncSalesAsync();

			return Ok(response);
		}
	}
}
