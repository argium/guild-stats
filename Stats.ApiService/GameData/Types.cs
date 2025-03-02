namespace Stats.GameData;

//
// Metadata
//
public record DataMessage<T>(T Data);

public record RateLimitData(
	int LimitPerHour,
	double PointsSpentThisHour,
	int PointsResetIn
);


//
// GUILD DATA
//
public record GuildData(
	Guild Guild
);

public record Guild(
	string Id,
	string Name
);

//
// LOGS AND REPORTS
//
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
	double AverageItemLevel,
	GameZone GameZone
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

public record GameZone(
	long Id,
	string Name
);

public enum Difficulty
{
	Normal = 3,
	Heroic = 4,
	Mythic = 5
}

//
// WORLD DATA
//
public record WorldDataMessage(WorldData WorldData);

public record WorldData(List<Expansion> Expansions);

public record Expansion(List<WCLZone> Zones);

public record WCLZone(int Id, string Name, List<Encounter> Encounters);

public record Encounter(int Id, string Name);
