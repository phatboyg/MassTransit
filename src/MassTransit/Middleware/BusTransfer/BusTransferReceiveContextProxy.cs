namespace MassTransit.Middleware.BusTransfer
{
    using Context;
    using DependencyInjection;


    public class BusTransferReceiveContextProxy<TBus> :
        ReceiveContextProxy
        where TBus : class, IBus
    {
        readonly BusTransferOptions _options;
        readonly IScopedBusContextProvider<TBus> _provider;

        public BusTransferReceiveContextProxy(ReceiveContext context, IScopedBusContextProvider<TBus> provider, BusTransferOptions options)
            : base(context)
        {
            _provider = provider;
            _options = options;
        }

        public override ISendEndpointProvider SendEndpointProvider =>
            _options.HasFlag(BusTransferOptions.Send)
                ? _provider.Context.SendEndpointProvider
                : base.SendEndpointProvider;

        public override IPublishEndpointProvider PublishEndpointProvider =>
            _options.HasFlag(BusTransferOptions.Publish)
                ? _provider.Context.PublishEndpointProvider
                : base.PublishEndpointProvider;
    }
}
