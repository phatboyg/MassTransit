namespace MassTransit.KafkaIntegration.Transport
{
    using System;
    using GreenPipes;


    public class KafkaProducerFactory<TKey, TValue> :
        IKafkaProducerFactory
        where TValue : class
    {
        readonly IKafkaProducerContext<TKey, TValue> _context;
        readonly KafkaTopicAddress _topicAddress;
        ConnectHandle _observerHandle;

        public KafkaProducerFactory(KafkaTopicAddress topicAddress, IKafkaProducerContext<TKey, TValue> context)
        {
            _context = context;
            _topicAddress = topicAddress;
        }

        public void OnStart()
        {
            _observerHandle = _context.ConnectObservers(this);
        }

        public Uri TopicAddress => _topicAddress;

        public void Dispose()
        {
            _context.Dispose();

            _observerHandle?.Dispose();
        }

        public ITopicProducer<TKey, TValue> CreateProducer(ConsumeContext consumeContext = null)
        {
            return new TopicProducer<TKey, TValue>(_topicAddress, _context, consumeContext);
        }
    }
}
