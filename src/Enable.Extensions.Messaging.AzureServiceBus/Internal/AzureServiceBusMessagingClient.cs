using System;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using MessageHandlerOptions = Enable.Extensions.Messaging.Abstractions.MessageHandlerOptions;

namespace Enable.Extensions.Messaging.AzureServiceBus.Internal
{
    // TODO Respect cancellationToken
    public class AzureServiceBusMessagingClient : BaseMessagingClient
    {
        private readonly string _connectionString;
        private readonly string _entityPath;
        private readonly MessageReceiver _messageReceiver;
        private readonly MessageSender _messageSender;

        private bool _disposed;

        public AzureServiceBusMessagingClient(
            string connectionString,
            string topicName,
            string subscriptionName)
        {
            _connectionString = connectionString;

            _entityPath = EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName);

            _messageReceiver = new MessageReceiver(
                _connectionString,
                _entityPath,
                ReceiveMode.PeekLock);

            _messageSender = new MessageSender(_connectionString, _entityPath);
        }

        public override Task AbandonAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _messageReceiver.AbandonAsync(message.LeaseId);
        }

        public override Task CompleteAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _messageReceiver.CompleteAsync(message.LeaseId);
        }

        public override async Task<IMessage> DequeueAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // This timeout is arbitrary. It is needed in order to return null
            // if no messages are queued, and must be long enough to allow time
            // for connection setup.
            var message = await _messageReceiver.ReceiveAsync(TimeSpan.FromSeconds(10));

            if (message == null)
            {
                return null;
            }

            return new AzureServiceBusMessage(message);
        }

        public override Task EnqueueAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _messageSender.SendAsync(new Message(message.Body) { ContentType = "application/json" });
        }

        public override Task RegisterMessageHandler(
            Func<IMessage, CancellationToken, Task> messageHandler,
            MessageHandlerOptions messageHandlerOptions)
        {
            if (messageHandlerOptions == null)
            {
                throw new ArgumentNullException(nameof(messageHandlerOptions));
            }

            var exceptionReceivedHandler = GetExceptionReceivedHandler(messageHandlerOptions);

            var options = new Microsoft.Azure.ServiceBus.MessageHandlerOptions(exceptionReceivedHandler)
            {
                AutoComplete = messageHandlerOptions.AutoComplete,
                MaxConcurrentCalls = messageHandlerOptions.MaxConcurrentCalls,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(5)
            };

            _messageReceiver.RegisterMessageHandler(
                (message, token) => messageHandler(new AzureServiceBusMessage(message), token),
                options);

            return Task.CompletedTask;
        }

        public override Task RenewLockAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _messageReceiver.RenewLockAsync(message.LeaseId);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _messageReceiver.CloseAsync()
                        .GetAwaiter()
                        .GetResult();

                    _messageSender.CloseAsync()
                        .GetAwaiter()
                        .GetResult();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private static Func<ExceptionReceivedEventArgs, Task> GetExceptionReceivedHandler(
            MessageHandlerOptions options)
        {
            if (options?.ExceptionReceivedHandler == null)
            {
                return _ => Task.CompletedTask;
            }

            Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
            {
                var context = new MessageHandlerExceptionContext(args.Exception);

                return options.ExceptionReceivedHandler(new MessageHandlerExceptionContext(args.Exception));
            }

            return ExceptionReceivedHandler;
        }
    }
}
