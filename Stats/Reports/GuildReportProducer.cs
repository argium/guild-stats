namespace Stats.Reports;

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Stats.GameData;

public class GuildReportProducer : IGuildReportProducer
{
	private readonly ILogger<GuildReportProducer> _log;
	private readonly IGameDataProvider _gameDataProvider;

	public GuildReportProducer(IGameDataProvider gameDataProvider, ILogger<GuildReportProducer> log)
	{
		_log = log;
		_gameDataProvider = gameDataProvider;
	}

	public async IAsyncEnumerable<RaidVelocityReportRow> GetRaidVelocityReportDataAsync(
		string guildName,
		string realmName,
		string region,
		int zoneID,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var rawReports = new List<Report>();
		var reports = new List<Report>();
		int i = 0;

		await foreach (var report in _gameDataProvider.GetAllFightReportsAsync(guildName, realmName, region, zoneID, cancellationToken))
		{
			rawReports.Add(report);

			if (true)//report.MasterData.Actors.Any(a => a.Name == "Sol\u00e4r"))
			{
				reports.Add(report);
			}
			else
			{
				_log.LogWarning("Skipping report {ReportCode}", report.Code);
			}

			var j = Interlocked.Increment(ref i);
			if (j % 5 == 0)
			{
				_log.LogInformation("Processed {Count} reports", j);
			}
		}

		// Reports will contain multiple kills of the same encounter. For this report, we're only interested in the first kill.
		var orderedReports = reports.OrderBy(x => x.StartTime);
		var killedEncounters = new HashSet<int>();
		var rowsForEncounter = new Dictionary<int, HashSet<RaidVelocityReportRow>>();
		foreach (var report in orderedReports)
		{
			foreach (var fight in report.Fights)
			{
				// TODO: filter by game zone ID
				if (fight.Difficulty != (int)Difficulty.Mythic || killedEncounters.Contains(fight.EncounterID))
				{
					continue;
				}

				if (!rowsForEncounter.TryGetValue(fight.EncounterID, out HashSet<RaidVelocityReportRow>? value))
				{
					value = new HashSet<RaidVelocityReportRow>();
					rowsForEncounter[fight.EncounterID] = value;
				}

				var row = new RaidVelocityReportRow(
					DateTimeOffset.FromUnixTimeMilliseconds(report.StartTime + fight.StartTime).ToOffset(TimeSpan.FromHours(11)),
					fight.EncounterID,
					fight.Name,
					fight.Kill,
					fight.FightPercentage,
					Math.Round(fight.AverageItemLevel, 0)
				);

				// Add the fight and, if it was the kill, record the encounter ID so we don't process future pulls.
				if (value.Add(row) && fight.Kill)
				{
					killedEncounters.Add(fight.EncounterID);
					var killTime = DateTimeOffset.FromUnixTimeMilliseconds(report.StartTime + fight.EndTime).ToOffset(TimeSpan.FromHours(11));
					_log.LogInformation("{Name} ({EncounterID}) first killed in report {ReportCode} at {Time}", fight.Name, fight.EncounterID, report.Code, killTime.ToString(CultureInfo.CurrentCulture));
				}
				else if (!value.Add(row))
				{
					_log.LogWarning("Duplicate fight in {ReportCode}: {Row}", report.Code, row);
				}
			}
		}

		foreach (var row in rowsForEncounter.Values.SelectMany(x => x))
		{
			yield return row;
		}
	}
}
