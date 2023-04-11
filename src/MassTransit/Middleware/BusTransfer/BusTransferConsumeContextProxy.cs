namespace MassTransit.Middleware.BusTransfer
{
    using System;
    using System.Threading.Tasks;
    using Context;
    using DependencyInjection;


    public class BusTransferConsumeContextProxy<TBus, TMessage> :
        ConsumeContextProxy<TMessage>
        where TBus : class, IBus
        where TMessage : class
    {
        readonly ConsumeContext<TMessage> _context;
        readonly BusTransferOptions _options;
        readonly IScopedBusContextProvider<TBus> _provider;

        public BusTransferConsumeContextProxy(ConsumeContext<TMessage> context, IScopedBusContextProvider<TBus> provider, BusTransferOptions options)
            : base(new BusTransferReceiveContextProxy<TBus>(context.ReceiveContext, provider, options), context)
        {
            _context = context;
            _provider = provider;
            _options = options;
        }

        public override Task<ISendEndpoint> GetPublishSendEndpoint<T>()
        {
            return _options.HasFlag(BusTransferOptions.Publish)
                ? _provider.Context.PublishEndpointProvider.GetPublishSendEndpoint<T>()
                : _context.GetPublishSendEndpoint<T>();
        }

        public override Task<ISendEndpoint> GetSendEndpoint(Uri address)
        {
            return _options.HasFlag(BusTransferOptions.Send)
                ? _provider.Context.SendEndpointProvider.GetSendEndpoint(address)
                : _context.GetSendEndpoint(address);
        }
    }
}
