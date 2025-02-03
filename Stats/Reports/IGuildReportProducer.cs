namespace Stats.Reports;

public interface IGuildReportProducer
{
	IAsyncEnumerable<RaidVelocityReportRow> GetRaidVelocityReportDataAsync(
		string guildName,
		string realmName,
		string region,
		int zoneID,
		CancellationToken cancellationToken = default);
}
