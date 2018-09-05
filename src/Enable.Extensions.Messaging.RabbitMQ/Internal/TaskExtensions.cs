using System;
using System.Threading.Tasks;

namespace Enable.Extensions.Messaging.RabbitMQ.Internal
{
    public static class TaskExtensions
    {
        public static void Ignore(this Task task)
        {
        }
    }
}
