using System;
using Enable.Extensions.Messaging.Abstractions;
using Enable.Extensions.Messaging.RabbitMQ.Internal;
using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ
{
    public class RabbitMQMessagingClientFactory : IMessagingClientFactory
    {
        private readonly ConnectionFactory _connectionFactory;

        public RabbitMQMessagingClientFactory(RabbitMQMessagingClientFactoryOptions options)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                VirtualHost = options.VirtualHost,
                UserName = options.UserName,
                Password = options.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
        }

        public IMessagePublisher GetMessagePublisher(string topicName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException(nameof(topicName));
            }

            return new RabbitMQMessagePublisher(_connectionFactory, topicName);
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

            return new RabbitMQMessageSubscriber(_connectionFactory, topicName, subscriptionName);
        }
    }
}
