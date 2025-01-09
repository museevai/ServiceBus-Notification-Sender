using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusNotificationSender.Dtos
{
    public class EmailSettingsDto
    {
        public string? SendFromEmail { get; set; }
        public string? SendFromName { get; set; }
        public string? NotificationBusConnection { get; set; }
        public string? QueueName { get; set; }
    }
}
