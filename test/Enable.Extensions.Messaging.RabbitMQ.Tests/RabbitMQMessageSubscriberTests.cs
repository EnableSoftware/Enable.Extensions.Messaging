using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using Xunit;

namespace Enable.Extensions.Messaging.RabbitMQ.Tests
{
    public class RabbitMQMessageSubscriberTests : IClassFixture<RabbitMQTestFixture>, IDisposable
    {
        private readonly RabbitMQTestFixture _fixture;

        private readonly IMessagePublisher _messagePublisher;

        private readonly IMessageSubscriber _sut;

        private bool _disposed;

        public RabbitMQMessageSubscriberTests(RabbitMQTestFixture fixture)
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

            _messagePublisher = messagingClientFactory.GetMessagePublisher(
                fixture.TopicName);

            _sut = messagingClientFactory.GetMessageSubscriber(
                fixture.TopicName,
                fixture.SubscriptionName);

            _fixture = fixture;
        }

        [Fact]
        public async Task DequeueAsync_CanInvoke()
        {
            // Act
            var message = await _sut.DequeueAsync(CancellationToken.None);

            // Assert
            Assert.Null(message);
        }

        [Fact]
        public async Task DequeueAsync_ReturnsEnqueuedMessage()
        {
            // Arrange
            var content = Guid.NewGuid().ToString();

            await _messagePublisher.EnqueueAsync(content, CancellationToken.None);

            // TODO Review the need for these delays.
            await Task.Delay(100);

            // Act
            var message = await _sut.DequeueAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(message);

            // Clean up
            await _sut.CompleteAsync(message, CancellationToken.None);
        }

        [Fact]
        public async Task DequeueAsync_CanDeserializeMessage()
        {
            // Arrange
            var content = Guid.NewGuid().ToString();

            await _messagePublisher.EnqueueAsync(content, CancellationToken.None);

            // TODO Review the need for these delays.
            await Task.Delay(100);

            // Act
            var message = await _sut.DequeueAsync(CancellationToken.None);

            // Assert
            Assert.Equal(content, message.GetBody<string>());

            Console.WriteLine(message.GetBody<string>());

            // Clean up
            await _sut.CompleteAsync(message, CancellationToken.None);
        }

        [Fact]
        public async Task AbandonAsync_CanInvoke()
        {
            // Arrange
            await _messagePublisher.EnqueueAsync(
                Guid.NewGuid().ToString(),
                CancellationToken.None);

            // TODO Review the need for these delays.
            await Task.Delay(100);

            var message = await _sut.DequeueAsync(CancellationToken.None);

            // Act
            await _sut.AbandonAsync(message, CancellationToken.None);
        }

        [Fact]
        public async Task CompleteAsync_CanInvoke()
        {
            // Arrange
            await _messagePublisher.EnqueueAsync(
                Guid.NewGuid().ToString(),
                CancellationToken.None);

            // TODO Review the need for these delays.
            await Task.Delay(100);

            var message = await _sut.DequeueAsync(CancellationToken.None);

            // Act
            await _sut.CompleteAsync(message, CancellationToken.None);
        }

        [Fact]
        public async Task RegisterMessageHandler_CanInvoke()
        {
            // Act
            await _sut.RegisterMessageHandler(
                (message, cancellationToken) => throw new Exception("There should be no messages to process."));
        }

        [Fact]
        public async Task RegisterMessageHandler_ThrowsOnMutipleMessageHandlerRegistrations()
        {
            // Arrange
            Task MessageHandler(IMessage message, CancellationToken cancellationToken)
            {
                throw new Exception("There should be no messages to process.");
            }

            await _sut.RegisterMessageHandler(MessageHandler);

            // Act
            var exception = await Record.ExceptionAsync(() => _sut.RegisterMessageHandler(MessageHandler));

            // Assert
            Assert.IsType<InvalidOperationException>(exception);
        }

        [Fact]
        public async Task RegisterMessageHandler_MessageHandlerInvoked()
        {
            // Arrange
            var evt = new ManualResetEvent(false);

            Task MessageHandler(IMessage message, CancellationToken cancellationToken)
            {
                evt.Set();
                return Task.CompletedTask;
            }

            await _sut.RegisterMessageHandler(MessageHandler);

            // Act
            await _messagePublisher.EnqueueAsync(
                Guid.NewGuid().ToString(),
                CancellationToken.None);

            // Assert
            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public async Task RegisterMessageHandler_ThrowsForNullMessageHandlerOptions()
        {
            // Arrange
            Task MessageHandler(IMessage message, CancellationToken cancellationToken)
            {
                throw new Exception("There should be no messages to process.");
            }

            // Act
            var exception = await Record.ExceptionAsync(
                () => _sut.RegisterMessageHandler(MessageHandler, null));

            // Assert
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task RegisterMessageHandler_CanSetMessageHandlerOptions()
        {
            // Arrange
            Task MessageHandler(IMessage message, CancellationToken cancellationToken)
            {
                throw new Exception("There should be no messages to process.");
            }

            var options = new MessageHandlerOptions
            {
                MaxConcurrentCalls = 1,
                ExceptionReceivedHandler = (_) => Task.CompletedTask
            };

            // Act
            await _sut.RegisterMessageHandler(MessageHandler, options);
        }

        [Fact]
        public async Task RegisterMessageHandler_ExceptionHandlerInvoked()
        {
            // Arrange
            var evt = new ManualResetEvent(false);

            Task MessageHandler(IMessage message, CancellationToken cancellationToken)
            {
                throw new Exception("Message failed processing.");
            }

            Task ExceptionHandler(MessageHandlerExceptionContext context)
            {
                evt.Set();
                return Task.CompletedTask;
            }

            var options = new MessageHandlerOptions
            {
                MaxConcurrentCalls = 1,
                ExceptionReceivedHandler = ExceptionHandler
            };

            await _sut.RegisterMessageHandler(MessageHandler, options);

            // Act
            await _messagePublisher.EnqueueAsync(
                Guid.NewGuid().ToString(),
                CancellationToken.None);

            // Assert
            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(1)));
        }

        [Fact]
        public async Task RenewLockAsync_CanInvoke()
        {
            // Arrange
            await _messagePublisher.EnqueueAsync(
                Guid.NewGuid().ToString(),
                CancellationToken.None);

            // TODO Review the need for these delays.
            await Task.Delay(100);

            var message = await _sut.DequeueAsync(CancellationToken.None);

            // Act
            var exception = await Record.ExceptionAsync(() => _sut.RenewLockAsync(message, CancellationToken.None));

            // Assert
            Assert.IsType<NotImplementedException>(exception);

            // Clean up
            await _sut.CompleteAsync(message, CancellationToken.None);
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
    }
}
