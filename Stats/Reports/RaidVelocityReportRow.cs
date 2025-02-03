using CsvHelper.Configuration;

namespace Stats.Reports;

public record RaidVelocityReportRow(
	DateTime Time,
	int EncounterID,
	string Name,
	bool Killed,
	double Percentage,
	double AverageItemLevel
);

// public class RaidVelocityReportRowRowMap : ClassMap<RaidVelocityReportRow>
// {
//     public ReportRowMap()
//     {
//         Map(m => m.Time).Index(0).Name("time");
// 		Map(m => m.EncounterID).Index(1).Name("encounter_id");
// 		Map(m => m.Name).Index(2).Name("name");
// 		Map(m => m.Percentage).Index(3).Name("percentage");
// 		Map(m => m.AverageItemLevel).Index(4).Name("average_item_level");
//     }
// }
