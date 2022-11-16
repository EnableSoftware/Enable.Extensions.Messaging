using System;
using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ
{
    public class BaseRabbitMQMessagingClientFactory
    {
        public BaseRabbitMQMessagingClientFactory(RabbitMQMessagingClientFactoryOptions options)
        {
            ConnectionFactory = new ConnectionFactory
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

        protected ConnectionFactory ConnectionFactory { get; set; }
    }
}
