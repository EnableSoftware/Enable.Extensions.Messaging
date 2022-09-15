using System;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using Xunit;

namespace Enable.Extensions.Messaging.RabbitMQ.Tests
{
    public class RabbitMQMessageProcessorTests : IClassFixture<RabbitMQTestFixture>, IDisposable
    {
        private readonly RabbitMQTestFixture _fixture;

        private readonly IMessagePublisher _messagePublisher;

        private readonly IMessageProcessor _sut;

        private bool _disposed;

        public RabbitMQMessageProcessorTests(RabbitMQTestFixture fixture)
        {
            var options = new RabbitMQMessagingClientFactoryOptions
            {
                HostName = fixture.HostName,
                Port = fixture.Port,
                VirtualHost = fixture.VirtualHost,
                UserName = fixture.UserName,
                Password = fixture.Password
            };

            var messagingClientFactory = new RabbitMQMessagingClientFactoryV2(options);

            var messageHandlerOptions = new MessageHandlerOptions
            {
                MaxConcurrentCalls = 1,
                ExceptionReceivedHandler = (_) => Task.CompletedTask
            };

            _messagePublisher = messagingClientFactory.GetMessagePublisher(
                fixture.TopicName);

            _sut = messagingClientFactory.GetMessageProcessor(
                fixture.TopicName,
                fixture.SubscriptionName,
                messageHandlerOptions);

            _fixture = fixture;
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

            evt.WaitOne(TimeSpan.FromSeconds(1));

            // Act
            await _messagePublisher.EnqueueAsync(
                Guid.NewGuid().ToString(),
                CancellationToken.None);

            // Assert
            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(1)));
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

            await _sut.RegisterMessageHandler(MessageHandler, ExceptionHandler);

            // Act
            await _messagePublisher.EnqueueAsync(
                Guid.NewGuid().ToString(),
                CancellationToken.None);

            // Assert
            Assert.True(evt.WaitOne(TimeSpan.FromSeconds(1)));
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
