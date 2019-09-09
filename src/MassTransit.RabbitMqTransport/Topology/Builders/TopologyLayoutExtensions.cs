namespace MassTransit.RabbitMqTransport.Topology.Builders
{
    using Context;


    public static class TopologyLayoutExtensions
    {
        public static void LogResult(this BrokerTopology layout)
        {
            foreach (var exchange in layout.Exchanges)
            {
                LogContext.LogInformation("Exchange: {ExchangeName}, type: {ExchangeType}, durable: {Durable}, auto-delete: {AutoDelete}", exchange.ExchangeName,
                    exchange.ExchangeType, exchange.Durable, exchange.AutoDelete);
            }

            foreach (var binding in layout.ExchangeBindings)
            {
                LogContext.LogInformation("Binding: source {Source}, destination: {Destination}, routingKey: {RoutingKey}", binding.Source.ExchangeName,
                    binding.Destination.ExchangeName, binding.RoutingKey);
            }
        }
    }
}
