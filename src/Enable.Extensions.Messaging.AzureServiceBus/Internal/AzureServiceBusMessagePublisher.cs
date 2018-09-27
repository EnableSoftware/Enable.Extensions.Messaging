using System;
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
            return _messageSender.SendAsync(new Message(message.Body) { ContentType = "application/json" });
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
    }
}
