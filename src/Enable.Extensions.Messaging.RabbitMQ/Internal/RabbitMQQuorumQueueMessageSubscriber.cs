using System.Collections.Generic;
using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    internal class RabbitMQQuorumQueueMessageSubscriber : BaseRabbitMQMessageSubscriber
    {
        public RabbitMQQuorumQueueMessageSubscriber(
            ConnectionFactory connectionFactory,
            string topicName,
            string subscriptionName)
            : base(connectionFactory, topicName, subscriptionName)
        {
            QueueArguments.Add("x-queue-type", "quorum");
            DLQueueArguments = new Dictionary<string, object>
                {
                    { "x-queue-type", "quorum" },
                };

            DeclareQueues();
        }
    }
}
