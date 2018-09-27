using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enable.Extensions.Messaging.Abstractions
{
    public abstract class BaseMessageSubscriber : IMessageSubscriber
    {
        public abstract Task AbandonAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task CompleteAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task<IMessage> DequeueAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        public Task RegisterMessageHandler(
            Func<IMessage, CancellationToken, Task> messageHandler)
        {
            var messageHandlerOptions = new MessageHandlerOptions();

            return RegisterMessageHandler(messageHandler, messageHandlerOptions);
        }

        public abstract Task RegisterMessageHandler(
            Func<IMessage, CancellationToken, Task> messageHandler,
            MessageHandlerOptions messageHandlerOptions);

        public abstract Task RenewLockAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
