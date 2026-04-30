using System.Net;

namespace RetailErp.Pos.Application.Common.Exceptions;

public sealed class NotFoundException(
	string message,
	string? errorCode = null)
	: AppException(
		message,
		HttpStatusCode.NotFound,
		"Resource not found.",
		errorCode);
