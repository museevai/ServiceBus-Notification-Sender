using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using ServiceBusNotificationSender.Dtos;

namespace ServiceBusNotificationSender
{
    public class NotificationReceiver
    {
        private readonly ILogger<NotificationReceiver> _logger;
        private readonly EmailSettingsDto emailSettingsDto;

        public NotificationReceiver(ILogger<NotificationReceiver> logger, IOptions<EmailSettingsDto> appSettings)
        {
            _logger = logger;
            this.emailSettingsDto = appSettings.Value;
        }

        [Function(nameof(NotificationReceiver))]
        // Read the API key from settings
        // The binding only accepts an app setting name, you can't pass the key righ here
        [SendGridOutput(ApiKey = "EmailSettings:SendGridApiKey")]
        public string Run(
            [ServiceBusTrigger("%EmailSettings:QueueName%",
            Connection = "EmailSettings:NotificationBusConnection")]
            ServiceBusReceivedMessage message)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            NotificationSenderDto? dto =
                            JsonConvert.DeserializeObject<NotificationSenderDto>(Encoding.UTF8.GetString(message.Body));

            var sendGridMessage = new SendGridMessage();

            sendGridMessage
                .SetFrom(new EmailAddress(emailSettingsDto.SendFromEmail, emailSettingsDto.SendFromName));

            sendGridMessage.SetGlobalSubject(dto?.MessageTitle);

            // Populate the To email addresses
            List<EmailAddress> emailAddresses = new List<EmailAddress>();

            dto?.SendTo.ForEach(x =>
            {
                emailAddresses.Add(new EmailAddress(x));
            });

            sendGridMessage.AddTos(emailAddresses);

            // Set the message content and ensure that we send an HTML message to allow the sender to provide us the proper message formatting
            sendGridMessage.AddContent(MimeType.Html, dto?.MessageBody);

            // Finaly serialize the SendGrid message and return the string to the output binding
            var messageJson = JsonConvert.SerializeObject(sendGridMessage);

            return messageJson;
        }
    }
}
