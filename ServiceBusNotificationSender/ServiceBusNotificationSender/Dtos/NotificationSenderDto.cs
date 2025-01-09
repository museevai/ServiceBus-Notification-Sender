using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusNotificationSender.Dtos
{
    public class NotificationSenderDto
    {
        public List<string> SendTo { get; set; } = new List<string>();

        public DateTime? SendAt { get; set; }

        public string? MessageTitle { get; set; }

        public string? MessageBody { get; set; }

    }
}
