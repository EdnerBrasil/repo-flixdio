using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

//builder.Services
//    .AddApplicationInsightsTelemetryWorkerService()
//    .ConfigureFunctionsApplicationInsights();
builder.Services.AddSingleton(s =>
{
    string? connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Environment variable 'CosmosDBConnection' is not set. Add it to local.settings.json or your environment.");
    }

    return new CosmosClient(connectionString);
});

builder.Build().Run();
