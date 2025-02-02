namespace Stats.DataWriter;

public interface IDataWriter
{
	Task WriteAsync<T>(string datasetName, Stream stream, IAsyncEnumerable<T> rows, CancellationToken cancellationToken = default);
}
