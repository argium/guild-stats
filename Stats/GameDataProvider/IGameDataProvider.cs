namespace Stats.GameDataProvider;

public interface IGameDataProvider
{
	Task<List<string>> GetReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, int zoneID, CancellationToken cancellationToken = default);

	Task<dynamic> GetReportDataAsync(string[] codes, CancellationToken cancellationToken = default);
}
