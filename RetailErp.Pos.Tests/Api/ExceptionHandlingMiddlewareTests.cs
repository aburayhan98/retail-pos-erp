using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using RetailErp.Pos.API.Middlewares;
using RetailErp.Pos.Application.Common.Exceptions;

namespace RetailErp.Pos.Tests.Api;

public sealed class ExceptionHandlingMiddlewareTests
{
	[Fact]
	public async Task InvokeAsync_ShouldReturnAppExceptionProblemDetails()
	{
		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();
		context.Request.Path = "/api/products";
		var middleware = new ExceptionHandlingMiddleware(
			_ => throw new BadRequestException("Barcode already exists.", "duplicate_barcode"),
			NullLogger<ExceptionHandlingMiddleware>.Instance);

		await middleware.InvokeAsync(context);

		context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
		context.Response.ContentType.Should().Be("application/json");

		var json = await ReadResponseBodyAsync(context);
		json.RootElement.GetProperty("title").GetString().Should().Be("Bad request.");
		json.RootElement.GetProperty("detail").GetString().Should().Be("Barcode already exists.");
		json.RootElement.GetProperty("instance").GetString().Should().Be("/api/products");
		json.RootElement.GetProperty("errorCode").GetString().Should().Be("duplicate_barcode");
		json.RootElement.TryGetProperty("traceId", out _).Should().BeTrue();
	}

	[Fact]
	public async Task InvokeAsync_ShouldReturnValidationProblemDetails_ForValidationException()
	{
		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();
		context.Request.Path = "/api/sales";
		var failures = new[]
		{
			new ValidationFailure("Items", "At least one item is required.")
		};
		var middleware = new ExceptionHandlingMiddleware(
			_ => throw new ValidationException(failures),
			NullLogger<ExceptionHandlingMiddleware>.Instance);

		await middleware.InvokeAsync(context);

		context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

		var json = await ReadResponseBodyAsync(context);
		json.RootElement.GetProperty("title").GetString().Should().Be("Validation failed.");
		json.RootElement.GetProperty("errorCode").GetString().Should().Be("validation_failed");
		GetPropertyIgnoreCase(json.RootElement, "errors").GetProperty("Items")[0].GetString()
			.Should().Be("At least one item is required.");
	}

	[Fact]
	public async Task InvokeAsync_ShouldReturnInternalServerError_ForUnexpectedException()
	{
		var context = new DefaultHttpContext();
		context.Response.Body = new MemoryStream();
		context.Request.Path = "/api/sync/sales";
		var middleware = new ExceptionHandlingMiddleware(
			_ => throw new InvalidOperationException("Boom"),
			NullLogger<ExceptionHandlingMiddleware>.Instance);

		await middleware.InvokeAsync(context);

		context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

		var json = await ReadResponseBodyAsync(context);
		json.RootElement.GetProperty("title").GetString().Should().Be("An unexpected error occurred.");
		json.RootElement.GetProperty("detail").GetString().Should().Be("An unexpected error occurred.");
		json.RootElement.GetProperty("errorCode").GetString().Should().Be("unexpected_error");
	}

	private static async Task<JsonDocument> ReadResponseBodyAsync(HttpContext context)
	{
		context.Response.Body.Seek(0, SeekOrigin.Begin);
		return await JsonDocument.ParseAsync(context.Response.Body);
	}

	private static JsonElement GetPropertyIgnoreCase(JsonElement element, string propertyName)
	{
		foreach (var property in element.EnumerateObject())
		{
			if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
			{
				return property.Value;
			}
		}

		throw new KeyNotFoundException($"Property '{propertyName}' was not found.");
	}
}
