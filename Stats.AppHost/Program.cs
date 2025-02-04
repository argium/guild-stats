var builder = DistributedApplication.CreateBuilder(args);

var token = builder.AddParameterFromConfiguration("WarcraftLogsToken", "WarcraftLogs__Token", secret: true);
var secret = builder.AddParameterFromConfiguration("WarcraftLogsClientSecret", "WarcraftLogs__ClientSecret", secret: true);

var cache = builder.AddRedis("cache")
                   .WithDataVolume(isReadOnly: false);

var apiService = builder.AddProject<Projects.Stats_ApiService>("apiservice")
	.WithEnvironment("WarcraftLogs__Token", token)
	.WithEnvironment("WarcraftLogs__ClientSecret", secret)
	.WithReference(cache);

builder.AddProject<Projects.Stats_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
