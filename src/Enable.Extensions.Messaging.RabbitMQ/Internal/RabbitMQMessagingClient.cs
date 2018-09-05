using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    internal class RabbitMQMessagingClient : BaseMessagingClient
    {
        private const string RedeliveryCountHeaderName = "x-redelivered-count";

        private readonly ConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private readonly string _queueName;
        private readonly string _deadLetterQueueName;

        private bool _messageHandlerRegistered;
        private bool _disposed;

        public RabbitMQMessagingClient(
            ConnectionFactory connectionFactory,
            string topicName,
            string subscriptionName)
        {
            _connectionFactory = connectionFactory;
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _exchangeName = topicName;
            _routingKey = string.Empty;

            _queueName = GetQueueName(topicName, subscriptionName);

            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);

            // Declare the dead letter queue.
            _deadLetterQueueName = GetDeadLetterQueueName(_queueName);

            _channel.QueueDeclare(
                _deadLetterQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: _deadLetterQueueName,
                exchange: _exchangeName,
                routingKey: _routingKey);

            // Declare the main queue.
            var queueArguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", _exchangeName },
                { "x-dead-letter-routing-key", _deadLetterQueueName }
            };

            _channel.QueueDeclare(
                _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments);

            _channel.QueueBind(
                queue: _queueName,
                exchange: _exchangeName,
                routingKey: _routingKey);

            // Here we are assuming a "worker queue", where multiple consumers are
            // competing to draw from a single queue in order to spread workload
            // across the consumers.
            ConfigureBasicQos(prefetchCount: 1);
        }

        public override Task AbandonAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var deliveryTag = Convert.ToUInt64(message.LeaseId);

            // Lock the channel before attempting to send an acknowledgement.
            // This is required because `IModel` is not thread safe.
            // See https://www.rabbitmq.com/dotnet-api-guide.html#model-sharing.
            lock (_channel)
            {
                // TODO Handle automatic dead-lettering after a certain number of requeues.
                // Storing a delivery count is on the roadmap for RabbitMQ 3.8, see
                // https://github.com/rabbitmq/rabbitmq-server/issues/502 for more information.
                // In the meantime, we settle for dead lettering a message on the first negative
                // acknowledgement.
                _channel.BasicReject(deliveryTag, requeue: false);
            }

            return Task.CompletedTask;
        }

        public override Task CompleteAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var deliveryTag = Convert.ToUInt64(message.LeaseId);

            lock (_channel)
            {
                _channel.BasicAck(deliveryTag, multiple: false);
            }

            return Task.CompletedTask;
        }

        public override Task<IMessage> DequeueAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            BasicGetResult result;

            lock (_channel)
            {
                result = _channel.BasicGet(_queueName, autoAck: false);
            }

            if (result == null)
            {
                return Task.FromResult<IMessage>(null);
            }

            var message = new RabbitMQMessage(
                result.Body,
                result.DeliveryTag,
                result.BasicProperties);

            return Task.FromResult<IMessage>(message);
        }

        public override Task EnqueueAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var body = message.Body;
            var messageProperties = GetBasicMessageProperties(_channel);

            lock (_channel)
            {
                _channel.BasicPublish(
                    _exchangeName,
                    _routingKey,
                    messageProperties,
                    body);
            }

            return Task.CompletedTask;
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

            lock (_channel)
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

                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += (channel, eventArgs) =>
                {
                    // Queue the processing of the message received. This allows
                    // the calling consumer instance to continue before the message
                    // handler completes, which is essential for handling multiple
                    // messages concurrently with a single consumer.
                    Task.Run(() => OnMessageReceivedAsync(messageHandler, messageHandlerOptions, eventArgs)).Ignore();
                };

                _channel.BasicConsume(_queueName, autoAck: false, consumer: consumer);

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
                    _channel.Dispose();
                    _connection.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private static IBasicProperties GetBasicMessageProperties(IModel channel)
        {
            var properties = channel.CreateBasicProperties();

            properties.ContentEncoding = Encoding.UTF8.HeaderName;
            properties.ContentType = "application/json";
            properties.Persistent = true;

            return properties;
        }

        private string GetQueueName(string topicName, string subscriptionName)
        {
            return $"{topicName}.{subscriptionName}";
        }

        private string GetDeadLetterQueueName(string queueName)
        {
            return $"{queueName}.dead-letter";
        }

        private void ConfigureBasicQos(int prefetchCount)
        {
            lock (_channel)
            {
                // Here we are assuming a "worker queue", where multiple consumers are
                // competing to draw from a single queue in order to spread workload
                // across the consumers.
                _channel.BasicQos(
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
                eventArgs.Body,
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
