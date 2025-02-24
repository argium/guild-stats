namespace Stats.Domain;

/// <summary>
/// Represents a collection of data points for a chart.
/// </summary>
public record ChartDataResponse(string GuildAndRealm, List<DataPoint> Series);

/// <summary>
/// Represents a single data point in a chart.
/// </summary>
/// <param name="SeriesName">The series name (eg. boss name)</param>
/// <param name="Time">The time.</param>
/// <param name="Value">The value (eg. boss percentage)</param>
public record DataPoint(string SeriesName, DateTime Time, decimal Value);
