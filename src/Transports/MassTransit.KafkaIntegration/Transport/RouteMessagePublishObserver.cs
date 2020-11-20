namespace MassTransit.KafkaIntegration.Transport
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;
    using Util;


    public class RouteMessagePublishObserver<TKey, TValue> :
        IPublishObserver
        where TValue : class
    {
        readonly KafkaProducerFactory<TKey, TValue> _factory;
        readonly KafkaKeyResolver<TKey, TValue> _keyResolver;

        public RouteMessagePublishObserver(KafkaProducerFactory<TKey, TValue> factory, KafkaKeyResolver<TKey, TValue> keyResolver)
        {
            _factory = factory;
            _keyResolver = keyResolver;
        }

        public Task PrePublish<T>(PublishContext<T> context)
            where T : class
        {
            return TaskUtil.Completed;
        }

        public Task PostPublish<T>(PublishContext<T> context)
            where T : class
        {
            if (context is PublishContext<TValue> publishContext)
            {
                publishContext.TryGetPayload(out ConsumeContext consumeContext);

                ITopicProducer<TKey, TValue> topicProducer = _factory.CreateProducer(consumeContext);

                var producer = new KeyedTopicProducer<TKey, TValue>(topicProducer, _keyResolver);

                return producer.Produce(context.Message, new Pipe<TValue>(publishContext));
            }

            return TaskUtil.Completed;
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception)
            where T : class
        {
            return TaskUtil.Completed;
        }


        class Pipe<T> :
            IPipe<SendContext<T>>
            where T : class
        {
            readonly PublishContext<T> _context;

            public Pipe(PublishContext<T> context)
            {
                _context = context;
            }

            public Task Send(SendContext<T> context)
            {
                context.MessageId = _context.MessageId;
                context.ConversationId = _context.ConversationId;
                context.CorrelationId = _context.CorrelationId;
                context.InitiatorId = _context.InitiatorId;

                foreach (var header in _context.Headers)
                    context.Headers.Set(header.Key, header.Value);

                return TaskUtil.Completed;
            }

            public void Probe(ProbeContext context)
            {
            }
        }
    }
}
