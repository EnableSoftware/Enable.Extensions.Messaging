using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    internal class RabbitMQMessagePublisher : BaseRabbitMQMessagePublisher
    {
        public RabbitMQMessagePublisher(
            ConnectionFactory connectionFactory,
            string topicName)
            : base(connectionFactory, topicName)
        {
            DeclareQueues();
        }
    }
}
