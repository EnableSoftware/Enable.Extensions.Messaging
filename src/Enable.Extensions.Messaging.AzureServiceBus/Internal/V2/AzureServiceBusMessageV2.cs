using Azure.Messaging.ServiceBus;
using Enable.Extensions.Messaging.Abstractions;

namespace Enable.Extensions.Messaging.AzureServiceBus.Internal.V2
{
    public class AzureServiceBusMessageV2 : BaseMessage
    {
        private readonly ServiceBusReceivedMessage _message;

        public AzureServiceBusMessageV2(ServiceBusReceivedMessage message)
        {
            _message = message;
        }

        public override string MessageId => _message.MessageId;

        public override string SessionId => _message.SessionId;

        public override byte[] Body => _message.Body.ToArray();

        public override uint DequeueCount => (uint)_message.DeliveryCount;

        public override string LeaseId => _message.LockToken;
    }
}
