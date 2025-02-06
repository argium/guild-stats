using Stats.Domain;

namespace Stats.GameData;

public interface IGameDataProvider
{
	IAsyncEnumerable<Report> GetAllFightReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, Zone zone, CancellationToken cancellationToken = default);

	Task SaveReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, Zone zone, IEnumerable<Report> reports, CancellationToken cancellationToken = default);
}
