namespace Enable.Extensions.Messaging.Abstractions
{
    public interface IMessagingClientFactory
    {
        IMessagingClient GetMessagingClient(string topicName, string subscriptionName);
    }
}
