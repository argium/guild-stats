using System.Net;

namespace Stats.GameData;

public class GameDataProviderException : Exception
{
	public GameDataProviderException()
	{
	}

	public GameDataProviderException(string message)
		: base(message)
	{
		StatusCode = GetStatusCode(message);
	}

	public GameDataProviderException(string message, Exception inner)
		: base(message, inner)
	{
		StatusCode = GetStatusCode(message);
	}

	public GameDataProviderException(string message, Exception inner, HttpStatusCode statusCode)
		: base(message, inner)
	{
		StatusCode = statusCode;
	}

	public HttpStatusCode GetStatusCode(string message)
	{
		if (message == "No guild exists for this name/server/region.") {
			return HttpStatusCode.BadRequest;
		}

		return HttpStatusCode.InternalServerError;
	}

	public HttpStatusCode StatusCode { get; } = HttpStatusCode.InternalServerError;
}
