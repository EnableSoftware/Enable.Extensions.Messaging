using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using Xunit;

namespace Enable.Extensions.Messaging.AzureServiceBus.Tests
{
    public class AzureServiceBusMessagePublisherTests : IClassFixture<AzureServiceBusTestFixture>, IDisposable
    {
        private readonly AzureServiceBusTestFixture _fixture;

        private readonly IMessagePublisher _sut;

        private bool _disposed;

        public AzureServiceBusMessagePublisherTests(AzureServiceBusTestFixture fixture)
        {
            var options = new AzureServiceBusMessagingClientFactoryOptions
            {
                ConnectionString = fixture.ConnectionString
            };

            var messagingClientFactory = new AzureServiceBusMessagingClientFactory(options);

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
        public async Task EnqueueAsync_CanInvokeWithStringCollection()
        {
            // Arrange
            var batch = new string[]
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            // Act
            await _sut.EnqueueAsync<string>(batch, CancellationToken.None);
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
        public async Task EnqueueAsync_CanInvokeWithCustomMessageTypeCollection()
        {
            // Arrange
            var batch = new CustomMessageType[]
            {
                new CustomMessageType { Message = Guid.NewGuid().ToString() },
                new CustomMessageType { Message = Guid.NewGuid().ToString() }
            };

            // Act
            await _sut.EnqueueAsync<CustomMessageType>(batch, CancellationToken.None);
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
                    _sut.Dispose();

                    try
                    {
                        // Make a best effort to clear our test queue.
                        _fixture.ClearQueue()
                            .GetAwaiter()
                            .GetResult();
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
