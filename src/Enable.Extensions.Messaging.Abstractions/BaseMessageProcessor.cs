using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enable.Extensions.Messaging.Abstractions
{
    public abstract class BaseMessageProcessor : IMessageProcessor
    {
        public abstract Task RegisterMessageHandler(
            Func<IMessage, CancellationToken, Task> messageHandler,
            Func<MessageHandlerExceptionContext, Task> errorHandler = null);

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
