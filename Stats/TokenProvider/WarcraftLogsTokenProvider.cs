
using Microsoft.Extensions.Options;

namespace Stats.TokenProvider;

public class WarcraftLogsTokenProvider : ITokenProvider
{
	private readonly IOptionsMonitor<WarcraftLogsOptions> _options;

	public WarcraftLogsTokenProvider(IOptionsMonitor<WarcraftLogsOptions> options)
	{
		_options = options;
	}

	public Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
	{
		if (!string.IsNullOrWhiteSpace(_options.CurrentValue.Token))
		{
			return Task.FromResult(_options.CurrentValue.Token);
		}

		throw new NotImplementedException();
	}
}
