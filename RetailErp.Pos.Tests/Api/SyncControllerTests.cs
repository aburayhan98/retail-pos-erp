using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RetailErp.Pos.API.Controllers;
using RetailErp.Pos.Application.DTOs.Sync;
using RetailErp.Pos.Application.Interfaces.IServices;

namespace RetailErp.Pos.Tests.Api;

public sealed class SyncControllerTests
{
	[Fact]
	public async Task SyncSales_ShouldReturnOk_WithResponse()
	{
		var response = new SyncResponse
		{
			TotalProcessed = 4,
			SuccessCount = 3,
			FailedCount = 1,
			DuplicateCount = 0,
			Message = "Sync complete"
		};
		var controller = new SyncController(new FakeSyncService
		{
			Response = response
		});

		var result = await controller.SyncSales();

		var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
		ok.Value.Should().BeSameAs(response);
	}

	private sealed class FakeSyncService : ISyncService
	{
		public SyncResponse Response { get; set; } = new();

		public Task<SyncResponse> SyncSalesAsync() => Task.FromResult(Response);
	}
}
