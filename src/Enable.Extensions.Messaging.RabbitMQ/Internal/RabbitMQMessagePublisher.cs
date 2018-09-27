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
    internal class RabbitMQMessagePublisher : BaseMessagePublisher
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        private readonly string _routingKey;

        private bool _disposed;

        public RabbitMQMessagePublisher(
            ConnectionFactory connectionFactory,
            string topicName)
        {
            _connectionFactory = connectionFactory;
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _exchangeName = topicName;
            _routingKey = string.Empty;

            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);
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
    }
}
