namespace Stats.Reports;

public interface IReportWriter
{
	Task WriteReportAsync(string reportName, IEnumerable<ReportRow> rows);
}
