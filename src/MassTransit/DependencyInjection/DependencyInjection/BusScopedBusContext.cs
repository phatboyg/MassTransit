#nullable enable
namespace MassTransit.DependencyInjection
{
    using System;
    using Clients;
    using Transports;


    public class BusScopedBusContext<TBus> :
        ScopedBusContext
        where TBus : class, IBus
    {
        readonly TBus _bus;
        readonly IClientFactory _clientFactory;
        readonly IServiceProvider _provider;
        IPublishEndpoint? _publishEndpoint;
        IPublishEndpointProvider? _publishEndpointProvider;
        IScopedClientFactory? _scopedClientFactory;
        ISendEndpointProvider? _sendEndpointProvider;

        public BusScopedBusContext(TBus bus, IClientFactory clientFactory, IServiceProvider provider)
        {
            _bus = bus;
            _clientFactory = clientFactory;
            _provider = provider;
        }

        public ISendEndpointProvider SendEndpointProvider
        {
            get { return _sendEndpointProvider ??= new ScopedSendEndpointProvider(_bus, _provider); }
        }

        public IPublishEndpointProvider PublishEndpointProvider
        {
            get { return _publishEndpointProvider ??= new ScopedPublishEndpointProvider(_bus, _provider); }
        }

        public IPublishEndpoint PublishEndpoint
        {
            get { return _publishEndpoint ??= new PublishEndpoint(PublishEndpointProvider); }
        }

        public IScopedClientFactory ClientFactory
        {
            get
            {
                return _scopedClientFactory ??=
                    new ScopedClientFactory(new ClientFactory(new ScopedClientFactoryContext(_clientFactory, _provider)), null);
            }
        }
    }
}
