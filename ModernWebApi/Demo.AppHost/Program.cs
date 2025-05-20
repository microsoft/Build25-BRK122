var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Demo_ApiService>("apiservice")
    .WithHttpsHealthCheck("/health");

builder.AddProject<Projects.Demo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpsHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
