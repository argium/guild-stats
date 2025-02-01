namespace Stats.GameDataProvider;

public class GameDataProviderException : Exception
{
	public GameDataProviderException()
	{
	}

	public GameDataProviderException(string message)
		: base(message)
	{
	}

	public GameDataProviderException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
