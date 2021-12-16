# Enable.Extensions.Messaging

[![Build status](https://ci.appveyor.com/api/projects/status/tkq8x7k9gx0yowhv/branch/main?svg=true)](https://ci.appveyor.com/project/EnableSoftware/enable-extensions-messaging/branch/main)

Messaging abstractions for building testable applications.

Message buses provide asynchronous message communication. They allow
applications to easily scale out workloads to multiple processors, improve the
resiliency of applications to sudden peaks in workloads and naturally lead to
loosely coupled components or applications.

`Enable.Extensions.Messaging` currently provides three messaging implementations:

- [`Enable.Extensions.Messaging.InMemory`]: An in memory implementation.
  This implementation is only valid for the lifetime of the host process.
  This is great for use in test code.

- [`Enable.Extensions.Messaging.RabbitMQ`]: A [RabbitMQ] implementation.

- [`Enable.Extensions.Messaging.AzureServiceBus`]: An [Azure Service Bus] implementation.

In addition to these packages, an additional [`Enable.Extensions.Messaging.Abstractions`]
package is available. This contains the basic abstractions that the implementations
listed above build upon. Use [`Enable.Extensions.Messaging.Abstractions`] to implement
your own messaging provider.

Package name                                | NuGet version
--------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
`Enable.Extensions.Messaging.Abstractions`    | [![NuGet](https://img.shields.io/nuget/v/Enable.Extensions.Messaging.Abstractions.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Enable.Extensions.Messaging.Abstractions/)
`Enable.Extensions.Messaging.InMemory`        | [![NuGet](https://img.shields.io/nuget/v/Enable.Extensions.Messaging.InMemory.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Enable.Extensions.Messaging.InMemory/)
`Enable.Extensions.Messaging.RabbitMQ`        | [![NuGet](https://img.shields.io/nuget/v/Enable.Extensions.Messaging.RabbitMQ.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Enable.Extensions.Messaging.RabbitMQ/)
`Enable.Extensions.Messaging.AzureServiceBus` | [![NuGet](https://img.shields.io/nuget/v/Enable.Extensions.Messaging.AzureServiceBus.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Enable.Extensions.Messaging.AzureServiceBus/)


## Queues vs. message buses

There are two types of messages, and therefore two types of messaging queues,
that we may want to leverage, depending on the intent of the publisher of a
message. The first type of messages are *commands* or *jobs* that must be
processed. The second are *events* that notify of something that has already
happened. In the former case, we would need an actor to action the command,
and we would want a single actor to do this, potentially reporting back the
outcome of the processing. In the latter case, we are simply notifying any
listeners of something that has already happened, and we don't have any
expectation of how that event is to be handled.

`Enable.Extensions.Messaging` provides the second of these types of messaging
queues. Use any of the queue implementations provided to queue up work items,
and have one out of any number of consumers process each individual message.

## Examples

The following example demonstrates publishing a message and then retrieving
the same message. This uses a single application to both send and receive
messages. In production systems you'll more likely have (multiple) different
components publishing and consuming messages.

Here we're using the RabbitMQ queue provider. How we work with messages is the
same across any of the available queue implementations. The only differences
are in the options that are passed in when constructing the queue factory.

```csharp
using System;
using System.Threading.Tasks;
using Enable.Extensions.Messaging.Abstractions;
using Enable.Extensions.Messaging.RabbitMQ;

namespace MessagingSamples
{
    public class Program
    {
        public static void Main() => MainAsync().GetAwaiter().GetResult();

        public static async Task MainAsync()
        {
            var options = new RabbitMQQueueClientFactoryOptions
            {
                HostName = "localhost",
                Port = 5672,
                VirtualHost = "/"
                UserName = "guest",
                Password = "guest",
            };

            var queueFactory = new RabbitMQMessagingClientFactory(options);

            // A topic is a essentially a queue that we want to publish
            // messages to.
            var topicName = "your-topic-name";

            // We start by getting a publisher to the topic that you want to
            // work with. The queue will be automatically created if it doesn't
            // exist.
            using (var publisher = queueFactory.GetMessagePublisher(topicName))
            {
                // Add a new item to the queue.
                // Here we're using a `string`, but any type of message can
                // be used as long as it can be serialised to a byte array.
                await publisher.EnqueueAsync("Hello, World!");
            }

            // Publishing messages to a topic and subscribing to messages on
            // that topic are spearated out into different interfaces. We
            // therefore now need to a subscriber to our topic. Each unique
            // subscription gets a unique name. A message published to a topic
            // will be delievered once to each unique subscription.
            var subscriptionName = "your-subscription-name";
            
            using (var subscriber = queueFactory.GetMessageSubscriber(topicName, subscriptionName))
            {
                // Retrieve our message from the queue.
                // We get this back as a type of `IMessage` (note that
                // we could get `null` back here, if there are no messages
                // in the queue).
                var message = await subscriber.DequeueAsync();

                // The contents of the message that we sent are available
                // from `IMessage.Body`. This property is our original
                // message, "Hello, World!", stored as a byte array. This
                // might not be that useful, so you can use the
                // `IMessage.GetBody<T>` method to deserialise this back
                // to your original type.
                // In this case, we want to get our `string` back:
                var payload = message.GetBody<string>();

                // The following will print `Hello, World!`.
                Console.WriteLine(payload);

                // Now, do some clever work with the message.
                // Here's your chance to shine…

                // Finally, we acknowledge the message once we're done.
                // We can either "complete" the message, which means that
                // we've successfully processed it. This will remove the
                // message from our queue.
                await subscriber.CompleteAsync(message);

                // Or if something goes wrong and we can't process the
                // message, we can "abandon" it (but don't call both
                // `CompleteAsync` and `AbandonAsync`!).
                // await subscriber.AbandonAsync(message);
            }
        }
    }
}
```

[RabbitMQ]: https://www.rabbitmq.com/
[Azure Service Bus]: https://azure.microsoft.com/services/service-bus/

[`Enable.Extensions.Messaging.Abstractions`]: https://www.nuget.org/packages/Enable.Extensions.Messaging.Abstractions/
[`Enable.Extensions.Messaging.InMemory`]: https://www.nuget.org/packages/Enable.Extensions.Messaging.InMemory/
[`Enable.Extensions.Messaging.RabbitMQ`]: https://www.nuget.org/packages/Enable.Extensions.Messaging.RabbitMQ/
[`Enable.Extensions.Messaging.AzureServiceBus`]: https://www.nuget.org/packages/Enable.Extensions.Messaging.AzureServiceBus/

