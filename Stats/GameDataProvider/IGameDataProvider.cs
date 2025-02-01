using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Stats.GameDataProvider;

public interface IGameDataProvider
{

	// Task<List<string>> GetReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, int zoneID, CancellationToken cancellationToken = default);

	// Task<List<string>> GetReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, int zoneID, CancellationToken cancellationToken = default);

	IAsyncEnumerable<Report> GetReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, int zoneID, CancellationToken cancellationToken = default);
}
