using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    internal class RabbitMQQuorumQueueMessagePublisher : BaseRabbitMQMessagePublisher
    {
        public RabbitMQQuorumQueueMessagePublisher(
           ConnectionFactory connectionFactory,
           string topicName)
           : this(connectionFactory, topicName, "fanout", string.Empty)
        {
        }

        public RabbitMQQuorumQueueMessagePublisher(
            ConnectionFactory connectionFactory,
            string topicName,
            string exchangeType,
            string routingKey)
            : base(connectionFactory, topicName, exchangeType, routingKey)
        {
            QueueArguments.Add("x-queue-type", "quorum");

            DeclareQueue();
        }
    }
}
