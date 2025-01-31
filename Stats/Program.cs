using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GraphQL.Client.Http;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL;
using GraphQL.Client.Serializer.SystemTextJson;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
IHostEnvironment env = builder.Environment;

string token = string.Empty;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

builder.Services
	.AddLogging(c => c.AddConsole())
	.AddScoped<IGraphQLWebsocketJsonSerializer, SystemTextJsonSerializer>()
	.AddScoped<IGraphQLWebSocketClient, GraphQLHttpClient>(sp => {
		var httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));
		httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
		var client = new GraphQLHttpClient("https://www.warcraftlogs.com/api/v2/client", new SystemTextJsonSerializer(), httpClient);
		return client;
	});

IHost app = builder.Build();

var log = app.Services.GetRequiredService<ILogger<Program>>();
log.LogInformation("Starting application");


var graphQLClient = app.Services.GetRequiredService<IGraphQLWebSocketClient>();
var heroRequest = new GraphQLRequest {
    Query = """
		query ($name: String!, $serverSlug: String!, $serverRegion: String!) {
			rateLimitData {
				limitPerHour
				pointsSpentThisHour
				pointsResetIn
			}
			characterData {
				character(name: $name, serverSlug: $serverSlug, serverRegion: $serverRegion) {
					name
					id
					classID
					recentReports(limit: 1) {
						data {
							fights(killType: Encounters) {
								encounterID
								name
								endTime
							}
						}
					}
				}
			}
		}
	""",
	Variables = new
	{
		name = "Argium",
		serverSlug = "frostmourne",
		serverRegion = "us"
	}
};

var reportsData = new GraphQLRequest {
    Query = """
		query ($guildName: String!, $guildServerSlug: String!, $guildServerRegion: String!) {
			rateLimitData {
				limitPerHour
				pointsSpentThisHour
				pointsResetIn
			}
			guildData {
				guild(name: $guildName, serverSlug: $guildServerSlug, serverRegion: $guildServerRegion) {
					name
					id
				}
			}
			reportData {
				reports(guildName: $guildName, guildServerSlug: $guildServerSlug, guildServerRegion: $guildServerRegion, limit:5) {
					data {
						code
						fights {
							name
						}
					}
				}
			}
		}
	""",
	Variables = new
	{
		guildName = "Resus",
		guildServerSlug = "frostmourne",
		guildServerRegion = "us"
	}
};


var graphQLResponse = await graphQLClient.SendQueryAsync<dynamic>(reportsData);

log.LogInformation($"GraphQL Response: {graphQLResponse}");

public class LoggingHandler : DelegatingHandler
{
    public LoggingHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
    }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("Request:");
        Console.WriteLine(request.ToString());
        if (request.Content != null)
        {
            Console.WriteLine(await request.Content.ReadAsStringAsync());
        }
        Console.WriteLine();

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        Console.WriteLine("Response:");
        Console.WriteLine(response.ToString());
        if (response.Content != null)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
        Console.WriteLine();

        return response;
    }
}
