using System.Text.Json;
using FluentValidation;

namespace RetailErp.Pos.API.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlingMiddleware> _logger;

	public ExceptionHandlingMiddleware(
		RequestDelegate next,
		ILogger<ExceptionHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unhandled exception while processing request {Method} {Path}.",
				context.Request.Method,
				context.Request.Path);

			await WriteErrorResponseAsync(context, exception);
		}
	}

	private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";

		var (statusCode, title, errors) = exception switch
		{
			ValidationException validationException => (
				StatusCodes.Status400BadRequest,
				"Validation failed.",
				validationException.Errors.Select(error => error.ErrorMessage).ToArray()),

			ArgumentException => (
				StatusCodes.Status400BadRequest,
				exception.Message,
				Array.Empty<string>()),

			KeyNotFoundException => (
				StatusCodes.Status404NotFound,
				exception.Message,
				Array.Empty<string>()),

			InvalidOperationException => (
				StatusCodes.Status409Conflict,
				exception.Message,
				Array.Empty<string>()),

			_ => (
				StatusCodes.Status500InternalServerError,
				"An unexpected error occurred.",
				Array.Empty<string>())
		};

		context.Response.StatusCode = statusCode;

		var payload = new
		{
			title,
			status = statusCode,
			traceId = context.TraceIdentifier,
			errors
		};

		await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
	}
}
