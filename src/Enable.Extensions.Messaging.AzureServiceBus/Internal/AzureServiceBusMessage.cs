using Enable.Extensions.Messaging.Abstractions;
using Microsoft.Azure.ServiceBus;

namespace Enable.Extensions.Messaging.AzureServiceBus.Internal
{
    public class AzureServiceBusMessage : BaseMessage
    {
        private readonly Message _message;

        public AzureServiceBusMessage(Message message)
        {
            _message = message;
        }

        public override string MessageId => _message.MessageId;

        public override string SessionId => _message.SessionId;

        public override byte[] Body => _message.Body;

        public override uint DequeueCount => (uint)_message.SystemProperties.DeliveryCount;

        public override string LeaseId => _message.SystemProperties.LockToken;
    }
}
