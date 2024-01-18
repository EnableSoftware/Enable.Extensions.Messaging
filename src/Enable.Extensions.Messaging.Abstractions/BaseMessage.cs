using System;
using System.Text;
using Newtonsoft.Json;

namespace Enable.Extensions.Messaging.Abstractions
{
    public abstract class BaseMessage : IMessage
    {
        public abstract string MessageId { get; }

        public virtual string SessionId => null;

        public abstract string LeaseId { get; }

        public abstract uint DequeueCount { get; }

        public virtual DateTime EnqueuedTimeUtc { get; }

        public abstract byte[] Body { get; }

        public T GetBody<T>()
        {
            var payload = Body;

            var json = Encoding.UTF8.GetString(payload);

            var body = JsonConvert.DeserializeObject<T>(json);

            return body;
        }
    }
}
