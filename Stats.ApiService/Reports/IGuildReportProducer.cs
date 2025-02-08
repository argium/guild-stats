using Stats.Domain;

namespace Stats.Reports;

public interface IGuildReportProducer
{
	IAsyncEnumerable<RaidVelocityReportRow> GetRaidVelocityReportDataAsync(
		string guildName,
		string realmName,
		string region,
		Zone zone,
		CancellationToken cancellationToken = default);
}
