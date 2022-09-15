using System;
using Enable.Extensions.Messaging.Abstractions;
using Enable.Extensions.Messaging.AzureServiceBus.Internal.V2;

namespace Enable.Extensions.Messaging.AzureServiceBus
{
    public class AzureServiceBusMessagingClientFactoryV2 : IMessagingClientFactoryV2
    {
        private readonly AzureServiceBusMessagingClientFactoryOptions _options;

        public AzureServiceBusMessagingClientFactoryV2(AzureServiceBusMessagingClientFactoryOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                throw new ArgumentNullException(nameof(options.ConnectionString));
            }

            _options = options;
        }

        public IMessagePublisher GetMessagePublisher(string topicName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException(nameof(topicName));
            }

            return new AzureServiceBusMessagePublisherV2(
                _options.ConnectionString,
                topicName);
        }

        public IMessageProcessor GetMessageProcessor(
            string topicName,
            string subscriptionName,
            MessageHandlerOptions messageHandlerOptions)
        {
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException(nameof(topicName));
            }

            if (string.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentException(nameof(subscriptionName));
            }

            return new AzureServiceBusMessageProcessor(
                _options.ConnectionString,
                topicName,
                subscriptionName,
                messageHandlerOptions);
        }
    }
}
