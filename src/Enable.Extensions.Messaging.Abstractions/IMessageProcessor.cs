using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enable.Extensions.Messaging.Abstractions
{
    public interface IMessageProcessor : IDisposable
    {
        /// <summary>
        /// Register a message handler. This handler is awaited each time that
        /// a new message is received.
        /// </summary>
        /// <param name="messageHandler">The handler that processes each message.</param>
        /// <param name="errorHandler">The handler that processes errors.</param>
        Task RegisterMessageHandler(
            Func<IMessage, CancellationToken, Task> messageHandler,
            Func<MessageHandlerExceptionContext, Task> errorHandler = null);
    }
}
