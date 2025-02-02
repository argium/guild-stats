namespace Stats.GameData;

// Metadata
public record RateLimitData(
	int LimitPerHour,
	double PointsSpentThisHour,
	int PointsResetIn
);

// Guild data
public record GuildData(
	Guild Guild
);

public record Guild(
	string Id,
	string Name
);


// Report list
public record ReportsListMessage(
	RateLimitData RateLimitData,
	ReportDataPage ReportData
);

public record Page<T>(
	int Total,
	int PerPage,
	int CurrentPage,
	int From,
	int To,
	int LastPage,
	bool HasMorePages,
	List<T> Data
);

public record ReportDataPage(
	Page<Report> Reports
);

// Report
public record ReportsDataMessage(
	RateLimitData RateLimitData,
	ReportsData ReportData
);

public record ReportsData(
	Report Report
);

public record Report(
	string Code,
	long StartTime,
	long EndTime,
	List<Fight> Fights,
	ReportMasterData MasterData
);

public record Fight(
	int ID,
	int EncounterID,
	string Name,
	int Difficulty,
	long StartTime,
	long EndTime,
	bool Kill,
	double FightPercentage,
	List<int> FriendlyPlayers,
	double AverageItemLevel
);

public record ReportMasterData(
	List<ReportActor> Actors
);

public record ReportActor(
	long GameID,
	int ID,
	string Name,
	string Type,
	string SubType,
	string Server
);

public enum Difficulty
{
	Normal = 3,

	Heroic = 4,

	Mythic = 5
}

public static class ZoneID
{
	public static readonly int NerubarPalace = 38;
}
