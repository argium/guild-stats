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

builder.Services.AddSingleton<ITokenProvider, WarcraftLogsTokenProvider>();

builder.Services.AddSingleton<IGraphQLWebsocketJsonSerializer, SystemTextJsonSerializer>()
	.AddTransient<IGraphQLWebSocketClient, GraphQLHttpClient>(sp => {
		var tokenProvider = sp.GetRequiredService<ITokenProvider>();
		var token = tokenProvider.GetTokenAsync().Result;
		var log = sp.GetRequiredService<ILogger<LoggingHandler>>();
		var httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler(), log));
		httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
		var client = new GraphQLHttpClient("https://www.warcraftlogs.com/api/v2/client", new SystemTextJsonSerializer(), httpClient);
		return client;
	});

builder.Services.AddTransient<IGameDataProvider, WarcraftLogsGameDataProvider>()
		.Configure<WarcraftLogsOptions>(builder.Configuration.GetSection("WarcraftLogs"));

builder.Services.AddTransient<IGuildReportProducer, GuildReportProducer>();;

builder.Services.AddSingleton<IDataWriter, CsvDataWriter>();;

builder.AddRedisDistributedCache(connectionName: "cache");
#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/reports/guild", async (GuildReportRequest args, [FromServices] IGuildReportProducer report, [FromServices] IDataWriter writer, CancellationToken ct) =>
{
	try
	{
		IAsyncEnumerable<RaidVelocityReportRow> data = report.GetRaidVelocityReportDataAsync(args.GuildName, args.RealmName, args.Region.ToString(), args.Zone, ct);
		switch (args.FileType)
		{
			case FileType.CSV:
				return Results.Stream(async stream => await writer.WriteAsync("RaidVelocity", stream, data, ct), "text/csv");

			case FileType.Chart:
				var chartData = new ChartData();

				await foreach (var row in data)
				{
					List<DataPoint> series;
					if (!chartData.Series.TryGetValue(row.Name, out series))
					{
						series = new List<DataPoint>();
						chartData.Series.Add(row.Name, series);
					}

					series.Add(new DataPoint(row.Time, new Decimal(row.Percentage)));
				}
				return Results.Ok(chartData);

			default:
				throw new NotSupportedException();
		}
	}
	catch (GameDataProviderException ex)
	{
		return Results.Problem(title: ex.Message, statusCode: (int)ex.StatusCode);
	}
});

app.MapDefaultEndpoints();

app.Run();
