using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    internal class BaseRabbitMQMessageSubscriber : BaseMessageSubscriber
    {
        private bool _messageHandlerRegistered;
        private bool _disposed;

        public BaseRabbitMQMessageSubscriber(
            ConnectionFactory connectionFactory,
            string topicName,
            string subscriptionName)
        {
            ConnectionFactory = connectionFactory;
            Connection = ConnectionFactory.CreateConnection();
            Channel = Connection.CreateModel();

            ExchangeName = GetExchangeName(topicName);
            DeadLetterExchangeName = GetDeadLetterExchangeName(topicName);
            RoutingKey = string.Empty;

            QueueName = GetQueueName(topicName, subscriptionName);
            DeadLetterQueueName = GetDeadLetterQueueName(topicName, subscriptionName);
            DLQueueArguments = null;

            // Declare the dead letter queue exchange.
            Channel.ExchangeDeclare(
                exchange: DeadLetterExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);

            // Declare the main queue exchange.
            Channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);

            QueueArguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", DeadLetterExchangeName },
                { "x-dead-letter-routing-key", DeadLetterQueueName }
            };
        }

        protected string ExchangeName { get; set; }
        protected string DeadLetterExchangeName { get; set; }
        protected ConnectionFactory ConnectionFactory { get; set; }
        protected IConnection Connection { get; set; }
        protected IModel Channel { get; set; }
        protected string RoutingKey { get; set; }
        protected string QueueName { get; set; }
        protected string DeadLetterQueueName { get; set; }
        protected Dictionary<string, object> QueueArguments { get; set; }
        protected Dictionary<string, object> DLQueueArguments { get; set; }

        public override Task AbandonAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var deliveryTag = Convert.ToUInt64(message.LeaseId);

            // Lock the channel before attempting to send an acknowledgement.
            // This is required because `IModel` is not thread safe.
            // See https://www.rabbitmq.com/dotnet-api-guide.html#model-sharing.
            lock (Channel)
            {
                // TODO Handle automatic dead-lettering after a certain number of requeues.
                // Storing a delivery count is on the roadmap for RabbitMQ 3.8, see
                // https://github.com/rabbitmq/rabbitmq-server/issues/502 for more information.
                // In the meantime, we settle for dead lettering a message on the first negative
                // acknowledgement.
                Channel.BasicReject(deliveryTag, requeue: false);
            }

            return Task.CompletedTask;
        }

        public override Task CompleteAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var deliveryTag = Convert.ToUInt64(message.LeaseId);

            lock (Channel)
            {
                Channel.BasicAck(deliveryTag, multiple: false);
            }

            return Task.CompletedTask;
        }

        public override Task<IMessage> DequeueAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            BasicGetResult result;

            lock (Channel)
            {
                result = Channel.BasicGet(QueueName, autoAck: false);
            }

            if (result == null)
            {
                return Task.FromResult<IMessage>(null);
            }

            var message = new RabbitMQMessage(
                result.Body.ToArray(),
                result.DeliveryTag,
                result.BasicProperties);

            return Task.FromResult<IMessage>(message);
        }

        public override Task RegisterMessageHandler(
            Func<IMessage, CancellationToken, Task> messageHandler,
            MessageHandlerOptions messageHandlerOptions)
        {
            if (messageHandlerOptions == null)
            {
                throw new ArgumentNullException(nameof(messageHandlerOptions));
            }

            if (messageHandlerOptions.MaxConcurrentCalls > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(messageHandlerOptions.MaxConcurrentCalls),
                    messageHandlerOptions.MaxConcurrentCalls,
                    $@"'{nameof(messageHandlerOptions.MaxConcurrentCalls)}' must be less than or equal to {ushort.MaxValue}.");
            }

            lock (Channel)
            {
                if (_messageHandlerRegistered)
                {
                    throw new InvalidOperationException("A message handler has already been registered.");
                }

                // Reconfigure quality of service on the channel in order to
                // support specified level of concurrency. Here we are changing
                // the prefetch count to a user-specified `MaxConcurrentCalls`.
                // This only affects new consumers on the channel, existing
                // consumers are unaffected and will have a `prefetchCount` of 1,
                // as specified in the constructor, above.
                ConfigureBasicQos(prefetchCount: messageHandlerOptions.MaxConcurrentCalls);

                var consumer = new EventingBasicConsumer(Channel);

                consumer.Received += (channel, eventArgs) =>
                {
                    // Queue the processing of the message received. This allows
                    // the calling consumer instance to continue before the message
                    // handler completes, which is essential for handling multiple
                    // messages concurrently with a single consumer.
                    Task.Run(() => OnMessageReceivedAsync(messageHandler, messageHandlerOptions, eventArgs)).Ignore();
                };

                Channel.BasicConsume(QueueName, autoAck: false, consumer: consumer);

                _messageHandlerRegistered = true;
            }

            return Task.CompletedTask;
        }

        public override Task RenewLockAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO Implement automatic `nack`-ing.
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Channel.Dispose();
                    Connection.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        protected void DeclareQueues()
        {
            // Declare the dead letter queue.
            Channel.QueueDeclare(
                DeadLetterQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: DLQueueArguments);

            Channel.QueueBind(
                queue: DeadLetterQueueName,
                exchange: DeadLetterExchangeName,
                routingKey: RoutingKey);

            // Declare the main queue.
            Channel.QueueDeclare(
                QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: QueueArguments);

            Channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey);

            // Here we are assuming a "worker queue", where multiple consumers are
            // competing to draw from a single queue in order to spread workload
            // across the consumers.
            ConfigureBasicQos(prefetchCount: 1);
        }

        private string GetExchangeName(string topicName)
        {
            return topicName;
        }

        private string GetQueueName(string topicName, string subscriptionName)
        {
            return $"{topicName}.{subscriptionName}";
        }

        private string GetDeadLetterExchangeName(string topicName)
        {
            return $"{topicName}.dead-letter";
        }

        private string GetDeadLetterQueueName(string topicName, string subscriptionName)
        {
            var queueName = GetQueueName(topicName, subscriptionName);

            return $"{queueName}.dead-letter";
        }

        private void ConfigureBasicQos(int prefetchCount)
        {
            lock (Channel)
            {
                // Here we are assuming a "worker queue", where multiple consumers are
                // competing to draw from a single queue in order to spread workload
                // across the consumers.
                Channel.BasicQos(
                    prefetchSize: 0,
                    prefetchCount: Convert.ToUInt16(prefetchCount),
                    global: false);
            }
        }

        private async Task OnMessageReceivedAsync(
            Func<IMessage, CancellationToken, Task> messageHandler,
            MessageHandlerOptions messageHandlerOptions,
            BasicDeliverEventArgs eventArgs)
        {
            var cancellationToken = CancellationToken.None;

            var message = new RabbitMQMessage(
                eventArgs.Body.ToArray(),
                eventArgs.DeliveryTag,
                eventArgs.BasicProperties);

            try
            {
                await messageHandler(message, cancellationToken);

                if (messageHandlerOptions.AutoComplete)
                {
                    await CompleteAsync(message, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var exceptionHandler = messageHandlerOptions?.ExceptionReceivedHandler;

                    if (exceptionHandler != null)
                    {
                        var context = new MessageHandlerExceptionContext(ex);

                        await exceptionHandler(context);
                    }
                }
                catch
                {
                }

                await AbandonAsync(message, cancellationToken);
            }
        }
    }
}
