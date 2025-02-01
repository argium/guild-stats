using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;

namespace Stats.Reports;

public class CsvReportWriter : IReportWriter
{
	public async Task WriteReportAsync(string reportName, IEnumerable<ReportRow> rows)
	{
		// check reportname is alphanumeric only
		if (!Regex.IsMatch(reportName, "^[a-zA-Z0-9]+$"))
		{
			throw new ArgumentException("Report name must be alphanumeric only");
		}

		using var writer = new StreamWriter(reportName + ".csv", false);
		using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
		await csv.WriteRecordsAsync(rows);
		await writer.FlushAsync();
	}
}
