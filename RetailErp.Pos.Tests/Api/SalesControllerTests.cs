using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using RetailErp.Pos.API.Controllers;
using RetailErp.Pos.Application.DTOs.Sales;
using RetailErp.Pos.Application.Interfaces.IServices;
using RetailErp.Pos.Tests.Attributes;

namespace RetailErp.Pos.Tests.Api;

public sealed class SalesControllerTests
{
	[FactInDebugOnly]
	[WithRollback]
	public async Task Create_ShouldReturnNoContent()
	{
		var service = new FakeSalesService();
		var controller = new SalesController(service);
		var request = new CreateSaleRequest
		{
			OutletId = Guid.NewGuid(),
			Items =
			[
				new CreateSaleItemRequest
				{
					ProductId = Guid.NewGuid(),
					Barcode = "12345",
					Quantity = 2,
					UnitPrice = 10m
				}
			]
		};

		var result = await controller.Create(request);

		result.Should().BeOfType<NoContentResult>();
		service.CreateRequest.Should().BeSameAs(request);
	}

	[FactInDebugOnly]
	[WithRollback]
	public async Task GetAll_ShouldReturnOk_WithSales()
	{
		var sales = new List<SaleResponse>
		{
			new()
			{
				SaleId = Guid.NewGuid(),
				OutletId = Guid.NewGuid(),
				SaleDate = DateTime.UtcNow,
				TotalAmount = 20m,
				SyncStatus = "Pending"
			}
		};
		var controller = new SalesController(new FakeSalesService
		{
			AllSales = sales
		});

		var result = await controller.GetAll();

		var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
		ok.Value.Should().BeSameAs(sales);
	}

	[FactInDebugOnly]
	public async Task GetById_ShouldReturnOk_WhenSaleExists()
	{
		var saleId = Guid.NewGuid();
		var sale = new SaleResponse
		{
			SaleId = saleId,
			OutletId = Guid.NewGuid(),
			SaleDate = DateTime.UtcNow,
			TotalAmount = 20m,
			SyncStatus = "Pending"
		};
		var service = new FakeSalesService
		{
			SaleById = sale
		};
		var controller = new SalesController(service);

		var result = await controller.GetById(saleId);

		var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
		ok.Value.Should().BeSameAs(sale);
		service.RequestedSaleId.Should().Be(saleId);
	}

	[FactInDebugOnly]
	public async Task GetById_ShouldReturnNotFound_WhenSaleDoesNotExist()
	{
		var saleId = Guid.NewGuid();
		var service = new FakeSalesService();
		var controller = new SalesController(service);

		var result = await controller.GetById(saleId);

		result.Result.Should().BeOfType<NotFoundResult>();
		service.RequestedSaleId.Should().Be(saleId);
	}

	private sealed class FakeSalesService : ISalesService
	{
		public CreateSaleRequest? CreateRequest { get; private set; }
		public IReadOnlyList<SaleResponse> AllSales { get; set; } = [];
		public Guid RequestedSaleId { get; private set; }
		public SaleResponse? SaleById { get; set; }

		public Task CreateAsync(CreateSaleRequest request)
		{
			CreateRequest = request;
			return Task.CompletedTask;
		}

		public Task<IReadOnlyList<SaleResponse>> GetAllAsync() => Task.FromResult(AllSales);

		public Task<SaleResponse?> GetByIdAsync(Guid saleId)
		{
			RequestedSaleId = saleId;
			return Task.FromResult(SaleById);
		}
	}
}
