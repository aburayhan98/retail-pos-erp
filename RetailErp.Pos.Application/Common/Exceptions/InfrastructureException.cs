using System.Net;

namespace RetailErp.Pos.Application.Common.Exceptions;

public sealed class InfrastructureException(
	string message,
	Exception? innerException = null,
	string? errorCode = null)
	: AppException(
		message,
		HttpStatusCode.ServiceUnavailable,
		"Infrastructure failure.",
		errorCode,
		innerException: innerException);
