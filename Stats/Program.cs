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
// List<string> codes = client.GetReportsAsync("Resus", "frostmourne", "us", ZoneID.NerubarPalace).Result;

// List<string> codes = "FraJ3HbG4RjZkm6M, NJxAMZHhra2CnKcm, XH496Jk3vGmbZC2K, 1ayqHRWB4z83fktM, mH7Vj4LBCx2P6zXG, JAm8KvQtyzTC19NW, AWMhRzTnxbC8VZ4c, DnMGtdqxXacH6Jp8, 4Qa8MAnmgNTf7Hhb, AfpV28vHbPLy6DJF, X4LjPW16q3a7YfHG, Dq6TvcAwKfatXzY3, XAC9FaGKjV6tHnRy, TMrnDzRjKyG2b8FV, wDKhGda7mpxqATcr, Fg84MhWxYrXQk2jq, vZRGXcAg2drJmP86, KRJgn9HDbQqhwWzr, yXbQ16zBPcqd7wLJ, 19pRJCZWfrm86vVz, pKWg6HVw2P9mq7ar, yxqA6dRCzBZatpGW, TfZz4WF6LAY8jn3k, cMkGnzZVYFTv12Pb, VcRJjKXpyYf81LNt, gw8T4DLBG6NxYcZk, MHjdkbmWxry7v43t, WL8ARGBv4hrJfgZY, Kjzwt9JgGf1TcpqD, mFaJ2Q6BHCTg9VGW, vC1ZaLHJkMcxh9qA, Wz9mxV4aCRK7bjMh, RFz7wZd4ncVxCmbT, 4QDvfCtwpYXmgckG, D6YwCZ4Q8RWxpVP2, nVrvzp8dTqgHcBCQ, g7LD8wGQrnky41KT, 8xmy1YzvtLJak2cK, 6LdWrA7k3jfQHXCF, 1bB46mKgYLVp2DCA, h1tAz6GZWwNXgQm9, dPJ7hBczRTYaVywb, Z7CbkcRDYTj1g6aA, Ta6F8bBh2trwXjMm, y1vP92H3zG4qbL8Y, m3BNwVpFWGD1QXAb, Fr6vG1fjDbz9w2ZX, hC6kQtx1YHBfFAqN, pZfVLW6qFPmxzaD2".Split(", ").ToList();

// log.LogInformation("Codes: {Codes}", string.Join(", ", codes));

await foreach (var report in client.GetReportsAsync("Resus", "frostmourne", "us", ZoneID.NerubarPalace))
{
	log.LogInformation("Report: {Report}", report);
}
