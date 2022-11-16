using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    internal class RabbitMQMessagePublisher : BaseRabbitMQMessagePublisher
    {
        public RabbitMQMessagePublisher(
            ConnectionFactory connectionFactory,
            string topicName)
            : this(connectionFactory, topicName, "fanout", string.Empty)
        {
        }

        public RabbitMQMessagePublisher(
            ConnectionFactory connectionFactory,
            string topicName,
            string exchangeType,
            string routingKey)
            : base(connectionFactory, topicName, exchangeType, routingKey)
        {
            DeclareQueue();
        }
    }
}
