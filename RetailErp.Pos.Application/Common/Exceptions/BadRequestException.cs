using System.Net;

namespace RetailErp.Pos.Application.Common.Exceptions;

public sealed class BadRequestException(
	string message,
	string? errorCode = null,
	IReadOnlyDictionary<string, string[]>? errors = null)
	: AppException(
		message,
		HttpStatusCode.BadRequest,
		"Bad request.",
		errorCode,
		errors);
