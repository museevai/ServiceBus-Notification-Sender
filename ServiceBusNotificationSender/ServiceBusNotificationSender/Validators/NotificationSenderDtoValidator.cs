using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceBusNotificationSender.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusNotificationSender.Validators
{
    public class NotificationSenderDtoValidator :
        AbstractValidator<NotificationSenderDto>
    {
        private readonly int messageMinLength;
        private readonly int messageTitleMaxLength;
        private readonly int messageBodyMaxLength;
        private readonly ILogger logger;
        private readonly IConfiguration config;

        public NotificationSenderDtoValidator(ILogger logger, IConfiguration config)
        {
            this.logger = logger;
            this.config = config;

            messageMinLength = config.GetValue<int>("NotificationSenderDtoValidatorSettings:MessageMinLength");
            messageTitleMaxLength = config.GetValue<int>("NotificationSenderDtoValidatorSettings:MessageTitleMaxLength");
            messageBodyMaxLength = config.GetValue<int>("NotificationSenderDtoValidatorSettings:MessageBodyMaxLength");


            RuleFor(x => x.MessageTitle)
                .Length(messageMinLength, messageTitleMaxLength)
                .WithMessage($"{nameof(NotificationSenderDto.MessageTitle)} should be between {messageMinLength} and {messageTitleMaxLength} characters");


            RuleFor(x => x.MessageBody)
               .Length(messageMinLength, messageBodyMaxLength)
               .WithMessage($"{nameof(NotificationSenderDto.MessageBody)} should be between {messageMinLength} and {messageBodyMaxLength} characters");


            RuleFor(x => x.SendAt)
                .NotEmpty()
                .WithMessage($"{nameof(NotificationSenderDto.SendAt)} is required")
                .Must(x => x >= DateTime.Now.AddMinutes(10)) // Ensure the date is in the future
                .WithMessage($"{nameof(NotificationSenderDto.SendAt)} must be at least 10 minutes from now");

            RuleFor(x => x.SendTo)
                .Must(sendTo => sendTo.Count > 0)
                .WithMessage($"{nameof(NotificationSenderDto.SendTo)} must contain at least one recipient");

            // Validate each email in the SendTo
            RuleForEach(x => x.SendTo).SetValidator(new EmailValidator());
        }
    }
}
