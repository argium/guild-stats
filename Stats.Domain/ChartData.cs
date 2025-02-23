namespace Stats.Domain;

public record ChartData
{
	public Dictionary<string, List<DataPoint>> Series { get; init; } = new();
}

public record DataPoint(DateTime Time, decimal Value);
