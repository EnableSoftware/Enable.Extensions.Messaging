using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Enable.Extensions.Messaging.Abstractions
{
    public abstract class BaseMessagingClient : IMessagingClient
    {
        public abstract Task AbandonAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task CompleteAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task<IMessage> DequeueAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract Task EnqueueAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken));

        public Task EnqueueAsync(
            byte[] content,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IMessage message = new Message(content);

            return EnqueueAsync(message, cancellationToken);
        }

        public Task EnqueueAsync(
            string content,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return EnqueueAsync<string>(content, cancellationToken);
        }

        public Task EnqueueAsync<T>(
            T content,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var json = JsonConvert.SerializeObject(content);

            var payload = Encoding.UTF8.GetBytes(json);

            IMessage message = new Message(payload);

            return EnqueueAsync(message, cancellationToken);
        }

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

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
