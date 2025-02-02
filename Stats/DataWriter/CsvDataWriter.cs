using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;

namespace Stats.DataWriter;

public class CsvDataWriter : IDataWriter
{
	public async Task WriteAsync<T>(string datasetName, IEnumerable<T> rows, CancellationToken cancellationToken = default)
	{
		// check reportname is alphanumeric or spaces only
		if (!Regex.IsMatch(datasetName, @"^[a-zA-Z0-9\s]*$"))
		{
			throw new ArgumentException("Report name must be alphanumeric or spaces only");
		}

		using var writer = new StreamWriter(datasetName + ".csv", false);
		using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
		// csv.Context.RegisterClassMap<RaidVelocityReportRow>();

		await csv.WriteRecordsAsync(rows, cancellationToken);
		await writer.FlushAsync(cancellationToken);
	}
}
