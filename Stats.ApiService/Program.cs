using System.Globalization;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Stats;
using Stats.DataWriter;
using Stats.GameData;
using Stats.Reports;
using Stats.TokenProvider;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-AU");

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

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
	.AddTransient<IGameDataProvider, WarcraftLogsGameDataProvider>()
	.AddTransient<IGuildReportProducer, GuildReportProducer>()
	.AddSingleton<IDataWriter, CsvDataWriter>()
	.AddDistributedMemoryCache();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/chart", (GuildReportRequest args, [FromServices] IGuildReportProducer report, [FromServices] IDataWriter writer, CancellationToken ct) =>
{
	IAsyncEnumerable<RaidVelocityReportRow> data = report.GetRaidVelocityReportDataAsync(args.GuildName, args.RealmName, args.Region.ToString(), (int)args.Zone, ct);
	switch (args.FileType)
	{
		case FileType.CSV:
			return Results.Stream(async stream => await writer.WriteAsync("RaidVelocity", stream, data, ct), "text/csv");

		case FileType.JPG:
			throw new NotImplementedException();

		default:
			throw new NotSupportedException();
	}
})
.WithName("CreateChart");




app.MapDefaultEndpoints();

app.Run();
