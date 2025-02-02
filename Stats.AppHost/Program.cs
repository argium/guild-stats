var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Stats_ApiService>("apiservice");

builder.AddProject<Projects.Stats_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
