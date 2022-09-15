using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Enable.Extensions.Messaging.Abstractions;

namespace Enable.Extensions.Messaging.AzureServiceBus.Internal.V2
{
    public class AzureServiceBusMessageProcessor : BaseMessageProcessor
    {
        private readonly ServiceBusProcessor _serviceBusProcessor;

        private bool _disposed;

        public AzureServiceBusMessageProcessor(
            string connectionString,
            string topicName,
            string subscriptionName,
            MessageHandlerOptions messageHandlerOptions)
        {
            if (messageHandlerOptions is null)
            {
                throw new ArgumentNullException(nameof(messageHandlerOptions));
            }

            var options = new ServiceBusProcessorOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                MaxConcurrentCalls = messageHandlerOptions.MaxConcurrentCalls,
                AutoCompleteMessages = messageHandlerOptions.AutoComplete
            };

            _serviceBusProcessor = new ServiceBusClient(connectionString)
                .CreateProcessor(
                    topicName,
                    subscriptionName,
                    options);
        }

        public override Task RegisterMessageHandler(
            Func<IMessage, CancellationToken, Task> messageHandler,
            Func<MessageHandlerExceptionContext, Task> errorHandler = null)
        {
            if (messageHandler is null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            if (errorHandler is null)
            {
                errorHandler = _ => Task.CompletedTask;
            }

            _serviceBusProcessor.ProcessMessageAsync += (processMessageEventArgs) =>
                messageHandler(new AzureServiceBusMessageV2(processMessageEventArgs.Message), default(CancellationToken));

            _serviceBusProcessor.ProcessErrorAsync += (processErrorEventArgs) =>
                errorHandler(new MessageHandlerExceptionContext(processErrorEventArgs.Exception));

            return _serviceBusProcessor.StartProcessingAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _serviceBusProcessor.CloseAsync()
                        .GetAwaiter()
                        .GetResult();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
