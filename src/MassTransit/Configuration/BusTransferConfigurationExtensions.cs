namespace MassTransit
{
    using System;
    using Configuration;
    using Middleware;


    public static class BusTransferConfigurationExtensions
    {
        public static void UseBusTransfer(this IConsumePipeConfigurator configurator, IServiceProvider provider,
            BusTransferOptions options = BusTransferOptions.Default,
            Action<IMessageTypeFilterConfigurator> configureMessageTypeFilter = null)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            var messageTypeFilterConfigurator = new MessageTypeFilterConfigurator();
            configureMessageTypeFilter?.Invoke(messageTypeFilterConfigurator);

            var observer = new ScopedConsumePipeSpecificationObserver(typeof(BusTransferFilter<,>), provider, messageTypeFilterConfigurator.Filter,
                typeof(IBus));

            configurator.ConnectConsumerConfigurationObserver(observer);
            configurator.ConnectSagaConfigurationObserver(observer);
        }
    }
}
