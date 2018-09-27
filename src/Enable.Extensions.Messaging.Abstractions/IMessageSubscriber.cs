using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enable.Extensions.Messaging.Abstractions
{
    public interface IMessageSubscriber : IDisposable
    {
        /// <summary>
        /// Negative acknowledgement that the message was not processed correctly.
        /// </summary>
        /// <remarks>
        /// Calling this method will increment the delivery count of the message and,
        /// if the devlivery count exceeds the maximum delivery count,
        /// asynchronously moves the message to the dead letter queue.
        /// </remarks>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task AbandonAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Acknowledge the message has been successfully received and processed.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task CompleteAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously retrieve a message from the message bus.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task<IMessage> DequeueAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Register a message handler. This handler is awaited each time that
        /// a new message is received.
        /// </summary>
        /// <param name="messageHandler">The handler that processes each message.</param>
        Task RegisterMessageHandler(
            Func<IMessage, CancellationToken, Task> messageHandler);

        /// <summary>
        /// Register a message handler. This handler is awaited each time that
        /// a new message is received.
        /// </summary>
        /// <param name="messageHandler">The handler that processes each message.</param>
        /// <param name="messageHandlerOptions">The settings used to configure how messages are received.</param>
        Task RegisterMessageHandler(
            Func<IMessage, CancellationToken, Task> messageHandler,
            MessageHandlerOptions messageHandlerOptions);

        Task RenewLockAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
