namespace Stats.GameData;

public interface IGameDataProvider
{
	IAsyncEnumerable<Report> GetAllFightReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, string? guildTag, Zone zone, CancellationToken cancellationToken = default);

	Task<List<Encounter>> GetEncountersAsync(Zone zone, CancellationToken cancellationToken = default);
}
