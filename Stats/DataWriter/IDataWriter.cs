namespace Stats.DataWriter;

public interface IDataWriter
{
	Task WriteAsync<T>(string datasetName, IEnumerable<T> rows, CancellationToken cancellationToken = default);
}
