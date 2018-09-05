namespace Enable.Extensions.Messaging.Abstractions
{
    internal class Message : BaseMessage
    {
        private readonly byte[] _payload;

        public Message()
        {
            _payload = new byte[0];
        }

        public Message(byte[] payload)
        {
            _payload = payload;
        }

        public override byte[] Body => _payload;

        public override uint DequeueCount => throw new System.NotImplementedException();

        public override string LeaseId => throw new System.NotImplementedException();

        public override string MessageId { get; }
    }
}
