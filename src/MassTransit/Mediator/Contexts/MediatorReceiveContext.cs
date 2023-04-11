namespace MassTransit.Mediator.Contexts
{
    using System;
    using System.Diagnostics;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Context;
    using DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Middleware;
    using Serialization;
    using Transports;
    using Util;


    static class MediatorReceiveContext
    {
        const string ContentTypeHeaderValue = "application/vnd.masstransit+obj";
        internal static readonly ContentType ObjectContentType = new ContentType(ContentTypeHeaderValue);
    }


    public sealed class MediatorReceiveContext<TMessage> :
        ProxyPipeContext,
        ReceiveContext
        where TMessage : class
    {
        readonly MediatorConsumeContext<TMessage> _consumeContext;
        readonly MessageIdMessageHeader _headers;
        readonly IReceiveObserver _observers;
        readonly BusTransferOptions _options;
        readonly PendingTaskCollection _receiveTasks;
        readonly Stopwatch _receiveTimer;
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IPublishEndpointProvider _publishEndpointProvider;

        public MediatorReceiveContext(SendContext<TMessage> sendContext, ISendEndpointProvider sendEndpointProvider,
            IPublishEndpointProvider publishEndpointProvider, IPublishTopology publishTopology, IReceiveObserver observers,
            IObjectDeserializer objectDeserializer, BusTransferOptions options)
            : base(sendContext)
        {
            _observers = observers;
            _options = options;

            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpointProvider = publishEndpointProvider;
            PublishTopology = publishTopology;

            _receiveTimer = Stopwatch.StartNew();

            var messageId = sendContext.MessageId ?? throw new ArgumentNullException(nameof(MessageContext.MessageId));

            _headers = new MessageIdMessageHeader(messageId);

            _receiveTasks = new PendingTaskCollection(4);

            var messageContext = new MediatorSendMessageContext<TMessage>(sendContext);

            var serializationContext = new MediatorSerializationContext<TMessage>(objectDeserializer, messageContext, sendContext.Message,
                MessageTypeCache<TMessage>.MessageTypeNames);

            _consumeContext = new MediatorConsumeContext<TMessage>(this, serializationContext, sendContext.Message);

            AddOrUpdatePayload<ConsumeContext>(() => _consumeContext, existing => _consumeContext);
        }

        public IPublishTopology PublishTopology { get; }

        public bool IsDelivered { get; internal set; }
        public bool IsFaulted { get; private set; }

        public bool PublishFaults => false;
        public MessageBody Body => new NotSupportedMessageBody();

        public Task ReceiveCompleted => _receiveTasks.Completed(CancellationToken);

        public void AddReceiveTask(Task task)
        {
            _receiveTasks.Add(task);
        }

        public ISendEndpointProvider SendEndpointProvider
        {
            get
            {
                if (_options.HasFlag(BusTransferOptions.Send))
                {
                    if (_consumeContext?.TryGetPayload<IServiceScope>(out var scope) ?? false)
                    {
                        var bus = scope.ServiceProvider.GetService<IBus>();
                        var provider = scope.ServiceProvider.GetService<ScopedConsumeContextProvider>();

                        if (bus != null && provider != null)
                        {
                            if (provider.HasContext)
                            {
                                return new ScopedConsumeSendEndpointProvider(bus, provider.GetContext(), scope.ServiceProvider);
                            }

                            return new ScopedSendEndpointProvider(bus, scope.ServiceProvider);
                        }
                    }
                }

                return _sendEndpointProvider;
            }
        }

        public IPublishEndpointProvider PublishEndpointProvider
        {
            get
            {
                if (_options.HasFlag(BusTransferOptions.Publish))
                {
                    if (_consumeContext?.TryGetPayload<IServiceScope>(out var scope) ?? false)
                    {
                        var bus = scope.ServiceProvider.GetService<IBus>();
                        var provider = scope.ServiceProvider.GetService<ScopedConsumeContextProvider>();

                        if (bus != null && provider != null)
                        {
                            if (provider.HasContext)
                            {
                                return new ScopedConsumePublishEndpointProvider(bus, provider.GetContext(), scope.ServiceProvider);
                            }

                            return new ScopedPublishEndpointProvider(bus, scope.ServiceProvider);
                        }
                    }
                }

                return _publishEndpointProvider;
            }
        }

        public bool Redelivered => false;
        public Headers TransportHeaders => _headers;

        public Task NotifyConsumed<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType)
            where T : class
        {
            IsDelivered = true;

            context.LogConsumed(duration, consumerType);

            return _observers.PostConsume(context, duration, consumerType);
        }

        public Task NotifyFaulted<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception)
            where T : class
        {
            IsFaulted = true;

            context.LogFaulted(duration, consumerType, exception);

            GetOrAddPayload<ConsumerFaultContext>(() => new FaultContext(TypeCache<T>.ShortName, consumerType));

            return _observers.ConsumeFault(context, duration, consumerType, exception);
        }

        public Task NotifyFaulted(Exception exception)
        {
            IsFaulted = true;

            this.LogFaulted(exception);

            return _observers.ReceiveFault(this, exception);
        }

        public TimeSpan ElapsedTime => _receiveTimer.Elapsed;
        public Uri InputAddress => _consumeContext.DestinationAddress;
        public ContentType ContentType => MediatorReceiveContext.ObjectContentType;


        class FaultContext :
            ConsumerFaultContext
        {
            public FaultContext(string messageType, string consumerType)
            {
                MessageType = messageType;
                ConsumerType = consumerType;
            }

            public string MessageType { get; }
            public string ConsumerType { get; }
        }
    }
}
