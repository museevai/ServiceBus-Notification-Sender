using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceBusNotificationSender;
using ServiceBusNotificationSender.Dtos;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true) 
    .AddJsonFile("local.settings.json", optional: true)
    .AddEnvironmentVariables();


// Get the configuration object to read the Service Bus connection string and queue name.
var config = builder.Configuration;

// Fill the EmailSettingsDto to be used when we need to read settings in functions using dependency injection (i.e. passing IOptions<EmailSettingsDto> appSettings)
builder.Services.AddOptions<EmailSettingsDto>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection("EmailSettings").Bind(settings);
    });

// We need to create a Service Bus client to be used to send messages to the queue
builder.Services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddServiceBusClient(config.GetValue<string>("EmailSettings:NotificationBusConnection"));
        });

// It's safe to create a singleton sender
// ServiceBusSender is the class that is used to send messages, it needs a ServiceBusClient
// We used the previously created ServiceBusClient to instantiate the sender
builder.Services.AddSingleton<ServiceBusSender>((provider) =>
{
    return provider
        .GetRequiredService<ServiceBusClient>()
        .CreateSender(config.GetValue<string>("EmailSettings:QueueName"));
}
);

builder.Build().Run();
