using Linq2GraphQL.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WarcraftLogs.Public;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

// create a service container
var serviceCollection = new ServiceCollection();
serviceCollection
    .AddWarcraftLogsPublicClient(opt => {})
    .WithHttpClient(
        httpClient => 
        { 
            httpClient.BaseAddress = new Uri("https://spacex-production.up.railway.app/"); 
        });
serviceCollection.AddLogging(c => c.AddConsole());

var services = serviceCollection.BuildServiceProvider();

var client = services.GetRequiredService<WarcraftLogsPublicClient>();
await client.Query.CharacterData().Select(c => c.Character.Name).ExecuteAsync();