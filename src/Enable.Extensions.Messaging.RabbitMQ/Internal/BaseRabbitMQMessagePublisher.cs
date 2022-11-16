using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using RabbitMQ.Client;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    internal class BaseRabbitMQMessagePublisher : BaseMessagePublisher
    {
        private readonly string _routingKey;

        private bool _disposed;

        public BaseRabbitMQMessagePublisher(
            ConnectionFactory connectionFactory,
            string topicName)
        {
            ConnectionFactory = connectionFactory;
            Connection = ConnectionFactory.CreateConnection();
            Channel = Connection.CreateModel();

            ExchangeName = topicName;

            ExchangeName = GetExchangeName(topicName);
            _routingKey = string.Empty;

            // Declare the delay queue. This is used to schedule messages.
            DelayQueueName = GetDelayQueueName(topicName);

            QueueArguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", ExchangeName },
                { "x-dead-letter-routing-key", _routingKey }
            };
        }

        protected string ExchangeName { get; set; }
        protected ConnectionFactory ConnectionFactory { get; set; }
        protected IConnection Connection { get; set; }
        protected IModel Channel { get; set; }
        protected string RoutingKey { get; set; }
        protected string DelayQueueName { get; set; }
        protected Dictionary<string, object> QueueArguments { get; set; }
        protected Dictionary<string, object> DLQueueArguments { get; set; }

        public override Task EnqueueAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var body = message.Body;
            var messageProperties = GetBasicMessageProperties(Channel);

            lock (Channel)
            {
                Channel.BasicPublish(
                    ExchangeName,
                    _routingKey,
                    messageProperties,
                    new ReadOnlyMemory<byte>(body));
            }

            return Task.CompletedTask;
        }

        public override Task EnqueueBatchAsync(
            IEnumerable<IMessage> messages,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Setting the mandatory option to false will silently drop a message
            // if it fails. This is the default value for the BasicPublish method,
            // which is used elsewhere when publishing single messages.
            var mandatory = false;

            var messageProperties = GetBasicMessageProperties(Channel);

            lock (Channel)
            {
                var batch = Channel.CreateBasicPublishBatch();

                foreach (var message in messages)
                {
                    batch.Add(
                        ExchangeName,
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
            var messageProperties = GetBasicMessageProperties(Channel);

            // Set a per-message time to live (TTL). Here we schedule a message
            // by placing it on to a "delay" queue with a TTL. After this TTL
            // expires, it is routed to the main message bus exchange.
            var now = DateTimeOffset.UtcNow;
            var delay = scheduledTimeUtc - now;
            var expiration = (int)delay.TotalMilliseconds;
            messageProperties.Expiration = expiration.ToString();

            lock (Channel)
            {
                Channel.BasicPublish(
                    string.Empty,
                    DelayQueueName,
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
                    Channel.Dispose();
                    Connection.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        protected void DeclareQueues()
        {
            Channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);

            Channel.QueueDeclare(
                DelayQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: QueueArguments);
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
