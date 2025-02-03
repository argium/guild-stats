using System.ComponentModel.DataAnnotations;

namespace Stats.Domain;

public record GuildReportRequest
{
	[Required]
	[Length(3, 20)]
	public string GuildName { get; set; } = string.Empty;

	[Required]
	[Length(5, 20)]
	public string RealmName { get; set; } = string.Empty;

	public Region Region { get; set; }
	public Zone Zone { get; set; }
	public FileType FileType { get; set; }
}

public enum Zone
{
	NerubarPalace = 38,
}

public enum FileType
{
	CSV,

	JPG,
}

public enum Region
{
	US,
}
