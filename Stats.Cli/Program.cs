using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GraphQL.Client.Http;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Serializer.SystemTextJson;
using Stats;
using Stats.TokenProvider;
using Stats.GameDataProvider;
using Stats.Reports;
using System.Text.Json;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-AU");
Thread.CurrentThread.CurrentCulture = new CultureInfo("en-AU");

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables();

builder.Services
	.AddSingleton<ITokenProvider, WarcraftLogsTokenProvider>()
	.AddSingleton<IGraphQLWebsocketJsonSerializer, SystemTextJsonSerializer>()
	.AddTransient<IGraphQLWebSocketClient, GraphQLHttpClient>(sp => {
		var tokenProvider = sp.GetRequiredService<ITokenProvider>();
		var token = tokenProvider.GetTokenAsync().Result;
		var log = sp.GetRequiredService<ILogger<LoggingHandler>>();
		var httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler(), log));
		httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
		var client = new GraphQLHttpClient("https://www.warcraftlogs.com/api/v2/client", new SystemTextJsonSerializer(), httpClient);
		return client;
	})
	.Configure<WarcraftLogsOptions>(builder.Configuration.GetSection("WarcraftLogs"))
	.AddScoped<IGameDataProvider, WarcraftLogsGameDataProvider>()
	// .AddSingleton<IGameDataProvider, FileGameDataProvider>()
	.AddSingleton<IReportWriter, CsvReportWriter>();

IHost app = builder.Build();

var log = app.Services.GetRequiredService<ILogger<Program>>();
var client = app.Services.GetRequiredService<IGameDataProvider>();
var writer = app.Services.GetRequiredService<IReportWriter>();

// var files = new FileGameDataProvider();


var rawReports = new List<Report>();
var reports = new List<Report>();
int i = 0;

const string guildName = "Da Bishes";
const string realmName = "frostmourne";

// await foreach (var report in client.GetReportsAsync("Resus", "frostmourne", "us", ZoneID.NerubarPalace))
// await foreach (var report in client.GetReportsAsync("Vortex", "barthilas", "us", ZoneID.NerubarPalace))
await foreach (var report in client.GetReportsAsync(guildName, realmName, "us", ZoneID.NerubarPalace))
{
	rawReports.Add(report);

	if (true)//report.MasterData.Actors.Any(a => a.Name == "Sol\u00e4r"))
	{
		reports.Add(report);
	}
	else
	{
		log.LogWarning("Skipping report {ReportCode}", report.Code);
	}

	var j = Interlocked.Increment(ref i);
	if (j % 5 == 0)
	{
		log.LogInformation("Processed {Count} reports", j);
	}
}

// write raw reports to file as json
// await files.SaveReportsAsync("Resus", "frostmourne", "us", ZoneID.NerubarPalace, rawReports);
var orderedReports = reports.OrderBy(x => x.StartTime);

var killedEncounters = new HashSet<int>();
var rowsForEncounter = new Dictionary<int, HashSet<ReportRow>>();

foreach (var report in orderedReports)
{
	foreach (var fight in report.Fights)
	{
		// TODO: filter by game zone ID
		if (fight.Difficulty != (int)Difficulty.Mythic || killedEncounters.Contains(fight.EncounterID))
		{
			continue;
		}

		if (!rowsForEncounter.TryGetValue(fight.EncounterID, out HashSet<ReportRow>? value))
		{
			value = new HashSet<ReportRow>();
			rowsForEncounter[fight.EncounterID] = value;
		}

		// TODO: remove the timezone when printing
		var row = new ReportRow(
			DateTimeOffset.FromUnixTimeMilliseconds(report.StartTime + fight.StartTime).ToOffset(TimeSpan.FromHours(11)),
			fight.EncounterID,
			fight.Name,
			fight.Kill,
			fight.FightPercentage,
			Math.Round(fight.AverageItemLevel, 0)
		);

		if (!value.Contains(row))
		{
			value.Add(row);

			if (fight.Kill)
			{
				killedEncounters.Add(fight.EncounterID);
				var killTime = DateTimeOffset.FromUnixTimeMilliseconds(report.StartTime + fight.EndTime).ToOffset(TimeSpan.FromHours(11));
				log.LogInformation("{Name} ({EncounterID}) first killed in report {ReportCode} at {Time}", fight.Name, fight.EncounterID, report.Code, killTime.ToString(CultureInfo.CurrentCulture));
			}
		}
		else
		{
			log.LogWarning("Duplicate row for {Name} ({EncounterID}) in report {ReportCode}", fight.Name, fight.EncounterID, report.Code);
		}
	}
}

await writer.WriteReportAsync(guildName, rowsForEncounter.Values.SelectMany(x => x));

log.LogInformation("Application finished");
