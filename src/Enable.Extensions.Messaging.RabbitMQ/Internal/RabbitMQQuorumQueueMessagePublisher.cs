using System.Collections.Generic;
using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    internal class RabbitMQQuorumQueueMessagePublisher : BaseRabbitMQMessagePublisher
    {
        public RabbitMQQuorumQueueMessagePublisher(
            ConnectionFactory connectionFactory,
            string topicName)
            : base(connectionFactory, topicName)
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
