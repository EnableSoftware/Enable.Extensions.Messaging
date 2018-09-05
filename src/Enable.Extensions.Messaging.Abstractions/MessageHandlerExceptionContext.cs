using System;

namespace Enable.Extensions.Messaging.Abstractions
{
    public class MessageHandlerExceptionContext
    {
        public MessageHandlerExceptionContext(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
