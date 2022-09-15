using System;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    public class RabbitMQMessageProcessor : BaseMessageProcessor
    {
        private readonly RabbitMQMessageSubscriber _rabbitMQMessageSubscriber;
        private readonly MessageHandlerOptions _messageHandlerOptions;

        public RabbitMQMessageProcessor(
            ConnectionFactory connectionFactory,
            string topicName,
            string subscriptionName,
            MessageHandlerOptions messageHandlerOptions)
        {
            _rabbitMQMessageSubscriber = new RabbitMQMessageSubscriber(connectionFactory, topicName, subscriptionName);
            _messageHandlerOptions = messageHandlerOptions ?? throw new ArgumentNullException(nameof(messageHandlerOptions));
        }

        public override Task RegisterMessageHandler(
            Func<IMessage, CancellationToken, Task> messageHandler,
            Func<MessageHandlerExceptionContext, Task> errorHandler = null)
        {
            if (messageHandler is null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            _messageHandlerOptions.ExceptionReceivedHandler = errorHandler;

            return _rabbitMQMessageSubscriber.RegisterMessageHandler(messageHandler, _messageHandlerOptions);
        }
    }
}
