using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using Xunit;

namespace Enable.Extensions.Messaging.RabbitMQ.Tests
{
    public class RabbitMQMessagePublisherTests : IClassFixture<RabbitMQTestFixture>, IDisposable
    {
        private readonly RabbitMQTestFixture _fixture;

        private readonly IMessagePublisher _sut;

        private bool _disposed;

        public RabbitMQMessagePublisherTests(RabbitMQTestFixture fixture)
        {
            var options = new RabbitMQMessagingClientFactoryOptions
            {
                HostName = fixture.HostName,
                Port = fixture.Port,
                VirtualHost = fixture.VirtualHost,
                UserName = fixture.UserName,
                Password = fixture.Password
            };

            var messagingClientFactory = new RabbitMQMessagingClientFactory(options);

            _sut = messagingClientFactory.GetMessagePublisher(
                fixture.TopicName);

            _fixture = fixture;
        }

        [Fact]
        public async Task EnqueueAsync_CanInvokeWithString()
        {
            // Arrange
            var content = Guid.NewGuid().ToString();

            // Act
            await _sut.EnqueueAsync(content, CancellationToken.None);
        }

        [Fact]
        public async Task EnqueueBatchAsync_CanInvokeWithStringCollection()
        {
            // Arrange
            var batch = new string[]
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            // Act
            await _sut.EnqueueBatchAsync(batch, CancellationToken.None);
        }

        [Fact]
        public async Task EnqueueAsync_CanInvokeWithByteArray()
        {
            // Arrange
            var content = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());

            // Act
            await _sut.EnqueueAsync(content, CancellationToken.None);
        }

        [Fact]
        public async Task EnqueueAsync_CanInvokeWithCustomMessageType()
        {
            // Arrange
            var content = new CustomMessageType
            {
                Message = Guid.NewGuid().ToString()
            };

            // Act
            await _sut.EnqueueAsync(content, CancellationToken.None);
        }

        [Fact]
        public async Task EnqueueBatchAsync_CanInvokeWithCustomMessageTypeCollection()
        {
            // Arrange
            var batch = new CustomMessageType[]
            {
                new CustomMessageType { Message = Guid.NewGuid().ToString() },
                new CustomMessageType { Message = Guid.NewGuid().ToString() }
            };

            // Act
            await _sut.EnqueueBatchAsync(batch, CancellationToken.None);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // With the RabbitMQ implementation, we must disconnect
                    // our consumer before purging the queue, otherwise,
                    // purging the queue won't remove unacked messages.
                    _sut.Dispose();

                    try
                    {
                        // Make a best effort to clear our test queue.
                        _fixture.ClearQueue();
                    }
                    catch
                    {
                    }
                }

                _disposed = true;
            }
        }

        private class CustomMessageType
        {
            public string Message { get; set; }
        }
    }
}
