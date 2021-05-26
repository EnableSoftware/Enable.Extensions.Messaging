using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Enable.Extensions.Messaging.Abstractions
{
    public interface IMessagePublisher : IDisposable
    {
        /// <summary>
        /// Asynchronously enqueue a message on to the message bus.
        /// </summary>
        /// <param name="message">The message to enqueue.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync(
            IMessage message,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously enqueue a batch of messages on to the message bus.
        /// </summary>
        /// <param name="messages">The collection of messages to enqueue.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync(
            IEnumerable<IMessage> messages,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously schedules a message on to the message bus to be published later.
        /// </summary>
        /// <param name="message">The message to enqueue.</param>
        /// <param name="scheduledTimeUtc">
        /// The UTC time at which the message should be visibile to dequeue.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync(
            IMessage message,
            DateTimeOffset scheduledTimeUtc,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously enqueue a message on to the message bus.
        /// </summary>
        /// <param name="content">The payload of the message to enqueue.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync(
            byte[] content,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously schedules a message on to the message bus to be published later.
        /// </summary>
        /// <param name="content">The payload of the message to enqueue.</param>
        /// <param name="scheduledTimeUtc">
        /// The UTC time at which the message should be visibile to dequeue.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync(
            byte[] content,
            DateTimeOffset scheduledTimeUtc,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously enqueue a message on to the message bus.
        /// </summary>
        /// <param name="content">The payload of the message to enqueue.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync(
            string content,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously enqueue a batch of messages on to the message bus.
        /// </summary>
        /// <param name="messages">The collection of payloads to enqueue.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync(
            IEnumerable<string> messages,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously schedules a message on to the message bus to be published later.
        /// </summary>
        /// <param name="content">The payload of the message to enqueue.</param>
        /// <param name="scheduledTimeUtc">
        /// The UTC time at which the message should be visibile to dequeue.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync(
            string content,
            DateTimeOffset scheduledTimeUtc,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously enqueue a message on to the message bus.
        /// </summary>
        /// <typeparam name="T">The type of the payload to enqueue.</typeparam>
        /// <param name="content">The payload of the message to enqueue.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync<T>(
            T content,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously enqueue a batch of messages on to the message bus.
        /// </summary>
        /// <typeparam name="T">The type of the payload to enqueue.</typeparam>
        /// <param name="messages">The collection of payload items to enqueue.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync<T>(
            IEnumerable<T> messages,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously schedules a message on to the message bus to be published later.
        /// </summary>
        /// <typeparam name="T">The type of the payload to enqueue.</typeparam>
        /// <param name="content">The payload of the message to enqueue.</param>
        /// <param name="scheduledTimeUtc">
        /// The UTC time at which the message should be visibile to dequeue.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for a task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task EnqueueAsync<T>(
            T content,
            DateTimeOffset scheduledTimeUtc,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
