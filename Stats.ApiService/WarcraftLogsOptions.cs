namespace Stats;

public record WarcraftLogsOptions
{
	public required string ClientId { get; set; }

	public required string ClientSecret { get; set; }

	public string? Token { get; set; }

	public TimeSpan BufferWindow { get; set; } = TimeSpan.FromMinutes(5);
}
