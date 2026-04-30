using System.Net;

namespace RetailErp.Pos.Application.Common.Exceptions;

public abstract class AppException : Exception
{
	protected AppException(
		string message,
		HttpStatusCode statusCode,
		string title,
		string? errorCode = null,
		IReadOnlyDictionary<string, string[]>? errors = null,
		Exception? innerException = null) : base(message, innerException)
	{
		StatusCode = (int)statusCode;
		Title = title;
		ErrorCode = errorCode;
		Errors = errors;
	}

	public int StatusCode { get; }

	public string Title { get; }

	public string? ErrorCode { get; }

	public IReadOnlyDictionary<string, string[]>? Errors { get; }
}
