namespace Stats.Reports;

public interface IGuildReportProducer
{
	IAsyncEnumerable<RaidVelocityReportRow> GetRaidVelocityReportDataAsync(
		string guildName,
		string realmName,
		string region,
		string? guildTag,
		Zone zone,
		CancellationToken cancellationToken = default);
}
