namespace MassTransit.DependencyInjection
{
    using System;
    using Transactions;


    public class TransactionalScopedBusContextProvider<TBus> :
        IScopedBusContextProvider<TBus>
        where TBus : class, IBus
    {
        public TransactionalScopedBusContextProvider(ITransactionalBus bus, Bind<TBus, IClientFactory> clientFactory,
            IScopedConsumeContextProvider<TBus> consumeContextProvider, IServiceProvider provider)
        {
            if (consumeContextProvider.HasContext)
                Context = new ConsumeContextScopedBusContext(consumeContextProvider.Context, clientFactory.Value);
            else
                Context = new BusScopedBusContext<IBus>(bus, clientFactory.Value, provider);
        }

        public ScopedBusContext Context { get; }
    }
}
