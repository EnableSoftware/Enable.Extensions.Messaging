namespace Enable.Extensions.Messaging.Abstractions
{
    public interface IMessagingClientFactory
    {
        IMessagePublisher GetMessagePublisher(string topicName);

        IMessagePublisher GetMessagePublisher(string topicName, string exchangeType, string routingKey);

        IMessageSubscriber GetMessageSubscriber(string topicName, string subscriptionName);
    }
}
