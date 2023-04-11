namespace MassTransit
{
    public interface IMediatorConfigurator :
        IReceiveEndpointConfigurator,
        IConsumeObserverConnector,
        ISendObserverConnector,
        IPublishObserverConnector
    {
        void UseBusTransfer(BusTransferOptions options = BusTransferOptions.Default);
    }
}
