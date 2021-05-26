using System;
using System.Collections.Generic;
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

        public abstract Task EnqueueAsync(
            IEnumerable<IMessage> messages,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task EnqueueAsync(
            IMessage message,
            DateTimeOffset scheduledTimeUtc,
            CancellationToken cancellationToken = default(CancellationToken));

        public Task EnqueueAsync(
            byte[] content,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IMessage message = new Message(content);

            return EnqueueAsync(message, cancellationToken);
        }

        public Task EnqueueAsync(
            byte[] content,
            DateTimeOffset scheduledTimeUtc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IMessage message = new Message(content);

            return EnqueueAsync(message, scheduledTimeUtc, cancellationToken);
        }

        public Task EnqueueAsync(
            string content,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return EnqueueAsync<string>(content, cancellationToken);
        }

        public Task EnqueueAsync(
            IEnumerable<string> messages,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return EnqueueAsync<string>(messages, cancellationToken);
        }

        public Task EnqueueAsync(
            string content,
            DateTimeOffset scheduledTimeUtc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return EnqueueAsync<string>(content, scheduledTimeUtc, cancellationToken);
        }

        public Task EnqueueAsync<T>(
            T content,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = SerializeMessage(content);

            return EnqueueAsync(message, cancellationToken);
        }

        public Task EnqueueAsync<T>(
            IEnumerable<T> messages,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var batch = SerializeMessages(messages);

            return EnqueueAsync(messages: batch, cancellationToken);
        }

        public Task EnqueueAsync<T>(
            T content,
            DateTimeOffset scheduledTimeUtc,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = SerializeMessage(content);

            return EnqueueAsync(message, scheduledTimeUtc, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        private IMessage SerializeMessage<T>(T content)
        {
            var json = JsonConvert.SerializeObject(content);

            var payload = Encoding.UTF8.GetBytes(json);

            return new Message(payload);
        }

        private IEnumerable<IMessage> SerializeMessages<T>(IEnumerable<T> messages)
        {
            foreach (var message in messages)
            {
                yield return SerializeMessage(message);
            }
        }
    }
}
