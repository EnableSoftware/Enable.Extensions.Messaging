using System;
using Enable.Extensions.Messaging.Abstractions;
using Enable.Extensions.Messaging.RabbitMQ.Internal;

namespace Enable.Extensions.Messaging.RabbitMQ
{
    public class RabbitMQQuorumQueueMessagingClientFactory : BaseRabbitMQMessagingClientFactory, IMessagingClientFactory
    {
        public RabbitMQQuorumQueueMessagingClientFactory(RabbitMQMessagingClientFactoryOptions options)
             : base(options)
        {
        }

        public IMessagePublisher GetMessagePublisher(string topicName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException(nameof(topicName));
            }

            return new RabbitMQQuorumQueueMessagePublisher(ConnectionFactory, topicName);
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

            return new RabbitMQQuorumQueueMessageSubscriber(ConnectionFactory, topicName, subscriptionName);
        }
    }
}
