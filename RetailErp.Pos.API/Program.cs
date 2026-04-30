using FluentValidation;
using Microsoft.OpenApi.Models;
using RetailErp.Pos.API.Filters;
using RetailErp.Pos.API.Middlewares;
using RetailErp.Pos.Application.DTOs.Products;
using RetailErp.Pos.Application.DTOs.Sales;
using RetailErp.Pos.Application.Interfaces.IRepositories.IProduct;
using RetailErp.Pos.Application.Interfaces.IRepositories.ISale;
using RetailErp.Pos.Application.Interfaces.IServices;
using RetailErp.Pos.Application.Services;
using RetailErp.Pos.Application.Validators;
using RetailErp.Pos.Infrastructure.Data;
using RetailErp.Pos.Infrastructure.Data.Command;
using RetailErp.Pos.Infrastructure.Data.Query;
using RetailErp.Pos.Infrastructure.Repositories.Commands;
using RetailErp.Pos.Infrastructure.Repositories.Interfaces.ISync;

var builder = WebApplication.CreateBuilder(args);

// =========================
// ?? Add Services
// =========================

builder.Services.AddScoped<ValidationFilter>();

builder.Services.AddControllers(options =>
{
	options.Filters.Add<ValidationFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Retail ERP POS API",
		Version = "v1"
	});
});

// =========================
// ?? Database Connection
// =========================

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// =========================
// ?? Application Services
// =========================

builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<ISyncService, SyncService>();

// =========================
// ?? Repositories (CQS)
// =========================

// Product
builder.Services.AddScoped<IProductCommand, ProductCommand>();
builder.Services.AddScoped<IProductQuery, ProductQuery>();

// Sale
builder.Services.AddScoped<ISaleCommand, SaleCommand>();
builder.Services.AddScoped<ISaleQuery, SaleQuery>();

// Sync
builder.Services.AddScoped<ISyncCommand, SyncCommand>();
builder.Services.AddScoped<ISyncQuery, SyncQuery>();

// =========================
// ?? FluentValidation
// =========================

builder.Services.AddTransient<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
builder.Services.AddTransient<IValidator<CreateSaleRequest>, CreateSalesRequestValidator>();
builder.Services.AddTransient<IValidator<CreateSaleItemRequest>, CreateSaleItemRequestValidator>();

// =========================
// ?? Build App
// =========================

var app = builder.Build();

// =========================
// ?? Middleware
// =========================

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
else
{
	app.UseHsts();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
