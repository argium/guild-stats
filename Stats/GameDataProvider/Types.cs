namespace Stats.GameDataProvider;

public record ReportsListMessage(
	RateLimitData RateLimitData,
	ReportData ReportData
);

public record GuildData(
	Guild Guild
);

public record Guild(
	string Id,
	string Name
);

public record RateLimitData(
	int LimitPerHour,
	double PointsSpentThisHour,
	int PointsResetIn
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

public record ReportData(
	Page<Report> Reports
);

public record Report(
	string Code,
	long EndTime,
	List<Fight> Fights
);

public record Fight(
	int ID,
	int EncounterID,
	string Name,
	long StartTime,
	long EndTime,
	bool Kill,
	double FightPercentage,
	List<int> FriendlyPlayers,
	double AverageItemLevel
);
