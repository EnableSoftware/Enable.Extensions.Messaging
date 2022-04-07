using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Enable.Extensions.Messaging.AzureServiceBus.Internal
{
    // TODO Respect cancellationToken
    public class AzureServiceBusMessagePublisher : BaseMessagePublisher
    {
        private readonly MessageSender _messageSender;

        private bool _disposed;

        public AzureServiceBusMessagePublisher(
            string connectionString,
            string topicName)
        {
            _messageSender = new MessageSender(connectionString, topicName);
        }

        public override Task EnqueueAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _messageSender.SendAsync(MapMessageToAzureServiceBusMessage(message));
        }

        public override Task EnqueueBatchAsync(
            IEnumerable<IMessage> messages,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageList = new List<Message>();

            foreach (var message in messages)
            {
                messageList.Add(MapMessageToAzureServiceBusMessage(message));
            }

            return _messageSender.SendAsync(messageList);
        }

        public override Task EnqueueAsync(
            IMessage message,
            DateTimeOffset scheduledTimeUtc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _messageSender.ScheduleMessageAsync(
                MapMessageToAzureServiceBusMessage(message),
                scheduledTimeUtc);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _messageSender.CloseAsync()
                        .GetAwaiter()
                        .GetResult();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private Message MapMessageToAzureServiceBusMessage(IMessage message)
        {
            var serviceBusMessage = new Message(message.Body)
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
