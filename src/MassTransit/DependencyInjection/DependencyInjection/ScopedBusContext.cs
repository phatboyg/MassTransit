namespace MassTransit.DependencyInjection
{
    public interface ScopedBusContext
    {
        ISendEndpointProvider SendEndpointProvider { get; }
        IPublishEndpointProvider PublishEndpointProvider { get; }
        IPublishEndpoint PublishEndpoint { get; }
        IScopedClientFactory ClientFactory { get; }
    }
}
