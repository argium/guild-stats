
using System.Runtime.CompilerServices;
using System.Text.Json;
using GraphQL;
using GraphQL.Client.Abstractions.Websocket;
using Microsoft.Extensions.Caching.Hybrid;

namespace Stats.GameData;
public class WarcraftLogsGameDataProvider : IGameDataProvider
{
	private readonly IGraphQLWebSocketClient _graphQLClient;
	private readonly ILogger<WarcraftLogsGameDataProvider> _log;
	private readonly HybridCache _cache;

	/// <summary>
	/// Lazy loaded world data. This should not change within the same game version.
	/// </summary>
	private static readonly Lazy<WorldData> WorldData = new(() =>
	{
		var json = File.ReadAllText("GameData/Zones.json");
		return JsonSerializer.Deserialize<DataMessage<WorldDataMessage>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))?.Data?.WorldData ?? throw new InvalidOperationException("Failed to deserialize WorldDataMessage");
	});

	/// <summary>
	/// Initializes a new instance of the <see cref="WarcraftLogsGameDataProvider"/> class.
	/// </summary>
	/// <param name="graphQLClient"></param>
	/// <param name="cache"></param>
	/// <param name="log"></param>
	public WarcraftLogsGameDataProvider(IGraphQLWebSocketClient graphQLClient, HybridCache cache, ILogger<WarcraftLogsGameDataProvider> log)
	{
		_graphQLClient = graphQLClient;
		_log = log;
		_cache = cache;
	}

	/// <inheritdoc/>
	public async IAsyncEnumerable<Report> GetAllFightReportsAsync(
		string guildName,
		string guildServerSlug,
		string guildServerRegion,
		string? guildTag,
		Zone zone,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		await foreach (var code in GetGuildReportCodesAsync(guildName, guildServerSlug, guildServerRegion, guildTag, zone, cancellationToken))
		{
			var reportsData = new GraphQLRequest
			{
				Query = GetFightDetailsQuery,
				Variables = new
				{
					code
				}
			};

			var cacheKey = "report:" + code;
			var value = await _cache.GetOrCreateAsync(
				cacheKey,
				async (ct) =>
				{
					this._log.LogInformation("Cache miss for report code {Code}", code);
					var reportsDataResp = await this.ExecuteAsync<ReportsDataMessage>(reportsData, cancellationToken);
					this.CheckRateLimit(reportsDataResp.RateLimitData);
					return reportsDataResp.ReportData.Report;
				},
				options: new() { Expiration = TimeSpan.FromHours(24) },
				cancellationToken: cancellationToken);

			yield return value;
		}
	}

	/// <summary>
	/// Get all report codes for a guild and/or guild tag.
	/// </summary>
	/// <param name="guildName"></param>
	/// <param name="guildServerSlug"></param>
	/// <param name="guildServerRegion"></param>
	/// <param name="guildTag">The guild tag (eg. "T1"). This takes precedence over all other guild arguments.</param>
	/// <param name="zone"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <exception cref="GameDataProviderException"></exception>
	private async IAsyncEnumerable<string> GetGuildReportCodesAsync(
		string guildName,
		string guildServerSlug,
		string guildServerRegion,
		string? guildTag,
		Zone zone,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		IEnumerable<string> reportCodes = Enumerable.Empty<string>();
		if (guildTag != null)
		{
			var guild = await this.ExecuteAsync<DataMessage<GuildDataMessage>>(new GraphQLRequest  // TODO: the DataMessage wrapper is not deserializing correctly.
			{
				Query = GetGuildQuery,
				Variables = new
				{
					guildName,
					guildServerSlug,
					guildServerRegion,
					guildTag,
				}
			}, cancellationToken);

			this.CheckRateLimit(guild.Data.RateLimitData);
			var guildTagData = guild.Data.GuildData.Guild.Tags.Where(t => t.Name == guildTag).FirstOrDefault();
			if (guildTagData == null)
			{
				throw new GameDataProviderException($"Guild tag {guildTag} not found.");
			}

			var reportsList = new GraphQLRequest
			{
				Query = GetReportsByGuildTagQuery,
				Variables = new
				{
					// the other arguments are still required, but they should be ignored by the API when tag is present
					guildName,
					guildServerSlug,
					guildServerRegion,
					guildTag = guildTagData.Name,
					zoneID = (int)zone,
				}
			};

			var resp = await this.ExecuteAsync<ReportsListMessage>(reportsList, cancellationToken);
			this.CheckRateLimit(resp.RateLimitData);
			reportCodes = resp.ReportData.Reports.Data.Select(r => r.Code);
		}
		else
		{
			// If no guild tag is provided, we can just use the guild name and server slug. This will get all reports for the guild.
			var reportsList = new GraphQLRequest
			{
				Query = GetReportsQuery,
				Variables = new
				{
					guildName,
					guildServerSlug,
					guildServerRegion,
					zoneID = (int)zone,
				}
			};
			var resp = await this.ExecuteAsync<ReportsListMessage>(reportsList, cancellationToken);
			this.CheckRateLimit(resp.RateLimitData);
			reportCodes = resp.ReportData.Reports.Data.Select(r => r.Code);
		}

		foreach (var code in reportCodes)
		{
			yield return code;
		}
	}

	/// <summary>
	/// Executes a GraphQL request and returns the response.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <exception cref="GameDataProviderException"></exception>
	private async Task<T> ExecuteAsync<T>(GraphQLRequest request, CancellationToken cancellationToken)
	{
		GraphQLResponse<T>? response;
		try {
			response = await _graphQLClient.SendQueryAsync<T>(request, cancellationToken);
		}
		catch (Exception ex)
		{
			throw new GameDataProviderException($"An error occurred while fetching data: {ex.Message}", ex);
		}

		if (response == null)
		{
			throw new GameDataProviderException("An error occurred while fetching data: null response");
		}

		if (response.Errors != null)
		{
			foreach (var error in response.Errors)
			{
				_log.LogError(error.Message);
			}

			throw new GameDataProviderException(response.Errors.FirstOrDefault()?.Message ?? "A GraphQL error occurred.");
		}

		if (response.Data == null)
		{
			throw new GameDataProviderException("An error occurred while fetching data: data is response");
		}

		return response.Data;
	}

	/// <summary>
	/// Get all encounters in a zone.
	/// </summary>
	/// <param name="zone"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <exception cref="GameDataProviderException"></exception>
	public Task<List<Encounter>> GetEncountersAsync(Zone zone, CancellationToken cancellationToken = default)
	{
		var zoneData = WorldData!.Value.Expansions.SelectMany(e => e.Zones).FirstOrDefault(z => z.Id == (int)zone);
		if (zoneData == null)
		{
			throw new GameDataProviderException($"Encounters in zone ID {zone} not found.");
		}
		return Task.FromResult(zoneData.Encounters);
	}

	private void CheckRateLimit(RateLimitData rateLimitData)
	{
		this._log.LogDebug("Rate limit: {LimitPerHour} points per hour, {PointsSpentThisHour} points spent this hour, {PointsResetIn} seconds until reset", rateLimitData.LimitPerHour, rateLimitData.PointsSpentThisHour, rateLimitData.PointsResetIn);

		if (rateLimitData.LimitPerHour - rateLimitData.PointsSpentThisHour < 300)
		{
			this._log.LogWarning("Rate limit warning: less than 300 points remaining this hour");
		}
	}

	private const string GetZonesQuery = """
query {
	worldData {
		expansions {
			zones {
				id
				name
				encounters { id name }
			}
		}
	}
}
""";


	private const string GetGuildQuery =
"""
query ($guildName: String!, $guildServerSlug: String!, $guildServerRegion: String!) {
    rateLimitData {
        limitPerHour
        pointsSpentThisHour
        pointsResetIn
    }
    guildData {
        guild(name: $guildName, serverSlug: $guildServerSlug, serverRegion: $guildServerRegion) {
            name
            id
			tags {
				id
				name
			}
        }
    }
}
""";

	private const string GetReportsQuery = """
query ($guildName: String!, $guildServerSlug: String!, $guildServerRegion: String!, $zoneID: Int!) {
    rateLimitData {
        limitPerHour
        pointsSpentThisHour
        pointsResetIn
    }
    reportData {
        reports(guildName: $guildName, guildServerSlug: $guildServerSlug, guildServerRegion: $guildServerRegion, zoneID: $zoneID) {
            total
            per_page
            current_page
            from
            to
            last_page
            has_more_pages
            data {
                code
                endTime
            }
        }
    }
}
""";

	private const string GetReportsByGuildTagQuery = """
query ($guildTag: String!, $zoneID: Int!) {
    rateLimitData {
        limitPerHour
        pointsSpentThisHour
        pointsResetIn
    }
    reportData {
        reports(guildTag: $guildTag, zoneID: $zoneID) {
            total
            per_page
            current_page
            from
            to
            last_page
            has_more_pages
            data {
                code
                endTime
            }
        }
    }
}
""";

	private const string GetFightDetailsQuery = """
query ($code: String!) {
    rateLimitData {
        limitPerHour
        pointsSpentThisHour
        pointsResetIn
    }
    reportData {
        report(code: $code) {
            code
            startTime
            endTime
            fights(killType: Encounters) {
                id
                encounterID
                name
                difficulty
                startTime
                endTime
                kill
                fightPercentage
                friendlyPlayers
                averageItemLevel
                gameZone {
                    id
                    name
                }
            }
            masterData {
                actors(type: "Player") {
                    id
                    gameID
                    name
                    server
                    type
                    subType
                }
            }
        }
    }
}
""";


}
