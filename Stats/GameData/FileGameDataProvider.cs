using System.Runtime.CompilerServices;
using System.Text.Json;
using Stats.Domain;

namespace Stats.GameData;

public class FileGameDataProvider : IGameDataProvider
{
	public async IAsyncEnumerable<Report> GetAllFightReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, Zone zone, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		using var file = new FileStream(GetFileName(guildName, guildServerSlug, guildServerRegion, zone), FileMode.Open);
		var reports = await JsonSerializer.DeserializeAsync<List<Report>>(file, cancellationToken: cancellationToken);

		if (reports == null)
		{
			yield break;
		}

		foreach (var report in reports)
		{
			yield return report;
		}
	}

	public async Task SaveReportsAsync(string guildName, string guildServerSlug, string guildServerRegion, Zone zone, IEnumerable<Report> reports, CancellationToken cancellationToken = default)
	{
		await using FileStream file = File.Create(GetFileName(guildName, guildServerSlug, guildServerRegion, zone));
        await JsonSerializer.SerializeAsync(file, reports, cancellationToken: cancellationToken);
	}

	private static string GetFileName(string guildName, string guildServerSlug, string guildServerRegion, Zone zone)
	{
		return $"{guildName}-{guildServerSlug}-{guildServerRegion}-{zone}.json";
	}
}
