using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Enable.Extensions.Messaging.AzureServiceBus.Tests
{
    public class AzureServiceBusTestFixture
    {
        public AzureServiceBusTestFixture()
        {
            ConnectionString = GetEnvironmentVariable("AZURE_SERVICE_BUS_CONNECTION_STRING");

            TopicName = GetEnvironmentVariable("AZURE_SERVICE_BUS_TOPIC_NAME");

            SubscriptionName = GetEnvironmentVariable("AZURE_SERVICE_BUS_SUBSCRIPTION_NAME");
        }

        public string ConnectionString { get; private set; }

        public string TopicName { get; private set; }

        public string SubscriptionName { get; private set; }

        public async Task ClearQueue()
        {
            var entityPath = EntityNameHelper.FormatSubscriptionPath(TopicName, SubscriptionName);

            var messageReceiver = new MessageReceiver(
                ConnectionString,
                entityPath,
                ReceiveMode.ReceiveAndDelete);

            messageReceiver.PrefetchCount = 256;

            IList<Message> messages;

            do
            {
                messages = await messageReceiver.ReceiveAsync(
                    maxMessageCount: 256,
                    operationTimeout: TimeSpan.FromSeconds(10));
            }
            while (messages?.Count > 0);

            await messageReceiver.CloseAsync();

            messageReceiver = null;
        }

        private static string GetEnvironmentVariable(string name)
        {
            try
            {
                var value = Environment.GetEnvironmentVariable(name);

                if (value == null)
                {
                    throw new Exception($"The environment variable '{name}' could not be found.");
                }

                return value;
            }
            catch (SecurityException ex)
            {
                throw new Exception($"The environment variable '{name}' is not accessible.", ex);
            }
        }
    }
}
