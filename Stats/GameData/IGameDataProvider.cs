namespace Stats.GameData;

public interface IGameDataProvider
{
	IAsyncEnumerable<Report> GetAllFightReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, int zoneID, CancellationToken cancellationToken = default);

	Task SaveReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, int zoneID, IEnumerable<Report> reports, CancellationToken cancellationToken = default);
}
