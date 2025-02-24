
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

	private static readonly Lazy<WorldDataMessage> WorldData = new(() =>
	{
		var json = File.ReadAllText("GameData/Zones.json");
		return JsonSerializer.Deserialize<WorldDataMessage>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new InvalidOperationException("Failed to deserialize WorldDataMessage");
	});

	public WarcraftLogsGameDataProvider(IGraphQLWebSocketClient graphQLClient, HybridCache cache, ILogger<WarcraftLogsGameDataProvider> log)
	{
		_graphQLClient = graphQLClient;
		_log = log;
		_cache = cache;
	}

	public async IAsyncEnumerable<Report> GetAllFightReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, Zone zone, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var reportsList = new GraphQLRequest
		{
			Query = GetReportsQuery,
			Variables = new
			{
				guildName = guildName,
				guildServerSlug = guildServerSlug,
				guildServerRegion = guildServerRegion,
				zoneID = (int)zone,
			}
		};

		var resp = await this.ExecuteAsync<ReportsListMessage>(reportsList, cancellationToken);
		this.CheckRateLimit(resp.RateLimitData);
		var reportCodes = resp.ReportData.Reports.Data.Select(r => r.Code);

		foreach (var code in reportCodes)
		{
			var reportsData = new GraphQLRequest
			{
				Query = GetFightDetailsQuery,
				Variables = new
				{
					code = code
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

	private async Task<T> ExecuteAsync<T>(GraphQLRequest request, CancellationToken cancellationToken)
	{
		GraphQLResponse<T>? response;
		try {
			response = await _graphQLClient.SendQueryAsync<T>(request, cancellationToken);
		}
		catch (Exception ex)
		{
			throw new GameDataProviderException("An error occurred while fetching data", ex);
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

	public Task<List<Encounter>> GetEncountersAsync(Zone zone, CancellationToken cancellationToken = default)
	{
		var zoneData = WorldData!.Value.Data.Expansions.SelectMany(e => e.Zones).FirstOrDefault(z => z.Id == (int)zone);
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

	private const string GetReportsQuery = """
query ($guildName: String!, $guildServerSlug: String!, $guildServerRegion: String!, $zoneID: Int!) {
    rateLimitData {
        limitPerHour
        pointsSpentThisHour
        pointsResetIn
    }
    guildData {
        guild(name: $guildName, serverSlug: $guildServerSlug, serverRegion: $guildServerRegion) {
            name
            id
        }
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
