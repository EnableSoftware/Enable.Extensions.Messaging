namespace Enable.Extensions.Messaging.Abstractions
{
    public interface IMessagingClientFactoryV2
    {
        IMessagePublisher GetMessagePublisher(string topicName);

        IMessageProcessor GetMessageProcessor(
            string topicName,
            string subscriptionName,
            MessageHandlerOptions messageHandlerOptions);
    }
}
