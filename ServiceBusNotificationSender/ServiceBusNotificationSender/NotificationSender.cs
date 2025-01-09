using Azure.Messaging.ServiceBus;
using ServiceBusNotificationSender.Dtos;
using FluentValidation.Results;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceBusNotificationSender.Validators;
using System.Net;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;


namespace ServiceBusNotificationSender
{
    public class NotificationSender
    {
        private readonly ILogger<NotificationSender> _logger;
        private readonly ServiceBusSender serviceBusSender;

        public NotificationSender(ILogger<NotificationSender> logger, ServiceBusSender serviceBusSender)
        {
            _logger = logger;
            this.serviceBusSender = serviceBusSender;
        }


        // Receive a dto object from the request body as a second method parameter ([FromBody] NotificationSenderDto dto)
        [Function("NotificationSender")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, [FromBody] NotificationSenderDto dto)
        {
            HttpResponseData? response = null;

            NotificationSenderDtoValidator validator = new NotificationSenderDtoValidator();

            ValidationResult validationResult = validator.Validate(dto);

            if (validationResult.IsValid)
            {
                // Prepare a 202 response 
                response = req.CreateResponse(HttpStatusCode.Accepted);

                // Create a new ServiceBus message 
                var message = new ServiceBusMessage(JsonConvert.SerializeObject(dto))
                {
                    // Tell Service Bus to schedule the message delivery 
                    ScheduledEnqueueTime = dto.SendAt!.Value
                };

                await serviceBusSender.SendMessageAsync(message);

                return response;
            }
            else
            {
                var errors = validationResult
                    .Errors
                    .Select(error => new
                    {
                        Property = error.PropertyName,
                        ErrorMessage = error.ErrorMessage
                    });

                response = req.CreateResponse(HttpStatusCode.BadRequest);

                response.WriteString(JsonConvert.SerializeObject(errors));

                return response;
            }
        }
    }
}
