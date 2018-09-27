using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Enable.Extensions.Messaging.Abstractions
{
    public abstract class BaseMessagePublisher : IMessagePublisher
    {
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
