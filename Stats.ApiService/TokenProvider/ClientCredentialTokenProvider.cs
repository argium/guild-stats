using Microsoft.Extensions.Options;

namespace Stats.TokenProvider;

public class ClientCredentialTokenProvider : ITokenProvider
{
	private readonly IOptionsMonitor<WarcraftLogsOptions> settings;

	public ClientCredentialTokenProvider(IOptionsMonitor<WarcraftLogsOptions> settings)
	{
		this.settings = settings;
	}

	public Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
	{
		// TODO: use the client id and secret to get a token
		throw new NotImplementedException();
	}
}
