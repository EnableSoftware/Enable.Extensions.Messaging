using System;
using Enable.Extensions.Messaging.Abstractions;
using Enable.Extensions.Messaging.RabbitMQ.Internal;

namespace Enable.Extensions.Messaging.RabbitMQ
{
    public class RabbitMQMessagingClientFactory : BaseRabbitMQMessagingClientFactory, IMessagingClientFactory
    {
        public RabbitMQMessagingClientFactory(RabbitMQMessagingClientFactoryOptions options)
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

        public IMessagePublisher GetMessagePublisher(string topicName, string exchangeType, string routingKey)
        {
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException(nameof(topicName));
            }

            return new RabbitMQMessagePublisher(ConnectionFactory, topicName, exchangeType, routingKey);
        }

        public IMessageSubscriber GetMessageSubscriber(string topicName, string subscriptionName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException(nameof(topicName));
            }

            if (string.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentException(nameof(subscriptionName));
            }

            return new RabbitMQMessageSubscriber(ConnectionFactory, topicName, subscriptionName);
        }
    }
}
