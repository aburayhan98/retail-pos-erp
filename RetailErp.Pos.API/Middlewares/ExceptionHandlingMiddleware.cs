using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RetailErp.Pos.Application.Common.Exceptions;

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
			_logger.LogError(
				exception,
				"Unhandled exception while processing request {Method} {Path}.",
				context.Request.Method,
				context.Request.Path);

			await WriteErrorResponseAsync(context, exception);
		}
	}

	private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";

		var problemDetails = exception switch
		{
			ValidationException validationException => CreateValidationProblemDetails(context, validationException),
			AppException appException => CreateProblemDetails(context, appException),
			ArgumentNullException argumentNullException => CreateProblemDetails(
				context,
				StatusCodes.Status400BadRequest,
				"Bad request.",
				argumentNullException.Message,
				"argument_null"),
			_ => CreateProblemDetails(
				context,
				StatusCodes.Status500InternalServerError,
				"An unexpected error occurred.",
				"An unexpected error occurred.",
				"unexpected_error")
		};

		context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

		await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
	}

	private static ProblemDetails CreateProblemDetails(HttpContext context, AppException exception)
	{
		var problemDetails = CreateProblemDetails(
			context,
			exception.StatusCode,
			exception.Title,
			exception.Message,
			exception.ErrorCode);

		if (exception.Errors is { Count: > 0 })
		{
			problemDetails.Extensions["errors"] = exception.Errors;
		}

		return problemDetails;
	}

	private static ProblemDetails CreateProblemDetails(
		HttpContext context,
		int statusCode,
		string title,
		string detail,
		string? errorCode)
	{
		var problemDetails = new ProblemDetails
		{
			Status = statusCode,
			Title = title,
			Detail = detail,
			Instance = context.Request.Path
		};

		problemDetails.Extensions["traceId"] = context.TraceIdentifier;

		if (!string.IsNullOrWhiteSpace(errorCode))
		{
			problemDetails.Extensions["errorCode"] = errorCode;
		}

		return problemDetails;
	}

	private static ValidationProblemDetails CreateValidationProblemDetails(
		HttpContext context,
		ValidationException exception)
	{
		var errors = exception.Errors
			.GroupBy(error => error.PropertyName)
			.ToDictionary(
				group => group.Key,
				group => group.Select(error => error.ErrorMessage).ToArray());

		var problemDetails = new ValidationProblemDetails(errors)
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "Validation failed.",
			Detail = "One or more validation errors occurred.",
			Instance = context.Request.Path
		};

		problemDetails.Extensions["traceId"] = context.TraceIdentifier;
		problemDetails.Extensions["errorCode"] = "validation_failed";

		return problemDetails;
	}
}
