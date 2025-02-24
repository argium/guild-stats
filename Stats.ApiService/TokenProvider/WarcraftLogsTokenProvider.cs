using System.Text;
using Microsoft.Extensions.Options;

namespace Stats.TokenProvider;

public class WarcraftLogsTokenProvider : ITokenProvider
{
	private static readonly Uri TokenUri = new("https://www.warcraftlogs.com/oauth/token");
	private readonly ReaderWriterLockSlim _lock = new();
	private DateTimeOffset _expiry = DateTimeOffset.MinValue;
	private TokenResponse? _token;
	private readonly HttpClient _httpClient;
	private readonly IOptionsMonitor<WarcraftLogsOptions> _options;
	private readonly ILogger<WarcraftLogsTokenProvider> _log;

	public WarcraftLogsTokenProvider(HttpClient httpClient, ILogger<WarcraftLogsTokenProvider> log, IOptionsMonitor<WarcraftLogsOptions> options)
	{
		_httpClient = httpClient;
		_options = options;
		_options.OnChange(_ => _expiry = DateTimeOffset.MinValue);
		_log = log;
	}

	public Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
	{
		_lock.EnterUpgradeableReadLock();
		try
		{
			var options = _options.CurrentValue;

			if (!string.IsNullOrWhiteSpace(_options.CurrentValue.Token))
			{
				_log.LogDebug("Returning token from settings.");
				return Task.FromResult(_options.CurrentValue.Token);
			}

			if (_token is null || ShouldRefreshToken(options.BufferWindow))
			{
				_lock.EnterWriteLock();
				try
				{
					if (_token is null || ShouldRefreshToken(options.BufferWindow))
					{
						RefreshTokenInternal(cancellationToken).Wait(cancellationToken);
					}
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}

			return  Task.FromResult(_token.access_token);
		}
		catch (Exception ex)
		{
			_log.LogError(ex, "Failed to get token.");
			throw;
		}
		finally
		{
			_lock.ExitUpgradeableReadLock();
		}
	}

	private async Task RefreshTokenInternal(CancellationToken cancellationToken)
	{
		_log.LogInformation("Refreshing token.");

		var options = _options.CurrentValue;
		_httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.ClientId}:{options.ClientSecret}"))}");
		var request = new HttpRequestMessage(HttpMethod.Post, TokenUri);
		request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["grant_type"] = "client_credentials"
		});

		var response = await _httpClient.SendAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();
		_token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
		if (_token is null)
		{
			throw new InvalidOperationException("Failed to deserialise token response.");
		}

		_expiry = DateTimeOffset.UtcNow.AddSeconds(_token.expires_in);
	}

	private bool ShouldRefreshToken(TimeSpan buffer) => DateTimeOffset.UtcNow >= _expiry - buffer;

	private sealed record TokenResponse(string access_token, string token_type, long expires_in);
}
