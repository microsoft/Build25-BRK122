var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Demo_Catalog>("catalog")
    .WithHttpsHealthCheck("/health");

builder.Build().Run();
