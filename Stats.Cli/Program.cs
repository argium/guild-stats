using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GraphQL.Client.Http;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Serializer.SystemTextJson;
using Stats;
using Stats.TokenProvider;
using Stats.GameData;
using Stats.Reports;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stats.DataWriter;

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
	.AddSingleton<IDataWriter, CsvDataWriter>();

IHost app = builder.Build();



const string guildName = "Da Bishes";
const string realmName = "frostmourne";

// await foreach (var report in client.GetReportsAsync("Resus", "frostmourne", "us", ZoneID.NerubarPalace))
// await foreach (var report in client.GetReportsAsync("Vortex", "barthilas", "us", ZoneID.NerubarPalace))



log.LogInformation("Application finished");
