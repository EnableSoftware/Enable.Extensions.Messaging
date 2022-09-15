using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Enable.Extensions.Messaging.Abstractions;

namespace Enable.Extensions.Messaging.AzureServiceBus.Internal.V2
{
    public class AzureServiceBusMessagePublisherV2 : BaseMessagePublisher
    {
        private readonly ServiceBusSender _serviceBusSender;

        private bool _disposed;

        public AzureServiceBusMessagePublisherV2(
            string connectionString,
            string topicName)
        {
            _serviceBusSender = new ServiceBusClient(connectionString).CreateSender(topicName);
        }

        public override Task EnqueueAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            return _serviceBusSender.SendMessageAsync(MapMessageToAzureServiceBusMessage(message));
        }

        public override Task EnqueueAsync(IMessage message, DateTimeOffset scheduledTimeUtc, CancellationToken cancellationToken = default)
        {
            return _serviceBusSender.ScheduleMessageAsync(
                MapMessageToAzureServiceBusMessage(message),
                scheduledTimeUtc);
        }

        public override Task EnqueueBatchAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default)
        {
            var messageList = new List<ServiceBusMessage>();

            foreach (var message in messages)
            {
                messageList.Add(MapMessageToAzureServiceBusMessage(message));
            }

            return _serviceBusSender.SendMessagesAsync(messageList);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _serviceBusSender.CloseAsync()
                        .GetAwaiter()
                        .GetResult();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private ServiceBusMessage MapMessageToAzureServiceBusMessage(IMessage message)
        {
            var serviceBusMessage = new ServiceBusMessage(message.Body)
            {
                ContentType = "application/json"
            };

            if (!string.IsNullOrEmpty(message.MessageId))
            {
                serviceBusMessage.MessageId = message.MessageId;
            }

            return serviceBusMessage;
        }
    }
}
