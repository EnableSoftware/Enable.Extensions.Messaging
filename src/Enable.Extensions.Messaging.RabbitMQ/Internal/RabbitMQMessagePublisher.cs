using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    internal class RabbitMQMessagePublisher : BaseMessagePublisher
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        private readonly string _delayQueueName;
        private readonly string _routingKey;

        private bool _disposed;

        public RabbitMQMessagePublisher(
            ConnectionFactory connectionFactory,
            string topicName)
            : this(connectionFactory, topicName, "fanout")
        {
        }

        public RabbitMQMessagePublisher(
            ConnectionFactory connectionFactory,
            string topicName,
            string exchangeType)
        {
            _connectionFactory = connectionFactory;
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _exchangeName = topicName;

            _exchangeName = GetExchangeName(topicName);
            _routingKey = string.Empty;

            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: exchangeType,
                durable: true,
                autoDelete: false,
                arguments: null);

            // Declare the delay queue. This is used to schedule messages.
            _delayQueueName = GetDelayQueueName(topicName);

            var queueArguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", _exchangeName },
                { "x-dead-letter-routing-key", _routingKey }
            };

            _channel.QueueDeclare(
                _delayQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArguments);
        }

        public Task EnqueueAsync(
            IMessage message,
            string routingKey)
        {
            var body = message.Body;
            var messageProperties = GetBasicMessageProperties(_channel);

            lock (_channel)
            {
                _channel.BasicPublish(
                    _exchangeName,
                    routingKey,
                    messageProperties,
                    new ReadOnlyMemory<byte>(body));
            }

            return Task.CompletedTask;
        }

        public override Task EnqueueAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return EnqueueAsync(message, _routingKey);
        }

        public override Task EnqueueBatchAsync(
            IEnumerable<IMessage> messages,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Setting the mandatory option to false will silently drop a message
            // if it fails. This is the default value for the BasicPublish method,
            // which is used elsewhere when publishing single messages.
            var mandatory = false;

            var messageProperties = GetBasicMessageProperties(_channel);

            lock (_channel)
            {
                var batch = _channel.CreateBasicPublishBatch();

                foreach (var message in messages)
                {
                    batch.Add(
                        _exchangeName,
                        _routingKey,
                        mandatory,
                        messageProperties,
                        new ReadOnlyMemory<byte>(message.Body));
                }

                batch.Publish();
            }

            return Task.CompletedTask;
        }

        public override Task EnqueueAsync(
            IMessage message,
            DateTimeOffset scheduledTimeUtc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var body = message.Body;
            var messageProperties = GetBasicMessageProperties(_channel);

            // Set a per-message time to live (TTL). Here we schedule a message
            // by placing it on to a "delay" queue with a TTL. After this TTL
            // expires, it is routed to the main message bus exchange.
            var now = DateTimeOffset.UtcNow;
            var delay = scheduledTimeUtc - now;
            var expiration = (int)delay.TotalMilliseconds;
            messageProperties.Expiration = expiration.ToString();

            lock (_channel)
            {
                _channel.BasicPublish(
                    string.Empty,
                    _delayQueueName,
                    messageProperties,
                    body);
            }

            return Task.CompletedTask;
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

        private static string GetExchangeName(string topicName)
        {
            return topicName;
        }

        private static string GetDelayQueueName(string topicName)
        {
            return $"{topicName}.delay";
        }
    }
}
