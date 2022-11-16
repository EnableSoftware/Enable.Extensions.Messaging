using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    internal class RabbitMQMessageSubscriber : BaseRabbitMQMessageSubscriber
    {
        public RabbitMQMessageSubscriber(
            ConnectionFactory connectionFactory,
            string topicName,
            string subscriptionName)
            : base(connectionFactory, topicName, subscriptionName)
        {
            DeclareQueues();
        }
    }
}
