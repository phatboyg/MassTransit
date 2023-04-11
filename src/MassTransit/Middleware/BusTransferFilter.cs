namespace MassTransit.Middleware
{
    using System.Threading.Tasks;
    using BusTransfer;
    using DependencyInjection;
    using Metadata;


    public class BusTransferFilter<TBus, TMessage> :
        IFilter<ConsumeContext<TMessage>>
        where TBus : class, IBus
        where TMessage : class
    {
        readonly IScopedBusContextProvider<TBus> _provider;

        public BusTransferFilter(IScopedBusContextProvider<TBus> provider)
        {
            _provider = provider;
        }

        public Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            var transferContext = new BusTransferConsumeContextProxy<TBus, TMessage>(context, _provider, BusTransferOptions.Default);

            return next.Send(transferContext);
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("busTransfer");
            scope.Add("destination", TypeMetadataCache<TBus>.ShortName);
        }
    }
}
