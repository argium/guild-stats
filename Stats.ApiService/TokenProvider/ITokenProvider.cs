namespace Stats.TokenProvider;

public interface ITokenProvider
{
	Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
}
