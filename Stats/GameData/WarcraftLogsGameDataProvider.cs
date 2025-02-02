namespace Stats.GameData;

using System.Runtime.CompilerServices;
using GraphQL;
using GraphQL.Client.Abstractions.Websocket;
using Microsoft.Extensions.Logging;

public class WarcraftLogsGameDataProvider : IGameDataProvider
{
	private readonly IGraphQLWebSocketClient graphQLClient;
	private readonly ILogger<WarcraftLogsGameDataProvider> log;

	public WarcraftLogsGameDataProvider(IGraphQLWebSocketClient graphQLClient, ILogger<WarcraftLogsGameDataProvider> log)
	{
		this.graphQLClient = graphQLClient;
		this.log = log;
	}

	public async IAsyncEnumerable<Report> GetAllFightReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, int zoneID, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var reportsList = new GraphQLRequest
		{
			Query = GetReportsQuery,
			Variables = new
			{
				guildName = guildName,
				guildServerSlug = guildServerSlug,
				guildServerRegion = guildServerRegion,
				zoneID = zoneID
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

			var reportsDataResp = await this.ExecuteAsync<ReportsDataMessage>(reportsData, cancellationToken);
			this.CheckRateLimit(reportsDataResp.RateLimitData);
			yield return reportsDataResp.ReportData.Report;
		}
	}

	private async Task<T> ExecuteAsync<T>(GraphQLRequest request, CancellationToken cancellationToken)
	{
		GraphQLResponse<T>? response;
		try {
			response = await graphQLClient.SendQueryAsync<T>(request, cancellationToken);
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
				log.LogError(error.Message);
			}

			throw new GameDataProviderException("An error occurred while fetching data: GraphQL errors");
		}

		return response.Data;
	}

	public Task SaveReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, int zoneID, IEnumerable<Report> reports, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	private void CheckRateLimit(RateLimitData rateLimitData)
	{
		this.log.LogDebug("Rate limit: {LimitPerHour} points per hour, {PointsSpentThisHour} points spent this hour, {PointsResetIn} seconds until reset", rateLimitData.LimitPerHour, rateLimitData.PointsSpentThisHour, rateLimitData.PointsResetIn);

		if (rateLimitData.LimitPerHour - rateLimitData.PointsSpentThisHour < 300)
		{
			this.log.LogWarning("Rate limit warning: less than 300 points remaining this hour");
		}
	}

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
