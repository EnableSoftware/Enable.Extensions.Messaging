using System;
using Enable.Extensions.Messaging.Abstractions;
using Enable.Extensions.Messaging.AzureServiceBus.Internal;

namespace Enable.Extensions.Messaging.AzureServiceBus
{
    public class AzureServiceBusMessagingClientFactory : IMessagingClientFactory
    {
        private readonly AzureServiceBusMessagingClientFactoryOptions _options;

        public AzureServiceBusMessagingClientFactory(AzureServiceBusMessagingClientFactoryOptions options)
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

        public IMessagingClient GetMessagingClient(string topicName, string subscriptionName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException(nameof(topicName));
            }

            if (string.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentException(nameof(subscriptionName));
            }

            return new AzureServiceBusMessagingClient(
                _options.ConnectionString,
                topicName,
                subscriptionName);
        }
    }
}
