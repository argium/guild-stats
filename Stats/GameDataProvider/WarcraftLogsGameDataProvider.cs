namespace Stats.GameDataProvider;

using System.Text.Json;
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

	public async Task<List<string>> GetReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, int zoneID, CancellationToken cancellationToken = default)
	{
		var reportsList = new GraphQLRequest
		{
			Query = File.ReadAllText("reportsList.graphql"),
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
		return resp.ReportData.Reports.Data.Select(r => r.Code).ToList();
	}

	public async Task<dynamic> GetReportDataAsync(string[] codes, CancellationToken cancellationToken = default)
	{
		var reportsData = new GraphQLRequest
		{
			Query = File.ReadAllText("reportsData.graphql"),
			Variables = new
			{
				codes = codes
			}
		};

		return this.ExecuteAsync<dynamic>(reportsData, cancellationToken);
	}

	private async Task<T> ExecuteAsync<T>(GraphQLRequest request, CancellationToken cancellationToken)
	{
		GraphQLResponse<T> response = await graphQLClient.SendQueryAsync<T>(request, cancellationToken);
		if (response.Errors != null)
		{
			foreach (var error in response.Errors)
			{
				log.LogError(error.Message);
			}

			throw new GameDataProviderException("An error occurred while fetching data");
		}

		return response.Data;
	}

	private void CheckRateLimit(RateLimitData rateLimitData)
	{
		this.log.LogInformation("Rate limit: {LimitPerHour} points per hour, {PointsSpentThisHour} points spent this hour, {PointsResetIn} seconds until reset", rateLimitData.LimitPerHour, rateLimitData.PointsSpentThisHour, rateLimitData.PointsResetIn);
	}
}
