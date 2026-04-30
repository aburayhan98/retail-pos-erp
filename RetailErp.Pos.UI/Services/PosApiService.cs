using RetailErp.Pos.Application.DTOs.Products;
using RetailErp.Pos.Application.DTOs.Sales;
using RetailErp.Pos.Application.DTOs.Sync;
using System.Net.Http.Json;

namespace RetailErp.Pos.UI.Services;

public sealed class PosApiService(HttpClient httpClient)
{
	private readonly HttpClient _httpClient = httpClient;

	// =========================
	// Product APIs
	// =========================

	public async Task<List<ProductResponse>> GetProductsAsync()
	{
		var result = await _httpClient.GetFromJsonAsync<List<ProductResponse>>("api/products");
		return result ?? new List<ProductResponse>();
	}

	public async Task CreateProductAsync(CreateProductRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync("api/products", request);

		if (!response.IsSuccessStatusCode)
		{
			var msg = await response.Content.ReadAsStringAsync();
			throw new Exception(msg);
		}
	}

	// =========================
	// Sales APIs
	// =========================

	public async Task CreateSaleAsync(CreateSaleRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync("api/sales", request);

		if (!response.IsSuccessStatusCode)
		{
			var msg = await response.Content.ReadAsStringAsync();
			throw new Exception(msg);
		}
	}

	// =========================
	// Sync API
	// =========================

	public async Task<SyncResponse?> SyncSalesAsync()
	{
		var response = await _httpClient.PostAsync("/sync-sales", null);

		if (!response.IsSuccessStatusCode)
		{
			var msg = await response.Content.ReadAsStringAsync();
			throw new Exception(msg);
		}

		return await response.Content.ReadFromJsonAsync<SyncResponse>();
	}
}