using System;
using Enable.Extensions.Messaging.Abstractions;
using Enable.Extensions.Messaging.RabbitMQ.Internal;

namespace Enable.Extensions.Messaging.RabbitMQ
{
    public class RabbitMQMessagingClientFactoryV2 : BaseRabbitMQMessagingClientFactory, IMessagingClientFactoryV2
    {
        public RabbitMQMessagingClientFactoryV2(RabbitMQMessagingClientFactoryOptions options)
            : base(options)
        {
        }

        public IMessagePublisher GetMessagePublisher(string topicName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException(nameof(topicName));
            }

            return new RabbitMQMessagePublisher(ConnectionFactory, topicName);
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

            if (messageHandlerOptions is null)
            {
                throw new ArgumentNullException(nameof(messageHandlerOptions));
            }

            return new RabbitMQMessageProcessor(ConnectionFactory, topicName, subscriptionName, messageHandlerOptions);
        }
    }
}
