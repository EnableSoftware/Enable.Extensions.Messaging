namespace Enable.Extensions.Messaging.RabbitMQ
{
    public class RabbitMQMessagingClientFactoryOptions
    {
        public string HostName { get; set; }

        public int Port { get; set; }

        public string VirtualHost { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
