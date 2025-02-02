namespace Stats.Reports;

public interface IReportProducer<T>
{
	IAsyncEnumerable<T> GetDataAsync(ReportArgs args, CancellationToken cancellationToken = default);
}
