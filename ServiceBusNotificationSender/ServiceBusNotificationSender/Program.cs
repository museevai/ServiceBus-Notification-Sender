using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ServiceBusNotificationSender;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("local.settings.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<NotificationReceiver>();

builder.Build().Run();
