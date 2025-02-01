using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GraphQL.Client.Http;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Serializer.SystemTextJson;
using Stats;
using Stats.TokenProvider;
using Stats.GameDataProvider;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables();

builder.Services
	.AddLogging(c => c.AddConsole())
	.AddSingleton<ITokenProvider, WarcraftLogsTokenProvider>()
	.AddScoped<IGraphQLWebsocketJsonSerializer, SystemTextJsonSerializer>()
	.AddScoped<IGraphQLWebSocketClient, GraphQLHttpClient>(sp => {
		var tokenProvider = sp.GetRequiredService<ITokenProvider>();
		var token = tokenProvider.GetTokenAsync().Result;
		var httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));
		httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
		var client = new GraphQLHttpClient("https://www.warcraftlogs.com/api/v2/client", new SystemTextJsonSerializer(), httpClient);
		return client;
	})
	.Configure<WarcraftLogsOptions>(builder.Configuration.GetSection("WarcraftLogs"))
	.AddScoped<IGameDataProvider, WarcraftLogsGameDataProvider>();

IHost app = builder.Build();

var log = app.Services.GetRequiredService<ILogger<Program>>();
log.LogInformation("Starting application");

var client = app.Services.GetRequiredService<IGameDataProvider>();
List<string> codes = client.GetReportsAsync("Resus", "frostmourne", "us", ZoneID.NerubarPalace).Result;

log.LogInformation("Codes: {Codes}", string.Join(", ", codes));
