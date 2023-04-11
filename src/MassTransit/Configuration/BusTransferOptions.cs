namespace MassTransit
{
    using System;


    [Flags]
    public enum BusTransferOptions
    {
        None = 0,

        Publish = 1,

        Send = 2,

        RequestClient = 4,

        Default = Publish | Send,
    }
}
