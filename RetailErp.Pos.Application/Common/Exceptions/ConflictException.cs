using System.Net;

namespace RetailErp.Pos.Application.Common.Exceptions;

public sealed class ConflictException(
	string message,
	string? errorCode = null)
	: AppException(
		message,
		HttpStatusCode.Conflict,
		"Conflict.",
		errorCode);
