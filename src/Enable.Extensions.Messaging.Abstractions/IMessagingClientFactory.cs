namespace Enable.Extensions.Messaging.Abstractions
{
    public interface IMessagingClientFactory
    {
        IMessagePublisher GetMessagePublisher(string topicName);

        IMessageSubscriber GetMessageSubscriber(string topicName, string subscriptionName);
    }
}
